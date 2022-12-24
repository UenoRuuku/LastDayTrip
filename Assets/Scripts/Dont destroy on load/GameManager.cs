using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    SavePoint LastSavePoint;

    public SavePoint GetLastSavePoint()
    {
        if (LastSavePoint == null)
        {
            LastSavePoint = LevelManager.Instance.GetDefaultSavePoint();
        }
        return LastSavePoint;
    }
    public Vector3 GetLastSavePointPos()
    {
        if (LastSavePoint == null)
        {
            LastSavePoint = LevelManager.Instance.GetDefaultSavePoint();
        }
        return LastSavePoint.transform.position;
    }

    public SavePoint SetLastSavePoint(SavePoint s) {
        return LastSavePoint = s;
    }
}
