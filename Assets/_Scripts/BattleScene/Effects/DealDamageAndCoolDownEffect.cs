using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageAndCoolDownEffect : Effect
{
    [SerializeField] private int damageAmount;
    [SerializeField] private float activeTime;
    public override float ActiveTime => activeTime;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new AttackGA(damageAmount, targets, caster);
    }
    public override Effect GetNextEffect()
    {
        return new MoveToCoolDownEffect();
    }

    public override TargetMode GetNextEffectTM()
    {
        return new NoTM();
    }
}