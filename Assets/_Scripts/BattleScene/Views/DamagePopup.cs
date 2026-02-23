using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text txt;
    [SerializeField] private float floatDistance = 0.6f;
    [SerializeField] private float duration = 0.9f;

    private Tween moveTween;
    private Tween fadeTween;
    private Vector3 startWorldPos;
    private Vector3 originalWorldPos;
    private Transform followTarget;
    private float originalTargetY;
    
    public void Show(int amount, Color? color = null, float scale = 1f, Transform followTarget = null)
    {
        // 完全重置动画状态
        moveTween?.Kill(true);
        fadeTween?.Kill(true);
        moveTween = null;
        fadeTween = null;
        
        // 重置位置和旋转
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one * scale;
        
        // 准备文字
        txt.text = amount.ToString();
        Color textColor = color ?? txt.color;
        textColor.a = 1f;
        txt.color = textColor;

        // 保存初始位置和跟随目标
        startWorldPos = transform.position;
        originalWorldPos = transform.position;
        this.followTarget = followTarget;
        
        // 保存目标初始的y坐标
        originalTargetY = followTarget != null ? followTarget.position.y : 0f;

        // 动画：向上移动并淡出
        
        // 使用 DOMoveY 移动世界坐标
        moveTween = transform.DOMoveY(startWorldPos.y + floatDistance, duration)
            .SetEase(Ease.OutCubic);

        // 直接对Text颜色进行淡出动画
        fadeTween = txt.DOColor(new Color(txt.color.r, txt.color.g, txt.color.b, 0f), duration)
            .SetDelay(0.15f);

        // 动画结束回收
        fadeTween.OnComplete(() => 
        {
            this.followTarget = null;
            // 确保动画完全停止
            moveTween?.Kill(true);
            moveTween = null;
            fadeTween = null;
            DamagePopupManager.Instance.ReturnToPool(this);
        });
    }

    public void KillImmediately()
    {
        // 完全停止所有动画
        moveTween?.Kill(true);
        fadeTween?.Kill(true);
        moveTween = null;
        fadeTween = null;
        
        followTarget = null;
        DamagePopupManager.Instance.ReturnToPool(this);
    }
    
    private void Update()
    {
        if (followTarget != null)
        {
            // 计算目标位置的偏移
            Vector3 targetOffset = followTarget.position - originalWorldPos;
            
            // 计算目标 y 坐标的变化
            float targetYOffset = followTarget.position.y - originalTargetY;
            
            // 应用 x 和 z 坐标的偏移
            Vector3 newPosition = transform.position;
            newPosition.x = startWorldPos.x + targetOffset.x;
            newPosition.z = startWorldPos.z + targetOffset.z;
            
            // 应用目标 y 坐标的偏移，同时保持向上漂浮的动画效果
            newPosition.y += targetYOffset;
            
            transform.position = newPosition;
        }
    }
}