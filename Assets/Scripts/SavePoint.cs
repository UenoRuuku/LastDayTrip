using UnityEngine;
using System.Collections;

public class SavePoint : MonoBehaviour
{
    [SerializeField]
    string Name;

    public string GetName() {
        return Name;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            GameManager.Instance.SetLastSavePoint(this);
        }
    }
}
