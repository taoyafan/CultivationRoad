using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageAndCoolDownEffect : Effect
{
    [SerializeField] private int damageAmount;
    [SerializeField] private float activeTime;
    [SerializeField] private float coolDownTime;
    public override float ActiveTime => activeTime;
    public float CoolDownTime => coolDownTime;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new AttackGA(damageAmount, targets, caster);
    }
    public override Effect GetNextEffect()
    {
        return new MoveToCoolDownEffect(coolDownTime);
    }

    public override TargetMode GetNextEffectTM()
    {
        return new NoTM();
    }
}