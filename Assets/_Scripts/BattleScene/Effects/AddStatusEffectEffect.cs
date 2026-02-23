using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SerializeReferenceEditor;

public class AddStatusEffectEffect : Effect
{
    [field: SerializeReference, SR] private StatusEffect statusEffect;
    public override bool TargetIsEnemy => statusEffect.TargetIsEnemy;
    public override GameAction GetGameAction(List<Damageable> targets, CardView caster)
    {
        // 为每个目标创建新的状态效果实例，避免共享实例导致的堆叠问题
        StatusEffect newStatusEffect = CreateNewStatusEffectInstance();
        newStatusEffect.SetCaster(caster);
        return new AddStatusEffectGA(newStatusEffect, targets, caster);
    }
    
    /// <summary>
    /// 创建新的状态效果实例
    /// </summary>
    private StatusEffect CreateNewStatusEffectInstance()
    {
        
        // 这里需要根据具体的状态效果类型创建新实例
        // 由于statusEffect是SerializeReference，我们需要使用序列化复制
        if (statusEffect is ArmorStatus armorStatus)
        {
            ArmorStatus newArmorStatus = new ArmorStatus();
            newArmorStatus.SetAbsorptionRatio(armorStatus.AbsorptionRatio);
            return newArmorStatus;
        }
        
        // 默认返回原始实例（作为临时解决方案）
        Debug.LogWarning("AddStatusEffectEffect: Unable to create new instance, using original");
        return statusEffect;
    }

    public StatusEffect GetStatusEffect()
    {
        return statusEffect;
    }
}
