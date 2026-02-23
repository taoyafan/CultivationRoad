using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = new();
    public bool IsPerforming { get; private set; } = false;
    private static readonly Dictionary<Type, List<Func<GameAction, IEnumerator>>> preSubs = new();
    private static readonly Dictionary<Type, List<Func<GameAction, IEnumerator>>> postSubs = new();
    private static readonly Dictionary<Type, List<Func<GameAction, IEnumerator>>> performSubs = new();

    public void Perform(GameAction action, Action onComplete = null)
    {
        if (IsPerforming)
        {
            Debug.Log("ActionSystem: Performing action while already performing action");
            return;
        }
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            onComplete?.Invoke();
            IsPerforming = false;
        }));
    }

    /// <summary>
    /// 执行一个GameAction并返回IEnumerator，允许调用者使用yield return等待其执行完成
    /// </summary>
    /// <param name="action">要执行的GameAction</param>
    /// <returns>IEnumerator，用于协程等待</returns>
    public IEnumerator PerformAndWait(GameAction action)
    {
        if (IsPerforming)
        {
            Debug.Log($"ActionSystem: Performing action {action} while already performing action");
            yield break;
        }
        
        IsPerforming = true;
        
        bool completed = false;
        StartCoroutine(Flow(action, () =>
        {
            completed = true;
            IsPerforming = false;
        }));
        
        // 等待动作执行完成
        while (!completed)
        {
            yield return null;
        }
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (!performSubs.ContainsKey(type))
        {
            performSubs[type] = new List<Func<GameAction, IEnumerator>>();
        }
        performSubs[type].Add(wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performSubs.ContainsKey(type))
        {
            performSubs.Remove(type);
        }
    }

    public static void SubscribeReaction<T>(Func<T, IEnumerator> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Func<GameAction, IEnumerator>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        IEnumerator wrappedReaction(GameAction action) => reaction((T)action);
        Type type = typeof(T);
        if (!subs.ContainsKey(type))
        {
            subs[type] = new();
        }
        subs[type].Add(wrappedReaction);
    }

    public static void UnsubscribeReaction<T>(Func<T, IEnumerator> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Func<GameAction, IEnumerator>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Type type = typeof(T);
        if (subs.ContainsKey(type))
        {
            IEnumerator wrappedReaction(GameAction action) => reaction((T)action);
            subs[type].Remove(wrappedReaction);
        }
    }

    private IEnumerator Flow(GameAction action, Action onComplete)
    {
        reactions = action.PreReactions;
        yield return PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformers(action);
        yield return PerformReactions();
        
        reactions = action.PostReactions;
        yield return PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        onComplete?.Invoke();
    }
    
    private IEnumerator Flow(GameAction action) // 重载Flow方法，用于嵌套调用
    {
        yield return Flow(action, null);
    }

    private IEnumerator PerformSubscribers(GameAction action, Dictionary<Type, List<Func<GameAction, IEnumerator>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            foreach (var subscriber in subs[type])
            {
                yield return subscriber(action);
            }
        }
    }

    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }

    private IEnumerator PerformPerformers(GameAction action)
    {
        Type type = action.GetType();
        if (performSubs.ContainsKey(type))
        {
            foreach (var performer in performSubs[type])
            {
                Debug.Log($"ActionSystem: Performing {type}");
                yield return performer(action);
            }
        }
        else
        {
            Debug.LogError($"ActionSystem: No performer for {type}");
        }
    }
}
