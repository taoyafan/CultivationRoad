using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class DestroySelfEffect : Effect
{
    public override float ActiveTime => 0;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new DestroyCardViewGA(caster);
    }
}