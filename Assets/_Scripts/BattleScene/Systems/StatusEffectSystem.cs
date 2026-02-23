using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StatusEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffect);
        ActionSystem.SubscribeReaction<DealDamageGA>(ProtectBeforeDamage, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<DealDamageGA>(ProtectAfterDamage, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(ProtectBeforeDamage, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(ProtectAfterDamage, ReactionTiming.POST);
    }
    public IEnumerator AddStatusEffect(AddStatusEffectGA addStatusEffectGA)
    {
        // 安全检查
        if (addStatusEffectGA == null)
        {
            Debug.LogError("StatusEffectSystem.AddStatusEffect: addStatusEffectGA is null!");
            yield break;
        }
        
        if (addStatusEffectGA.Targets == null)
        {
            Debug.LogError("StatusEffectSystem.AddStatusEffect: Targets is null!");
            yield break;
        }
        
        if (addStatusEffectGA.StatusEffect == null)
        {
            Debug.LogError("StatusEffectSystem.AddStatusEffect: StatusEffect is null!");
            yield break;
        }
        
        foreach (Damageable targetCombatant in addStatusEffectGA.Targets)
        {
            if (targetCombatant == null)
            {
                Debug.LogWarning("StatusEffectSystem.AddStatusEffect: targetCombatant is null, skipping");
                continue;
            }
            
            // 添加状态效果到目标
            targetCombatant.AddStatusEffect(addStatusEffectGA.StatusEffect, addStatusEffectGA.Caster);
            
            // 如果状态效果与caster生命周期相关，将其添加到caster的tiedStatusEffects列表中
            if (addStatusEffectGA.StatusEffect.IsTiedToCasterLifecycle && addStatusEffectGA.Caster != null)
            {
                // 获取caster的Card对象
                Card casterCard = addStatusEffectGA.Caster.Card;
                if (casterCard != null)
                {
                    casterCard.AddTiedStatusEffect(addStatusEffectGA.StatusEffect, targetCombatant);
                }
                else
                {
                    Debug.LogWarning("AddStatusEffect: Caster card is null");
                }
            }
            else
            {
                if (!addStatusEffectGA.StatusEffect.IsTiedToCasterLifecycle)
                {
                }
                if (addStatusEffectGA.Caster == null)
                {
                    Debug.LogWarning("AddStatusEffect: Caster is null");
                }
            }
            
            yield return null;
        }
    }

    // PRE reaction for DealDamageGA: handle armor/shield movement and split damage
    public IEnumerator ProtectBeforeDamage(DealDamageGA dealDamageGA)
    {
        if (dealDamageGA == null || dealDamageGA.Target == null) yield break;
        Damageable target = dealDamageGA.Target;

        ArmorStatus armor = target.GetArmorStatus();
        var targetTransform = ((Component)target).transform;
        Vector3 targetPosition;

        if (MovementTracker.TryGetDestination(targetTransform, out var dest))
        {
            targetPosition = dest;
        }
        else
        {
            targetPosition = targetTransform.position;
        }

        if (dealDamageGA.Caster != null)
        {
            Vector3 flightDir = targetPosition - dealDamageGA.Caster.transform.position;
            if (flightDir.sqrMagnitude > 0.0001f)
            {
                targetPosition = targetPosition - flightDir.normalized * 1f;
            }
        }
        targetPosition.z -= 1f;

        if (armor != null && armor.AbsorptionRatio > 0f)
        {
            CardView shieldCaster = armor.Caster;
            if (shieldCaster != null && shieldCaster.gameObject != null)
            {
                dealDamageGA.ShieldCaster = shieldCaster;
                dealDamageGA.ShieldOriginalPosition = shieldCaster.transform.position;
                Tween t = MovementTracker.MoveAndTrack(shieldCaster.transform, targetPosition, 0.2f);

                // 计算原本要吸收的伤害量
                int absorbed = Mathf.RoundToInt(dealDamageGA.Amount * armor.AbsorptionRatio);
                // 如果要吸收的伤害量大于盾牌的当前血量，实际吸收的伤害量就是盾牌的当前血量
                absorbed = Mathf.Min(absorbed, shieldCaster.CurrentHealth);
                
                // 计算剩余伤害量
                int remain = dealDamageGA.Amount - absorbed;
                dealDamageGA.Amount = remain;

                // damage the shield (the caster of the armor) using a DealDamageGA
                DealDamageGA shieldDmg = new DealDamageGA(absorbed, shieldCaster, dealDamageGA.Caster);
                ActionSystem.Instance.AddReaction(shieldDmg);
            }
        }
        else
        {
            // no shield, move attacker to target
            Tween t = dealDamageGA.Caster.transform.DOMove(targetPosition, 0.2f);
            yield return t.WaitForCompletion();
        }
    }

    // POST reaction for DealDamageGA: return shield caster to original position if any
    public IEnumerator ProtectAfterDamage(DealDamageGA dealDamageGA)
    {
        if (dealDamageGA == null) yield break;
        if (dealDamageGA.ShieldCaster != null && dealDamageGA.ShieldOriginalPosition.HasValue)
        {
            dealDamageGA.ShieldCaster.transform.DOMove(dealDamageGA.ShieldOriginalPosition.Value, 0.2f);
        }
        yield return null;
    }

}
