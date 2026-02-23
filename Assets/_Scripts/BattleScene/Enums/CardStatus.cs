/// <summary>
/// 卡牌状态枚举
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// Init status
    /// </summary>
    Invalid,

    /// <summary>
    /// 在手中
    /// </summary>
    InHand,
    
    /// <summary>
    /// 释放中
    /// </summary>
    Casting,
    
    /// <summary>
    /// 施法成功
    /// </summary>
    CastSuccess,
    
    
    /// <summary>
    /// 待生效
    /// </summary>
    Pending,
    
    /// <summary>
    /// 已生效
    /// </summary>
    Activated,

    /// <summary>
    /// 需要重新选择目标
    /// </summary>
    RechooseTarget
}
