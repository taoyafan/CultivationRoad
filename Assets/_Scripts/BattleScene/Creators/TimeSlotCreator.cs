using UnityEngine;

/// <summary>
/// TimeSlot 创建器类
/// 负责创建和初始化 TimeSlot 对象
/// </summary>
public class TimeSlotCreator : Singleton<TimeSlotCreator>
{
    [SerializeField] private TimeSlot _timeSlotPrefab;
    
    [SerializeField] private float fixedWidth = 1.0f;
    
    /// <summary>
    /// 创建 TimeSlot 对象
    /// 仅允许 TimeLineView 调用此方法
    /// </summary>
    /// <param name="timeLineView">时间轴视图</param>
    /// <param name="sprite">精灵图像</param>
    /// <param name="timePosition">时间位置</param>
    /// <returns>创建的 TimeSlot 对象</returns>
    public TimeSlot CreateTimeSlot(TimeLineView timeLineView, Sprite sprite, float timePosition, bool isEnemy = false)
    {
        // 检查调用者是否为 TimeLineView
        // 这里通过堆栈跟踪检查调用者类型（简单实现）
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        bool isCalledFromTimeLineView = false;
        
        for (int i = 1; i < stackTrace.FrameCount; i++)
        {
            System.Reflection.MethodBase method = stackTrace.GetFrame(i).GetMethod();
            if (method.DeclaringType == typeof(TimeLineView))
            {
                isCalledFromTimeLineView = true;
                break;
            }
        }
        
        if (!isCalledFromTimeLineView)
        {
            Debug.LogError("CreateTimeSlot 方法只能通过 TimeLineView.AddTimeSlot 调用");
            return null;
        }
        
        if (timeLineView == null || _timeSlotPrefab == null)
        {
            Debug.LogError("TimeLineView 或 _timeSlotPrefab 未设置");
            return null;
        }
        
        // 实例化 TimeSlot 预制体，但暂时不设置父对象
        TimeSlot timeSlot = Instantiate(_timeSlotPrefab, timeLineView.transform);
        
        // 设置 TimeSlot
        timeSlot.Setup(sprite, fixedWidth, isEnemy);
        
        // 获取时间轴的端点坐标
        Vector3 startPos = timeLineView.timeLine.GetPosition(0);
        Vector3 endPos = timeLineView.timeLine.GetPosition(1);
        
        // 计算时间位置在时间轴上的比例（反转：0在右端，最大值在左端）
        float positionRatio = 1 - (timePosition / timeLineView.countdownEndTime);
        
        // 根据比例计算x坐标（线性插值）
        float xPosition = Mathf.Lerp(startPos.x, endPos.x, positionRatio);
        
        // 更新 TimeSlot 位置（使用局部坐标，因为父对象会在 TimeLineView 中设置）
        timeSlot.transform.localPosition = new Vector3(xPosition, timeSlot.transform.localPosition.y, timeSlot.transform.localPosition.z);

        return timeSlot;
    }
}