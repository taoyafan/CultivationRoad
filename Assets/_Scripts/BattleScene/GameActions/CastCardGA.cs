using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastCardGA : GameAction
{
    public Card Card { get; set; }
    public CastCardGA(Card card)
    {
        Card = card;
    }
}
