using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardSystem : Singleton<CardSystem>
{
    public CardsView HandView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    public CardsView heroCardsView;
    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();
    
    public void Setup(List<CardData> deckData)
    {
        foreach (CardData cardData in deckData)
        {
            drawPile.Add(new Card(cardData));
        }
        
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null)
        {
            heroCardsView = HeroSystem.Instance.HeroView.CardsView;
        }
        else
        {
            Debug.LogError("CardSystem.Setup: HeroSystem or HeroView is null!");
        }
    }

    public void ResetCards()
    {
        HandView?.ClearCards();
        heroCardsView?.ClearCards();
        hand.Clear();
        drawPile.Clear();
        discardPile.Clear();
    }

    private void OnEnable()
    {
        // 对象启用时注册动作处理器到ActionSystem
        ActionSystem.AttachPerformer<CastCardGA>(CastCardPerformer);
        ActionSystem.AttachPerformer<CastSuccessGA>(CastSuccessPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<DestroyCardViewGA>(DestroyCardViewPerformer);

        ActionSystem.SubscribeReaction<PauseTimeGA>(PauseTimeReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<ResumeTimeGA>(ResumeTimeReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        // 对象禁用时从ActionSystem取消注册动作处理器
        ActionSystem.DetachPerformer<CastCardGA>();
        ActionSystem.DetachPerformer<CastSuccessGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();

        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<DestroyCardViewGA>();

        ActionSystem.UnsubscribeReaction<PauseTimeGA>(PauseTimeReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<ResumeTimeGA>(ResumeTimeReaction, ReactionTiming.POST);
    }

    private IEnumerator PauseTimeReaction(PauseTimeGA pauseTimeGA)
    {
        // foreach (var cardView in HandView.GetAllCards())
        // {
        //     cardView.RestoreState();
        // }
        yield break;
    }

    private IEnumerator ResumeTimeReaction(ResumeTimeGA resumeTimeGA)
    {
        // foreach (var cardView in HandView.GetAllCards())
        // {
        //     cardView.DisableAndSaveState();
        // }
        yield break;
    }
    
    

    /// <summary>
    /// 抽卡牌动作的处理器
    /// </summary>
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;
        
        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }

        if (notDrawnAmount > 0)
        {
            RefillDeck();
        }

        for (int i = 0; i < notDrawnAmount; i++)
        {
            yield return DrawCard();
        }

    }
    
    /// <summary>
    /// 弃掉所有卡牌动作的处理器
    /// </summary>
    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA action)
    {
        foreach (Card card in hand)
        {
            CardView cardView = HandView.RemoveCard(card);
            if (cardView != null)
            {
                yield return DiscardCard(cardView);
            }
            else
            {
                Debug.LogError($"HandView中未找到卡牌视图：{card}");
            }
        }
        hand.Clear();
    }
    
    /// <summary>
    /// 播放单张卡牌动作的处理器
    /// </summary>
    private IEnumerator CastCardPerformer(CastCardGA CastCardGA)
    {
        hand.Remove(CastCardGA.Card);
        CardView cardView = HandView.RemoveCard(CastCardGA.Card);
        
        if (cardView != null)
        {
            // 标记为正在施法 - 边框逻辑会自动处理
            Interactions.Instance.PlayerIsCasting = true;
            CastCardGA.Card.SetStatus(CardStatus.Casting);

            float scale = heroCardsView.CardScale;
            
            // Scale to 0
            heroCardsView.AddCard(cardView, 0);

            // 消耗法力值
            SpendManaGA spendManaGA = new(CastCardGA.Card.Mana);
            ActionSystem.Instance.AddReaction(spendManaGA);

            // 将释放成功动作添加到时间系统，倒计时为卡牌的释放时间
            CastSuccessGA castSuccessGA = new(CastCardGA.Card);
            
            // 获取TimeLineView组件
            TimeLineView timeLineView = GameObject.FindObjectOfType<TimeLineView>();
            
            // 创建TimeSlot并关联到该动作
            TimeSlot timeSlot = null;
            if (timeLineView != null)
            {
                timeSlot = timeLineView.AddTimeSlot(HeroSystem.Instance.HeroView.spritesRender.sprite, CastCardGA.Card.CastTime, false);
            }
            TimeSystem.Instance.AddAction(castSuccessGA, CastCardGA.Card.CastTime, timeSlot);

            // 等待移除卡牌和添加卡牌的动画完成
            yield return HandView.WaitForCardQueueToComplete();
            yield return heroCardsView.WaitForCardQueueToComplete();

            cardView.ScaleToTarget(scale);
            cardView.ShowCastingEffect();
        }
        else
        {
            Debug.LogError($"HandView中未找到卡牌视图：{CastCardGA.Card}");
        }
        yield return null;
    }

    /// <summary>
    /// 弃掉单张卡牌
    /// </summary>
    private IEnumerator DiscardCard(CardView cardView)
    {
        // 弃牌堆添加卡牌
        discardPile.Add(cardView.Card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
    
    /// <summary>
    /// 补充牌堆
    /// </summary>
    private void RefillDeck()
    {
        // 将弃牌堆的所有卡牌添加到牌堆
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        
        // 随机打乱牌堆
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // 交换卡牌
            (drawPile[randomIndex], drawPile[i]) = (drawPile[i], drawPile[randomIndex]);
        }
    }
    
    private IEnumerator DrawCard() 
    {
        if (drawPile.Count > 0)
        {
            // 从牌堆顶部抽取一张卡牌
            Card drawnCard = drawPile.Draw();
            
            // 添加到玩家手牌
            hand.Add(drawnCard);
            
            // 创建卡牌视图并添加到HandView
            if (HandView != null)
            {
                CardView cardView = CardViewCreator.Instance.CreateCardView(
                    drawnCard, drawPilePoint.position, drawPilePoint.rotation, 1f);
                if (cardView != null)
                {
                    HandView.AddCard(cardView);
                    drawnCard.SetStatus(CardStatus.InHand);
                }
                else
                {
                    Debug.LogError("CardView创建失败，请检查CardViewCreator设置");
                }
            }
            else
            {
                Debug.LogError("HandView未分配，请检查CardSystem设置");
            }
            
            // 每抽一张卡牌等待一小段时间，以实现动画效果
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogWarning("牌堆为空，无法抽牌");
        }
    }

    private IEnumerator CastSuccessPerformer(CastSuccessGA action)
    {
        Interactions.Instance.PlayerIsCasting = false;
        CardView cardView = heroCardsView.GetCardView(action.Card);
        if (cardView != null)
        {
            action.Card.SetStatus(CardStatus.CastSuccess);
            cardView.HideCastingEffect();
            
            if (cardView.Card.ManualTargetEffect == null)
            {
                PlayCardGA playCardGA = new(action.Card);
                ActionSystem.Instance.AddReaction(playCardGA);
            }

            ActionSystem.Instance.AddReaction(new PauseTimeGA());
            
            // 施法完成后，更新手牌中所有卡牌的边框显示
            UpdateHandCardOutlines();
        }
        else
        {
            Debug.LogError($"heroCardsView中未找到卡牌视图：{action.Card}");
        }
        yield return null;
    }
    
    /// <summary>
     /// 更新手牌中所有卡牌的边框显示
     /// </summary>
     private void UpdateHandCardOutlines()
     {
         if (HandView != null)
         {
             foreach (var cardView in HandView.GetAllCards())
             {
                 if (cardView.Card != null && !cardView.Card.IsEnemy)
                 {
                     cardView.Card.UpdateOutlineDisplay();
                 }
             }
         }
     }

    /// <summary>
    /// 播放单张卡牌动作的处理器
    /// </summary>
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        // 标记为已激活
        playCardGA.Card.SetStatus(CardStatus.Pending);

        // 卡牌上升变大又回落动画
        if (playCardGA.Card.CardView != null)
        {
            Vector3 originalPosition = playCardGA.Card.CardView.transform.position;
            Vector3 originalScale = playCardGA.Card.CardView.transform.localScale;

            // Tween upTween = playCardGA.Card.CardView.transform.DOMove(originalPosition + Vector3.up * 0.5f, 0.3f);
            // Tween scaleUpTween = playCardGA.Card.CardView.transform.DOScale(originalScale * 1.2f, 0.3f);

            // yield return upTween.WaitForCompletion();
            // yield return scaleUpTween.WaitForCompletion();

            // Tween downTween = playCardGA.Card.CardView.transform.DOMove(originalPosition, 0.1f);
            // Tween scaleDownTween = playCardGA.Card.CardView.transform.DOScale(originalScale, 0.1f);

            // yield return downTween.WaitForCompletion();
            // yield return scaleDownTween.WaitForCompletion();
        }
        
        if (playCardGA.ManualTarget != null)
        {
            Effect effect = playCardGA.Card.ManualTargetEffect;
            TargetMode targetMode = new ManualTM(playCardGA.ManualTarget);
            AutoTargetEffect autoTargetEffect = new(targetMode, effect);
            playCardGA.Card.PerformEffect(autoTargetEffect);
        }

        // 执行卡牌效果
        foreach (var effect in playCardGA.Card.OtherEffects)
        {
            playCardGA.Card.PerformEffect(effect);
        }
        yield return null;
    }

    /// <summary>
    /// 销毁卡牌视图动作的处理器
    /// </summary>
    private IEnumerator DestroyCardViewPerformer(DestroyCardViewGA destroyCardViewGA)
    {
        if (destroyCardViewGA.CardView != null)
        {
            // 从CardView所在的CardsView中移除
            if (destroyCardViewGA.CardView.CardsView != null)
            {
                Debug.Log($"DestroyCardViewPerformer: Removing {destroyCardViewGA.CardView} from its CardsView");
                destroyCardViewGA.CardView.CardsView.RemoveCardView(destroyCardViewGA.CardView);
            }
            
            // 获取Card对象并移除所有与该卡片生命周期相关的状态效果
            Card card = destroyCardViewGA.CardView.Card;
            if (card != null)
            {
                Debug.Log("DestroyCardViewPerformer: Removing tied status effects for card " + destroyCardViewGA.CardView.name);
                card.RemoveAllTiedStatusEffects();
            }
            else
            {
                Debug.LogWarning("DestroyCardViewPerformer: Card is null, cannot remove tied status effects");
            }
            
            // 从TimeSystem中移除所有由该CardView发起的PerformEffectGA
            TimeSystem.Instance.RemoveActions<PerformEffectGA>(eff => eff.Caster != null && eff.Caster == destroyCardViewGA.CardView);
            TimeSystem.Instance.CleanWhenObjectDies(destroyCardViewGA.CardView);
            // 然后销毁卡牌游戏对象
            Destroy(destroyCardViewGA.CardView.gameObject);
        }
        else
        {
            Debug.LogError($"DestroyCardViewPerformer: CardView is null, cannot destroy");
        }
        yield return null;
    }


}
