using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public List<CardData> Deck { get; private set; }
    [field: SerializeReference, SR] public IEnemyStrategy Strategy { get; private set; }
}
