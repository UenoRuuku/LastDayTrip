using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCodeManager : Singleton<KeyCodeManager>
{
    [SerializeField]
    public KeyCode Forward;
    [SerializeField]
    public KeyCode Backward;
    [SerializeField]
    public KeyCode Up;
    [SerializeField]
    public KeyCode Down;
    [SerializeField]
    public KeyCode Dash;
    [SerializeField]
    public KeyCode Special;
    [SerializeField]
    public KeyCode Jump = KeyCode.Space;
}
