using UnityEngine;
using System.Collections;

public class WorldState0 : InGameState
{
    public override void Init()
    {
       //Debug.Log("Enter World 0 State: Pampero");
//        Managers.Tiled.Load(Managers.Register.PamperoFile);
        //Managers.Tiled.Load("/Levels/DemoLevel.tmx");
        base.Init();

    }

    public override void DeInit()
    {
        //Debug.Log("Exit the current State and returning map");
		Managers.Register.Stage1 = Managers.Register.currentLevelFile ;

        Managers.Tiled.Unload();
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
        //Debug.Log("Called Pampero Resume!");

		if (Managers.Objects.Player && !Managers.Game.IsPlaying)
        {
            Managers.Game.LoadMap(Managers.Register.Stage1);

            //Debug.Log("Reloaded Pampero Level!");

            Managers.Game.IsPlaying = true;
            Managers.Game.IsPaused = false;
        }


    }
}
