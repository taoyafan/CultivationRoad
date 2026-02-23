using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();

    public EnemyView AddEnemy(EnemyData enemyData)
    {
        Transform slot = slots[EnemyViews.Count];
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        enemyView.transform.SetParent(slot);
        EnemyViews.Add(enemyView);
        return enemyView;
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        Destroy(enemyView.gameObject);
        yield return null;
    }

    public void ClearAllEnemies()
    {
        foreach (var enemyView in EnemyViews)
        {
            if (enemyView != null)
            {
                // 清空敌人释放的卡牌
                if (enemyView.Cards != null)
                {
                    enemyView.Cards.Clear();
                }
                Destroy(enemyView.gameObject);
            }
        }
        EnemyViews.Clear();
    }
}
