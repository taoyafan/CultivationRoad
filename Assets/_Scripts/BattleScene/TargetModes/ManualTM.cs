using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTM : TargetMode
{
    private readonly Damageable target;

    public ManualTM(Damageable target)
    {
        this.target = target;
    }

    public override List<Damageable> GetTargets(bool targetIsEnemy)
    {
        return new List<Damageable>() { target };
    }
}
