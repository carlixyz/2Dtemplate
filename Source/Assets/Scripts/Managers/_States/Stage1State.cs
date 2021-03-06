using UnityEngine;
using System.Collections;

public class Stage1State : InGameState
{
    public override void Init()
    {
		Managers.Game.IsPaused = false;
		Managers.Game.IsPlaying = true;

		Managers.Game.LoadMap("/Levels/DemoLevel.tmx");
//		Managers.Tiled.Load("/Levels/DemoLevel.tmx");
        base.Init();

    }

    public override void DeInit()
    {
		Managers.Game.IsPaused = true;
		Managers.Game.IsPlaying = false;

		Managers.Game.UnloadMap ();
//		Managers.Tiled.Unload ();
        base.DeInit();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void Pause()
    {

    }

    public override void Resume()
    {
//		if (Managers.Game.PlayerPrefab && !Managers.Game.IsPlaying)
//		{
//			Managers.Tiled.Load(Managers.Register.PamperoFile);
//			
//			Managers.Game.IsPlaying = true;
//			Managers.Game.IsPaused = false;
//		}
    }
}
