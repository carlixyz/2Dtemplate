using UnityEngine;
using System.Collections;

public class SuperController : MonoBehaviour 
{

//	AnimSprite Anime;
//
//	// Use this for initialization
//	void Start ()
//	{
//		Anime = GetComponent<AnimSprite> ();
//	}
//	
//	// Update is called once per frame
//	void Update () 
//	{
//		if (Input.GetAxisRaw ("Horizontal") != 0)
//		{
//			Anime.PlayFramesFast (0, 8, 10);
//			renderer.transform.localScale = new Vector3 (Mathf.Sign (Input.GetAxis("Horizontal")), 1, 1);						// Flip Sprite
//		} 
//		else 
//		{
//			Anime.PlayFramesFast (16, 1, 1);
//		}
//	}

	private Transform thisTransform;				// own player tranform cached
	private CharacterController controller;
//	[HideInInspector] public PlayerProperties properties;
	[HideInInspector] public AnimSprite animPlay; 				// : Component
	
	[HideInInspector] public int orientation = 1;					// Move Direction: -1 == left, +1 == right
	[HideInInspector] public Vector3 velocity = Vector3.zero;	    // Start quiet 
	
	public float gravity = 20.0f;
	public float fallSpeed = 0.5f;				// speed of falling down ( division factor )
	
	public float walkSpeed = 1.0f;				// standard walk speed
	public float runSpeed = 2.0f; 				// running speed 
	bool SuperJumpEnable = false;		        // toggle for run jump
	
	public float walkJump = 8.0f;				// jump height from walk
	public float runJump = 9.0f;				// jump height from run	
	public float Depth = .25f;				    // Depth in position.z for the player's character
	
	public AudioClip JumpSound;
	
	private bool jumpEnable = false;			// toggle for default jump
	private bool runJumpEnable = false;		    // toggle for run jump
	
	private float afterHitForceDown = 1.0f;		// toggle for crouch jump
	private int isHoldingObj = 0;			    // anim row index change when player holds something
	
	private int layerMask = 1 << 8;
	
	BoxCollider Collider;
	
	delegate void InputDelegate();              // This it's like a Pointer Function from C++, but this it's in a C# ( Ugly)
	InputDelegate UpdateInput;
	
	
	void Start()						        //	BallScript ballScript = target.GetComponent<BallScript>() as BallScript;
	{
		thisTransform = transform;
		controller = GetComponent<CharacterController>();
//		properties = GetComponent<PlayerProperties>();
		animPlay = GetComponent<AnimSprite>();
		animPlay.PlayFrames(16, 1, orientation);
		
		Physics.IgnoreCollision(controller.collider, transform.GetComponentInChildren<BoxCollider>());
		
		Collider = transform.GetComponentInChildren<BoxCollider>();
		
		if (Managers.Register.PlayerAutoRunning)
			UpdateInput = new InputDelegate(ControlAuto);
		else
			UpdateInput = new InputDelegate(ControlClassic);
		
	}
	
	void Update()
	{
		UpdateInput();
	}
	
	void ControlAuto()
	{
//		isHoldingObj = System.Convert.ToByte(properties._pickedObject != null);
		bool Stand = Physics.Linecast(thisTransform.position, thisTransform.TransformPoint(-Vector3.up), layerMask);
		Collider.center = Vector3.zero;
		
		if (controller.isGrounded)
		{
			runJumpEnable = false;
			
			velocity = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
			
			if (Input.GetAxis("Horizontal") == 0)									// IDLE -> keep stand quiet
//				animPlay.PlayFramesFixed(2 + isHoldingObj, 0, 1, orientation);
				animPlay.PlayFramesFast(16,1);
			
			if (Input.GetAxis("Horizontal") != 0)									// WALK
			{
				orientation = (int)(Mathf.Sign(velocity.x)); 		                // If move direction changes -> flip sprite
				renderer.transform.localScale = new Vector3 (orientation, 1, 1);						// Flip Sprite
				
				velocity.x *= walkSpeed;										    // If player is moving ->  Animate Walking..
//				animPlay.PlayFramesFixed((0 + isHoldingObj), 0, 8, orientation);
				animPlay.PlayFramesFast(0,8);
				
			}
			
			if ((Mathf.Abs(Input.GetAxis("Horizontal")) >= 1) && !Input.GetButton("Fire1") && !Managers.Dialog.IsInConversation())						// RUN 
			{
				velocity *= runSpeed;
//				animPlay.PlayFramesFixed(2, 1, 2, orientation, 1.005f);
				animPlay.PlayFramesFast(17,2);
				
			}
			
			if (velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 && !Managers.Dialog.IsInConversation())     // Crouch        
			{
//				animPlay.PlayFrames(3, 3, 1, orientation);
				animPlay.PlayFramesFast(37,1);
				Collider.center = Vector3.down * 0.25f;
			}
			
			
			if (Input.GetButtonDown("Jump"))                                                     // Always running jump
			{
				velocity.y = runJump;
				//Instantiate ( particleJump, particlePlacement, transform.rotation );
				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
				runJumpEnable = true;
			}
		}
		
		if (Input.GetButtonDown("Jump") && Stand)	            // slope jump
		{
			velocity.y = walkJump;
			Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.25f);
			runJumpEnable = true;
		}
		
		if (!controller.isGrounded && !Stand)	// && !Stand	    // Do Slide
		{
			velocity.x = Input.GetAxis("Horizontal");
			//animPlay.PlayFrames ( 2, 5, 1, orientation );
			
			if (Input.GetButtonUp("Jump"))						// check if the player keep pressing jump button..
				velocity.y *= fallSpeed;							// if not then brake the jump
			
			if (velocity.x != 0)
			{
				orientation = (int)Mathf.Sign(velocity.x); 			// If move direction changes -> update & flip sprite
				renderer.transform.localScale = new Vector3 (orientation, 1, 1);						// Flip Sprite
			}
			
			if (runJumpEnable)
			{
				velocity.x *= runSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
				animPlay.PlayFramesFast(20,1);
				
			}
			
			if (velocity.y < 0 && !Stand) 						// check when player stops elevation & becomes down..
			{
//				animPlay.PlayFrames(2 + isHoldingObj, 5, 1, orientation);
				animPlay.PlayFramesFast(21,1);
				
				if (Input.GetButton("Jump") && isHoldingObj == 0) // check if the player keep pressing jump button..
				{
					velocity.y += 18 * Time.deltaTime;
//					animPlay.PlayFrames(2, 6, 2, orientation);
					animPlay.PlayFramesFast(22,2);

				}
			}
		}
		
		if (controller.collisionFlags == CollisionFlags.Above)
		{
			velocity.y = 0;											// set velocity on Y to 0, stop upward motion
			velocity.y -= afterHitForceDown;						// apply force downward so player doesn't have in the air
		}
		
//		if (properties.BurnOut)
//			BurnOut();
		
		velocity.y -= gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
		
		var Pos = thisTransform.position;
		Pos.z = Depth;
		thisTransform.position = Pos;
		thisTransform.rotation = Quaternion.Euler(0, 0, 0);
	}
	
	void ControlClassic()
	{
//		isHoldingObj = System.Convert.ToByte(properties._pickedObject != null);
		bool Stand = Physics.Linecast(thisTransform.position, thisTransform.TransformPoint(-Vector3.up), layerMask);
		Collider.center = Vector3.zero;
		
		if (controller.isGrounded)
		{
			jumpEnable = false;
			runJumpEnable = false;
			SuperJumpEnable = false;
			
			velocity = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
			
			if (Input.GetAxis("Horizontal") == 0)									// IDLE -> keep stand quiet
//				animPlay.PlayFramesFixed(2 + isHoldingObj, 0, 1, orientation);
				animPlay.PlayFramesFast(2,1);
				
			
			if (Input.GetAxis("Horizontal") != 0)									// WALK
			{
				orientation = (int)(Mathf.Sign(velocity.x)); 		// (int)Math.Ceiling()	// If move direction changes -> flip sprite
				
				velocity.x *= walkSpeed;										// If player is moving ->  Animate Walking..
//				animPlay.PlayFramesFixed((0 + isHoldingObj), 0, 8, orientation);
					animPlay.PlayFramesFast(0,8);
				
			}
			
			if ((velocity.x != 0) && Input.GetButton("Fire1"))						// RUN 
			{
				velocity *= runSpeed;
//				animPlay.PlayFramesFixed(2, 1, 2, orientation, 1.005f);
				animPlay.PlayFramesFast(10,2);
			
			}

			
			if (velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 && !Managers.Dialog.IsInConversation())          // Crouch
			{
//				animPlay.PlayFrames(3, 3, 1, orientation);
					animPlay.PlayFramesFast(27,1);
				
				Collider.center = Vector3.up * -0.25f;
			}                           
			
			if (Input.GetButtonDown("Jump") && (!Input.GetButton("Fire1") || velocity.x == 0))	// Quiet jump
			{											// check player dont make a Running Jump being quiet in the same spot
				velocity.y = walkJump;
				//Instantiate ( particleJump, particlePlacement, transform.rotation );
				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.0f);
				jumpEnable = true;
			}
			
			if (Input.GetButtonDown("Jump") && ((Input.GetButton("Fire1") && velocity.x != 0) )) //|| properties.BurnOut))// running jump
			{
				velocity.y = runJump;
				//Instantiate ( particleJump, particlePlacement, transform.rotation );
				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
				runJumpEnable = true;
			}
			
			if (Input.GetButtonDown("Jump") && velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 ) //&& !properties.BurnOut)
			{
				velocity.y = 10.5f;        // SuperJump!
				//Instantiate(particleJump, particlePlacement, transform.rotation);
				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
				SuperJumpEnable = true;
			}
		}
		
		if (Input.GetButtonDown("Jump") && Stand)	// slope jump
		{
			velocity.y = walkJump;
			Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.25f);
			jumpEnable = true;
		}
		
		if (!controller.isGrounded && !Stand)	// && !Stand	// Do Slide
		{
			velocity.x = Input.GetAxis("Horizontal");
			//animPlay.PlayFrames ( 2, 5, 1, orientation );
			
			if (Input.GetButtonUp("Jump"))						// check if the player keep pressing jump button..
				velocity.y *= fallSpeed;							// if not then brake the jump
			
			if (velocity.x != 0)
				orientation = (int)Mathf.Sign(velocity.x); 				// If move direction changes -> update & flip sprite
			
			
			if (jumpEnable)
			{
				velocity.x *= walkSpeed;	 						// If player is jumping -> Update & Animate jumping type.
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
					animPlay.PlayFramesFast(34,1);
				
			}
			
			if (runJumpEnable)
			{
				velocity.x *= runSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
				animPlay.PlayFramesFast(34,1);
				
			}
			
			if (SuperJumpEnable)
			{
				velocity.x *= walkSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
				animPlay.PlayFramesFast(34,1);
			}
			
			if (velocity.y < 0 && !Stand) 						// check when player stops elevation & becomes down..
			{
//				animPlay.PlayFrames(2 + isHoldingObj, 5, 1, orientation);
				animPlay.PlayFramesFast(21,1);
				
				if (Input.GetButton("Jump") && isHoldingObj == 0) // check if the player keep pressing jump button..
				{
					velocity.y += 18 * Time.deltaTime;
//					animPlay.PlayFrames(2, 6, 2, orientation);
					animPlay.PlayFramesFast(50,2);
				}
			}
		}
		
		if (controller.collisionFlags == CollisionFlags.Above)
		{
			velocity.y = 0;											// set velocity on Y to 0, stop upward motion
			velocity.y -= afterHitForceDown;						// apply force downward so player doesn't have in the air
		}
		
		
//		if (properties.BurnOut)
//			BurnOut();
		
		velocity.y -= gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
		
		
		var Pos = thisTransform.position;
		Pos.z = Depth;
		thisTransform.position = Pos;
		thisTransform.rotation = Quaternion.Euler(0, 0, 0);
	}
	
	void BurnOut()
	{
		velocity.x = orientation * runSpeed * 2;
//		animPlay.PlayFramesFixed(5, 0, 4, orientation, 1.005f);
		animPlay.PlayFramesFast(5,4);
	}
}



//public class PlayerControls : MonoBehaviour
//{
//	private Transform thisTransform;				// own player tranform cached
//	private CharacterController controller;
//	[HideInInspector] public PlayerProperties properties;
//	[HideInInspector] public AnimSprite animPlay; 				// : Component
//	
//	[HideInInspector] public int orientation = 1;					// Move Direction: -1 == left, +1 == right
//	[HideInInspector] public Vector3 velocity = Vector3.zero;	    // Start quiet 
//	
//	public float gravity = 20.0f;
//	public float fallSpeed = 0.5f;				// speed of falling down ( division factor )
//	
//	public float walkSpeed = 1.0f;				// standard walk speed
//	public float runSpeed = 2.0f; 				// running speed 
//	bool SuperJumpEnable = false;		        // toggle for run jump
//	
//	public float walkJump = 8.0f;				// jump height from walk
//	public float runJump = 9.0f;				// jump height from run	
//	public float Depth = .25f;				    // Depth in position.z for the player's character
//	
//	public AudioClip JumpSound;
//	
//	private bool jumpEnable = false;			// toggle for default jump
//	private bool runJumpEnable = false;		    // toggle for run jump
//	
//	private float afterHitForceDown = 1.0f;		// toggle for crouch jump
//	private int isHoldingObj = 0;			    // anim row index change when player holds something
//	
//	private int layerMask = 1 << 8;
//	
//	BoxCollider Collider;
//	
//	delegate void InputDelegate();              // This it's like a Pointer Function from C++, but this it's in a C# ( Ugly)
//	InputDelegate UpdateInput;
//	
//	
//	void Start()						        //	BallScript ballScript = target.GetComponent<BallScript>() as BallScript;
//	{
//		thisTransform = transform;
//		controller = GetComponent<CharacterController>();
//		properties = GetComponent<PlayerProperties>();
//		animPlay = GetComponent<AnimSprite>();
//		animPlay.PlayFrames(2, 0, 1, orientation);
//		
//		Physics.IgnoreCollision(controller.collider, transform.GetComponentInChildren<BoxCollider>());
//		
//		Collider = transform.GetComponentInChildren<BoxCollider>();
//		
//		if (Managers.Register.PlayerAutoRunning)
//			UpdateInput = new InputDelegate(ControlAuto);
//		else
//			UpdateInput = new InputDelegate(ControlClassic);
//		
//	}
//	
//	void Update()
//	{
//		UpdateInput();
//	}
//	
//	void ControlAuto()
//	{
//		isHoldingObj = System.Convert.ToByte(properties._pickedObject != null);
//		bool Stand = Physics.Linecast(thisTransform.position, thisTransform.TransformPoint(-Vector3.up), layerMask);
//		Collider.center = Vector3.zero;
//		
//		if (controller.isGrounded)
//		{
//			runJumpEnable = false;
//			
//			velocity = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
//			
//			if (Input.GetAxis("Horizontal") == 0)									// IDLE -> keep stand quiet
//				animPlay.PlayFramesFixed(2 + isHoldingObj, 0, 1, orientation);
//			
//			if (Input.GetAxis("Horizontal") != 0)									// WALK
//			{
//				orientation = (int)(Mathf.Sign(velocity.x)); 		                // If move direction changes -> flip sprite
//				
//				velocity.x *= walkSpeed;										    // If player is moving ->  Animate Walking..
//				animPlay.PlayFramesFixed((0 + isHoldingObj), 0, 8, orientation);
//			}
//			
//			if ((Mathf.Abs(Input.GetAxis("Horizontal")) >= 1) && !Input.GetButton("Fire1") && !Managers.Dialog.IsInConversation())						// RUN 
//			{
//				velocity *= runSpeed;
//				animPlay.PlayFramesFixed(2, 1, 2, orientation, 1.005f);
//			}
//			
//			if (velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 && !Managers.Dialog.IsInConversation())     // Crouch        
//			{
//				animPlay.PlayFrames(3, 3, 1, orientation);
//				Collider.center = Vector3.down * 0.25f;
//			}
//			
//			
//			if (Input.GetButtonDown("Jump"))                                                     // Always running jump
//			{
//				velocity.y = runJump;
//				//Instantiate ( particleJump, particlePlacement, transform.rotation );
//				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
//				runJumpEnable = true;
//			}
//		}
//		
//		if (Input.GetButtonDown("Jump") && Stand)	            // slope jump
//		{
//			velocity.y = walkJump;
//			Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.25f);
//			runJumpEnable = true;
//		}
//		
//		if (!controller.isGrounded && !Stand)	// && !Stand	    // Do Slide
//		{
//			velocity.x = Input.GetAxis("Horizontal");
//			//animPlay.PlayFrames ( 2, 5, 1, orientation );
//			
//			if (Input.GetButtonUp("Jump"))						// check if the player keep pressing jump button..
//				velocity.y *= fallSpeed;							// if not then brake the jump
//			
//			if (velocity.x != 0)
//				orientation = (int)Mathf.Sign(velocity.x); 			// If move direction changes -> update & flip sprite
//			
//			
//			if (runJumpEnable)
//			{
//				velocity.x *= runSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
//			}
//			
//			if (velocity.y < 0 && !Stand) 						// check when player stops elevation & becomes down..
//			{
//				animPlay.PlayFrames(2 + isHoldingObj, 5, 1, orientation);
//				
//				if (Input.GetButton("Jump") && isHoldingObj == 0) // check if the player keep pressing jump button..
//				{
//					velocity.y += 18 * Time.deltaTime;
//					animPlay.PlayFrames(2, 6, 2, orientation);
//				}
//			}
//		}
//		
//		if (controller.collisionFlags == CollisionFlags.Above)
//		{
//			velocity.y = 0;											// set velocity on Y to 0, stop upward motion
//			velocity.y -= afterHitForceDown;						// apply force downward so player doesn't have in the air
//		}
//		
//		if (properties.BurnOut)
//			BurnOut();
//		
//		velocity.y -= gravity * Time.deltaTime;
//		controller.Move(velocity * Time.deltaTime);
//		
//		var Pos = thisTransform.position;
//		Pos.z = Depth;
//		thisTransform.position = Pos;
//		thisTransform.rotation = Quaternion.Euler(0, 0, 0);
//	}
//	
//	void ControlClassic()
//	{
//		isHoldingObj = System.Convert.ToByte(properties._pickedObject != null);
//		bool Stand = Physics.Linecast(thisTransform.position, thisTransform.TransformPoint(-Vector3.up), layerMask);
//		Collider.center = Vector3.zero;
//		
//		if (controller.isGrounded)
//		{
//			jumpEnable = false;
//			runJumpEnable = false;
//			SuperJumpEnable = false;
//			
//			velocity = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
//			
//			if (Input.GetAxis("Horizontal") == 0)									// IDLE -> keep stand quiet
//				animPlay.PlayFramesFixed(2 + isHoldingObj, 0, 1, orientation);
//			
//			if (Input.GetAxis("Horizontal") != 0)									// WALK
//			{
//				orientation = (int)(Mathf.Sign(velocity.x)); 		// (int)Math.Ceiling()	// If move direction changes -> flip sprite
//				
//				velocity.x *= walkSpeed;										// If player is moving ->  Animate Walking..
//				animPlay.PlayFramesFixed((0 + isHoldingObj), 0, 8, orientation);
//			}
//			
//			if ((velocity.x != 0) && Input.GetButton("Fire1"))						// RUN 
//			{
//				velocity *= runSpeed;
//				animPlay.PlayFramesFixed(2, 1, 2, orientation, 1.005f);
//			}
//			
//			if (velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 && !Managers.Dialog.IsInConversation())          // Crouch
//			{
//				animPlay.PlayFrames(3, 3, 1, orientation);
//				Collider.center = Vector3.up * -0.25f;
//			}                           
//			
//			if (Input.GetButtonDown("Jump") && (!Input.GetButton("Fire1") || velocity.x == 0))	// Quiet jump
//			{											// check player dont make a Running Jump being quiet in the same spot
//				velocity.y = walkJump;
//				//Instantiate ( particleJump, particlePlacement, transform.rotation );
//				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.0f);
//				jumpEnable = true;
//			}
//			
//			if (Input.GetButtonDown("Jump") && ((Input.GetButton("Fire1") && velocity.x != 0) || properties.BurnOut))// running jump
//			{
//				velocity.y = runJump;
//				//Instantiate ( particleJump, particlePlacement, transform.rotation );
//				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
//				runJumpEnable = true;
//			}
//			
//			if (Input.GetButtonDown("Jump") && velocity.x == 0 && Input.GetAxisRaw("Vertical") < 0 && !properties.BurnOut)
//			{
//				velocity.y = 10.5f;        // SuperJump!
//				//Instantiate(particleJump, particlePlacement, transform.rotation);
//				Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 0.75f);
//				SuperJumpEnable = true;
//			}
//		}
//		
//		if (Input.GetButtonDown("Jump") && Stand)	// slope jump
//		{
//			velocity.y = walkJump;
//			Managers.Audio.Play(JumpSound, thisTransform, 1.0f, 1.25f);
//			jumpEnable = true;
//		}
//		
//		if (!controller.isGrounded && !Stand)	// && !Stand	// Do Slide
//		{
//			velocity.x = Input.GetAxis("Horizontal");
//			//animPlay.PlayFrames ( 2, 5, 1, orientation );
//			
//			if (Input.GetButtonUp("Jump"))						// check if the player keep pressing jump button..
//				velocity.y *= fallSpeed;							// if not then brake the jump
//			
//			if (velocity.x != 0)
//				orientation = (int)Mathf.Sign(velocity.x); 				// If move direction changes -> update & flip sprite
//			
//			
//			if (jumpEnable)
//			{
//				velocity.x *= walkSpeed;	 						// If player is jumping -> Update & Animate jumping type.
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
//			}
//			
//			if (runJumpEnable)
//			{
//				velocity.x *= runSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
//			}
//			
//			if (SuperJumpEnable)
//			{
//				velocity.x *= walkSpeed;
//				animPlay.PlayFrames(2 + isHoldingObj, 4, 1, orientation);
//			}
//			
//			if (velocity.y < 0 && !Stand) 						// check when player stops elevation & becomes down..
//			{
//				animPlay.PlayFrames(2 + isHoldingObj, 5, 1, orientation);
//				
//				if (Input.GetButton("Jump") && isHoldingObj == 0) // check if the player keep pressing jump button..
//				{
//					velocity.y += 18 * Time.deltaTime;
//					animPlay.PlayFrames(2, 6, 2, orientation);
//				}
//			}
//		}
//		
//		if (controller.collisionFlags == CollisionFlags.Above)
//		{
//			velocity.y = 0;											// set velocity on Y to 0, stop upward motion
//			velocity.y -= afterHitForceDown;						// apply force downward so player doesn't have in the air
//		}
//		
//		
//		if (properties.BurnOut)
//			BurnOut();
//		
//		velocity.y -= gravity * Time.deltaTime;
//		controller.Move(velocity * Time.deltaTime);
//		
//		
//		var Pos = thisTransform.position;
//		Pos.z = Depth;
//		thisTransform.position = Pos;
//		thisTransform.rotation = Quaternion.Euler(0, 0, 0);
//	}
//	
//	void BurnOut()
//	{
//		velocity.x = orientation * runSpeed * 2;
//		animPlay.PlayFramesFixed(5, 0, 4, orientation, 1.005f);
//	}
//	
//	
//}