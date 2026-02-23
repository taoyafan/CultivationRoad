using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSuccessGA : GameAction
{
    public Card Card { get; private set; }

    public CastSuccessGA(Card card)
    {
        Card = card;
    }
}
