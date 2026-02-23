using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackGA : GameAction, IHaveCaster
{
    public List<Damageable> Targets { get; private set; }
    public CardView Caster { get; private set; }
    public int Amount { get; private set; }
    public Vector3 OriginalPosition { get; set; }

    public AttackGA(int amount, List<Damageable> targets, CardView caster)
    {
        Targets = targets;
        Amount = amount;
        Caster = caster;
    }
}

