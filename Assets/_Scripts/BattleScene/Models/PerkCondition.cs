using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PerkCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract void SubscribeCondition(Func<GameAction, IEnumerator> reaction);
    public abstract void UnsubscribeCondition(Func<GameAction, IEnumerator> reaction);
    public abstract bool SubConditionIsMet(GameAction gameAction);
}
