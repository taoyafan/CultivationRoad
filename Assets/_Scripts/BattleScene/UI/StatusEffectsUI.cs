using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite;
    private Dictionary<Type, StatusEffectUI> statusEffectUIs = new();
    
    public void UpdateStatusEffectUI(Type effectType, int stackCount)
    {
        
        if (stackCount <= 0)
        {
            if (statusEffectUIs.TryGetValue(effectType, out StatusEffectUI statusEffectUI))
            {
                statusEffectUIs.Remove(effectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (!statusEffectUIs.TryGetValue(effectType, out StatusEffectUI statusEffectUI))
            {
                if (statusEffectUIPrefab == null)
                {
                    Debug.LogError("StatusEffectsUI.UpdateStatusEffectUI: statusEffectUIPrefab is null!");
                    return;
                }
                statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                statusEffectUIs[effectType] = statusEffectUI;
            }
            
            Sprite sprite = effectType == typeof(ArmorStatus) ? armorSprite : 
                           effectType == typeof(BurnStatus) ? burnSprite : null;
            
            if (sprite == null)
            {
                Debug.LogWarning($"StatusEffectsUI.UpdateStatusEffectUI: No sprite found for {effectType.Name}");
            }
            statusEffectUI.Set(sprite);
        }
    }
}


