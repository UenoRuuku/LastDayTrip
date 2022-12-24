using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DeathState : IState
{
    PlayerStateMachine Player;
    PlayerParameter PlayerStatus;
    Animator am;
    Rigidbody2D rb;
    SpriteRenderer sp;
    Vector2 dir;
    bool isFinished;

    public DeathState(PlayerStateMachine sm, PlayerParameter p)
    {
        PlayerStatus = p;
        Player = sm;
        am = Player.GetComponent<Animator>();
        rb = Player.GetComponent<Rigidbody2D>();
        sp = Player.GetComponent<SpriteRenderer>();
    }

    public void OnEnter()
    {
        isFinished = false;
        am.speed = 0;
        rb.velocity = new Vector2(0, 0);
        rb.gravityScale = 0;
        dir = Player.transform.position - Player.GetKiller().transform.position;
        dir.Normalize();
        rb.AddForce(dir * PlayerStatus.DeathForce, ForceMode2D.Impulse);
        Player.StartCoroutine(Dying());
    }

    IEnumerator Dying() {
        DOVirtual.Float(1, 0, PlayerStatus.DeathTime, SetColor).OnComplete(()=> {
            isFinished = true;
        });
        Player.InstantiateDeath();
        yield return new WaitUntil(()=> isFinished);
        Player.TransState(PlayerStateName.Normal_Controlable);
        yield return null;
    }

    void SetColor(float a)
    {
        sp.color = new Color(sp.color.r,sp.color.g,sp.color.b,a); 

    }



    public void OnExit()
    {
        Player.transform.position = GameManager.Instance.GetLastSavePointPos();
        am.speed = 1;
        rb.gravityScale = 1;
        sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, 1);
    }

    public void OnUpdate()
    {
    }
}
