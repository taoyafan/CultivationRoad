using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;

    public void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackGA>(AttackPerform);
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerform);

        ActionSystem.SubscribeReaction<AttackGA>(AttackPostReaction, ReactionTiming.POST);
    }

    public void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackGA>();
        ActionSystem.DetachPerformer<DealDamageGA>();

        ActionSystem.UnsubscribeReaction<AttackGA>(AttackPostReaction, ReactionTiming.POST);
    }

    // Performers
    private IEnumerator DealDamagePerform(DealDamageGA dealDamageGA)
    {
        try
        {
            Damageable target = dealDamageGA.Target;
            if (target is Component component)
            {
                if (component == null || component.gameObject == null)
                {
                    yield break;
                }

                target.Damage(dealDamageGA.Amount);
                Instantiate(damageVFX, component.transform.position, Quaternion.identity);
                DamagePopupManager.Instance.ShowDamage(component.transform.position, -dealDamageGA.Amount, null, 1, component.transform);

                // Handle death logic
                if (target.CurrentHealth <= 0)
                {
                    if (target is EnemyView enemyView)
                    {
                        // 检查dealDamageGA.Caster是否已经被销毁
                        if (dealDamageGA.Caster != null && dealDamageGA.Caster.gameObject != null)
                        {
                            KillEnemyGA killEnemyGA = new (enemyView, dealDamageGA.Caster);
                            ActionSystem.Instance.AddReaction(killEnemyGA);
                        }
                        else
                        {
                            // 如果caster已经被销毁，使用null创建KillEnemyGA
                            KillEnemyGA killEnemyGA = new (enemyView, null);
                            ActionSystem.Instance.AddReaction(killEnemyGA);
                        }
                    }
                    else if (target is HeroView)
                    {
                        TimeSystem.Instance.AddAction(new LoseGameGA(), 0f);
                    }
                    else if (target is CardView cardView)
                    {
                        DestroyCardViewGA destroyCardViewGA = new (cardView);
                        ActionSystem.Instance.AddReaction(destroyCardViewGA);
                    }
                }
            }
        }
        catch (MissingReferenceException)
        {
        }
        yield return null;
    }

    private IEnumerator AttackPerform(AttackGA attackGA)
    {
        if (attackGA.Caster != null && attackGA.Caster.gameObject != null)
        {
            attackGA.OriginalPosition = attackGA.Caster.transform.position;
        }
        foreach (Damageable target in attackGA.Targets)
        {
            DealDamageGA deal = new DealDamageGA(attackGA.Amount, target, attackGA.Caster);
            ActionSystem.Instance.AddReaction(deal);
        }
        yield return null;
    }

    // POST reaction for AttackGA: return attacker to original position
    public IEnumerator AttackPostReaction(AttackGA attackGA)
    {
        if (attackGA == null) yield break;
        if (attackGA.Caster == null) yield break;
        if (attackGA.Caster.gameObject == null) yield break;
        if (!attackGA.Caster.gameObject.activeInHierarchy) yield break;
        
        // 检查OriginalPosition是否有效（不是默认的Vector3.zero）
        if (attackGA.OriginalPosition == Vector3.zero)
        {
            // 如果OriginalPosition无效，使用当前位置
            yield break;
        }
        
        Tween t = MovementTracker.MoveAndTrack(attackGA.Caster.transform, attackGA.OriginalPosition, 0.2f);
        yield return t.WaitForCompletion();
    }
}
