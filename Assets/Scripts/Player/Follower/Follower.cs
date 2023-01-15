using UnityEngine;
using System.Collections;

public class Follower : MonoBehaviour
{
    [SerializeField]
    NormalController nm;
    Vector3 ls;

    bool isRunning;
    bool isJumping;
    bool isFalling;
    bool isOnWall;
    Animator am;

    private void Start(){
        am = GetComponent<Animator>();
        ls = transform.localScale;
    }

    private void Update(){
        UpdateFollower();
        UpdateAnim();
    }

    void UpdateFollower(){
        if (nm.GetTimeBackStatus() != null)
        {
            transform.position = nm.GetTimeBackStatus().pos;
            transform.localScale = new Vector3(Mathf.Abs(ls.x) * nm.GetTimeBackStatus().scale, ls.y, ls.z);
            isRunning = nm.GetTimeBackStatus().isRunning;
            isJumping = nm.GetTimeBackStatus().isJumping;
            isFalling = nm.GetTimeBackStatus().isFalling;
            isOnWall = nm.GetTimeBackStatus().isOnWall;
        }
    }

    void UpdateAnim() {
        am.SetBool("isRunning", isRunning);
        am.SetBool("isJumping", isJumping);
        am.SetBool("isFalling", isFalling);
        am.SetBool("isOnWall", isOnWall);
    }
}
