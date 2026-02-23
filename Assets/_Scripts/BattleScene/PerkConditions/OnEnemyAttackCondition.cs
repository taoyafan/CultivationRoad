using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnemyAttackCondition : PerkCondition
{
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        // Can check attacker
        return true;
    }

    public override void SubscribeCondition(Func<GameAction, IEnumerator> reaction)
    {
        ActionSystem.SubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Func<GameAction, IEnumerator> reaction)
    {
        ActionSystem.UnsubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }
}
