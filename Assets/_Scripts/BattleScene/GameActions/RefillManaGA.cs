using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefillManaGA : GameAction
{
    public int Amount { get; private set; }
    public RefillManaGA(int amount)
    {
        Amount = amount;
    }
}
