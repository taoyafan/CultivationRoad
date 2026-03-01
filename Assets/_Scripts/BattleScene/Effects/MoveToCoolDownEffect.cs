using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCoolDownEffect : Effect
{
    public override float ActiveTime => 0;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new MoveToCoolDownGA(caster);
    }
}