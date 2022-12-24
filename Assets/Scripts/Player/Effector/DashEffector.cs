using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DashEffector : MonoBehaviour
{
    [SerializeField] private DashShadow prefab;
    [SerializeField] private int minCapcitySize = 10;//这两个变量用来定义对象池也就是栈的存储空间
    [SerializeField] private int maxCapcitySize = 100;
    [SerializeField] private int activeCount => pool.CountActive;
    [SerializeField] private int inacitveCount => pool.CountInactive;
    [SerializeField] private int totalCount => pool.CountAll;
    ObjectPool<DashShadow> pool;
    //对象池仅仅发生激活和非激活状态之间的切换，只有调用ObjectPool.Clear()或者Dispose()才会清除对象池中的元素
    private void Awake()
    {
        pool = new ObjectPool<DashShadow>(OnCreatePoolItem, OnGetPoolItem, OnReleasePoolItem, OnDestoryPoolItem, true, minCapcitySize, maxCapcitySize);
    }


    public void GenerateEffect(Transform t, float time,Sprite s)
    {
        StartCoroutine(GeneratingShadow(t, time,s));
    }

    IEnumerator GeneratingShadow(Transform t, float time,Sprite s) {
        float interval = time / 4;
        for (int i = 0; i < 3; i++) {
            var sh = pool.Get();
            sh.Play(interval * 8, t,s);
            yield return new WaitForSeconds(interval);
        }
        yield return null;
    }

    private void OnDestoryPoolItem(DashShadow obj)
    {
        Destroy(obj.gameObject);
    }

    private void OnReleasePoolItem(DashShadow obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void OnGetPoolItem(DashShadow obj)
    {
        obj.gameObject.SetActive(true);
    }

    private DashShadow OnCreatePoolItem()
    {
        var dashShadow = Instantiate(prefab);
        dashShadow.SetDeactiveAction(delegate { pool.Release(dashShadow); }); //在实例化宝石后再调用release函数回收这个宝石

        return dashShadow;
    }
}
