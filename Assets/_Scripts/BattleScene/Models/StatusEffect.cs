using System.Collections.Generic;

[System.Serializable]
public abstract class StatusEffect
{
    public virtual bool TargetIsEnemy => true;
    public int StackCount { get; protected set; } = 1;
    public CardView Caster { get; protected set; }
    public bool IsTiedToCasterLifecycle { get; set; } = true; // 是否与Caster生命周期一致
    
    public void SetCaster(CardView caster) 
    {
        Caster = caster;
    }

    public virtual void AddStack()
    {
        StackCount++;
    }
    
    public virtual void RemoveStack(int amount)
    {
        StackCount -= amount;
    }
    
    public virtual bool IsEmpty => StackCount <= 0;
}
