using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isOnGround {
        get;private set;
    }

    public bool isRiding {
        get; private set;
    }

    public Vector2 RidingSpeed {
        get; private set;
    }

    Ridable ridable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Walkable")) {
            isOnGround = true;
            if (collision.gameObject.TryGetComponent<Ridable>(out Ridable Out)) {
                isRiding = true;
                ridable = Out;
            }
        }
    }

    private void Update()
    {
        if (isRiding) {
            RidingSpeed = ridable.GetSpeed();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.CompareTag("Walkable"))
        {
            isOnGround = false;
            isRiding = false;
            RidingSpeed = Vector2.zero;
            ridable = null;
        }
    }
}
