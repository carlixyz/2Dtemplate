using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartState : GameState
{
	public InGameState[] LevelStates;
	public GameObject[] SpritesButtons;
	public GameObject BlockedButton;
	public List<cStageNode> StageList = new List<cStageNode>();

	public int TotalStages;
	public int TotalRows;
	public int TotalColumns;

	public float Margin = 50;
	public float Offset = 0.1f;
//	public Vector2D Offset = Vector2.one;

	public Rect Area;
	
    public override void Init()
    {
		TotalStages = LevelStates.Length;

		Vector2 upperLeft   = Camera.main.ScreenToWorldPoint( new Vector2 (Margin, 				Margin		  		) );
		Vector2 lowerRight  = Camera.main.ScreenToWorldPoint( new Vector2 (Screen.width-Margin, Screen.height-Margin) );

		Area = new Rect (upperLeft.x, upperLeft.y, lowerRight.x, lowerRight.y);
		Offset = ((Area.width - Area.x) / TotalColumns) / 2;	// Offset entre piezas es igual a largo soga / total col (div. 2 para tener 1/2 margen)  
//		Offset.y = ((Area.width - Area.x) / TotalColumns) / 2;	  
//		Offset.x = ((Area.height - Area.y) / TotalRows) / 2;	 

		for ( int y = 0, index = 0; y < TotalRows; y++) 
		{		
			for (int x = 0; x < TotalColumns; x++, index++) 										// 
			{
				if ( index < TotalStages )
					StageList.Add ( new cStageNode ( LevelStates [index], ((GameObject) Instantiate( SpritesButtons[index],
				                                                                                new Vector2( Area.xMin + Offset + ( x * (2*Offset)),
								            																 Area.yMin + (y * Offset)), 
								                                                                                 Quaternion.identity))) );
				else
					StageList.Add ( new cStageNode ( null, ((GameObject) Instantiate( BlockedButton,
					                                                                            new Vector2( Area.xMin +  Offset + ( x * (2*Offset)),
					            																			 Area.yMin + (y * Offset)), 
				                                                                                 				Quaternion.identity))) );
			}

//			for (int x = 0; x < TotalColumns; x++, index++) 										// 
//			{
//				if ( index < TotalStages )
//					StageList.Add ( new cStageNode ( LevelStates [index], ((GameObject) Instantiate( SpritesButtons[index],
			//					                                                                                new Vector2( Area.xMin +  Offset.x + ( x * (2*Offset.x)),
//					            																					Area.yMin + Offset.y + (y * (2*Offset.y))), 
//					                                                                                				Quaternion.identity))) );
//				else
//					StageList.Add ( new cStageNode ( null, ((GameObject) Instantiate( BlockedButton, new Vector2( Area.xMin +  Offset.x + ( x * (2*Offset.x)),
				//					            																	Area.yMin + Offset.y + (y * (2*Offset.y))), 
				//					                                                                 				Quaternion.identity))) );
//			}
		}
    }

    public override void DeInit()
    {
		foreach (cStageNode Stage in StageList)
				Destroy (Stage.Image);

		StageList.Clear();
    }

    public override void OnUpdate()
    {
		if (Input.GetKeyDown("escape") || Input.GetButtonDown("Select"))
		{
			Managers.Game.PopState();
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if(hit != null && hit.collider != null)
			{
//				Debug.Log ("Mouse Hit : " + hit.collider.gameObject );
				foreach( cStageNode StageNode in StageList)
					if (StageNode.Image == hit.collider.gameObject && StageNode.Stage)
					{
						Managers.Game.ChangeState(StageNode.Stage.GetType());
						break;
					}
			}
		}

	}

    public override void Pause()
    {

    }

    public override void Resume()
    {

    }

	public bool ShowMarginLimits = true;
	void  OnDrawGizmos ()
	{
		if (ShowMarginLimits)
		{
			//		float Margin = 50;
			//		Rect Area = new Rect (Margin, Margin, Screen.width-Margin<<2, Screen.height-Margin<<2);
			//		Vector2 lowerLeft   = Camera.main.ScreenToWorldPoint(new Vector2 (Area.xMin, Area.yMax) );
			//		Vector2 upperLeft   = Camera.main.ScreenToWorldPoint(new Vector2 (Area.xMin, Area.yMin) );
			//		Vector2 lowerRight  = Camera.main.ScreenToWorldPoint(new Vector2 (Area.xMax, Area.yMax) );
			//		Vector2 upperRight  = Camera.main.ScreenToWorldPoint(new Vector2 (Area.xMax, Area.yMin) );

			//			Vector2 lowerLeft   = new Vector2 (Area.x, Area.height	  );
			//			Vector2 upperLeft   = new Vector2 (Area.x, Area.y		  );
			//			Vector2 lowerRight  = new Vector2 (Area.width, Area.height);
			//			Vector2 upperRight  = new Vector2 (Area.width, Area.y	  );
			
			Rect Area = new Rect (Margin, Margin, Screen.width-Margin, Screen.height-Margin);
			Vector2 lowerLeft   = Camera.main.ScreenToWorldPoint( new Vector2 (Area.x, Area.height	  ) );
			Vector2 upperLeft   = Camera.main.ScreenToWorldPoint( new Vector2 (Area.x, Area.y		  ) );
			Vector2 lowerRight  = Camera.main.ScreenToWorldPoint( new Vector2 (Area.width, Area.height) );
			Vector2 upperRight  = Camera.main.ScreenToWorldPoint( new Vector2 (Area.width, Area.y	  ) );
			
			Gizmos.color        = Color.red;
			Gizmos.DrawLine (lowerLeft, upperLeft);
			Gizmos.DrawLine (upperLeft, upperRight);
			Gizmos.DrawLine (upperRight, lowerRight);
			Gizmos.DrawLine (lowerRight, lowerLeft);
		}
	}
}

public class cStageNode
{

	public uint totalStars = 0;
	public bool levelBlocked = false;
	public GameState Stage; 
	public GameObject Image;

	public cStageNode(GameState State, GameObject img)
	{
		Stage = State	;
		Image = img	;
	}

}
