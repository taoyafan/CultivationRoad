using System.Collections.Generic;

/// <summary>
/// 卡牌释放决策
/// </summary>
public class CardReleaseDecision
{
    /// <summary>
    /// 选中的卡牌索引
    /// </summary>
    public int CardIndex { get; private set; }
    
    /// <summary>
    /// 手动目标（如果需要手动选择目标）
    /// </summary>
    public Damageable ManualTarget { get; private set; }
    
    /// <summary>
    /// 是否应该释放卡牌
    /// </summary>
    public bool ShouldRelease => CardIndex >= 0;
    
    /// <summary>
    /// 创建释放卡牌的决策
    /// </summary>
    /// <param name="cardIndex">卡牌索引</param>
    /// <param name="manualTarget">手动目标</param>
    public CardReleaseDecision(int cardIndex, Damageable manualTarget = null)
    {
        CardIndex = cardIndex;
        ManualTarget = manualTarget;
    }
    
    /// <summary>
    /// 创建不释放卡牌的决策
    /// </summary>
    public static CardReleaseDecision DoNotRelease()
    {
        return new CardReleaseDecision(-1);
    }
}