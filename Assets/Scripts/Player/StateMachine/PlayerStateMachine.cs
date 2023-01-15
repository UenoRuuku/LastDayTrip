using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum PlayerStateName {
    Normal_Controlable, Death,Dialogue
}


[Serializable]
public class PlayerParameter {
    public DeathEffector DeathAnimation;
    public float DeathForce;
    public float DeathTime;
    public Transform DeathPos;

}

public class PlayerStateMachine : MonoBehaviour
{
#pragma warning disable CS0436 // Type conflicts with imported type
    Dictionary<PlayerStateName, IState> States;
#pragma warning restore CS0436 // Type conflicts with imported type
    PlayerStateName CurrentState;
    GameObject Killer;
    [SerializeField]
    PlayerParameter p;



    
    
    // Start is called before the first frame update
    void Start()
    {
        States = new Dictionary<PlayerStateName, IState>();
        States.Add(PlayerStateName.Normal_Controlable, new NormalState(this));
        States.Add(PlayerStateName.Death, new DeathState(this,p));
        CurrentState = PlayerStateName.Normal_Controlable;
        States[CurrentState].OnEnter();

    }


    public void TransState(PlayerStateName nextState) {
        States[CurrentState].OnExit();
        CurrentState = nextState;
        States[CurrentState].OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        States[CurrentState].OnUpdate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Killer") && CurrentState != PlayerStateName.Death) {

            Killer = collision.gameObject;
            //DeathPos = collision.bounds.ClosestPoint(transform.position);
            TransState(PlayerStateName.Death);
        }
        if (collision.CompareTag("NPC") && CurrentState != PlayerStateName.Dialogue && CurrentState != PlayerStateName.Death) {
            TransState(PlayerStateName.Dialogue);
        }
    }

    public GameObject GetKiller() {
        return Killer;
    }

    public void InstantiateDeath() {
        p.DeathAnimation.Play(p.DeathPos.position);
        //p.DeathAnimation.Play();
    }

}
