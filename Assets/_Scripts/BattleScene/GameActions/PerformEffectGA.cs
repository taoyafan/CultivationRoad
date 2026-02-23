using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public AutoTargetEffect Effect { get; set; }
    public CardView Caster { get; set; }
    public PerformEffectGA(AutoTargetEffect effect, CardView caster)
    {
        Effect = effect;
        Caster = caster;
    }
}
