using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyView : CombatantView
{
    private readonly List<Card> _cards = new();
    
    public List<Card> Cards => _cards;
    public bool EnemyIsCasting { get; private set; }
    public IEnemyStrategy Strategy { get; private set; }

    public void Setup(EnemyData enemyData)
    {
        IsEnemy = true;
        SetupBase(enemyData.Health, enemyData.Image);
        
        // 复制Deck数据到本地Cards列表
        if (enemyData.Deck != null)
        {
            foreach (CardData cardData in enemyData.Deck)
            {
                _cards.Add(new Card(cardData, true));
            }
        }
        
        // 初始化策略
        Strategy = enemyData.Strategy ?? new DefaultEnemyStrategy();
    }

    public void CastCard(float delay)
    {
        if (Cards != null && Cards.Count > 0 && CardsView != null)
        {
            // 获取玩家场上的卡牌
            List<CardView> playerCards = GetPlayerFieldCards();
            
            // 使用策略选择要释放的卡牌和目标
            CardReleaseDecision decision = Strategy.SelectCardToRelease(this, Cards, playerCards);
            
            if (decision != null && decision.ShouldRelease && decision.CardIndex < Cards.Count)
            {
                Card card = Cards[decision.CardIndex];
                
                // 检查灵力是否足够
                if (ManaSystem.Instance.HasEnoughMana(this, card.Mana))
                {
                    EnemyIsCasting = true;
                    
                    // 设置手动目标（如果需要）
                    if (decision.ManualTarget != null)
                    {
                        card.ManualTarget = decision.ManualTarget;
                    }
                    
                    // 创建并执行SpendManaGA消耗灵力
                    SpendManaGA spendManaGA = new(card.Mana, this);
                    TimeSystem.Instance.AddAction(spendManaGA, delay);
                    
                    // 创建并执行EnemyCastCardGA
                    EnemyCastCardGA enemyCastCardGA = new(card, this);
                    TimeSystem.Instance.AddAction(enemyCastCardGA, delay);

                    // 从Cards列表中删除这张卡牌
                    Cards.RemoveAt(decision.CardIndex);
                }
                else
                {
                    Debug.LogWarning($"Enemy {name} doesn't have enough mana to cast");
                }
            }
        }
    }

    public void CastSucceed()
    {
        EnemyIsCasting = false;
    }

    public void CheckAndReleaseNextCard()
    {
        // 通用判断条件：检查是否满足基本出牌条件
        if (this != null &&
            !EnemyIsCasting &&
            Cards != null &&
            Cards.Count > 0 &&
            CardsView != null &&
            CardsView.GetAllCards().Count < 3)
        {
            CastCard(0);
        }
    }
    
    /// <summary>
    /// 获取玩家场上的卡牌
    /// </summary>
    /// <returns>玩家场上的卡牌列表</returns>
    private List<CardView> GetPlayerFieldCards()
    {
        // 通过CardSystem获取玩家场上的卡牌
        if (CardSystem.Instance != null && CardSystem.Instance.heroCardsView != null)
        {
            return CardSystem.Instance.heroCardsView.GetAllCards();
        }
        
        // 备用方案：通过HeroSystem查找玩家的CardsView
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null && HeroSystem.Instance.HeroView.CardsView != null)
        {
            return HeroSystem.Instance.HeroView.CardsView.GetAllCards();
        }
        
        // 如果以上方法都失败，返回空列表
        Debug.LogWarning("GetPlayerFieldCards: 无法获取玩家场上的卡牌");
        return new List<CardView>();
    }
}
