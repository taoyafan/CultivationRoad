using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTM : TargetMode
{
    public override List<Damageable> GetTargets(bool targetIsEnemy)
    {
        return null;
    }
}
