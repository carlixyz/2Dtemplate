using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using System.Text;
using Ionic.Zlib;

/* TILEMANAGER - RULES OF CONSIDERATIONS:
 * 
 * - Load Files From Assets Datafolder as: "/SomeFolder/SomeFile.SomeExtension" ( Don't use 'Ñ' letter)
 *   (Always before every Build Add again the "Levels" Folder with maps inside the 'ProjectName/ProjectName_Data/'Folder)
 * 
 * - Use Tiled Editor and U can rename file extensions from '.TMX' to '.XML' (And save in Gzip + Base64 Compression)
 * 
 * - Remember To always Use 2x Power Texture sizes..
 * 
 * - In Tiled Never mix two diferent TileSet images inside the same Layer
 *  (when Using Resources.Load(..) always quit the first '/' & the last '.whatever' extension)
 *  
 * */

public class TileManager : MonoBehaviour 
{

    uint FlippedHorizontallyFlag    		= 0x80000000;
    uint FlippedVerticallyFlag      		= 0x40000000;
    uint FlippedDiagonallyFlag      		= 0x20000000;

    public Vector3 TileOutputSize           = new Vector3(1, 1, 0);			        // scrollLayer Poligonal Modulation inside Unity(Plane)
	public Vector2 eps                      = new Vector2(0.000005f, 0.000005f);	// epsilon to fix some Texture bleeding	// 5e-06
    public bool CombineMesh                 = true;

    int LastUsedMat = 0;
	List<cTileSet> TileSets = new List<cTileSet>();

	public Transform MapTransform;

    //----------------------------------------------------------------------------------------//
	
	public void Setup(XmlElement docElement, string mapName )
	{
		if (MapTransform)
			return ;

		MapTransform = new GameObject (mapName).transform;			// Create inside the editor hierarchy & take map transform cached
		
		Managers.Display.CameraScroll.ResetBounds (new Rect (0, 0,					// Set Level bounds for camera 
		                                                     int.Parse (docElement.Attributes ["width"].Value) * TileOutputSize.x,
		                                                     int.Parse (docElement.Attributes ["height"].Value) * TileOutputSize.y));
		
		// SEEK BITMAP SOURCE FILE	 
		foreach (XmlNode TileSetInfo in docElement.GetElementsByTagName("tileset")) 			// array of the level nodes.
			TileSets.Add(new cTileSet(TileSetInfo, Managers.Register.currentLevelFile));
	}

	public void Load(XmlNode layer)	// Previously Iterated
	{
		StartCoroutine ( BuildLayer (layer ));
	}

    public void Unload()
    {
        if ( MapTransform == null ) 
            return;
 
        TileSets.Clear();
        LastUsedMat = 0;

        TileOutputSize = new Vector3(1, 1, 0);			         
        CombineMesh = true;

        if ( MapTransform != null )
        {
            Destroy(MapTransform.gameObject);
            MapTransform = null;
        }
    }

	    //----------------------------------------------------------------------------------------//

    public IEnumerator BuildLayer(XmlNode LayerInfo)
    {
        GameObject Layer = new GameObject(LayerInfo.Attributes["name"].Value); // add Layer Childs inside hierarchy.

        var LayerTransform = Layer.transform;
        LayerTransform.position = new Vector3(Layer.transform.position.x, Layer.transform.position.y, TileOutputSize.z);
        LayerTransform.parent = MapTransform;
        //TileOutputSize.z += 0.5f;

        int ColIndex = 0;
        int RowIndex = int.Parse(LayerInfo.Attributes["height"].Value) - 1;
        uint CollisionLayer = 0;

        XmlElement Data = (XmlElement)LayerInfo.FirstChild;
        while (Data.Name != "data") 
        {
            foreach( XmlElement property in Data)
            {
                if (property.GetAttribute("name").ToLower() == "collision")
                    if (property.GetAttribute("value").ToLower() != "plane") 
                        CollisionLayer = 1;                                         // check if it's a boxed collision and setup
                    else 
                        CollisionLayer = 2;                                         // else it's a plane type collsion Layer..

                if (property.GetAttribute("name").ToLower() == "depth")
                    LayerTransform.position = new Vector3( LayerTransform.position.x, LayerTransform.position.y, float.Parse(property.GetAttribute("value")));

            }
            Data = (XmlElement)Data.NextSibling;
        }

		// -- Always use Gzip compression else input here the parser code from google.code SVN Niangapiry --

        if ( LayerTransform.position.z == TileOutputSize.z)
            TileOutputSize.z += 0.5f;


        // & CHECK IF DATA IS GZIP COMPRESSED OR DEFAULT XML AND CREATE OR BUILD ALL TILES INSIDE EACH LAYER			
        if (Data.HasAttribute("compression") && Data.Attributes["compression"].Value == "gzip")
        {
            // Decode Base64 and then Uncompress Gzip scrollLayer Information
            byte[] decodedBytes = Ionic.Zlib.GZipStream.UncompressBuffer(System.Convert.FromBase64String(Data.InnerText));
            for (int tile_index = 0; tile_index < decodedBytes.Length; tile_index += 4)
            {
				GameObject TileRef = BuildTile(	  (uint)  (decodedBytes[tile_index]  |
	                                       		  decodedBytes[tile_index + 1] << 8  |
			                                      decodedBytes[tile_index + 2] << 16 |
                                           		  decodedBytes[tile_index + 3] << 24)	); // BuildTile( uint global_tile_id ); // >> Build a Tile with that
				

                if (TileRef != null)
                {
                    TileRef.transform.position = new Vector3( ColIndex * TileOutputSize.x, RowIndex * TileOutputSize.y, LayerTransform.position.z);

                    TileRef.transform.parent = LayerTransform;

                    if (CollisionLayer > 0)
                        if ( CollisionLayer == 1 && TileRef.GetComponent<BoxCollider>() == null) 
                           (TileRef.AddComponent<BoxCollider>() as BoxCollider).size = Vector3.one; // simple boxed collision
                        else if ( TileRef.GetComponent<MeshCollider>() == null)
                           (TileRef.AddComponent<MeshCollider>()as MeshCollider).sharedMesh = 
                            (Mesh)Resources.Load("Prefabs/Collision/1_0 ColPlane", typeof(Mesh));   // One-Way Plane Collision 
                }

                ColIndex++;
                RowIndex -= System.Convert.ToByte(ColIndex >= int.Parse(LayerInfo.Attributes["width"].Value));
                ColIndex = ColIndex % int.Parse(LayerInfo.Attributes["width"].Value);      // ColIndex % TotalColumns 

            }//end of each scrollLayer GZIP Compression Info 
        }
        else Debug.LogError(" Format Error: Save Tiled File in XML style or Compressed mode(Gzip + Base64)");

        if (CombineMesh && LastUsedMat == 0)
            Layer.AddComponent<CombineMeshes>();

        yield return 0;
    }


    GameObject BuildTile(uint TileId)
    {
        bool Flipped_X = false;
        bool Flipped_Y = false;
        bool Rotated = false;

        if (TileId != 0)    //	 if ( FirstGid => TileId && TileId <= TotalTiles)	// Si es mayor que 0!				 	 
        {
            uint Index = TileId & ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag);
            string TileName = "Tile_" + Index;

            if ((TileId & FlippedHorizontallyFlag) != 0)
            { Flipped_X = true; TileName += "_H"; }

            if ((TileId & FlippedVerticallyFlag) != 0)
            { Flipped_Y = true; TileName += "_V"; }

            if ((TileId & FlippedDiagonallyFlag) != 0)
            { Rotated = true; TileName += "_VH"; }

            for (int i = TileSets.Count - 1; i >= 0; i--)		// Recorrer en reversa la lista y checar el TileSet firstGid
            {
                if (Index >= TileSets[i].FirstGid)
                {

                    int localIndex = (int)(Index - TileSets[i].FirstGid) + 1;

                    GameObject Tile = new GameObject();                                             // Build new scrollLayer inside layer

                    Tile.name = TileName;
                    Tile.transform.position = Vector3.zero;

                    MeshFilter meshFilter = (MeshFilter)Tile.AddComponent<MeshFilter>();            // Add mesh Filter & Renderer
                    MeshRenderer meshRenderer = (MeshRenderer)Tile.AddComponent<MeshRenderer>();
             
                    meshRenderer.sharedMaterial = TileSets[i].mat;                                  // Set His own Material w/ texture
                    LastUsedMat = i;
                    //Debug.Log("\n Mat index: " + i);

                    Mesh m = new Mesh();                                                            // Prepare mesh UVs offsets
                    m.name = TileName + "_mesh";
                    m.vertices = new Vector3[4]	{
									new Vector3( TileOutputSize.x, 			  	   0, 0.01f),
									new Vector3( 0, 					 		   0, 0.01f),
									new Vector3( 0, 			 	TileOutputSize.y, 0.01f),
									new Vector3( TileOutputSize.x, 	TileOutputSize.y, 0.01f) };

                    int offset_x = localIndex % TileSets[i].SrcColumns;
                    int offset_y = TileSets[i].SrcRows - (Mathf.FloorToInt(localIndex / TileSets[i].SrcColumns) +
                                                System.Convert.ToByte((localIndex % TileSets[i].SrcRows) != 0));


                    Vector2 vt1 = new Vector2((offset_x - System.Convert.ToByte(Flipped_X)) * TileSets[i].ModuleWidth,
                                             ((offset_y + System.Convert.ToByte(Flipped_Y)) * TileSets[i].ModuleHeight));
                    Vector2 vt2 = new Vector2((offset_x - System.Convert.ToByte(!Flipped_X)) * TileSets[i].ModuleWidth,
                                             ((offset_y + System.Convert.ToByte(Flipped_Y)) * TileSets[i].ModuleHeight));
                    Vector2 vt3 = new Vector2((offset_x - System.Convert.ToByte(!Flipped_X)) * TileSets[i].ModuleWidth,
                                             ((offset_y + System.Convert.ToByte(!Flipped_Y)) * TileSets[i].ModuleHeight));
                    Vector2 vt4 = new Vector2((offset_x - System.Convert.ToByte(Flipped_X)) * TileSets[i].ModuleWidth,
                                             ((offset_y + System.Convert.ToByte(!Flipped_Y)) * TileSets[i].ModuleHeight));

					int Xsign = (Flipped_X ? -1 : 1);											// if ( Flipped_X ) Sign = -1;
					int Ysign = (Flipped_Y ? -1 : 1);

                    vt1.x += -eps.x * Xsign;    vt1.y +=  eps.y * Ysign;
                    vt2.x +=  eps.x * Xsign;    vt2.y +=  eps.y * Ysign;
                    vt3.x +=  eps.x * Xsign;    vt3.y += -eps.y * Ysign;
                    vt4.x += -eps.x * Xsign;    vt4.y += -eps.y * Ysign;

                    if (Rotated)                                                                // This is some Math for flip 
                    {
                        if (Flipped_X && Flipped_Y || !Flipped_X && !Flipped_Y)                 // & I don't want to explain!
                        {
                            Vector2 vtAux1 = vt2;
                            vt2 = vt4;
                            vt4 = vtAux1;
                        }
                        else
                        {
                            Vector2 vtAux2 = vt3;
                            vt3 = vt1;
                            vt1 = vtAux2;
                        }
                    }

                    m.uv = new Vector2[4] { vt1, vt2, vt3, vt4 };
                    m.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
                    m.RecalculateNormals();
                    m.Optimize();
                    meshFilter.sharedMesh = m;
                    m.RecalculateBounds();

                    /////////////////////////////////////////////////////////////////////////////////////////////////////
                                            
                    if (TileSets[i].Collisions.ContainsKey(localIndex))                         // If there's a Collision property,
                        switch ((string)(TileSets[i].Collisions[localIndex]).ToLower())                   // Check type and Setup:
                        {
                            case "plane":
                                Tile.AddComponent<MeshCollider>().sharedMesh =
                                    (Mesh)Resources.Load("Prefabs/Collision/1_0 ColPlane", typeof(Mesh));   // One-Way Plane Collision 
                                break;

                            case "slope":
                                Tile.layer = 8;
                                if (Flipped_X && !Flipped_Y || !Flipped_X && Flipped_Y && Rotated || !Flipped_X && Flipped_Y && !Rotated)
                                {
                                    Tile.AddComponent<MeshCollider>().sharedMesh =
                                        (Mesh)Resources.Load("Prefabs/Collision/SlopeLeft", typeof(Mesh));  // Left Slope Collision
                                }
                                else
                                {
                                    Tile.AddComponent<MeshCollider>().sharedMesh =
                                        (Mesh)Resources.Load("Prefabs/Collision/SlopeRight", typeof(Mesh)); // Right Slope Collision
                                }
                                break;

                            default:
                                (Tile.AddComponent("BoxCollider") as BoxCollider).size = Vector3.one;       // or default's Box Collision 
                                break;
                		}

        			return Tile;
				}	//break;
			}
    	}
    return null;
    }

																							// End of BuidTile Function
    //----------------------------------------------------------------------------------------//

}

class cTileSet
{

    public int SrcColumns       = 1;
    public int SrcRows          = 1;
    public int FirstGid         = 0;

    public float ModuleWidth    = 1.0f;
    public float ModuleHeight   = 1.0f;

    public string SrcImgName    = "";											        // Image Source data references
    public string SrcImgPath    = "";

    public Material mat;
    public Dictionary<int, string> Collisions = new Dictionary<int, string>();

    public cTileSet(XmlNode TileSet, string FilePath)			// if ( TileSet.HasChildNodes ) {  var lTileSet : cTileSet = new cTileSet(); lTileSet.Load(
    {

        foreach (XmlNode TileSetNode in TileSet)
        {
            if (TileSetNode.Name == "image")
            {
                int TileInputWidth = System.Convert.ToInt32(TileSet.Attributes["tilewidth"].Value); // scrollLayer width inside bitmap file  ( 64 )
                int TileInputHeight = System.Convert.ToInt32(TileSet.Attributes["tileheight"].Value);// scrollLayer height inside bitmap file 

                int SrcImgWidth = System.Convert.ToInt32(TileSetNode.Attributes["width"].Value); // File Resolution (512)
                int SrcImgHeight = System.Convert.ToInt32(TileSetNode.Attributes["height"].Value);

                SrcColumns = SrcImgWidth / TileInputWidth;
                SrcRows = SrcImgHeight / TileInputHeight;

                this.ModuleWidth = 1.0f / SrcColumns;
                this.ModuleHeight = 1.0f / SrcRows;

                FirstGid = System.Convert.ToInt16(TileSet.Attributes["firstgid"].Value);

                SrcImgName = TileSet.Attributes["name"].Value;						    // Image Source data references
                SrcImgPath = TileSetNode.Attributes["source"].Value;
                //SrcImgPath = FilePath.Remove(FilePath.LastIndexOf("/") + 1) + SrcImgName;

            }
            else if (TileSetNode.Name == "tile")
            {
                if (TileSetNode.FirstChild.FirstChild.Attributes["name"].Value.ToLower() == "collision")
                    Collisions.Add(int.Parse(TileSetNode.Attributes["id"].Value) + 1,
                                                TileSetNode.FirstChild.FirstChild.Attributes["value"].Value);
            }


        #if TEXTURE_RESOURCE
            //SrcImgPath = "Assets" + FilePath.Remove(FilePath.LastIndexOf("/") + 1) + SrcImgPath;
            //Texture2D tex = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(SrcImgPath, typeof(Texture2D));
            //Debug.Log(SrcImgPath);

            SrcImgPath = FilePath.Remove(FilePath.LastIndexOf("/") + 1) + SrcImgName;
            Texture2D tex = (Texture2D)Resources.Load(SrcImgPath.Remove(0, 11), typeof(Texture2D));

        #else
            //Debug.Log(        "file://" + Application.dataPath + FilePath.Remove(FilePath.LastIndexOf("/") + 1) + SrcImgPath);
            WWW www = new WWW("file://" + Application.dataPath + FilePath.Remove(FilePath.LastIndexOf("/") + 1) + SrcImgPath);
            Texture2D tex = www.texture;

        #endif

            if (tex == null)
            {
                Debug.LogError( SrcImgPath + " texture file not found, put it in the same Tiled map folder: " + FilePath);
                return;   //	 this.close; 
            }
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.anisoLevel = 0;

            //this.mat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            this.mat = new Material(Shader.Find("Unlit/Transparent"));
            this.mat.mainTexture = tex;

        }
    }


}


