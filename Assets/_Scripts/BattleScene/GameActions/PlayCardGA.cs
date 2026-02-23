using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardGA : GameAction
{
    public Damageable ManualTarget { get; private set; }
    public Card Card { get; set; }
    public PlayCardGA(Card card)
    {
        Card = card;
        ManualTarget = null;
    }

    public PlayCardGA(Card card, Damageable manualTarget)
    {
        Card = card;
        ManualTarget = manualTarget;
    }
}
