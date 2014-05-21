using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using System.Text;
using Ionic.Zlib;

public class GameManager : MonoBehaviour
{
    public bool ShowState   = true;
    public bool IsPlaying   = false;
    public bool IsPaused    = false;

    //////////////////////////////////////////////////////////////

    public List<GameState> states = new List<GameState>();
    private bool Running;



    void Start()
    {
//		Managers.Display.MainCamera = Camera.main;
//		Managers.Display.CamTransform = Camera.main.transform;

        PushState(typeof(IntroState)); // Loading some State
    }

    void OnApplicationQuit() 	// "DeInit()"
    {
		UnloadMap();
		
        // cleanup the all states
        while (states.Count > 0)
        {
            states[states.Count - 1].DeInit();
            states.RemoveAt(states.Count - 1);
        }

        //PlayerPrefs.SetInt("UnlockedStages", UnlockedStages);
    }

    void Update()
    {
        Managers.Register.Health = Mathf.Clamp(Managers.Register.Health, 0, 3);

        if (((Managers.Register.Fruits % 85) == 0) && System.Convert.ToBoolean(Managers.Register.Fruits))
        { 
            ShowState = true;
            Managers.Register.Lifes++;
            Managers.Register.Fruits++;
        }

        if (states.Count > 0)
            states[states.Count - 1].OnUpdate();
    }

    public void Render()
    {
        if (states.Count > 0)
            states[states.Count - 1].OnRender();
    }

    public void ChangeState(System.Type newStateType)						// Swap two states
    {
        
		if (states.Count > 0)												// if not Empty CleanUp current State				
        {
            states[states.Count - 1].DeInit();
            states.RemoveAt(states.Count - 1);
        }

		states.Add(GetComponentInChildren(newStateType) as GameState);		// store and init the new state		
        states[states.Count - 1].Init();
    }

    public void PushState(System.Type newStateType)							// Hold back previous states
    {
        
        if (states.Count > 0)
			states[states.Count - 1].Pause();								// pause current state				

        states.Add(GetComponentInChildren(newStateType) as GameState); 		// store and init the new state
        states[states.Count - 1].Init();
    }

    public void PopState()
    {

		if (states.Count > 0)			        							// cleanup the current state
        {
            states[states.Count - 1].DeInit();
            states.RemoveAt(states.Count - 1);
        }

		if (states.Count > 0)												// resume previous state	
            states[states.Count - 1].Resume();
    }

    public GameState State
    {
        get { return states[states.Count - 1]; }

        //if (Managers.Game.State ==  Managers.Game.GetComponentInChildren< ExampleState >()  ) // Metodo para chequear 
        //    Debug.Log("Si, Soy el estado que estás buscando!");
        //    Debug.Log(" y aqui accedes a un dato del Estado:" + (( ExampleState )Managers.Game.State).PublicData);
    }
    
    //////////////////////////////////////////////////////////////


    
	public bool LoadMap(string filePath)
	{
		//Debug.Log(Application.dataPath + filePath);
		if (Managers.Tiled.MapTransform != null) 
		{
			Debug.LogWarning ("To create a new Map Unload previous one first");
			return false;
		} 
		
		if (!File.Exists (Application.dataPath + filePath)) 
		{
			Debug.LogWarning ("Couldn't Load the TileMap, File Don't Exists!");
			return false;
		} 
		
		string fileName = filePath.Remove (0, filePath.LastIndexOf ("/") + 1);			    // quit folder path structure
		fileName = fileName.Remove (fileName.LastIndexOf ("."));							    // quit .tmx or .xml extension
		
		StreamReader sr = File.OpenText (Application.dataPath + filePath);				    // Do Stream Read
		XmlDocument Doc = new XmlDocument ();
		
		Doc.LoadXml (sr.ReadToEnd ());                                                       // and Read XML
		sr.Close ();
		
		// CHECK IT'S A TMX FILE FROM TILED	
		if (Doc.DocumentElement.Name == "map") 												// Access root Map	
		{												
			Managers.Register.currentLevelFile = filePath;

			if (Doc.DocumentElement.FirstChild.Name == "properties")
				foreach (XmlNode MapProperty in Doc.DocumentElement.FirstChild) 
				{
					if(MapProperty.Attributes ["name"].Value.ToLower () == "music")
						Managers.Audio.PlayMusic ((AudioClip)Resources.Load ("Sound/" + MapProperty.Attributes ["value"].Value, typeof(AudioClip)), .45f, 1);
					
					if(MapProperty.Attributes ["name"].Value.ToLower () == "zoom")
						Managers.Objects.Player.GetComponent<CameraTargetAttributes> ().distanceModifier = 3.5f;
				}

			////////////////////////////////////////////////////////////////////////////////
		
			Managers.Tiled.Load(Doc.DocumentElement);
			Managers.Tiled.MapTransform.gameObject.name = fileName;

			Managers.Objects.Load(Doc.GetElementsByTagName("objectgroup"));

			Managers.Scroll.Load(Doc.GetElementsByTagName("imagelayer"));
			
			Debug.Log ("Tiled Level Build Finished: " + fileName);
			return true;
		} 
		
		Debug.LogError (fileName + " it's not a Tiled File!, wrong load at: " + filePath);
		return false;
	}


	public void UnloadMap()
	{
		StopAllCoroutines();

		Managers.Audio.StopMusic();
		Managers.Display.CameraScroll.ResetBounds();

		Managers.Objects.Unload ();
		Managers.Scroll.Unload ();
		Managers.Tiled.Unload ();
		
		Managers.Register.currentLevelFile = string.Empty;
	}






    //////////////////////////////////////////////////////////////

	internal static bool ToggleUp = true;
    public bool InputUp                            						// This it's a little oneShot Up Axis check for doors & like   
    {
        get
        {
            if (Input.GetAxisRaw("Vertical") != 1)                      // It's like an "Input.GetAxisDown" 
                ToggleUp = true;

            if (ToggleUp && Input.GetAxisRaw("Vertical") >= 1)
            {
                ToggleUp = false;
                return true;
            }
            return false;
        }
    }

	internal static bool ToggleDown = true;
    public bool InputDown                             					// This it's a little oneShot Down Axis check for doors & like   
    {
        get
        {
            if (Input.GetAxisRaw("Vertical") != -1)                     // It's like an "Input.GetAxisDown" 
                ToggleDown = true;

            if (ToggleDown && Input.GetAxisRaw("Vertical") <= -1)
            {
                ToggleDown = false;
                return true;
            }
            return false;
        }
    }

	internal static bool ToggleLeft = true;
    public bool InputLeft                             					// This it's a little oneShot Left Axis check for doors & like   
    {
        get
        {
            if (Input.GetAxisRaw("Horizontal") != -1)                   // It's like an "Input.GetAxisDown" 
                ToggleLeft = true;

            if (ToggleLeft && Input.GetAxisRaw("Horizontal") <= -1)
            {
                ToggleLeft = false;
                return true;
            }
            return false;
        }
    }

	internal static bool ToggleRight = true;
    public bool InputRight                             					// This it's a little oneShot RIght Axis check for doors & like   
    {
        get
        {
            if (Input.GetAxisRaw("Horizontal") != 1)                    // It's like an "Input.GetAxisDown" 
                ToggleRight = true;

            if (ToggleRight && Input.GetAxisRaw("Horizontal") >= 1)
            {
                ToggleRight = false;
                return true;
            }
            return false;
        }
    }

}

