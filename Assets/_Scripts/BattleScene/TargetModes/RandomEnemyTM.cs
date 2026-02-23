using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEnemyTM : TargetMode
{
    public override List<Damageable> GetTargets(bool targetIsEnemy)
    {
        List<Damageable> allAttackable = new();
        List<CombatantView> targetViews;
        
        if (targetIsEnemy)
        {
            targetViews = new List<CombatantView>(EnemySystem.Instance.EnemyViews);
        }
        else
        {
            targetViews = new List<CombatantView>(){ HeroSystem.Instance.HeroView };
        }
        allAttackable.AddRange(targetViews);


        foreach (CombatantView enemyView in targetViews)
        {
            if (enemyView.CardsView != null)
            {
                allAttackable.AddRange(enemyView.CardsView.GetAllCardsAttackable());
            }
        }
        
        if (allAttackable.Count > 0)
        {
            Damageable randomTarget = allAttackable[Random.Range(0, allAttackable.Count)];
            return new() { randomTarget };
        }
        else
        {
            Debug.LogError("RandomEnemyTM: No attackable targets found");
            return new List<Damageable>();
        }
    }
}
