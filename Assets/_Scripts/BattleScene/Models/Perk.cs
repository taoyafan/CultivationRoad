using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Perk
{
    public Sprite Image => data.Image;
    private readonly PerkData data;
    private readonly PerkCondition condition;
    public readonly Effect effect;
    public Perk(PerkData data)
    {
        this.data = data;
        condition = data.PerkCondition;
        effect = data.Effect;
    }

    public void OnAdd()
    {
        condition.SubscribeCondition(Reaction);
    }

    public void OnRemove()
    {
        condition.UnsubscribeCondition(Reaction);
    }
    private IEnumerator Reaction(GameAction gameAction)
    {
        // if (condition.SubConditionIsMet(gameAction))
        // {
        //     List<CombatantView> targets = new();
        //     if (data.AddActionCasterToTarget && gameAction is IHaveCaster caster)
        //     {
        //         targets.Add(caster.Caster); 
        //     }
        //     targets.AddRange(data.TargetMode?.GetTargets() ?? Enumerable.Empty<CombatantView>());
        //     GameAction perkEffectAction = effect.GetGameAction(targets, HeroSystem.Instance.HeroView);
        //     ActionSystem.Instance.AddReaction(perkEffectAction);
        // }
        yield break;
    }
}
