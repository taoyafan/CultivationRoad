using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    private readonly CardData data;
    public string Title => data.name;
    public string Description => data.Description;
    public float CastTime => data.CastTime;
    public Sprite Image => data.Image;
    public Effect ManualTargetEffect => data.ManualTargetEffect;
    public Damageable ManualTarget {get; set;} = null;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public int Mana {get; private set;}
    public int MaxHealth {get; private set;}
    public int CurrentHealth {get; private set;}

    public CardStatus Status {get; private set;} = CardStatus.Invalid;
    public CardView CardView {get; set;} = null;
    public bool IsEnemy = false;


    public void SetStatus(CardStatus newStatus)
    {
        Status = newStatus;
        
        if (CardView == null) return;
        if (IsEnemy) 
        {
            CardView.DisableOutline();
            return;
        } 

        // 更新边框显示
        UpdateOutlineDisplay();
    }
    
    /// <summary>
    /// 更新卡牌边框显示，考虑状态、施法状态和灵力条件
    /// </summary>
    public void UpdateOutlineDisplay()
    {
        if (CardView == null || IsEnemy) return;
        
        // 检查是否应该显示边框
        bool shouldShowOutline = ShouldShowOutline();
        
        if (shouldShowOutline)
        {
            CardView.EnableOutline();
        }
        else
        {
            CardView.DisableOutline();
        }
    }
    
    /// <summary>
    /// 判断是否应该显示边框
    /// </summary>
    private bool ShouldShowOutline()
    {
        // 玩家正在施法时不显示边框
        if (Interactions.Instance.PlayerIsCasting)
        {
            return false;
        }
        
        // 只有在特定状态下才显示边框
        switch (Status)
        {
            case CardStatus.InHand:
                // InHand状态的卡牌需要检查灵力
                if (!ManaSystem.Instance.HasEnoughMana(Mana))
                {
                    return false;
                }
                return true;
            case CardStatus.CastSuccess:
            case CardStatus.RechooseTarget:
                // 这些状态始终显示边框，不需要检查灵力
                return true;
            default:
                return false;
        }
    }
    private readonly List<AutoTargetEffect> pendingEffects = new();

    public Card(CardData data, bool isEnemy = false)
    {
        this.data = data;
        Mana = data.Mana;
        this.IsEnemy = isEnemy;
        MaxHealth = data.MaxHealth;
        CurrentHealth = MaxHealth;
    }
    
    // 保存与caster生命周期相关的状态效果及其目标
    private List<(StatusEffect effect, Damageable target)> tiedStatusEffects = new List<(StatusEffect effect, Damageable target)>();
    
    public void AddTiedStatusEffect(StatusEffect effect, Damageable target)
    {
        if (effect != null && effect.IsTiedToCasterLifecycle && target != null)
        {
            tiedStatusEffects.Add((effect, target));
        }
    }
    
    public void RemoveAllTiedStatusEffects()
    {
        Debug.Log("RemoveAllTiedStatusEffects: Removing all tied status effects");
        Debug.Log("RemoveAllTiedStatusEffects: Number of tied status effects to remove: " + tiedStatusEffects.Count);
        
        foreach ((StatusEffect effect, Damageable target) in tiedStatusEffects)
        {
            if (effect != null && target != null)
            {
                Debug.Log("RemoveAllTiedStatusEffects: Removing effect " + effect.GetType().Name + " from target " + target.name);
                Debug.Log("RemoveAllTiedStatusEffects: Effect stack count: " + effect.StackCount);
                // 移除状态效果
                target.RemoveStatusEffect(effect.GetType(), effect.StackCount);
            }
            else
            {
                Debug.LogWarning("RemoveAllTiedStatusEffects: Effect or target is null");
            }
        }
        
        int count = tiedStatusEffects.Count;
        tiedStatusEffects.Clear();
        Debug.Log("RemoveAllTiedStatusEffects: Cleared all tied status effects, removed " + count + " effects");
    }

    public void PerformEffect(AutoTargetEffect effect)
    {
        AddPendingEffect(effect);
        PerformEffectGA perfEffectGA = new(effect, CardView);
        
        TimeSlot timeSlot = CreateTimeSlot(effect.Effect.ActiveTime);
        
        TimeSystem.Instance.AddAction(perfEffectGA, effect.Effect.ActiveTime, timeSlot);
    }

    public void EffectPerformed(AutoTargetEffect effect)
    {
        // 1. 从施法者卡片的待处理效果列表中移除当前效果
        // 这表示该效果已经开始执行，不再处于待处理状态
        RemovePendingEffect(effect);
        
        // 2. 获取效果的目标并创建对应的GameAction
        // 通过效果的TargetMode获取实际目标，结合施法者创建具体的游戏动作
        GameAction effectAction = effect.Effect.GetGameAction(
            effect.TargetMode.GetTargets(!IsEnemy), CardView);
        
        // 3. 如果成功创建了游戏动作，则执行相应处理
        if (effectAction != null)
        {
            // 3.1 将效果动作添加到ActionSystem的反应列表中
            // 这样会触发所有对该类型动作感兴趣的观察者
            ActionSystem.Instance.AddReaction(effectAction);
            
            // 3.2 获取当前效果的下一个连锁效果
            // 实现效果的链式执行机制
            Effect nextEffect = effect.Effect.GetNextEffect();
            
            // 3.3 创建下一个效果的自动目标效果
            // 获取新的 TargetMode, 如果没有则使用当前的 TargetMode
            TargetMode nextTM = effect.Effect.GetNextEffectTM();
            AutoTargetEffect nextAutoTargetEffect = new(nextTM ?? effect.TargetMode, nextEffect);
            
            // 3.4 如果存在下一个效果，则创建新的执行效果动作并添加到时间系统
            if (nextEffect != null)
            {
                TimeSlot timeSlot = CreateTimeSlot(nextEffect.ActiveTime);
                
                // 创建新的PerformEffectGA来执行下一个效果
                PerformEffectGA nextPerfEffectGA = new(nextAutoTargetEffect, CardView);  
                
                // 将下一个效果动作添加到时间系统，设置倒计时为下一个效果的激活时间
                // 这样下一个效果会在指定时间后自动执行
                TimeSystem.Instance.AddAction(nextPerfEffectGA, nextEffect.ActiveTime, timeSlot);
            }
        }
    }

    private TimeSlot CreateTimeSlot(float activeTime)
    {
        if (activeTime <= 0)
        {
            return null;
        }

        // 获取TimeLineView组件
        TimeLineView timeLineView = GameObject.FindObjectOfType<TimeLineView>();
        
        // 创建TimeSlot并关联到该动作（只能通过TimeLineView.AddTimeSlot创建）
        TimeSlot timeSlot = null;
        if (timeLineView != null)
        {
            timeSlot = timeLineView.AddTimeSlot(Image, activeTime, IsEnemy);
        }
        return timeSlot;
    }

    public void AddPendingEffect(AutoTargetEffect effect)
    {
        pendingEffects.Add(effect);
    }

    public void RemovePendingEffect(AutoTargetEffect effect)
    {
        pendingEffects.Remove(effect);
    }
    
}
