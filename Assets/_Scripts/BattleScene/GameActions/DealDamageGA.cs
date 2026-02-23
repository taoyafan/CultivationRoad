using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    // Single target damage action
    public Damageable Target { get; private set; }
    public CardView Caster { get; private set; }
    public int Amount { get; set; }

    // bookkeeping for reactions (original positions)
    public Vector3? ShieldOriginalPosition { get; set; }
    public CardView ShieldCaster { get; set; }

    public DealDamageGA(int amount, Damageable target, CardView caster)
    {
        Target = target;
        Amount = amount;
        Caster = caster;
    }

}
