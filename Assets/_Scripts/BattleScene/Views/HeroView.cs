using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroView : CombatantView
{
    public void Setup(HeroData heroData)
    {
        IsEnemy = false;
        if (heroData == null)
        {
            SetupBase(0, null);
        }
        else
        {
            SetupBase(heroData.Health, heroData.Image);
        }
    }
}
