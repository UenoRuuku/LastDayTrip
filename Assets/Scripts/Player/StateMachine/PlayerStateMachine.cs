using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum PlayerStateName {
    Normal_Controlable, Death
}


[Serializable]
public class PlayerParameter {
    public GameObject DeathAnimation;
    public float DeathForce;
    public float DeathTime;

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
            TransState(PlayerStateName.Death);
            Killer = collision.gameObject;
        }
    }

    public GameObject GetKiller() {
        return Killer;
    }

    public void InstantiateDeath() {
        Instantiate(p.DeathAnimation, transform.position, new Quaternion(0,0,0,0));
    }

}
