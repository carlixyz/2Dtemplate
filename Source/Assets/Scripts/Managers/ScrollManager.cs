﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

/* SCROLLMANAGER - RULES OF CONSIDERATIONS:
 * 
 * 
 * - Load Files From Assets Datafolder as: "/SomeFolder/SomeImage.SomeExtension" ( Don't use 'Ñ' letter)
 *   (Always before every Build Add again the "Levels" Folder with maps inside the 'ProjectName/ProjectName_Data/'Folder)
 * 
 * - A new Scroll Parallax system was added, the only issue it's that these layers not follow world position b
 * because they are Camera's childs, but the depth property is still working (just with a diferent output)
 * 
 * */

public class ScrollManager : MonoBehaviour 
{
	public Vector3 oldPos;
	public Vector3 scrollValue;
	
	public float  ScrollBaseSpeed = 1;
	public ScrollLayer[] ScrollLayers;

	public Transform CamTransform;
//	Transform PlayerTransform;

	void Start()
	{
		CamTransform = Managers.Display.CamTransform;
	}

	public void Load(XmlNode scrollLayer) 
	{
		if (CamTransform == null)
			CamTransform = Managers.Display.CamTransform;
		
		StartCoroutine ( BuildScrollLayers (scrollLayer));

//		SetupScroll ();
	}

	public void Load(XmlNodeList scrollLayers) 
	{
		if (CamTransform == null)
			CamTransform = Managers.Display.CamTransform;

		for (int index = scrollLayers.Count-1; index >= 0; index--) 										// TagName: Reversed TileSet Layers.
			StartCoroutine ( BuildScrollLayers (scrollLayers.Item (index)));

//		SetupScroll ();
	}

	public void Unload() 
	{
        ScrollBaseSpeed = 1;
		oldPos = Vector3.zero;
		scrollValue = Vector3.zero;

		if ( ScrollLayers != null && ScrollLayers.Length > 0)
			for ( int LayerIndex = ScrollLayers.Length - 1; LayerIndex >= 0 ; LayerIndex-- )
				if ( ScrollLayers[LayerIndex] != null )
					Destroy(ScrollLayers[LayerIndex].gameObject);
		
		ScrollLayers = null;
	}

	public IEnumerator BuildScrollLayers(XmlNode LayerInfo)
	{
		if (!Camera.main)
			yield break;
		
//		var cam = Camera.main;
		//float Depth = (TileOutputSize.z - cam.transform.position.z);
		
		//float Depth = TileOutputSize.z ;
		//TileOutputSize.z += 0.5f;
		
		//if ( Depth == 0)
		//    Depth = 1;
		
		float Depth = -120;               //bool AutoDepth = true;						// -120 is Flag number (There won't be nothing deeper than 120)
		
		GameObject scrollLayer = new GameObject(LayerInfo.Attributes["name"].Value);   	// Build new scrollLayer inside layer

		scrollLayer.transform.parent = CamTransform;  	// Config Layer position from Tiled file 'Depth' property or else by Layer order by Default  
		scrollLayer.transform.localScale = Vector3.one;
		
		var scroll = scrollLayer.AddComponent<ScrollLayer>();							// Add magic scroll component
		
		foreach (XmlNode LayerProp in ((XmlElement)LayerInfo).GetElementsByTagName("property") ) //if (LayerInfo.LastChild.Name == "properties")
		{																					 //    foreach (XmlNode LayerProp in LayerInfo.LastChild)
			
			//Debug.Log(LayerProp.Attributes["name"].Value + ": " + LayerProp.Attributes["value"].Value );
			switch (LayerProp.Attributes["name"].Value.ToLower())
			{
				case "depth":
					Depth = float.Parse(LayerProp.Attributes["value"].Value);               // Set scroll Layer depth
					break;
					
				case "scroll":
					scroll.scroll = (LayerProp.Attributes["value"].Value.ToLower() == "auto" ? ScrollType.Auto : ScrollType.Relative);
					break;
					
				case "size":
					if (LayerProp.Attributes["value"].Value.ToLower() == "fullscreen" )
						scroll.pixelPerfect = !(scroll.streched = true);                   // Set texture streched 
					else if (LayerProp.Attributes["value"].Value.ToLower() == "original")
						scroll.pixelPerfect = !(scroll.streched = false);                  // Set texture pixelperfectv
					else if (LayerProp.Attributes["value"].Value.ToLower() == "repeatx")
						scroll.pixelPerfect = !(scroll.tileY = scroll.streched = false);   // Set pixelperfect tiling X
					else if (LayerProp.Attributes["value"].Value.ToLower() == "repeaty")
						scroll.pixelPerfect = !(scroll.tileX = scroll.streched = false);   // Set pixelperfect tiling Y
					else if (LayerProp.Attributes["value"].Value.ToLower() == "norepeat")
						scroll.pixelPerfect = !(scroll.tileX = scroll.tileY = scroll.streched = false);//Set without tile
					break;
					
				case "speed":
					scroll.speed = ReadVector(LayerProp.Attributes["value"].Value, 0); 		//Debug.Log("Speed: " +ReadVector(LayerProp.Attributes["value"].Value, 0)); 
					break;
					
				case "offset":
					scroll.offset = ReadVector(LayerProp.Attributes["value"].Value, 0);		//Debug.Log("offset: " + scroll.offset);
					break;
					
				case "scale":
					scroll.scale = ReadVector(LayerProp.Attributes["value"].Value);	       //Debug.Log("scale: " + scroll.scale);
					break;
					
				case "padding":
					scroll.padding = ReadVector(LayerProp.Attributes["value"].Value);      //Debug.Log("padding: " + scroll.padding);
					break;
				case "heightrange":
					scroll.range = ReadVector(LayerProp.Attributes["value"].Value);	       //Debug.Log("padding: " + scroll.padding);
					break;
			}
		}
		
		// Config Layer position from Tiled file 'Depth' property or else by Layer order by Default
		//scrollLayer.transform.position =  new Vector3( cam.transform.position.x, cam.transform.position.y, Depth - cam.transform.position.z);
		
		if (Depth == -120 )       //if ( AutoDepth)         
		{
			Depth = Managers.Tiled.TileOutputSize.z -( System.Convert.ToSingle(Managers.Tiled.TileOutputSize.z == 0) );
			Managers.Tiled.TileOutputSize.z += 0.5f * System.Convert.ToSingle(Managers.Tiled.TileOutputSize.z != 0);
		}
		
		scrollLayer.transform.position =  new Vector3( CamTransform.position.x, CamTransform.position.y, Depth );
		
		
		#if TEXTURE_RESOURCE
		string AuxPath = LayerInfo.FirstChild.Attributes["source"].Value;
		Texture2D tex = (Texture2D)Resources.Load( AuxPath.Remove(AuxPath.LastIndexOf(".")+1), typeof(Texture2D) );
		
		#else
		// Add textures
		WWW www = new WWW( "file://" 
		                  + Application.dataPath 
		                  + Managers.Register.currentLevelFile.Remove(Managers.Register.currentLevelFile.LastIndexOf("/") + 1)
		                  + LayerInfo.FirstChild.Attributes["source"].Value );
		Texture2D tex = www.texture;
		
		#endif
		
		tex.filterMode = FilterMode.Point;
		tex.anisoLevel = 0;
		scroll.SetTexture(tex);
		
		Camera.main.ResetProjectionMatrix();
		
		scroll.UpdateLayer();
		
		///////////////////////////////////////////////////////////////////////
		yield return 0;
	}
	
	
	public void SetupScroll()
	{
		List<ScrollLayer> scrollList = new List<ScrollLayer>(FindObjectsOfType(typeof(ScrollLayer)) as ScrollLayer[]);
		
		foreach (ScrollLayer scroll in scrollList)
		{
			scroll.SetWeight(Vector3.Distance( Managers.Display.CamTransform.position, scroll.transform.position));
		}
		#if UNITY_FLASH
		scrollList.sort(ScrollLayer.Comparision);
		#else
		scrollList.Sort();
		#endif
		ScrollLayers = scrollList.ToArray();

	}
	
	void UpdateScroll()
	{
		foreach (var scrollLayer in ScrollLayers)
		{
			if (!scrollLayer)
				continue;
			
			scrollLayer.UpdateLayer(true, false, false);
		}
	}


	
	void LateUpdate()		// only for scroll Layers
	{
		if ( ScrollLayers == null) return;
		
		UpdateScroll();
		scrollValue = CamTransform.position - oldPos;
		oldPos = CamTransform.position;
		
		foreach (ScrollLayer scrollLayer in ScrollLayers)
		{
			if (!scrollLayer)
				continue;
			
			if ( Managers.Objects.PlayerTransform )
				scrollLayer.gameObject.SetActive( ( scrollLayer.range.y > Managers.Display.MainCamera.transform.position.y &&
				                                   scrollLayer.range.x < Managers.Display.MainCamera.transform.position.y ) );
			
			if (scrollLayer.GetMaterial())
			{
				foreach (string textureName in scrollLayer.GetTextureNames())
				{
					if (string.IsNullOrEmpty(textureName)) 
						continue;
					
					if (scrollLayer.GetMaterial().HasProperty(textureName))
					{
						scrollLayer.GetMaterial().SetTextureOffset( textureName, 
                           WrapVector( scrollLayer.GetMaterial().GetTextureOffset(textureName) +
									 	ScrollBaseSpeed * (scrollLayer.GetScrollType() == ScrollType.Auto ?
   										scrollLayer.GetSpeed() * Time.deltaTime :
   										new Vector2(scrollValue.x * scrollLayer.GetSpeed().x, scrollValue.y * scrollLayer.GetSpeed().y) )));
						// So, basically If scrollLayer mode is Auto, update by a deltaTime else it's Relative,
						// create a new Vector with the new Player Position multiplied by each axis speed(stay quiet if zero) 
					}
				}
			}
		}
	}
	
	private Vector2 ReadVector(string input, float equalAxis = 1) // AxisY sets value for both axis in case of found only 1 value                                                     
	{                                                                                   // seek float values inside string
		if (input.Contains(","))                                                        // if there's a comma, separate things
		{
			return new Vector2( float.Parse( input.Remove( input.IndexOf(",") )),
		                   float.Parse( input.Remove(0, input.IndexOf(",") + 1) ));			// else set just the X Axis 			
		}

		return new Vector2( float.Parse(input), equalAxis * float.Parse(input));            // or both if 'AxisY' is enabled    
	}
	
	private Vector2 WrapVector(Vector2 input)
	{
		return new Vector2(input.x - (int)input.x, input.y - (int)input.y);
	}

}
