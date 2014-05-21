using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ObjManager : MonoBehaviour 
{

    public GameObject Player;
    public Transform PlayerTransform;

	public bool Load(XmlNodeList Layer)
	{
		foreach (XmlNode ObjGrp in Layer)
			StartCoroutine (BuildPrefabs (ObjGrp));

		return true;
	}
	
	public void Unload()
	{
//		foreach (XmlNode ObjGrp in Layer)
//			StartCoroutine (BuildPrefabs (ObjGrp));
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
				GameObject ObjPrefab =(GameObject)Instantiate( Resources.Load( "Prefabs/" + ObjName , typeof(GameObject)));
				
				Transform ObjTransform = ObjPrefab.transform;
				ObjTransform.position = new Vector3(
					(float.Parse(ObjInfo.Attributes["x"].Value) / tilewidth) + (ObjTransform.localScale.x * .5f),        // X
					height - (float.Parse(ObjInfo.Attributes["y"].Value) / tileheight - ObjTransform.localScale.y * .5f),// Y		 		     
					Managers.Tiled.MapTransform.position.z);		 // Z
				
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
					Managers.Register.Player = ObjPrefab;
					Managers.Register.PlayerTransform = ObjPrefab.transform;
					Managers.Display.CameraScroll.SetTarget( Managers.Register.PlayerTransform, false );
					PlayerTransform = Managers.Register.PlayerTransform;
					//Debug.Log("setting up position in TileManager");
					Managers.Register.SetPlayerPos();
					
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "zoom" )
							((CameraTargetAttributes)ObjPrefab.GetComponent<CameraTargetAttributes>()).distanceModifier = 
								float.Parse(ObjProp.Attributes["value"].Value.ToLower());
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "offset" )
							((CameraTargetAttributes)ObjPrefab.GetComponent<CameraTargetAttributes>()).Offset = 
								ReadVector( ObjProp.Attributes["value"].Value.ToLower(), 0);
					}
				}
					break;
				case "door":
					goto case "warp";
				case "warp":
//				{
//					Portal portal = (Portal)ObjPrefab.GetComponent<Portal>();
//					portal.SetType( (Portal.type)Enum.Parse( typeof(Portal.type), ObjName));
//					
//					if ( ((XmlElement)ObjInfo).GetElementsByTagName("property").Item(0) != null )
//						portal.SetTarget( ((XmlElement)ObjInfo).GetElementsByTagName("property").Item(0).Attributes["value"].Value);
//					
//					portal.SetId( ( ObjInfo.Attributes["name"] != null ? ObjInfo.Attributes["name"].Value : ObjName ) );
//				}
					break;
					
				case "flyPlatformA":
					goto case "flyPlatform";
				case "flyPlatformB":
					goto case "flyPlatform";
				case "flyPlatform":
				{
					PlatformMove platform = (PlatformMove)ObjPrefab.GetComponent<PlatformMove>();
					
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
					Conversation chat = (Conversation)ObjPrefab.GetComponent<Conversation>();
					
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
							((CameraBounds)ObjPrefab.GetComponent<CameraBounds>()).ZoomFactor = 
								float.Parse(ObjProp.Attributes["value"].Value);
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "offset" )
							((CameraBounds)ObjPrefab.GetComponent<CameraBounds>()).Offset = 
								ReadVector( ObjProp.Attributes["value"].Value, 0);
					}
				}
					break;
					
				default:
					foreach (XmlNode ObjProp in ((XmlElement)ObjInfo).GetElementsByTagName("property") )
					{
						if (ObjProp.Attributes["name"].Value.ToLower() == "depth" )
							ObjPrefab.transform.position += Vector3.forward * 
								float.Parse(ObjProp.Attributes["value"].Value);
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "rotation" )
							ObjPrefab.transform.localRotation =  Quaternion.Euler( new Vector3( 0, 0,
							                                                                   float.Parse(ObjProp.Attributes["value"].Value)  ) );
						
						if (ObjProp.Attributes["name"].Value.ToLower() == "scale" )
						{
							ObjPrefab.transform.localScale = ReadVector(ObjProp.Attributes["value"].Value) ;
							ObjPrefab.transform.localScale += Vector3.forward;
						}
						
					}
					break;
				}
				
				#endregion
				
			}
			else Debug.LogWarning("Object '" + ObjName + "' Was not found at: " + "Resources/Prefabs/");
			
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
