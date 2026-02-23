using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }

    /// <summary>
    /// 执行效果的动作处理器
    /// 负责处理PerformEffectGA类型的游戏动作，调用Card的EffectPerformed方法
    /// </summary>
    /// <param name="perfEffectGA">要处理的PerformEffectGA动作</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator PerformEffectPerformer(PerformEffectGA perfEffectGA)
    {
        // 调用Card的EffectPerformed方法执行效果
        perfEffectGA.Caster.Card.EffectPerformed(perfEffectGA.Effect);
        
        // 等待一帧，确保协程正常执行
        yield return null;
    }
}
