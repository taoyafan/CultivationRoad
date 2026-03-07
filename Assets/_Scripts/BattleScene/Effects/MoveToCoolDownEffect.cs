using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCoolDownEffect : Effect
{
    private float coolDownTime;
    public override float ActiveTime => 0;

    public MoveToCoolDownEffect(float coolDownTime)
    {
        this.coolDownTime = coolDownTime;
    }

    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new MoveToCoolDownGA(caster, coolDownTime);
    }
}