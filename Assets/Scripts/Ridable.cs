using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ridable : MonoBehaviour
{
    [SerializeField]
    public float RidableMovingAcceleration;
    [SerializeField]
    public Vector2 Direction;
    Rigidbody2D rb;
    [SerializeField]
    Transform target;
    [SerializeField]
    bool isMoving;
    public virtual void Start()
    {
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D Out))
        {
            rb = Out;
        }
        SetTarget();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (isMoving) {
            if (Vector2.Distance(target.position, transform.position) < 0.1f) {
                rb.velocity = Vector2.zero;
            }
        }
    }

    public virtual void SetTarget() {
        Direction = (target.position - transform.position);
        Direction.Normalize();
    }

    public virtual void SetTarget(Vector2 xy) {

    }

    public virtual void StartMove() {

    }

    public virtual Vector2 GetSpeed() {
        return rb.velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isMoving && (collision.CompareTag("Player") || collision.CompareTag("GroundCheck") || collision.CompareTag("WallCheck"))) {
            isMoving = true;
            SetTarget();
            rb.velocity = RidableMovingAcceleration * Direction;
        }
    }
}
