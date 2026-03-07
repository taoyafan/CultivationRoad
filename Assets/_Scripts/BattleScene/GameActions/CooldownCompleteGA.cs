using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownCompleteGA : GameAction
{
    public Card Card { get; }

    public CooldownCompleteGA(Card card)
    {
        Card = card;
    }
}
