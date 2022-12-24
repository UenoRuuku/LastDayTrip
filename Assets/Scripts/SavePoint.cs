using UnityEngine;
using System.Collections;

public class SavePoint : MonoBehaviour
{
    [SerializeField]
    string Name;

    public string GetName() {
        return Name;
    }
}
