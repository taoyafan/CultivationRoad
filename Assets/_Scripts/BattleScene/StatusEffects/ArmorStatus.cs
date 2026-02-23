using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorStatus : StatusEffect
{
    [Range(0f, 1f)]
    [SerializeField] private float absorptionRatio = 0.8f;
    public override bool TargetIsEnemy => false;
    
    public float AbsorptionRatio => absorptionRatio;
    
    public void SetAbsorptionRatio(float ratio)
    {
        absorptionRatio = ratio;
    }
}
