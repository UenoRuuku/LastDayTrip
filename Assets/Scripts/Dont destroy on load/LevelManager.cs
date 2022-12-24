using UnityEngine;
using System.Collections;

//每个场景都需要配备LevelManager
//LevelManager 的功能
//配置当前关卡默认存档点
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        GameManager.Instance.SetLastSavePoint(DefaultSavePoint);
    }
    [SerializeField]
    SavePoint DefaultSavePoint;

    public SavePoint GetDefaultSavePoint() {
        return DefaultSavePoint;
    }
}
