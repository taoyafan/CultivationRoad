using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawReason
{
    Initial,
    CardEffect,
    Timed
}

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }
    public DrawReason Reason { get; set; }

    public DrawCardsGA(int amount, DrawReason reason = DrawReason.Initial)
    {
        Amount = amount;
        Reason = reason;
    }
}
