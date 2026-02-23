using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public class TimeSystem : Singleton<TimeSystem>
{
    [SerializeField] private PauseContinueButton pauseContinueButton;
    [SerializeField] private bool enableDebugLog = false;
    // isPlaying = true 表示是没有暂停或者只是因为播放动作临时暂停
    public bool IsPlaying { get; private set; } = false;
    
    private class TimedGameAction
    {
        public GameAction Action { get; private set; }
        public float Countdown { get; set; }
        public float TotalTime { get; private set; } // 总倒计时时间
        public TimeSlot TimeSlot { get; set; }
        
        public TimedGameAction(GameAction action, float countdown, TimeSlot timeSlot)
        {
            Action = action;
            Countdown = countdown;
            TotalTime = countdown;
            TimeSlot = timeSlot;
        }
    }
    
    private readonly List<TimedGameAction> timedActions = new();
    private bool isPaused = true;
    private bool needPaused = true;
    private bool isExecutingAction = false;
    

    private void OnEnable()
    {
        // 为PauseTimeGA注册performer
        ActionSystem.AttachPerformer<PauseTimeGA>(PauseTimeActionPerformer);
        // 为ResumeTimeGA注册performer
        ActionSystem.AttachPerformer<ResumeTimeGA>(ResumeTimeActionPerformer);

        pauseContinueButton.ManualUpdate();
        needPaused = isPaused;
    }

    private void OnDisable()
    {
        // 移除PauseTimeGA的performer
        ActionSystem.DetachPerformer<PauseTimeGA>();
        // 移除ResumeTimeGA的performer
        ActionSystem.DetachPerformer<ResumeTimeGA>();
    }
    
    public void CleanWhenObjectDies(Damageable deadTarget)
    {
        // 查询并处理所有与当前死亡敌人相关的PerformEffectGA
        bool needPause = false;
        // 1. 获取时间系统中所有的PerformEffectGA
        List<GameAction> allActions = GetAllActions();
        
        // 2. 遍历所有动作，找出需要处理的PerformEffectGA
        foreach (GameAction action in allActions)
        {
            if (action is PerformEffectGA performEffectGA)
            {
                // 检查Caster是否已经被销毁
                if (performEffectGA.Caster == null || performEffectGA.Caster.gameObject == null)
                {
                    continue;
                }
                
                // 检查Card是否已经被销毁
                if (performEffectGA.Caster.Card == null)
                {
                    continue;
                }
                
                bool targetIsEnemy = !performEffectGA.Caster.Card.IsEnemy;
                // 3. 检查是否是手动目标且目标为当前死亡敌人
                if (performEffectGA.Effect.TargetMode is ManualTM manualTM)
                {
                    var targets = manualTM.GetTargets(targetIsEnemy);
                    if (targets.Count > 0 && (System.Object)targets[0] == deadTarget)
                    {
                        // 4. 删除该PerformEffectGA
                        RemoveActions<PerformEffectGA>(eff => eff == performEffectGA);
                        
                        // 5. 找到对应的CardView
                        CardView cardView = performEffectGA.Caster;
                        if (cardView != null && cardView.Card != null)
                        {
                            // 6. 从卡片的待处理效果中移除该效果
                            Card card = cardView.Card;
                            card.RemovePendingEffect(performEffectGA.Effect);
                            
                            // 7. 将卡片状态设置为需要重新选择目标
                            card.SetStatus(CardStatus.RechooseTarget);
                            needPause = true;
                        }
                    }
                }
            }
        }
        
        // 8. 如果有需要暂停的情况，暂停时间系统
        if (needPause)
        {
            AddAction(new PauseTimeGA(), 0f);
        }
    }

    private IEnumerator PauseTimeActionPerformer(PauseTimeGA action)
    {
        needPaused = true;
        Assert.IsTrue(IsPaused(), "执行暂停任务时，时间系统应该已暂停");
        IsPlaying = false;
        // ManualUpdate 必须在更新 IsPlaying 后调用
        pauseContinueButton.ManualUpdate();
        yield return null;
    }
    
    private IEnumerator ResumeTimeActionPerformer(ResumeTimeGA action)
    {
        needPaused = false;
        Resume();
        IsPlaying = true;
        // ManualUpdate 必须在更新 IsPlaying 后调用
        pauseContinueButton.ManualUpdate();
        yield return null;
    }
    
    private void Update()
    {        
        if (isExecutingAction || timedActions.Count == 0 || ActionSystem.Instance.IsPerforming)
            return;

        if (!isPaused)
        {
            // 更新所有动作的倒计时
            for (int i = 0; i < timedActions.Count; i++)
            {
                timedActions[i].Countdown -= Time.deltaTime;
                
                // 实时更新TimeSlot的时间
                if (timedActions[i].TimeSlot != null)
                {
                    // 获取TimeSlot的父对象，假设是TimeLineView
                    Transform parent = timedActions[i].TimeSlot.transform.parent;
                    TimeLineView timeLineView = null;
                    if (parent != null)
                    {
                        timeLineView = parent.GetComponent<TimeLineView>();
                    }
                    if (timeLineView != null)
                    {
                        // 获取时间轴的端点坐标
                        Vector3 startPos = timeLineView.timeLine.GetPosition(0);
                        Vector3 endPos = timeLineView.timeLine.GetPosition(1);
                        
                        // 调用UpdateTime方法，传递必要的参数
                        timedActions[i].TimeSlot.UpdateTime(
                            timedActions[i].Countdown, 
                            startPos.x, 
                            endPos.x, 
                            timeLineView.countdownEndTime
                        );
                    }
                }
            }
        }

        // 按照倒计时排序
        timedActions.Sort((a, b) => a.Countdown.CompareTo(b.Countdown));
        if (timedActions.Count > 0 && timedActions[0].Countdown <= 0)
        {
            StartCoroutine(ExecuteAction());
        }
    }
    
    private IEnumerator ExecuteAction()
    {
        isExecutingAction = true;
        // 先暂停时间
        needPaused = isPaused;
        Pause();
        // 执行所有倒计时已到的动作
        while (timedActions.Count > 0 && timedActions[0].Countdown <= 0)
        {
            // 执行动作
            DebugLog($"TimeSystem: Execute Action: {timedActions[0].Action}, " +
                    $"total actions: {timedActions.Count}");
            yield return ActionSystem.Instance.PerformAndWait(timedActions[0].Action);
                        
            // 如果存在TimeSlot，通过TimeLineView安全销毁它
            SafelyDestroyTimeSlot(timedActions[0].TimeSlot);
            
            // 从列表中删除动作
            if (timedActions.Count > 0 && timedActions[0].Countdown <= 0)
            {
                timedActions.RemoveAt(0);
            }
            else
            {
                Debug.LogError("执行结束的动作无法删除？");
            }

            // 执行动作时可能有新的动作加入 TimeSystem，需要重新排序
            timedActions.Sort((a, b) => a.Countdown.CompareTo(b.Countdown));
        }
        
        // 检查是否需要暂停
        if (!needPaused)
        {
            Resume();
        }
        isExecutingAction = false;
    }
    
    /// <summary>
    /// 添加一个GameAction到时间系统，设置倒计时
    /// </summary>
    /// <param name="action">要添加的GameAction</param>
    /// <param name="countdown">倒计时时间（秒）</param>
    /// <param name="timeSlot">时间槽</param>
    public void AddAction(GameAction action, float countdown, TimeSlot timeSlot = null)
    {
        timedActions.Add(new TimedGameAction(action, countdown, timeSlot));
        DebugLog($"TimeSystem: Add Action: {action}, Countdown: {countdown}, total actions: {timedActions.Count}");
    }
    
    /// <summary>
    /// 暂停时间系统
    /// </summary>
    private void Pause()
    {
        DebugLog("TimeSystem: Pause");
        isPaused = true;
    }
    
    /// <summary>
    /// 恢复时间系统
    /// </summary>
    private void Resume()
    {
        DebugLog("TimeSystem: Resume");
        isPaused = false;
    }
    
    /// <summary>
    /// 检查时间系统是否暂停
    /// </summary>
    /// <returns>如果暂停返回true，否则返回false</returns>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// 查询指定GameAction的剩余倒计时
    /// </summary>
    /// <param name="action">要查询的GameAction</param>
    /// <returns>剩余倒计时时间（秒），如果动作不存在返回-1</returns>
    public float GetRemainingCountdown(GameAction action)
    {
        foreach (var timedAction in timedActions)
        {
            if (timedAction.Action == action)
            {
                return timedAction.Countdown;
            }
        }
        return -1;
    }
    
    
    /// <summary>
    /// 查询指定类型GameAction的剩余倒计时（使用条件筛选）
    /// </summary>
    /// <typeparam name="T">要查询的GameAction类型</typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns>剩余倒计时时间（秒），如果动作不存在返回-1</returns>
    public float GetRemainingCountdown<T>(Predicate<T> predicate) where T : GameAction
    {
        foreach (var timedAction in timedActions)
        {
            if (timedAction.Action is T action && predicate(action))
            {
                return timedAction.Countdown;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 获取所有当前排队的动作
    /// </summary>
    /// <returns>所有GameAction的列表</returns>
    public List<GameAction> GetAllActions()
    {
        List<GameAction> actions = new();
        foreach (var timedAction in timedActions)
        {
            actions.Add(timedAction.Action);
        }
        return actions;
    }
    
    /// <summary>
    /// 清空所有排队的动作
    /// </summary>
    public void ClearAllActions()
    {
        // 销毁所有关联的TimeSlot
        for (int i = timedActions.Count - 1; i >= 0; i--)
        {
            SafelyDestroyAction(i);
        }
    }
    
    /// <summary>
    /// 移除符合条件的指定类型的GameAction
    /// </summary>
    /// <typeparam name="T">要移除的GameAction类型</typeparam>
    /// <param name="predicate">移除条件</param>
    public void RemoveActions<T>(Predicate<T> predicate) where T : GameAction
    {
        for (int i = timedActions.Count - 1; i >= 0; i--)
        {
            if (timedActions[i].Action is T action && predicate(action))
            {
                SafelyDestroyAction(i);
            }
        }
    }
    
    private void SafelyDestroyAction(int i)
    {
        // 检查动作是否正在执行（只有第一个动作可能在执行）
        if (!(isExecutingAction && i == 0))
        {
            // 如果存在TimeSlot，通过TimeLineView安全销毁它
            SafelyDestroyTimeSlot(timedActions[i].TimeSlot);
            DebugLog($"TimeSystem: Remove Action: {timedActions[i]}, Countdown: {timedActions[i].Countdown}");
            timedActions.RemoveAt(i);
        }
    }
    
    /// <summary>
    /// 安全地销毁TimeSlot对象
    /// 通过TimeLineView.RemoveTimeSlot方法销毁，确保符合安全销毁机制
    /// </summary>
    /// <param name="timeSlot">要销毁的TimeSlot对象</param>
    private void SafelyDestroyTimeSlot(TimeSlot timeSlot)
    {
        if (timeSlot != null)
        {
            Transform parent = timeSlot.transform.parent;
            if (parent != null)
            {
                if (parent.TryGetComponent<TimeLineView>(out var timeLineView))
                {
                    timeLineView.RemoveTimeSlot(timeSlot);
                }
            }
        }
    }

    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}