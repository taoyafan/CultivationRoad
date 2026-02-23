using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;
    [SerializeField] private TMP_Text NoManaWarningText;
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int initMana = 60;
    [SerializeField] private int refillAmountPerSecond = 2;
    [SerializeField] private bool enableDebugLogs = false; // 总开关，默认关闭，只控制Debug.Log
    
    // 存储每个 CombatantView 的 mana
    private Dictionary<CombatantView, int> combatantMana = new();
    private Dictionary<CombatantView, int> combatantMaxMana = new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();
    }

    /// <summary>
    /// 注册一个 CombatantView，为其初始化 mana
    /// </summary>
    public void RegisterCombatant(CombatantView combatantView, int? _maxMana = null, int? _initMana = null)
    {
        if (combatantView == null)
        {
            Debug.LogError("RegisterCombatant: combatantView is null");
            return;
        }
        
        if (!combatantMana.ContainsKey(combatantView))
        {
            int initialMana = _initMana ?? initMana;
            int maximumMana = _maxMana ?? maxMana;
            combatantMana[combatantView] = initialMana;
            combatantMaxMana[combatantView] = maximumMana;
            if (enableDebugLogs) Debug.Log($"RegisterCombatant: Registered {combatantView.name} with {initialMana}/{maximumMana} mana");
            UpdateCombatantMana(combatantView);
        }
        else
        {
            Debug.LogWarning($"RegisterCombatant: {combatantView.name} is already registered");
        }
    }

    /// <summary>
    /// 取消注册一个 CombatantView
    /// </summary>
    public void UnregisterCombatant(CombatantView combatantView)
    {
        if (combatantView == null)
        {
            Debug.LogError("UnregisterCombatant: combatantView is null");
            return;
        }
        
        bool removedFromMana = combatantMana.Remove(combatantView);
        bool removedFromMaxMana = combatantMaxMana.Remove(combatantView);
        
        if (removedFromMana || removedFromMaxMana)
        {
            if (enableDebugLogs) Debug.Log($"UnregisterCombatant: Unregistered {combatantView.name}");
        }
        else
        {
            Debug.LogWarning($"UnregisterCombatant: {combatantView.name} was not registered");
        }
    }

    public void Setup()
    {
        if (manaUI == null)
        {
            manaUI = FindObjectOfType<ManaUI>();
        }
        
        // 设置时不直接初始化，而是等待注册
        TimeSystem.Instance.AddAction(new RefillManaGA(refillAmountPerSecond), 1f);
    }

    public void Reset()
    {
        // 清空所有注册信息
        combatantMana.Clear();
        combatantMaxMana.Clear();
        if (enableDebugLogs) Debug.Log("Reset: Cleared all mana registrations");
    }

    public void ShowNoManaWarning()
    {
        NoManaWarningText.gameObject.SetActive(true);
        StartCoroutine(HideNoManaWarningAfterDelay());
    }

    private IEnumerator HideNoManaWarningAfterDelay()
    {
        // 创建有冲击力的警告动画效果
        yield return StartCoroutine(CreateWarningAnimation());
        NoManaWarningText.gameObject.SetActive(false);
    }
    
    private IEnumerator CreateWarningAnimation()
    {
        Transform textTransform = NoManaWarningText.transform;
        Vector3 originalScale = textTransform.localScale;
        
        // 创建警告动画序列
        Sequence warningSequence = DOTween.Sequence();
        float largeScale = 1.5f;

        // 1. 弹出效果（快速放大）
        warningSequence.Append(textTransform.DOScale(originalScale * largeScale, 0.15f).SetEase(Ease.OutBack))
                      // 3. 脉动效果（放大缩小3次）
                      .Append(textTransform.DOScale(originalScale, 0.15f))
                      .Append(textTransform.DOScale(originalScale * largeScale, 0.15f))
                      .Append(textTransform.DOScale(originalScale, 0.15f))
                      .Append(textTransform.DOScale(originalScale * largeScale, 0.15f))
                      .Append(textTransform.DOScale(originalScale, 0.15f))
                      // 4. 淡出准备（稍微缩小）
                      .Append(textTransform.DOScale(originalScale * 0.9f, 0.2f));
        
        yield return warningSequence.WaitForCompletion();
    }

    /// <summary>
    /// 检查指定 CombatantView 是否有足够的 mana
    /// </summary>
    public bool HasEnoughMana(CombatantView combatantView, int amount)
    {
        if (combatantView == null)
        {
            Debug.LogError("HasEnoughMana: combatantView is null");
            return false;
        }
        
        if (combatantMana.TryGetValue(combatantView, out int currentMana))
        {
            bool hasEnough = currentMana >= amount;
            if (enableDebugLogs) Debug.Log($"HasEnoughMana: {combatantView.name} has {currentMana}/{combatantMaxMana[combatantView]} mana. Check for {amount}: {hasEnough}");
            return hasEnough;
        }
        
        Debug.LogWarning($"HasEnoughMana: {combatantView.name} not found in mana dictionary");
        return false;
    }
    
    /// <summary>
    /// 检查玩家是否有足够的 mana（默认检查英雄的mana）
    /// </summary>
    public bool HasEnoughMana(int amount)
    {
        if (HeroSystem.Instance == null || HeroSystem.Instance.HeroView == null)
        {
            Debug.LogError("HasEnoughMana: HeroSystem or HeroView is null");
            return false;
        }
        
        return HasEnoughMana(HeroSystem.Instance.HeroView, amount);
    }
    


    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        if (spendManaGA == null)
        {
            Debug.LogError("SpendManaPerformer: spendManaGA is null");
            yield break;
        }
        
        // 确定消耗mana的目标
        CombatantView target = spendManaGA.Target;
        
        // 如果没有指定目标，默认消耗英雄的mana（保持向后兼容）
        if (target == null)
        {
            target = HeroSystem.Instance.HeroView;
        }
        
        if (target == null)
        {
            Debug.LogError("SpendManaPerformer: No valid target for mana spending");
            yield break;
        }
        
        if (!HasEnoughMana(target, spendManaGA.Amount))
        {
            Debug.LogError($"SpendManaPerformer: Not enough mana to spend for {target.name}. Required: {spendManaGA.Amount}, Current: {combatantMana[target]}");
            ShowNoManaWarning();
            yield break;
        }

        // 消耗 mana
        int previousMana = combatantMana[target];
        combatantMana[target] -= spendManaGA.Amount;
        if (enableDebugLogs) Debug.Log($"SpendManaPerformer: Spent {spendManaGA.Amount} mana from {target.name}. {previousMana} -> {combatantMana[target]}");
        UpdateCombatantMana(target);
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        if (refillManaGA == null)
        {
            Debug.LogError("RefillManaPerformer: refillManaGA is null");
            yield break;
        }
        
        // 创建 keys 的副本以避免枚举时修改集合的问题
        var combatants = new List<CombatantView>(combatantMana.Keys);
        if (enableDebugLogs) Debug.Log($"RefillManaPerformer: Refilling {refillManaGA.Amount} mana for {combatants.Count} combatants");
        
        // 对所有注册的 CombatantView 恢复相同的 mana
        foreach (var combatant in combatants)
        {
            // 检查 combatant 仍然在字典中（可能在迭代过程中被移除）
            if (combatantMana.ContainsKey(combatant) && combatantMaxMana.ContainsKey(combatant))
            {
                int previousMana = combatantMana[combatant];
                combatantMana[combatant] += refillManaGA.Amount;
                
                // 不超过最大 mana
                int maxMana = combatantMaxMana[combatant];
                if (combatantMana[combatant] > maxMana)
                {
                    combatantMana[combatant] = maxMana;
                }
                
                if (previousMana != combatantMana[combatant])
                {
                    if (enableDebugLogs) Debug.Log($"RefillManaPerformer: Refilled {combatant.name} mana from {previousMana} to {combatantMana[combatant]}");
                    UpdateCombatantMana(combatant);
                }
            }
            else
            {
                Debug.LogWarning($"RefillManaPerformer: {combatant.name} no longer registered, skipping");
            }
        }
        
        TimeSystem.Instance.AddAction(new RefillManaGA(refillAmountPerSecond), 1f);
        yield return null;
    }

    /// <summary>
    /// 更新 CombatantView 的 mana 显示
    /// </summary>
    private void UpdateCombatantMana(CombatantView combatantView)
    {
        if (combatantView == null)
        {
            Debug.LogError("UpdateCombatantMana: combatantView is null");
            return;
        }
        
        if (combatantMana.TryGetValue(combatantView, out int currentMana))
        {
            int maxMana = combatantMaxMana[combatantView];
            if (enableDebugLogs) Debug.Log($"UpdateCombatantMana: Updating {combatantView.name} mana display to {currentMana}/{maxMana}");
            
            // 更新 CombatantView 的 mana 文本
            combatantView.UpdateManaText(currentMana, maxMana);
            
            // 只有 HeroView 使用 manaUI
            if (combatantView is HeroView)
            {
                if (manaUI == null)
                {
                    Debug.LogError("UpdateCombatantMana: manaUI is null for HeroView");
                }
                else
                {
                    manaUI.UpdateManaText(currentMana);
                }
                
                // 当英雄灵力变化时，更新玩家手牌中卡牌的边框显示
                UpdatePlayerHandCardOutlines();
            }
        }
        else
        {
            Debug.LogWarning($"UpdateCombatantMana: {combatantView.name} not found in mana dictionary");
        }
    }
    
    /// <summary>
    /// 更新玩家手牌中卡牌的边框显示（只有InHand状态的卡牌需要更新）
    /// </summary>
    private void UpdatePlayerHandCardOutlines()
    {
        // 只更新HandView中的卡牌（这些卡牌是InHand状态）
        var cardSystem = FindObjectOfType<CardSystem>();
        if (cardSystem != null && cardSystem.HandView != null)
        {
            foreach (var cardView in cardSystem.HandView.GetAllCards())
            {
                if (cardView.Card != null && !cardView.Card.IsEnemy)
                {
                    cardView.Card.UpdateOutlineDisplay();
                }
            }
        }
    }
}