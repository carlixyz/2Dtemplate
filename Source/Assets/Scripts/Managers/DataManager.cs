using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


public class DataManager : MonoBehaviour 
{

/// <summary>
/// COMMON GAMEPLAY DATA FIELDS
/// Here We have the most important ingame properties
/// </summary>

    public int Score        = 0;
	public int Fruits       = 0;
	public int TotalFruits 	= 0;
    public int TopScore     = 100;

    public int  FireGauge   = 0;
    public int  Key         = 0;
    public int Health       = 3;
    public int Lifes        = 3;

    public enum Items { Empty = 0, Hat = 1, Whistler = 2, Invisibility = 4, Smallibility = 8, Fire = 16 };
    public Items Inventory = Items.Empty;					// Inventory system activation

    private GameState currentStage;
    public int UnlockedStages 	= 1;
    public int TotalStages 		= 20;

    public string currentLevelFile = string.Empty;
	public string previousLevelFile = string.Empty;
    public Dictionary<string, Vector3> MapCheckPoints = new Dictionary<string, Vector3>();


    //public List<int> TopScore = new List<int>();
    //public Dictionary<int, string> TopScorePlayers = new Dictionary<int, string>();


    public bool FirstTimePlay     = true;
    public bool PlayerAutoRunning = true;

    // each Stage Last File Loaded

    #region STAGES files Path + name references:

    public string Stage1 = "/Levels/Stage1map.tmx";    
	public bool Secret1 = false;

    #endregion
    ////////////////////////////////////////////////////    


    // Use this for initialization
	void Start ()
    {
//		Managers.Register.Load();
		if (this.MapCheckPoints.Count > 0)
			return;

		MapCheckPoints.Add(Stage1 , Vector3.zero);
//		MapCheckPoints.Add(Stage2 , Vector3.zero);
//		MapCheckPoints.Add(Stage3 , Vector3.zero);
//		MapCheckPoints.Add(Stage4 , Vector3.zero);
//		MapCheckPoints.Add(Stage5 , Vector3.zero);


	}

	private void OnApplicationQuit()
	{
//		Managers.Register.Save();
	}

    // Player setup inside level position
    public void SetPlayerPos()
    {
        if (!MapCheckPoints.ContainsKey(currentLevelFile))                      // If our current file isn't registered yet
            MapCheckPoints.Add(currentLevelFile, Vector3.zero);

                                                                                // If there's a previous saved position use it
        if (MapCheckPoints[currentLevelFile] != Vector3.zero)
			Managers.Objects.PlayerTransform.position = MapCheckPoints[currentLevelFile];
        else
			MapCheckPoints[currentLevelFile] = Managers.Objects.PlayerTransform.position;
  
    }           

    public void SoftReset()
    {
        Debug.Log("Doing Soft Reset");
        //ShowState = false;

        FireGauge = 0;
        Score = 0;
        Fruits = 0;
		TotalFruits = 0;
        Key = 0;
        Health = 3;
        Lifes = 3;

        //FireGauge = 0;
        //Inventory = Items.Empty;
    }

    public void HardReset()
    {
        Score = 0;
        Fruits = 0;
		TotalFruits = 0;
        //TopScore = 100;

        FireGauge = 0;
        Key = 0;
        Health = 3;
        Lifes = 3;
        Inventory = Items.Empty;

        //currentStage;
        UnlockedStages = 1;
		FirstTimePlay = true;

		Stage1 	= "/Levels/Stage1map.tmx";
		Secret1 = false;	
		
        MapCheckPoints.Clear();
    }


    public void Save()
    {
		PlayerPrefs.SetInt("UnlockedStages", Managers.Register.UnlockedStages);
		PlayerPrefs.SetInt("Score", Managers.Register.Score);
		PlayerPrefs.SetInt("TopScore", Managers.Register.TopScore);
		PlayerPrefs.SetInt("Fruits", Managers.Register.Fruits);
		PlayerPrefs.SetInt("TotalFruits", Managers.Register.TotalFruits);
		PlayerPrefs.SetInt("Lifes", Managers.Register.Lifes);
		PlayerPrefs.SetInt("Health", Managers.Register.Health);
		PlayerPrefs.SetInt("Inventory", (int) Managers.Register.Inventory);

		PlayerPrefs.SetString("StageFile1", Managers.Register.Stage1);
		PlayerPrefs.SetInt("Secret1", !Managers.Register.Secret1 ? 0 : 1);
		PlayerPrefs.SetInt("FirstTimePlay", !Managers.Register.FirstTimePlay ? 0 : 1);
		PlayerPrefs.SetInt("PlayerAutoRunning", !Managers.Register.PlayerAutoRunning ? 0 : 1);

		string str1 = string.Empty;
		using (Dictionary<string, Vector3>.KeyCollection.Enumerator enumerator = MapCheckPoints.Keys.GetEnumerator()) 
		{
			while (enumerator.MoveNext()) 
			{
				string current = enumerator.Current;
				
				object[] objArray = new object[5]; 					// path's string  it's sepparated in two parts: 
				
				string str2 = str1;
				objArray[0] = (object) str2;						// part 1: all Previous strings of paths + Vectors
				
				string str3 = current;
				objArray[1] = (object) str3;						// part 2: with the addition of last item  
				
				string str4 = "*";
				objArray[2] = (object) str4;						// Then We add a '*' FLAG sepparator between 1º & 2º Therm
				
				Vector3 local =  MapCheckPoints[current];			// Then We hack the Vectors Value directly inside the Register Array as it 
				objArray[3] = (object) local.ToString("G4");
				
				string str5 = "*";
				objArray[4] = (object) str5;						// And Finally another '*' FLAG sepparator To end this sentence
				str1 = string.Concat(objArray);						
				// the We split everything between all '*' and to Read this we say: even indexes are paths and uneven ones are Vector3s, piece of cake! 
			}
		}
		PlayerPrefs.SetString("Map", str1);							// Format Example is: 'AllPrevPathsNVec3' + '/Levels/Stage1fileMap.tmx * (2,1.5,3) *'
		

    }

    public void Load()
    {
		Managers.Register.UnlockedStages 	= PlayerPrefs.GetInt("UnlockedStages");
		Managers.Register.Score 			= PlayerPrefs.GetInt("Score");
		Managers.Register.TopScore 			= PlayerPrefs.GetInt("TopScore");
		Managers.Register.Fruits 			= PlayerPrefs.GetInt("Fruits");
		Managers.Register.TotalFruits 		= PlayerPrefs.GetInt("TotalFruits");
		Managers.Register.Lifes 			= PlayerPrefs.GetInt("Lifes");
		Managers.Register.Health 			= PlayerPrefs.GetInt("Health");
		Managers.Register.Inventory 		= (DataManager.Items) PlayerPrefs.GetInt("Inventory");

		Managers.Register.Stage1 = PlayerPrefs.GetString("StageFile1");

		Managers.Register.Secret1 = PlayerPrefs.GetInt("Secret1") == 1;

		Managers.Register.FirstTimePlay = PlayerPrefs.GetInt("FirstTimePlay") == 1;

		Managers.Audio._MusicEnable = PlayerPrefs.GetInt("Music") == 1;
		Managers.Audio.SoundEnable = PlayerPrefs.GetInt("Sound") == 1;

		string MapString = PlayerPrefs.GetString("Map");
    	char[] chArray = new char[1];
    	int index1 = 0;
    	int num = 42;	
    	chArray[index1] = (char) num;											// Flag '*' to separate things
		string[] strArray = MapString.Split(chArray);
		
		for (int index2 = 0; index2 < strArray.Length - 1; ++index2)
		{
			if (index2 % 2 == 0)												// Check par (even) indexs only
			{
				Vector3 vector3string = getVector3(strArray[index2 + 1]);

				if (!this.MapCheckPoints.ContainsKey(this.currentLevelFile))
					MapCheckPoints.Add(strArray[index2], vector3string);
				else
					MapCheckPoints[strArray[index2]] = vector3string;
			}
		}
		

    }

	public Vector3 getVector3(string rString)				// Read a Vector3 from the string
	{
		string str = rString.Substring(1, rString.Length - 2);
		char[] chArray = new char[1];
		int index = 0;
		int num = 44;	
		chArray[index] = (char) num;						// Flag ',' to detect commas
		string[] strArray = str.Split(chArray);
		return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
	}


//	private void SaveVec3Values()
//	{
//		string str1 = string.Empty;
//		using (Dictionary<string, Vector3>.KeyCollection.Enumerator enumerator = MapCheckPoints.Keys.GetEnumerator()) 
//		{
//			while (enumerator.MoveNext()) 
//			{
//				string current = enumerator.Current;
//				
//				object[] objArray = new object[5]; 					// path's string  it's sepparated in two parts: 
//				
//				string str2 = str1;
//				objArray[0] = (object) str2;						// part 1: all Previous strings of paths + Vectors
//				
//				string str3 = current;
//				objArray[1] = (object) str3;						// part 2: with the addition of last item  
//				
//				string str4 = "*";
//				objArray[2] = (object) str4;						// Then We add a '*' FLAG sepparator between 1º & 2º Therm
//				
//				Vector3 local =  MapCheckPoints[current];			// Then We hack the Vectors Value directly inside the Register Array as it 
//				objArray[3] = (object) local.ToString("G4");
//				
//				string str5 = "*";
//				objArray[4] = (object) str5;						// And Finally another '*' FLAG sepparator To end this sentence
//				str1 = string.Concat(objArray);						
//				// the We split everything between all '*' and to Read this we say: even indexes are paths and uneven ones are Vector3s, piece of cake! 
//			}
//		}
//		PlayerPrefs.SetString("Map", str1);							// Format Example is: 'AllPrevPathsNVec3' + '/Levels/Stage1fileMap.tmx * (2,1.5,3) *'
//		
//	}
//	
//	
//	private void ReadVec3Values()
//	{
//		string MapString = PlayerPrefs.GetString("Map");
//		char[] chArray = new char[1];
//		int index1 = 0;
//		int num = 42;	
//		chArray[index1] = (char) num;							// Flag '*' to separate things
//		string[] strArray = MapString.Split(chArray);
//		
//		for (int index2 = 0; index2 < strArray.Length - 1; ++index2)
//		{
//			if (index2 % 2 == 0)								// Check par (even) indexs only
//			{
//				//				Vector3 zero = Vector3.zero;
//				Vector3 vector3string = getVector3(strArray[index2 + 1]);
//				
//				//				if (!MapCheckPoints.ContainsKey(currentLevelFile))
//				if (!MapCheckPoints.ContainsKey(strArray[index2]))
//					MapCheckPoints.Add(strArray[index2], vector3string);
//				else
//					MapCheckPoints[strArray[index2]] = vector3string;
//			}
//		}
//	}

}






//Store Register in PlayerPrefs.
//Usage:
//var myObject = new MyClass();
//Prefs.Save<MyClass>("my object", myObject);
//var anotherObject = Prefs.Load<MyClass>("my object")


//or something like that...

//using UnityEngine;
//using System.Collections;
//using System.Xml.Serialization;
//using System.IO;


//public class Prefs
//{

//    public static void Save<T> (string name, T instance)
//    {
//        XmlSerializer serializer = new XmlSerializer (typeof(T));
//        using (var ms = new MemoryStream ()) {
//            serializer.Serialize (ms, instance);
//            PlayerPrefs.SetString (name, System.Text.ASCIIEncoding.ASCII.GetString (ms.ToArray ()));
//        }
//    }

//    public static T Load<T> (string name)
//    {
//        if(!PlayerPrefs.HasKey(name)) return default(T);
//        XmlSerializer serializer = new XmlSerializer (typeof(T));
//        T instance;
//        using (var ms = new MemoryStream (System.Text.ASCIIEncoding.ASCII.GetBytes (PlayerPrefs.GetString (name)))) {
//            instance = (T)serializer.Deserialize (ms);
//        }
//        return instance;
//    }
    
//}