using UnityEngine;
using System.Collections;

public class IntroState : GameState 
{
	public Texture2D[] Images;
	public GameState nextState;

	int index = 0;
	int total = 0;

	public float fadeInSpeed = .5f;
	public float fadeOutSpeed = 1;
	public float totalTime = 10;


	float timeLapse = 0;

    public override void Init()
    {
											//        Managers.Display.ShowFadeOut(10);
        Managers.Display.ShowFade(10);

		timeLapse = totalTime + 2;					// 2 is Start Delay Time
		index = 0;
		total = Images.Length;
    }
	
    public override void DeInit()
    {
//        Managers.Display.ShowFadeIn(10);
		Managers.Display.ShowFade(5, false);
    }

    public override void OnUpdate()
    {
		timeLapse -= Time.deltaTime;

		if (index < total) 
		{
			if ((int)timeLapse == (int)(totalTime * .9f))
				Managers.Display.ShowImage (Images[index], timeLapse);

			if ((int)timeLapse == (int)(totalTime * .8f))
					Managers.Display.ShowFade (fadeOutSpeed, false);

			if ((int)timeLapse == (int)(totalTime * .2f))
					Managers.Display.ShowFade (fadeInSpeed);

			if ((int)timeLapse < 0) 
			{
					timeLapse = totalTime;
					index++;
			}
		}
		else 
			Managers.Game.ChangeState(nextState.GetType());
//            Managers.Game.ChangeState(typeof(MainMenuState));
    }

    public override void Pause() { ;}

    public override void Resume() { ;}
}