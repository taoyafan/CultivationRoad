using System.Collections.Generic;
using UnityEngine;

public class DefaultEnemyStrategy : IEnemyStrategy
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
        // 基本策略：选择第一张灵力足够的卡牌
        if (enemyCards == null || enemyCards.Count == 0)
        {
            return CardReleaseDecision.DoNotRelease();
        }
        
        // 检查第一张卡牌的灵力是否足够
        if (ManaSystem.Instance.HasEnoughMana(enemyView, enemyCards[0].Mana))
        {
            // 默认策略：攻击卡牌目标为玩家本体，护盾卡牌保护敌人本体
            Damageable manualTarget = SelectManualTarget(enemyCards[0], enemyView, playerCards);
            return new CardReleaseDecision(0, manualTarget);
        }
        
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
            // 攻击卡牌：目标为玩家本体
            if (IsAttackCard(card))
            {
                // 查找玩家本体（HeroView）
                HeroView heroView = FindHeroView();
                return heroView;
            }
            // 护盾卡牌：目标为敌人本体
            else if (IsShieldCard(card))
            {
                return enemyView;
            }
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
    /// 判断是否为护盾卡牌
    /// </summary>
    private bool IsShieldCard(Card card)
    {
        return card.ManualTargetEffect is AddStatusEffectEffect;
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
}
