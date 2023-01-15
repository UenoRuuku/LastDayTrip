using UnityEngine;
using System.Collections;

public class NormalState : IState
{
    PlayerStateMachine Player;


    public NormalState(PlayerStateMachine sm) {
        Player = sm;
    }

    public void OnEnter()
    {
        Player.GetComponent<NormalController>().enabled = true;
    }

    public void OnExit()
    {

        Player.GetComponent<NormalController>().Stop();
    }

    public void OnUpdate()
    {
    }
}
