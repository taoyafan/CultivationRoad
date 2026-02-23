using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroTM : TargetMode
{
    public override List<Damageable> GetTargets(bool targetIsEnemy)
    {
        return new List<Damageable>() { HeroSystem.Instance.HeroView };
    }
}
