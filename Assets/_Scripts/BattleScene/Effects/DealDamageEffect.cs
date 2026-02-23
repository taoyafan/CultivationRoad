using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageEffect : Effect
{
    [SerializeField] private int damageAmount;

    [SerializeField] private float activeTime;
    public override float ActiveTime => activeTime;

    public DealDamageEffect() {}
    public DealDamageEffect(int damageAmount, float activeTime)
    {
        this.damageAmount = damageAmount;
        this.activeTime = activeTime;
    }

    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new AttackGA(damageAmount, targets, caster);
    }

    public override Effect GetNextEffect()
    {
        return new DealDamageEffect(damageAmount, activeTime);
    }
}
