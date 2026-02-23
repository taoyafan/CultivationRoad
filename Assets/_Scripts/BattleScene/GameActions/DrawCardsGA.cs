using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardsGA : GameAction
{
    // Store amount
    public int Amount { get; set; }
    // Constructor
    public DrawCardsGA(int amount)
    {
        Amount = amount;
    }
}
