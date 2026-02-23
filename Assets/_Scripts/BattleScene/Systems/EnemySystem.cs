using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<EnemyView> EnemyViews => enemyBoardView.EnemyViews;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerform);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerform);
        ActionSystem.AttachPerformer<EnemyCastCardGA>(EnemyCastCardPerformer);
        ActionSystem.AttachPerformer<EnemyPlayCardGA>(EnemyPlayCardPerformer);

        ActionSystem.SubscribeReaction<DestroyCardViewGA>(DestroyCardViewReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<RefillManaGA>(RefillManaReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.DetachPerformer<EnemyCastCardGA>();
        ActionSystem.DetachPerformer<EnemyPlayCardGA>();

        ActionSystem.UnsubscribeReaction<DestroyCardViewGA>(DestroyCardViewReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<RefillManaGA>(RefillManaReaction, ReactionTiming.POST);
    }

    public void Setup(List<EnemyData> enemyDataList)
    {
        if (enemyBoardView == null)
        {
            enemyBoardView = FindObjectOfType<EnemyBoardView>();
        }
        
        if (enemyBoardView == null)
        {
            Debug.LogError("EnemySystem.Setup: enemyBoardView is null!");
            return;
        }
        
        foreach (EnemyData enemyData in enemyDataList)
        {
            EnemyView enemyView = enemyBoardView.AddEnemy(enemyData);
            enemyView.CastCard(0.1f);
        }
    }

    public void ResetEnemies()
    {
        enemyBoardView.ClearAllEnemies();
    }

    // Performers
    private IEnumerator AttackHeroPerform(AttackHeroGA attackHeroGA)
    {
        // Enemy can no longer directly attack, remove direct attack logic
        yield break;
    }

    private IEnumerator KillEnemyPerform(KillEnemyGA killEnemyGA)
    {
        foreach (var cardView in killEnemyGA.EnemyView.CardsView.GetAllCards())
        {
            CardViewCreator.Instance.DestroyCardView(cardView);
        }

        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        // If all enemies are killed, trigger WinGameGA
        if (EnemyViews.Count == 0)
        {
            TimeSystem.Instance.AddAction(new WinGameGA(), 0f);
        }
        else
        {
            TimeSystem.Instance.CleanWhenObjectDies(killEnemyGA.EnemyView);
        }
    }

    private IEnumerator EnemyCastCardPerformer(EnemyCastCardGA enemyCastCardGA)
    {
        Card card = enemyCastCardGA.Card;
        EnemyView enemyView = enemyCastCardGA.EnemyView;
        
        if (card != null && enemyView != null && enemyView.CardsView != null)
        {
            // 创建 CardView 并添加到敌人的 CardsView
            CardView cardView = CardViewCreator.Instance.CreateCardView(
                card, enemyView.transform.position, Quaternion.identity, enemyView.CardsView.CardScale);
            
            if (cardView != null)
            {
                // 设置卡牌状态
                card.SetStatus(CardStatus.Casting);
                card.CardView = cardView;
                cardView.ShowCastingEffect();
                
                // 添加到敌人的卡牌视图
                enemyView.CardsView.AddCard(cardView);
                
                // 获取TimeLineView组件
                TimeLineView timeLineView = GameObject.FindObjectOfType<TimeLineView>();
                TimeSlot timeSlot = null;
                if (timeLineView != null)
                {
                    timeSlot = timeLineView.AddTimeSlot(enemyView.spritesRender.sprite, card.CastTime, true);
                }
                // 创建 EnemyPlayCardGA 并添加到时间系统
                EnemyPlayCardGA enemyPlayCardGA = new(card, enemyView);
                TimeSystem.Instance.AddAction(enemyPlayCardGA, card.CastTime, timeSlot);
            }
        }
        
        yield return null;
    }

    
    private IEnumerator EnemyPlayCardPerformer(EnemyPlayCardGA enemyPlayCardGA)
    {
        Card card = enemyPlayCardGA.Card;
        EnemyView enemyView = enemyPlayCardGA.EnemyView;
        
        if (card != null && card.IsEnemy && card.CardView != null)
        {
            // 更新卡牌状态为待生效
            card.SetStatus(CardStatus.Pending);
            card.CardView.HideCastingEffect();

            if (card.ManualTargetEffect != null)
            {
                Effect effect = card.ManualTargetEffect;
                // ManualTargetEffect必须设置目标对象
                Debug.Assert(card.ManualTarget != null, "ManualTargetEffect必须设置目标对象");
                TargetMode targetMode = new ManualTM(card.ManualTarget);
                
                AutoTargetEffect autoTargetEffect = new(targetMode, effect);
                card.PerformEffect(autoTargetEffect);
            }

            // 执行卡牌效果
            foreach (var effect in card.OtherEffects)
            {
                card.PerformEffect(effect);
            }
            
        }

        // 释放下一张卡牌
        enemyView.CastSucceed();
        enemyView.CheckAndReleaseNextCard();
        
        yield return null;
    }

    private IEnumerator DestroyCardViewReaction(DestroyCardViewGA destroyCardViewGA)
    {
        if (destroyCardViewGA.CardView.Card.IsEnemy)
        {
            foreach (EnemyView enemyView in EnemyViews)
            {
                enemyView.CheckAndReleaseNextCard();
            }
        }
        yield break;
    }
    
    private IEnumerator RefillManaReaction(RefillManaGA refillManaGA)
    {
        // 灵力恢复后，检查所有敌人是否有足够的灵力出下一张牌
        foreach (EnemyView enemyView in EnemyViews)
        {
            enemyView.CheckAndReleaseNextCard();
        }
        yield break;
    }
    
}
