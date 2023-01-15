using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffector : MonoBehaviour
{
    Animator am;
    private void Start()
    {
        am = gameObject.GetComponent<Animator>();
    }

    public void Play(Vector3 pos) {
        transform.position = pos;
        am.Play("Base Layer.Idle",0,0);
    }

    public void Play() {
        am.Play("Base Layer.Idle", 0, 0);
    }
}
