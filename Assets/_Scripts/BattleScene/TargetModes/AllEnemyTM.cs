using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllEnemyTM : TargetMode
{
    public override List<Damageable> GetTargets(bool targetIsEnemy)
    {
        List<CombatantView> targetViews;
        List<Damageable> targets = new();

        if (targetIsEnemy)
        {
            targetViews = new List<CombatantView>(EnemySystem.Instance.EnemyViews);
        }
        else
        {
            targetViews = new List<CombatantView>(){ HeroSystem.Instance.HeroView };
        }
        
        targets.AddRange(targetViews);
        foreach (CombatantView enemyView in targetViews)
        {
            if (enemyView.CardsView != null)
            {
                targets.AddRange(enemyView.CardsView.GetAllCardsAttackable());
            }
        }

        return targets;
    }
}
