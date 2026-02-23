using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectGA : GameAction
{
    public StatusEffect StatusEffect { get; private set; }
    public CardView Caster { get; private set; }
    public List<Damageable> Targets { get; private set; }
    public AddStatusEffectGA(StatusEffect statusEffect, List<Damageable> targets, CardView caster)
    {
        StatusEffect = statusEffect;
        Targets = targets;
        Caster = caster;
    }
}
