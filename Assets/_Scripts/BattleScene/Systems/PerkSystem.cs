using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;
    private readonly List<Perk> perks = new();
    public void AddPerk(Perk perk)
    {
        perk.OnAdd();
        perksUI.AddPerkUI(perk);
        perks.Add(perk);
    }
    public void RemovePerk(Perk perk)
    {
        perks.Remove(perk);
        perksUI.RemovePerkUI(perk);
        perk.OnRemove();
    }
}
