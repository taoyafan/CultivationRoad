using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 时间轴视图类
/// 拥有一个 Line 表示时间轴，最右端表示倒计时结束。
/// 每个在时间系统中的卡牌用有一个 TimeSlot prefab 在时间轴上记录其位置。
/// TimeSlot prefab 有一个 SlotLine 为当前位置的 LineRenderer。
/// 一个 SpriteRenderer，若与其他 SpriteRenderer 重叠则向左移动合适距离。
/// 一个 Link Line，端点 0 的 x 需要更改为 SpriteRenderer 一致。
/// </summary>
public class TimeLineView : MonoBehaviour
{
    /// <summary>时间轴的 LineRenderer 组件 - 用于绘制主时间轴</summary>
    public LineRenderer timeLine;
    
    /// <summary>倒计时结束时间点 - 时间轴最右端对应的时间值</summary>
    public float countdownEndTime;
    
    /// <summary>
    /// 获取时间轴的实际宽度
    /// 通过LineRenderer的端点坐标计算时间轴的物理长度
    /// </summary>
    public float TimeLineWidth
    {
        get
        {
            if (timeLine == null || timeLine.positionCount < 2)
                return 0f;
            
            Vector3 startPos = timeLine.GetPosition(0);
            Vector3 endPos = timeLine.GetPosition(1);
            return Mathf.Abs(endPos.x - startPos.x);
        }
    }
    
    
    
    /// <summary>存储所有 TimeSlot - 管理时间轴上的所有插槽</summary>
    private List<TimeSlot> timeSlots = new List<TimeSlot>();
    
    /// <summary>
    /// 为卡牌添加 TimeSlot
    /// 创建新的时间插槽并添加到时间轴上
    /// </summary>
    /// <param name="sprite">精灵图像 - 显示在 TimeSlot 上的图像</param>
    /// <param name="timePosition">时间位置 - 卡牌在时间轴上的时间值</param>
    /// <returns>创建的 TimeSlot 对象</returns>
    public TimeSlot AddTimeSlot(Sprite sprite, float timePosition, bool isEnemy = false)
    {
        if (TimeSlotCreator.Instance != null)
        {
            TimeSlot timeSlot = TimeSlotCreator.Instance.CreateTimeSlot(this, sprite, timePosition, isEnemy);
            if (timeSlot != null)
            {
                timeSlot.transform.SetParent(transform);
                timeSlots.Add(timeSlot);
                CheckAndResolveOverlap();
                return timeSlot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 移除单个 TimeSlot
    /// 销毁指定的时间插槽并清理相关资源
    /// </summary>
    /// <param name="timeSlot">要移除的 TimeSlot 对象</param>
    public void RemoveTimeSlot(TimeSlot timeSlot)
    {
        if (timeSlot != null && timeSlots.Contains(timeSlot))
        {
            // 销毁对象
            Destroy(timeSlot.gameObject);
            // 从列表中移除
            timeSlots.Remove(timeSlot);
            // 检查并解决重叠问题
            CheckAndResolveOverlap();
        }
    }
    
    /// <summary>
    /// 移除所有 TimeSlot
    /// 销毁所有时间轴上的插槽并清空列表
    /// </summary>
    public void ClearTimeSlots()
    {
        foreach (var slot in timeSlots)
        {
            if (slot != null)
            {
                // 销毁对象
                Destroy(slot.gameObject);
            }
        }
        timeSlots.Clear();
    }
    
    /// <summary>
    /// 处理 TimeSlot 销毁事件
    /// 当 TimeSlot 被销毁时自动调用，确保列表同步和资源清理
    /// </summary>
    /// <param name="timeSlot">被销毁的 TimeSlot 对象</param>
    internal void OnTimeSlotDestroyed(TimeSlot timeSlot)
    {
        if (timeSlot != null && timeSlots.Contains(timeSlot))
        {
            // 从列表中移除
            timeSlots.Remove(timeSlot);
            // 检查并解决重叠问题
            CheckAndResolveOverlap();
        }
    }
    
    /// <summary>
    /// 检查并解决 TimeSlot 图像重叠问题
    /// 若 SpriteRenderer 与其他 SpriteRenderer 重叠则向左移动合适距离
    /// </summary>
    private void CheckAndResolveOverlap()
    {
        if (timeSlots.Count <= 1)
            return;
        
        timeSlots.Sort((a, b) => 
        {
            float aX = a.GetSlotLineX();
            float bX = b.GetSlotLineX();
            return aX.CompareTo(bX);
        });
        
        // 先假设当前 currentSprite.x, linkLine[0].x 和 slotLineX 一致。
        // 若重叠则更改。
        for (int i = timeSlots.Count - 1; i >= 0; i--)
        {
            TimeSlot currentTimeSlot = timeSlots[i];
            float slotLineX = currentTimeSlot.GetSlotLineX(); // slotLine的全局x坐标
            float currentImageLocalX = currentTimeSlot.GetImageLocalX();
            
            // 计算需要移动的距离：将图片与slotLineX对齐
            // 由于图片的localPosition.x是相对于TimeSlot的，而TimeSlot的transform.position.x等于slotLineX
            // 我们需要将图片的localPosition.x重置为0，这样图片的全局坐标就会与slotLineX对齐
            float moveDistance = -currentImageLocalX;
            
            if (i < timeSlots.Count - 1)
            {
                TimeSlot rightTimeSlot = timeSlots[i + 1];
                float rightSlotLineX = rightTimeSlot.GetSlotLineX();
                float rightImageGlobalX = rightTimeSlot.GetImageGlobalX();
                
                SpriteRenderer currentSprite = currentTimeSlot.GetComponentInChildren<SpriteRenderer>();
                if (currentSprite != null)
                {
                    // 计算图片与slotLineX对齐后的右边界（全局坐标）
                    float alignedImageRight = slotLineX + currentSprite.bounds.size.x / 2;
                    // 计算右侧图片的左边界（全局坐标）
                    float rightImageLeft = rightImageGlobalX - currentSprite.bounds.size.x / 2;
                    
                    // 检查重叠
                    if (alignedImageRight > rightImageLeft)
                    {
                        float overlapDistance = alignedImageRight - rightImageLeft;
                        // 需要额外向左移动的距离
                        moveDistance -= overlapDistance;
                    }
                }
            }
            
            // 最后执行实际的位置调整
            if (moveDistance != 0)
            {
                currentTimeSlot.AdjustImagePosition(moveDistance);
            }
        }
    }
    
}