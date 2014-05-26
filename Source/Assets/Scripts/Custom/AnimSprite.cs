using UnityEngine;
using System.Collections;

//public class SpriteCollection
//{
//	private Sprite[] sprites;
//	private string[] names;
//	
//	public SpriteCollection(string spritesheet)
//	{
//		sprites = Resources.LoadAll<Sprite>(spritesheet);
//		names = new string[sprites.Length];
//		
//		for(var i = 0; i < names.Length; i++)
//		{
//			names[i] = sprites[i].name;
//		}
//	}
//	
//	public Sprite GetSprite(string name)
//	{
//		return sprites[System.Array.IndexOf(names, name)];
//	}
//}

//[AddComponentMenu("")]

public class AnimSprite : MonoBehaviour {

	public Sprite[] sprites; // here via inspector lay sprites in order

	int spritesLength = 0;
	SpriteRenderer mainSprite;
	

	void Start()
	{
		mainSprite = (SpriteRenderer)this.GetComponent<SpriteRenderer>();
		spritesLength = sprites.Length;
	}

	public void PlayFrames(int frameStart, int totalFrames, int FPS = 16)
	{
//		frameStart = Mathf.Clamp (frameStart, 0, spritesLength - 1);
//		totalFrames = Mathf.Clamp (totalFrames, 1, spritesLength -1);
		
		int index = (int)(Time.time * FPS) % totalFrames;								// Increments from 0 to totalFrames with the time
		mainSprite.sprite = sprites[frameStart + index % (spritesLength-frameStart)];	// Traverse sprite's index from frameStart to Max SpritesLength
	}

	public void PlayFramesFast(int frameStart, int totalFrames, int FPS = 16)			// slighty Faster but more insecure to array index overflows
	{
		mainSprite.sprite = sprites[frameStart +  (int)(Time.time * FPS) % totalFrames];								
	}

	public void PlayFramesDir(int frameStart, int totalFrames, int flipped = 1, int FPS = 16)
	{
		int index = (int)(Time.time * FPS) % totalFrames;								// Increments from 0 to totalFrames with the time

		mainSprite.sprite = sprites[frameStart + index % (spritesLength-frameStart)];	// Traverse sprite's index from frameStart to Max SpritesLength

		renderer.transform.localScale = new Vector3(flipped, 1, 1);						// Flip Sprite
	}

}