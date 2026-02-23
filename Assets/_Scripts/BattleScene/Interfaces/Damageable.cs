using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

public abstract class Damageable : MonoBehaviour
{
    public int CurrentHealth { get; protected set; }
    public int MaxHealth { get; protected set; }
    public bool IsEnemy { get; set; }
    public SpriteRenderer spritesRender;
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    private HashSet<StatusEffect> statusEffects = new();

    protected abstract void UpdateHealth();

    public void Damage(int amount)
    {
        try
        {
            if (gameObject == null)
            {
                return;
            }
            
            CurrentHealth -= amount;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
            }
            transform.DOShakePosition(0.2f, 0.5f);
            UpdateHealth();
        }
        catch (MissingReferenceException)
        {
        }
    }

    public ArmorStatus GetArmorStatus()
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect is ArmorStatus armor)
            {
                return armor;
            }
        }
        return null;
    }

    public void AddStatusEffect(StatusEffect statusEffect, CardView caster)
    {
        StatusEffect existingEffect = null;
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == statusEffect.GetType())
            {
                existingEffect = effect;
                break;
            }
        }
        
        if (existingEffect != null)
        {
            existingEffect.AddStack();
        }
        else
        {
            statusEffects.Add(statusEffect);
        }
        
        int stackCount = GetStatusEffectStackCount(statusEffect.GetType());
        
        
        if (statusEffectsUI == null)
        {
            Debug.LogError($"Damageable.AddStatusEffect: statusEffectsUI is null on {name}!");
            return;
        }
        
        statusEffectsUI.UpdateStatusEffectUI(statusEffect.GetType(), stackCount);
        
    }

    public void RemoveStatusEffect(Type effectType, int stackCount)
    {
        
        // 使用列表副本进行遍历，避免修改集合时的错误
        StatusEffect foundEffect = null;
        foreach (StatusEffect effect in new List<StatusEffect>(statusEffects))
        {
            if (effect.GetType() == effectType)
            {
                foundEffect = effect;
                break;
            }
        }
        
        if (foundEffect != null)
        {
            foundEffect.RemoveStack(stackCount);
                        
            if (foundEffect.IsEmpty)
            {
                statusEffects.Remove(foundEffect);
            }
        }
        else
        {
            Debug.LogWarning($"RemoveStatusEffect: Could not find effect of type {effectType.Name}");
        }
        
        int remainingStacks = GetStatusEffectStackCount(effectType);
        statusEffectsUI.UpdateStatusEffectUI(effectType, remainingStacks);
    }

    public int GetStatusEffectStackCount(Type effectType)
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == effectType)
            {
                return effect.StackCount;
            }
        }
        return 0;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}