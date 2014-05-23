using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;


/* OBJECTMANAGER - RULES OF CONSIDERATIONS:
 * 
 * - Load Files From Assets Datafolder as: "/SomeFolder/SomeFile.SomeExtension" ( Don't use 'Ñ' letter)
 *   (Always before every Build Add again the "Levels" Folder with maps inside the 'ProjectName/ProjectName_Data/'Folder)
 * 
 * - Use Tiled Editor and U can rename file extensions from '.TMX' to '.XML' (And save in Gzip + Base64 Compression)
 * 
 * - WIP
 *  
 * */
public class ObjManager : MonoBehaviour 
{

	public List<GameObject> ObjList = new List<GameObject>();
	public int ObjTotal = 0;

	public GameObject Player;
	public Transform PlayerTransform;

//	public bool Load(XmlNodeList Layer)
//	{
//		if ( ObjList == null)
//			ObjList = new List<GameObject>();
//
//		foreach (XmlNode ObjGrp in Layer)
//			StartCoroutine (BuildPrefabs (ObjGrp));
//			
////		ObjList = objectsList.ToArray();
//		return true;
//	}
	
//	public bool LoadObj(XmlNode Layer)
//	{
//		if ( ObjList == null)
//			ObjList = new List<GameObject>();
//		
//		StartCoroutine (BuildPrefabs (Layer));
//		
//		return true;
//	}
	
	public void Unload()
	{
		if ( ObjTotal <= 0)
			return;
		
		ObjTotal = 0;
		ObjList.Clear ();
//		ObjList = null;

		if ( PlayerTransform != null )
		{
			if (Player)
				Destroy(Player);
			PlayerTransform = null;
			Player = null;
		}
	}

	public IEnumerator BuildPrefabs(XmlNode ObjectsGroup)
	{
		int height = int.Parse(ObjectsGroup.ParentNode.Attributes["height"].Value);
		int tilewidth = int.Parse(ObjectsGroup.ParentNode.Attributes["tilewidth"].Value);
		int tileheight = int.Parse(ObjectsGroup.ParentNode.Attributes["tileheight"].Value);
		GameObject ObjGroup = new GameObject(ObjectsGroup.Attributes["name"].Value);
		Transform GrpTransform = ObjGroup.transform;
		GrpTransform.parent = Managers.Tiled.MapTransform;
		
		//if (ObjectsGroup.Attributes["type"] != null)
		//    Debug.Log("Obj Null Type: "+ ObjectsGroup.Attributes["type"].Value);           // Get complete Obj Layer Props.
		
		foreach (XmlNode ObjInfo in ObjectsGroup.ChildNodes)
		{
			string ObjName;
			
			if ( ObjInfo.Attributes["type"] != null)
				ObjName = ObjInfo.Attributes["type"].Value.ToLower();                      // Check type match
			else if ( ObjInfo.Attributes["name"] != null )
				ObjName = ObjInfo.Attributes["name"].Value.ToLower();                      // else take it's name as type
			else 
				continue;                                                                   // else discard object
			
			if ( Resources.Load( "Prefabs/" + ObjName, typeof(GameObject) ) )                
			{
				Transform ObjTransform = ((GameObject)Instantiate( Resources.Load( "Prefabs/" + ObjName , typeof(GameObject))) ).transform;

				ObjList.Add( ObjTransform.gameObject );

				ObjTransform.position = new Vector3(
					(float.Parse(ObjInfo.Attributes["x"].Value) / tilewidth) + (ObjTransform.localScale.x * .5f),        // X
					height - (float.Parse(ObjInfo.Attributes["y"].Value) / tileheight - ObjTransform.localScale.y * .5f),// Y		 		     
					Managers.Tiled.MapTransform.position.z);		 													 // Z
				
				if (ObjInfo.Attributes["gid"] == null)                          //  If not a gid it's a Trigger Volume (Great)
				{
					ObjTransform.localScale = new Vector3(  float.Parse(ObjInfo.Attributes["width"].Value)/tilewidth,
					                                      float.Parse(ObjInfo.Attributes["height"].Value)/tileheight, 1 )  ;
					
					ObjTransform.position += new Vector3(  (ObjTransform.localScale.x * .5f) - .5f,
					                                     -(ObjTransform.localScale.y * .5f + .5f), 
					                                     Managers.Tiled.MapTransform.position.z );                // Model your own space!
				}
				
				
				ObjTransform.name = ObjName.Remove(0, ObjName.LastIndexOf("/") + 1);
				ObjTransform.parent = GrpTransform;
				
				#region OBJS PROPS
				
				switch (ObjName.ToLower())
				{
				case "pombero":
				{
					Managers.Objects.Player = ObjTransform.gameObject;
					Managers.Objects.PlayerTransform = ObjTransform;
					Managers.Display.CameraScroll.SetTarget( Managers.Objects.PlayerTransform, false );
					PlayerTransform = Managers.Objects.PlayerTransform;
					//Debug.Log("setting up position in TileManager");
					Managers.Register.SetPlayerPos();
					
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "zoom" )
							((CameraTargetAttributes)ObjTransform.GetComponent<CameraTargetAttributes>()).distanceModifier = 
								float.Parse(ObjProp.Attributes["value"].Value.ToLower());
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "offset" )
							((CameraTargetAttributes)ObjTransform.GetComponent<CameraTargetAttributes>()).Offset = 
								ReadVector( ObjProp.Attributes["value"].Value.ToLower(), 0);
					}
				}
					break;
				case "door":
					goto case "warp";
				case "warp":
				{
					Portal portal = (Portal)ObjTransform.GetComponent<Portal>();
					portal.SetType( (Portal.type)Enum.Parse( typeof(Portal.type), ObjName));
					
					if ( ((XmlElement)ObjInfo).GetElementsByTagName("property").Item(0) != null )
						portal.SetTarget( ((XmlElement)ObjInfo).GetElementsByTagName("property").Item(0).Attributes["value"].Value);
					
					portal.SetId( ( ObjInfo.Attributes["name"] != null ? ObjInfo.Attributes["name"].Value : ObjName ) );
				}
					break;
					
				case "flyPlatformA":
					goto case "flyPlatform";
				case "flyPlatformB":
					goto case "flyPlatform";
				case "flyPlatform":
				{
					PlatformMove platform = (PlatformMove)ObjTransform.GetComponent<PlatformMove>();
					
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "target" )
						{
							var target = ReadVector(ObjProp.Attributes["value"].Value, 0);
							platform.EndPosition = target;
						}
						if (ObjProp.Attributes["name"].Value.ToLower() == "speed" )
							platform.Speed = float.Parse( ObjProp.Attributes["value"].Value );
					}
				}
					break;
					
				case "chat":
				{
					Conversation chat = (Conversation)ObjTransform.GetComponent<Conversation>();
					
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "file" )
							chat.ConversationFile = (TextAsset)Resources.Load( ObjProp.Attributes["value"].Value, typeof(TextAsset));
						
						if ( ObjProp.Attributes["name"].Value.ToLower() == "oneshotid" ) 
						{
							chat.OneShot = true;
							chat.oneShotId = ObjProp.Attributes["value"].Value;
							//chat.NameId = chat.oneShotId ;
						}
						
						if ( ObjProp.Attributes["name"].Value.ToLower() == "sound" ) 
						{
							chat.soundChat = (AudioClip)Resources.Load( ObjProp.Attributes["value"].Value, typeof(AudioClip));
						}
						
						if ( ObjProp.Attributes["name"].Value.ToLower() == "nameid" ) 
							chat.NameId = ObjProp.Attributes["value"].Value;
						
						if ( ObjProp.Attributes["name"].Value.ToLower() == "zoom" ) 
							chat.zoom = float.Parse(ObjProp.Attributes["value"].Value);
						//Managers.Dialog.Init(chat.ConversationFile);
					}
					Debug.Log("Deploying Conversation");
				}
					break;
					
				case "camerabound":
				{
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "zoom" )
							((CameraBounds)ObjTransform.GetComponent<CameraBounds>()).ZoomFactor = 
								float.Parse(ObjProp.Attributes["value"].Value);
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "offset" )
							((CameraBounds)ObjTransform.GetComponent<CameraBounds>()).Offset = 
								ReadVector( ObjProp.Attributes["value"].Value, 0);
					}
				}
					break;
					
				default:
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "depth" )
							ObjTransform.position += Vector3.forward * 
								float.Parse(ObjProp.Attributes["value"].Value);
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "rotation" )
							ObjTransform.localRotation =  Quaternion.Euler( new Vector3( 0, 0, float.Parse(ObjProp.Attributes["value"].Value)  ) );
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "scale" )
						{
							ObjTransform.localScale = ReadVector(ObjProp.Attributes["value"].Value) ;
							ObjTransform.localScale += Vector3.forward;
						}
						
					}
					break;
				}
				
				#endregion
								
			}
			else Debug.LogWarning("Object '" + ObjName + "' Was not found at: " + "Resources/Prefabs/");

			ObjTotal =  ObjList.Count;
			yield return 0;
		}
	}

	//----------------------------------------------------------------------------------------//
	
	private Vector2 ReadVector(string input, float equalAxis = 1) // equalAxis sets value for both axis in case of found only 1 value                                                     
	{                                                                                   // seek float values inside string
		if (input.Contains(","))                                                        // if there's a comma, separate things
		{
			return new Vector2( float.Parse( input.Remove( input.IndexOf(",") )),
			                   float.Parse( input.Remove(0, input.IndexOf(",") + 1) ));
		}
		// else set just the X Axis 
		return new Vector2( float.Parse(input), equalAxis * float.Parse(input));            // or both if 'AxisY' is enabled    
	}
}
