using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DashShadow : MonoBehaviour
{
    System.Action<DashShadow> deactiveAcion;
    SpriteRenderer sp;
    Color32 c;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(float time, Transform pos,Sprite s)
    {

        sp = GetComponent<SpriteRenderer>();
        c = sp.color;
        transform.position = pos.position;
        transform.localScale = pos.localScale;
        sp.sprite = s;
        Tweener t = DOVirtual.Int(100, 0, time, SetOpac).OnComplete(
            ()=>
            {
                deactiveAcion.Invoke(this);
        });
    }

    void SetOpac(int a) {
        sp.color = new Color32(c.r, c.g, c.b, (byte)a);
    }

    public void SetDeactiveAction(System.Action<DashShadow> deactiveAcion)
    {
        this.deactiveAcion = deactiveAcion;
    }
}
