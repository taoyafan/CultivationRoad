using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[CreateAssetMenu(menuName = "Data/Perk")]
public class PerkData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public bool AddActionCasterToTarget { get; private set; } = false;

    [field: SerializeReference, SR] public PerkCondition PerkCondition { get; private set; }
    [field: SerializeReference, SR] public TargetMode TargetMode { get; private set; }
    [field: SerializeReference, SR] public Effect Effect { get; private set; }

}
