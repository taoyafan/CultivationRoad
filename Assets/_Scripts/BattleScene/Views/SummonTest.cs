using UnityEngine;

public class SummonController : MonoBehaviour
{
    public Material auraMat;
    public float duration = 2.5f; // 整个过程持续几秒
    private float timer = 0f;

    void Start()
    {
        if (auraMat == null)
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                auraMat = r.material;
            }
        }
    }

    void Update()
    {
        if (auraMat == null)
        {
            Debug.LogWarning("auraMat is null!");
            return;
        }

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // 使用平滑曲线 (Ease In)，让后半段加速更明显
        float progress = t * t; 

        // 1. 设置 Shader 参数
        auraMat.SetFloat("_Progress", progress);
        // 亮度随进度从 1 飙升到 6 (产生刺眼感)
        auraMat.SetFloat("_Intensity", 1.0f + progress * 5.0f);

        // 2. 物体缩放：从小到大
        // float scale = Mathf.Lerp(0.5f, 3.5f, progress);
        // transform.localScale = new Vector3(scale, scale, 1);

        // 3. 循环演示（实际游戏里你应该在这里销毁并生成卡牌）
        if (timer >= duration)
        {
            timer = 0f;
        }
    }

    // 退出时重置参数，防止编辑器里材质变花
    void OnDisable()
    {
        if (auraMat)
        {
            auraMat.SetFloat("_Progress", 0);
            auraMat.SetFloat("_Intensity", 1);
        }
    }
}