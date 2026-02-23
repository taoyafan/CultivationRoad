using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardsEffect : Effect
{
    [SerializeField] private int _drawCount;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        return new DrawCardsGA(_drawCount);
    }
}
