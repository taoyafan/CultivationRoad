using UnityEngine;

/// <summary>
/// TimeSlot 类
/// 表示时间轴上的一个插槽，用于记录卡牌的位置
/// </summary>
public class TimeSlot : MonoBehaviour
{
    /// <summary>SlotLine - 当前位置的 LineRenderer，用于绘制插槽的垂直指示线</summary>
    [SerializeField] private LineRenderer slotLine;
    
    /// <summary>SpriteRenderer - 插槽的图像显示，用于视觉化插槽位置</summary>
    [SerializeField] private SpriteRenderer slotSpriteRenderer;
    
    /// <summary>LinkLine - 连接插槽和其他元素的 LineRenderer</summary>
    [SerializeField] private LineRenderer linkLine;

    /// <summary>图像固定宽度 - 所有图像自动缩放到此宽度</summary>
    [SerializeField] private float fixedWidth = 0.5f;

    /// <summary>HeroColor - 英雄颜色，用于区分不同英雄的插槽</summary>
    [SerializeField] private Color heroColor;

    /// <summary>EnemyColor - 敌人颜色，用于区分不同敌人的插槽</summary>
    [SerializeField] private Color enemyColor;

    /// <summary>BackGroundSR - 插槽的背景 SpriteRenderer，用于视觉化插槽位置</summary>
    [SerializeField] private SpriteRenderer backGroundSR;
    
    /// <summary>
    /// 设置 TimeSlot 的初始状态
    /// </summary>
    /// <param name="sprite">要显示的精灵图像</param>
    /// <param name="imageFixedWidth">图像固定宽度</param>
    /// <param name="isEnemy">是否为敌人插槽</param>
    public void Setup(Sprite sprite, float imageFixedWidth = 1.0f, bool isEnemy = false)
    {
        slotSpriteRenderer.sprite = sprite;
        if (sprite != null)
        {
            float width = fixedWidth > 0 ? fixedWidth : imageFixedWidth;
            float originalWidth = sprite.bounds.size.x;
            float scaleFactor = width / originalWidth;
            slotSpriteRenderer.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // 设置背景颜色
            backGroundSR.color = isEnemy ? enemyColor : heroColor;
        }
    }
    
    /// <summary>
    /// 更新 TimeSlot 在时间轴上的位置
    /// 同步更新 SlotLine、SpriteRenderer 和 Link Line 的位置
    /// </summary>
    /// <param name="xPosition">x坐标 - TimeSlot在时间轴上的x位置</param>
    public void UpdatePosition(float xPosition)
    {
        // 更新TimeSlot对象的位置
        transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
        
        // 更新插槽线的位置（相对于TimeSlot）
        if (slotLine != null)
        {
            Vector3 startPos = slotLine.GetPosition(0);
            Vector3 endPos = slotLine.GetPosition(1);
            startPos.x = 0; // 相对于TimeSlot中心的位置
            endPos.x = 0;   // 相对于TimeSlot中心的位置
            slotLine.SetPosition(0, startPos);
            slotLine.SetPosition(1, endPos);
        }
        
        // 更新插槽精灵的位置（相对于TimeSlot）
        if (slotSpriteRenderer != null)
        {
            slotSpriteRenderer.transform.localPosition = Vector3.zero; // 相对于TimeSlot中心的位置
        }

        if (backGroundSR != null)
        {
            backGroundSR.transform.localPosition = Vector3.zero; // 相对于TimeSlot中心的位置
        }
        
        // 更新链接线的起点位置（相对于TimeSlot）
        if (linkLine != null)
        {
            Vector3 startPos = linkLine.GetPosition(0);
            startPos.x = 0; // 相对于TimeSlot中心的位置
            linkLine.SetPosition(0, startPos);
        }
    }
    
    /// <summary>
    /// 调整精灵位置以避免重叠
    /// 只调整 SpriteRenderer 和 Link Line[0] 的 x 坐标，不改变整个 TimeSlot 的位置
    /// </summary>
    /// <param name="offset">偏移量 - 调整的距离，负值表示向左移动</param>
    public void AdjustImagePosition(float offset)
    {
        // 调整SpriteRenderer的x坐标
        if (slotSpriteRenderer != null)
        {
            Vector3 spritePos = slotSpriteRenderer.transform.localPosition;
            spritePos.x += offset;
            slotSpriteRenderer.transform.localPosition = spritePos;
        }

        // 调整背景SpriteRenderer的x坐标
        if (backGroundSR != null)
        {
            Vector3 bgPos = backGroundSR.transform.localPosition;
            bgPos.x += offset;
            backGroundSR.transform.localPosition = bgPos;
        }
        
        // 调整Link Line[0]的x坐标
        if (linkLine != null && linkLine.positionCount > 0)
        {
            Vector3 linkStartPos = linkLine.GetPosition(0);
            linkStartPos.x += offset;
            linkLine.SetPosition(0, linkStartPos);
        }
    }
    
    /// <summary>
    /// 获取当前TimeSlot的精灵图像全局x坐标
    /// </summary>
    /// <returns>当前TimeSlot的精灵图像全局x坐标</returns>
    public float GetImageGlobalX()
    {
        if (slotSpriteRenderer != null)
        {
            return slotSpriteRenderer.transform.position.x;
        }
        return 0f;
    }

    /// <summary>
    /// 获取当前TimeSlot的精灵图像局部x坐标
    /// </summary>
    /// <returns>当前TimeSlot的精灵图像局部x坐标</returns>
    public float GetImageLocalX()
    {
        if (slotSpriteRenderer != null)
        {
            return slotSpriteRenderer.transform.localPosition.x;
        }
        return 0f;
    }



    /// <summary>
    /// 更新TimeSlot的时间显示或位置
    /// </summary>
    /// <param name="countdown">剩余倒计时时间</param>
    /// <param name="timeLineStartX">时间轴起点x坐标</param>
    /// <param name="timeLineEndX">时间轴终点x坐标</param>
    /// <param name="countdownEndTime">时间轴总时间</param>
    public void UpdateTime(float countdown, float timeLineStartX, float timeLineEndX, float countdownEndTime)
    {
        // 计算剩余时间在总时间中的比例（反转：0在右端，最大值在左端）
        float positionRatio = 1 - (countdown / countdownEndTime);
        
        // 根据比例计算x坐标（线性插值）
        float xPosition = Mathf.Lerp(timeLineStartX, timeLineEndX, positionRatio);
        
        // 更新TimeSlot的位置
        transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
        
        // 这里可以添加更新时间显示的逻辑（如果有时间文本显示的话）
    }
    
    /// <summary>
    /// 获取slotLine的x坐标位置
    /// 用于在TimeLineView中计算重叠和排序
    /// </summary>
    /// <returns>slotLine的全局x坐标</returns>
    public float GetSlotLineX()
    {
        if (slotLine != null)
        {
            Vector3 slotLinePos = slotLine.GetPosition(0);
            return transform.position.x + slotLinePos.x;
        }
        return transform.position.x;
    }

    
    /// <summary>
    /// 销毁时通知TimeLineView进行清理
    /// </summary>
    private void OnDestroy()
    {
        // 通知TimeLineView进行清理
        TimeLineView timeLineView = transform.parent?.GetComponent<TimeLineView>();
        if (timeLineView != null)
        {
            timeLineView.OnTimeSlotDestroyed(this);
        }
    }
}