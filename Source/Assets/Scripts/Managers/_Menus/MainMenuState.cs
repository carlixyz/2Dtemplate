using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MainMenuState : GameState 
{
//	public Texture2D BackText
	public GameObject Background;
	GameObject IntroPrefab;

	public GameObject Title;
	GameObject TitlePrefab;
	public Rect TitleRect = new Rect(0,0,1,1);
	
	public GameState[]  OptionsSet;
	public Rect OptionsRect = new Rect(0,0,0,0);

//    public bool isLoading = false;
    public bool MusicIntro = true;
    public float timeTrailer = 30;

	bool mouseOver = false;

	public AudioClip PreOpening;
	public AudioClip Opening;

    int ChooseOption = 0;
    int TotalOptions = 0;
	Dictionary<int, string>  OptionsList = new Dictionary<int, string>();
//	public List<GameState>  OptionsSet = new List<GameState>();

    string FullText = "";

    GUISkin gSkin;

    public override void Init()
    {
//        IntroPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Intro/IntroPrefab", typeof(GameObject)) );
		IntroPrefab = (GameObject)Instantiate(Background, transform.position, transform.rotation);			// Background Instantiation
		IntroPrefab.transform.localScale = Vector3.one;

		Vector2 BackSize = IntroPrefab.GetComponent<SpriteRenderer> ().sprite.bounds.size;
		Vector2 ScreenSize = new Vector2( Camera.main.orthographicSize * 2, Camera.main.orthographicSize * 2 / Screen.height * Screen.width);

		IntroPrefab.transform.localScale = new Vector2(ScreenSize.y / BackSize.x, ScreenSize.x / BackSize.y);


        timeTrailer = 30;

		if (OptionsRect.Equals( new Rect(0,0,0,0)) )
			OptionsRect = new Rect((Screen.width * .45f), (Screen.height * .75f), 600, 200);

		TitlePrefab = (GameObject)Instantiate(Title, transform.position, transform.rotation);			// Background Instantiation
		TitlePrefab.transform.position = new Vector2 (TitleRect.x, TitleRect.y);
		TitlePrefab.transform.localScale = new Vector2 (TitleRect.width, TitleRect.height);

        Managers.Audio.Play(PreOpening, Managers.Display.MainCamera.transform, 1, 1);

        gSkin = (GUISkin)Resources.Load("GUI/GUISkin B", typeof(GUISkin));

//		TotalOptions = OptionsSet.Count;
		TotalOptions = OptionsSet.Length;
		
		for (int index = 0; index < TotalOptions; index++)
		{
//			OptionsList.Add(index, OptionsSet[index].GetType());	// This is to Delete the 'State' type from the full name: "SomethingState"
			OptionsList.Add(index, OptionsSet[index].GetType().ToString().TrimEnd("State".ToCharArray()));
		}

        foreach (int Option in OptionsList.Keys) 
			FullText += (OptionsList [Option] + System.Environment.NewLine); // This it's a fix to reduce the options drawcalls
    }
	
    public override void DeInit()
    {
        Managers.Display.MainCamera.enabled = true;
        Managers.Display.MainCamera.tag = "MainCamera";

        DestroyImmediate(IntroPrefab);
        IntroPrefab = null;

		DestroyImmediate(TitlePrefab);
		TitlePrefab = null;

//		OptionsSet.Clear();
		System.Array.Clear(OptionsSet, 0, OptionsSet.Length);
        OptionsList.Clear();

        FullText = "";
        Managers.Audio.StopMusic();

        //PlayerPrefs.SetInt("TopScore", (int)Managers.Game.TopScore);
    }

    public override void OnUpdate()
    {

        if (Managers.Game.InputUp && (ChooseOption > 0) )
            ChooseOption--;
        if (Managers.Game.InputDown && (ChooseOption < (TotalOptions - 1)) )
            ChooseOption++;

		if (Input.GetButtonDown ("Fire1") || Input.GetKeyDown ("return") || Input.GetButtonDown ("Start") || (Input.GetMouseButtonUp(0) && mouseOver)) 
		{
            Managers.Display.ShowFlash(1);
			Managers.Game.PushState(  OptionsSet[ChooseOption].GetType() );
		}
	}

    public override void OnRender()
    {
        if (Opening && !Managers.Audio.Music.isPlaying && Managers.Audio.SoundEnable && MusicIntro)
            Managers.Audio.PlayMusic(Opening, 1, 1);

        if (gSkin)
        {
            GUI.skin = gSkin;
            GUI.skin.label.fontSize = 32;
            GUI.color = new Color(1, 0.36f, 0.22f, 1);
        }
        else 
			Debug.Log("MainMenuGUI : GUI skin object missing!");

        GUI.color = new Color(1, 1, 1, 1);

//		if (Background)
		//			GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height), BackText);

        GUI.color = new Color(1, 0.36f, 0.22f, 1);
//        GUI.Label(new Rect((Screen.width * .01f), (Screen.height * .01f), 400, 200), "Top Score\n" + Managers.Register.TopScore);

		GUI.DrawTexture( OptionsRect, Resources.Load("GUI/PixelBlack") as Texture2D);
		GUI.Label(OptionsRect, FullText);
        string jump = "";

		Vector3 mousePosition =  Input.mousePosition;
		/* adjust the y-coordinate for the GUI's coordinate system */
		mousePosition.y = Screen.height - mousePosition.y;
		mouseOver = true;

        foreach (int Option in OptionsList.Keys)
        {
			if ((new Rect(OptionsRect.x, OptionsRect.y + GUI.skin.label.fontSize * Option, OptionsRect.width, GUI.skin.label.fontSize ).Contains(mousePosition) ))	
			{
				ChooseOption =  (int)Option;
				mouseOver = true;
			}

			if (Option == ChooseOption)
			{
				GUI.color = Color.white;
				GUI.Label( OptionsRect, jump + OptionsList[Option]);
			}
			
			jump += System.Environment.NewLine;
        }

        GUI.color = Color.white;
    }

 
    public override void Pause()
    {
		DeInit ();
    }

    public override void Resume()
    {
		Init ();
        //DisplayMenu = true;

//        Managers.Audio.StopMusic();

    }
  
}