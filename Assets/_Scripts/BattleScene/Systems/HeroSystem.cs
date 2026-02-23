using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field: SerializeField] public HeroView HeroView { get; private set; }

    public void Setup(HeroData heroData)
    {
        if (HeroView == null)
        {
            HeroView = FindObjectOfType<HeroView>();
        }
        
        if (HeroView != null)
        {
            HeroView.Setup(heroData);
        }
        else
        {
            Debug.LogError("HeroView not found in scene!");
        }
    }

}
