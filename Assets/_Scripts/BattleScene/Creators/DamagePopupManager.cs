using System.Collections.Generic;
using UnityEngine;

public class DamagePopupManager : Singleton<DamagePopupManager>
{
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private int initialPool = 10;
    private Transform poolParent;

    private Queue<DamagePopup> pool = new();

    protected override void Awake()
    {
        base.Awake();
        if (poolParent == null) poolParent = this.transform;
        
        if (popupPrefab != null)
        {
            for (int i = 0; i < initialPool; i++) CreateOneToPool();
        }
    }

    private DamagePopup CreateOneToPool()
    {
        var go = Instantiate(popupPrefab, poolParent);
        go.gameObject.SetActive(false);
        pool.Enqueue(go);
        return go;
    }

    private DamagePopup GetFromPool()
    {
        if (pool.Count == 0) CreateOneToPool();
        var p = pool.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void ReturnToPool(DamagePopup p)
    {
        p.gameObject.SetActive(false);
        p.transform.SetParent(poolParent, true);
        pool.Enqueue(p);
    }

    // worldPos: 世界坐标（例如 target.transform.position + Vector3.up * 1f）
    // followTarget: 要跟随的目标
    public void ShowDamage(Vector3 worldPos, int amount, Color? color = null, float scale = 1f, Transform followTarget = null)
    {
        var popup = GetFromPool();
        
        // 不设置父对象，避免缩放影响
        popup.transform.SetParent(null, true);
        popup.transform.position = worldPos;
        popup.transform.localScale = Vector3.one * scale;
        
        // 直接传递followTarget给Show方法
        popup.Show(amount, color, 1f, followTarget);
    }
}