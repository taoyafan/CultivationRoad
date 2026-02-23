using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCardGA : GameAction
{
    public EnemyView ManualTarget { get; private set; }
    public Card Card { get; set; }
    public ActiveCardGA(Card card)
    {
        Card = card;
        ManualTarget = null;
    }

    public ActiveCardGA(Card card, EnemyView manualTarget)
    {
        Card = card;
        ManualTarget = manualTarget;
    }
}
