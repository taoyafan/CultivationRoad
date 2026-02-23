using UnityEngine;

public class SpendManaGA : GameAction
{
    public int Amount { get; private set; }
    public CombatantView Target { get; private set; }
    public SpendManaGA(int amount)
    {
        Amount = amount;
    }
    public SpendManaGA(int amount, CombatantView target)
    {
        Amount = amount;
        Target = target;
    }
}