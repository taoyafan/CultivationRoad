using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using Unity.VisualScripting;

public class CardsView : MonoBehaviour
{
    [SerializeField] public float CardScale = 1.0f; // 卡牌缩放比例
    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private float _cardSpacing = 0.1f;
    [SerializeField] private float _animationDuration = 0.15f;
    [SerializeField] private float _cardDelay = 0f; // 卡牌之间的延迟时间

    private readonly List<CardView> _cards = new();
    private readonly Queue<CardView> _cardQueue = new();
    private bool _isProcessing = false;

    /// <summary>
    /// 添加卡牌到手中（使用队列处理，避免阻塞）
    /// </summary>
    public void AddCard(CardView cardView, float? scale = null)
    {
        if (cardView == null) return;
        if (scale.HasValue)
        {
            cardView.TargetScale = scale.Value;
        }
        else
        {
            cardView.TargetScale = CardScale;
        }

        // 设置CardView的CardsView引用
        cardView.CardsView = this;
        
        // 将卡牌加入队列
        _cardQueue.Enqueue(cardView);

        // 如果没有正在处理的卡牌，开始处理队列
        if (!_isProcessing)
        {
            StartCoroutine(ProcessCardQueue());
        }
    }

    public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if (cardView != null)
        {
            _cards.Remove(cardView);
            StartCoroutine(OnlyRedistributeAllCards());
        }
        else
        {
            Debug.LogError($"HandView 中未找到卡牌视图：{card}");
        }
        return cardView;
    }

    /// <summary>
    /// 直接移除指定的卡牌视图
    /// </summary>
    public bool RemoveCardView(CardView cardView)
    {
        if (cardView != null && _cards.Contains(cardView))
        {
            _cards.Remove(cardView);
            StartCoroutine(OnlyRedistributeAllCards());
            return true;
        }
        return false;
    }

    public CardView GetCardView(Card card)
    {
        return _cards.Where(c => c.Card == card).FirstOrDefault();
    }

    /// <summary>
    /// 处理卡牌队列的协程
    /// 优化：移除嵌套协程调用，将逻辑直接合并到主协程中
    /// </summary>
    private IEnumerator ProcessCardQueue()
    {
        _isProcessing = true;

        // 处理队列中的所有卡牌
        while (_cardQueue.Count > 0)
        {
            CardView cardView = _cardQueue.Dequeue();
            
            // 保存卡牌到列表
            _cards.Add(cardView);
            
            // 重新分布所有卡牌
            yield return RedistributeAllCards();

            // 等待一小段时间再处理下一张卡牌
            yield return new WaitForSeconds(_cardDelay);
        }

        _isProcessing = false;
    }

    public IEnumerator OnlyRedistributeAllCards()
    {
        _isProcessing = true;
        yield return RedistributeAllCards();
        // 避免此时有卡牌加入 Queue，等待处理完成
        yield return ProcessCardQueue();
        _isProcessing = false;
    }

    /// <summary>
    /// 重新分布所有卡牌在曲线上
    /// </summary>
    private IEnumerator RedistributeAllCards()
    {
        // 为所有卡牌创建位置和旋转动画
        List<Tween> animations = new();

        for (int i = 0; i < _cards.Count; i++)
        {
            float normalizedPosition = CalculateCardPositionOnSpline(i);
            Vector3 targetPos = _splineContainer.EvaluatePosition(normalizedPosition);
            targetPos.z = i * -0.01f - 0.01f;
            
            // 为卡牌创建位置和旋转动画（同时执行）
            Tween posAnim = _cards[i].transform.DOMove(targetPos, _animationDuration)
                .SetEase(Ease.OutCubic);
            Tween scaleAnim = _cards[i].transform.DOScale(Vector3.one * _cards[i].TargetScale, _animationDuration)
                .SetEase(Ease.OutCubic);
            animations.Add(posAnim);
            animations.Add(scaleAnim);
        }

        // 等待所有动画完成
        yield return new WaitForSeconds(_animationDuration);
    }

    private float CalculateCardPositionOnSpline(int cardIndex)
    {
        if (_cards.Count == 1) return 0.5f; // 只有一张卡牌时居中

        // 计算卡牌在曲线上的归一化位置（0到1之间）
        float totalSpacing = (_cards.Count - 1) * _cardSpacing;
        float startPosition = 0.5f - (totalSpacing / 2);
        return Mathf.Clamp01(startPosition + (cardIndex * _cardSpacing));
    }

    /// <summary>
    /// 等待卡牌队列处理完成
    /// </summary>
    public IEnumerator WaitForCardQueueToComplete()
    {
        while (_isProcessing || _cardQueue.Count > 0)
        {
            yield return null;
        }
    }
    
    /// <summary>
    /// 获取所有卡牌视图
    /// </summary>
    public List<CardView> GetAllCards()
    {
        return new List<CardView>(_cards);
    }

    /// <summary>
    /// 获取所有可攻击卡牌视图
    /// </summary>
    public List<CardView> GetAllCardsAttackable()
    {
        return new List<CardView>(_cards.Where(c => c.Card.Status > CardStatus.Casting));
    }

    /// <summary>
    /// 清除所有卡牌视图
    /// </summary>
    public void ClearCards()
    {
        foreach (var cardView in _cards)
        {
            if (cardView != null)
            {
                Destroy(cardView.gameObject);
            }
        }
        _cards.Clear();
        _cardQueue.Clear();
        _isProcessing = false;
    }
}
