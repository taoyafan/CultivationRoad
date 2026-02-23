using System.Collections.Generic;

[System.Serializable]
public abstract class Effect
{
    public virtual float ActiveTime => 0f;
    public virtual bool TargetIsEnemy => true;
    public abstract GameAction GetGameAction(List<Damageable> targets, CardView caster);
    public virtual Effect GetNextEffect()
    {
        return null;
    }
    public virtual TargetMode GetNextEffectTM()
    {
        return null;
    }
}
