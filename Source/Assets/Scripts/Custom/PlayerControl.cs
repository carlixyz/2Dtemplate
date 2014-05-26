using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public float MaxSpeed = 5;
	public float jumpForce = 7;
	[HideInInspector] 
	public int Orientation = 1;
	public Transform groundCheck ;
	public LayerMask whatIsGround;
	float groundRadius = 0.2f;
	bool grounded = false;
	bool jumping = false;

	Animator anim;


	// Use this for initialization
	void Start () 
	{
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundRadius, whatIsGround);
		anim.SetBool ("Ground", grounded);
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
		
		float move = Input.GetAxis ("Horizontal");
		anim.SetFloat ("Speed", Mathf.Abs(move));

		if (move != 0) 
		{
			Orientation = (int)Mathf.Sign (move);
			transform.localScale = new Vector3( Orientation, 1, 1);
			rigidbody2D.velocity = new Vector2 (move * MaxSpeed , rigidbody2D.velocity.y);
			anim.SetBool ("Jumping", !grounded);
		}

		if (grounded && Input.GetButtonDown("Jump")) 
		{
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
			anim.SetBool ("Jumping", true);
			anim.SetBool ("Ground", false);
		}

		if ( !Input.GetButtonDown("Jump") && !grounded)
		{
			anim.SetBool ("Planing", true);
		}
		else
			anim.SetBool ("Planing", false);
			

	}
}
