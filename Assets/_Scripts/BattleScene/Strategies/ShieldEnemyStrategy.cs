using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class ShieldEnemyStrategy : IEnemyStrategy
{
    /// <summary>
    /// 选择要释放的卡牌和目标
    /// </summary>
    /// <param name="enemyView">敌人视图</param>
    /// <param name="enemyCards">敌人手牌</param>
    /// <param name="playerCards">玩家场上的卡牌</param>
    /// <returns>选中的卡牌索引和目标，如果没有合适的卡牌返回null</returns>
    public CardReleaseDecision SelectCardToRelease(EnemyView enemyView, List<Card> enemyCards, List<CardView> playerCards)
    {
        Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 开始选择卡牌，敌人: {enemyView?.name}, 手牌数量: {enemyCards?.Count}");
        
        if (enemyCards == null || enemyCards.Count == 0)
        {
            Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 没有手牌，不释放");
            return CardReleaseDecision.DoNotRelease();
        }
        
        // 检查场上是否已经有护盾卡牌
        bool hasShieldCardOnField = HasShieldCardOnField(enemyView);
        Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 场上已有护盾卡牌: {hasShieldCardOnField}");
        
        // 场上卡牌数量
        int fieldCardCount = enemyView.CardsView?.GetAllCards()?.Count ?? 0;
        Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 场上卡牌数量: {fieldCardCount}");
        
        // 策略优先级：
        // 1. 如果场上没有护盾卡牌，优先释放护盾卡牌
        // 2. 如果场上卡牌数量不足3个，优先释放持续伤害卡牌
        // 3. 最后释放其他非护盾卡牌
        
        // 第一优先级：护盾卡牌（如果场上没有护盾卡牌）
        if (!hasShieldCardOnField)
        {
            Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 尝试查找护盾卡牌");
            int shieldCardIndex = FindShieldCard(enemyCards, enemyView);
            if (shieldCardIndex >= 0)
            {
                Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 找到护盾卡牌，索引: {shieldCardIndex}");
                Damageable manualTarget = SelectManualTarget(enemyCards[shieldCardIndex], enemyView, playerCards);
                return new CardReleaseDecision(shieldCardIndex, manualTarget);
            }
            else
            {
                Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 没有找到护盾卡牌");
            }
        }
        
        // 第二优先级：持续伤害卡牌（如果场上卡牌数量不足3个）
        if (fieldCardCount < 3)
        {
            Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 尝试查找持续伤害卡牌");
            int damageOverTimeCardIndex = FindDamageOverTimeCard(enemyCards, enemyView);
            if (damageOverTimeCardIndex >= 0)
            {
                Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 找到持续伤害卡牌，索引: {damageOverTimeCardIndex}");
                Damageable manualTarget = SelectManualTarget(enemyCards[damageOverTimeCardIndex], enemyView, playerCards);
                return new CardReleaseDecision(damageOverTimeCardIndex, manualTarget);
            }
            else
            {
                Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 没有找到持续伤害卡牌");
            }
        }
        
        // 第三优先级：其他非护盾卡牌
        Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 尝试查找其他非护盾卡牌");
        int otherCardIndex = FindOtherCard(enemyCards, enemyView);
        if (otherCardIndex >= 0)
        {
            Debug.Log($"ShieldEnemyStrategy.SelectCardToRelease: 找到其他卡牌，索引: {otherCardIndex}");
            Damageable manualTarget = SelectManualTarget(enemyCards[otherCardIndex], enemyView, playerCards);
                return new CardReleaseDecision(otherCardIndex, manualTarget);
        }
        
        Debug.Log("ShieldEnemyStrategy.SelectCardToRelease: 没有找到任何合适的卡牌，不释放");
        return CardReleaseDecision.DoNotRelease();
    }
    
    /// <summary>
    /// 选择手动释放目标
    /// </summary>
    private Damageable SelectManualTarget(Card card, EnemyView enemyView, List<CardView> playerCards)
    {
        // 如果卡牌需要手动目标，根据卡牌类型选择目标
        if (card.ManualTargetEffect != null)
        {
            Debug.Log($"SelectManualTarget: 卡牌 {card.Title} 需要手动目标，ManualTargetEffect类型: {card.ManualTargetEffect.GetType()}");
            
            // 攻击卡牌：目标为玩家本体
            if (IsAttackCard(card))
            {
                // 查找玩家本体（HeroView）
                HeroView heroView = FindHeroView();
                Debug.Log($"SelectManualTarget: 攻击卡牌 {card.Title}，目标为玩家本体: {heroView?.name}");
                return heroView;
            }
            // 护盾卡牌：目标为敌人本体
            else if (IsShieldCard(card))
            {
                Debug.Log($"SelectManualTarget: 护盾卡牌 {card.Title}，目标为敌人本体: {enemyView?.name}");
                return enemyView;
            }
            else
            {
                Debug.Log($"SelectManualTarget: 卡牌 {card.Title} 既不是攻击卡牌也不是护盾卡牌");
            }
        }
        else
        {
            Debug.Log($"SelectManualTarget: 卡牌 {card.Title} 不需要手动目标");
        }
        
        return null;
    }
    
    /// <summary>
    /// 判断是否为攻击卡牌
    /// </summary>
    private bool IsAttackCard(Card card)
    {
        return card.ManualTargetEffect is DealDamageEffect || 
               card.ManualTargetEffect is DealOnceDamageEffect;
    }
    
    /// <summary>
    /// 查找玩家本体
    /// </summary>
    private HeroView FindHeroView()
    {
        // 通过HeroSystem获取玩家本体
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null)
        {
            return HeroSystem.Instance.HeroView;
        }
        
        // 备用方案：在场景中查找HeroView组件
        HeroView heroView = GameObject.FindObjectOfType<HeroView>();
        if (heroView != null && !heroView.IsEnemy)
        {
            return heroView;
        }
        
        Debug.LogWarning("FindHeroView: 无法找到玩家本体");
        return null;
    }
    
    /// <summary>
    /// 检查敌人是否有护盾状态
    /// </summary>
    private bool HasArmorStatus(EnemyView enemyView)
    {
        return enemyView.GetStatusEffectStackCount(typeof(ArmorStatus)) > 0;
    }
    
    /// <summary>
    /// 检查场上是否已经有护盾卡牌
    /// </summary>
    private bool HasShieldCardOnField(EnemyView enemyView)
    {
        if (enemyView.CardsView == null)
        {
            return false;
        }
        
        // 获取场上所有卡牌
        List<CardView> fieldCards = enemyView.CardsView.GetAllCards();
        
        // 检查每张卡牌是否为护盾卡牌
        foreach (CardView cardView in fieldCards)
        {
            if (cardView.Card != null && IsShieldCard(cardView.Card))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 查找护盾卡牌（AddStatusEffectEffect且StatusEffect为ArmorStatus）
    /// </summary>
    private int FindShieldCard(List<Card> enemyCards, EnemyView enemyView)
    {
        for (int i = 0; i < enemyCards.Count; i++)
        {
            Card card = enemyCards[i];
            if (ManaSystem.Instance.HasEnoughMana(enemyView, card.Mana) && IsShieldCard(card))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 查找持续伤害卡牌（DealDamageEffect）
    /// </summary>
    private int FindDamageOverTimeCard(List<Card> enemyCards, EnemyView enemyView)
    {
        for (int i = 0; i < enemyCards.Count; i++)
        {
            Card card = enemyCards[i];
            if (ManaSystem.Instance.HasEnoughMana(enemyView, card.Mana) && IsDamageOverTimeCard(card))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 查找其他非护盾卡牌
    /// </summary>
    private int FindOtherCard(List<Card> enemyCards, EnemyView enemyView)
    {
        for (int i = 0; i < enemyCards.Count; i++)
        {
            Card card = enemyCards[i];
            if (ManaSystem.Instance.HasEnoughMana(enemyView, card.Mana) && !IsShieldCard(card))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 判断是否为护盾卡牌
    /// </summary>
    private bool IsShieldCard(Card card)
    {
        // 检查手动目标效果是否为AddStatusEffectEffect且StatusEffect为ArmorStatus
        if (card.ManualTargetEffect is AddStatusEffectEffect addStatusEffect)
        {
            if (addStatusEffect.GetStatusEffect() is ArmorStatus)
            {
                Debug.Log($"IsShieldCard: 卡牌 {card.Title} 是护盾卡牌（手动目标效果）");
                return true;
            }
            else
            {
                Debug.Log($"IsShieldCard: 卡牌 {card.Title} 有AddStatusEffectEffect但不是护盾，StatusEffect类型: {addStatusEffect.GetStatusEffect()?.GetType()}");
            }
        }
        
        // 检查其他效果中是否有AddStatusEffectEffect且StatusEffect为ArmorStatus
        foreach (var effect in card.OtherEffects)
        {
            if (effect.Effect is AddStatusEffectEffect otherAddStatusEffect)
            {
                if (otherAddStatusEffect.GetStatusEffect() is ArmorStatus)
                {
                    Debug.Log($"IsShieldCard: 卡牌 {card.Title} 是护盾卡牌（其他效果）");
                    return true;
                }
                else
                {
                    Debug.Log($"IsShieldCard: 卡牌 {card.Title} 有AddStatusEffectEffect但不是护盾，StatusEffect类型: {otherAddStatusEffect.GetStatusEffect()?.GetType()}");
                }
            }
        }
        
        Debug.Log($"IsShieldCard: 卡牌 {card.Title} 不是护盾卡牌");
        return false;
    }
    
    /// <summary>
    /// 判断是否为持续伤害卡牌
    /// </summary>
    private bool IsDamageOverTimeCard(Card card)
    {
        // 检查手动目标效果是否为DealDamageEffect
        if (card.ManualTargetEffect is DealDamageEffect)
        {
            return true;
        }
        
        // 检查其他效果
        foreach (var effect in card.OtherEffects)
        {
            if (effect.Effect is DealDamageEffect)
            {
                return true;
            }
        }
        
        return false;
    }
}