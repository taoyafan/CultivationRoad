using UnityEngine;
using TMPro;
using System.Collections;

public class CardView : Damageable
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private TMP_Text currentHealth;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private GameObject castingEffect;
    [SerializeField] private LayerMask CastLayer;
    [SerializeField] private LayerMask ActiveLayer;

    [SerializeField] private SpriteRenderer backGroundSR;
    [SerializeField] private Material outlineMaterial;

    public float TargetScale { get; set; } = 1;
    private Material defaultMaterial;
    private bool outlineEnabled = true;
    private bool savedOutlineState;

    public Card Card { get; private set; }
    public CardsView CardsView { get; set; }
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    
    public void SetCard(Card card)
    {
        Card = card;
        IsEnemy = card.IsEnemy;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        imageSR.sprite = card.Image;
        MaxHealth = card.MaxHealth;
        CurrentHealth = card.MaxHealth;
        currentHealth.text = CurrentHealth.ToString();
        defaultMaterial = new Material(Shader.Find("Sprites/Default"));

        // The Card might already has a card view.
        if (card.CardView == null) return;
        if (card.CardView.outlineEnabled)
        {
            EnableOutline();
        }
        else
        {
            DisableOutline();
        }
    }
    
    void Update()
    {
        if (Card.Status == CardStatus.Casting)
        {
            Material auraMat = castingEffect.GetComponent<Renderer>().material;

            float remainingTime = 0;
            if (Card.IsEnemy)
            {
                // 敌人下一次动作是 EnemyPlayCardGA
                remainingTime = TimeSystem.Instance.GetRemainingCountdown<EnemyPlayCardGA>(c => c.Card == Card);
            }
            else
            {
                remainingTime = TimeSystem.Instance.GetRemainingCountdown<CastSuccessGA>(c => c.Card == Card);
            }
            float elapsedTime = Card.CastTime - remainingTime;
            float t = Mathf.Clamp01(elapsedTime / Card.CastTime);

            // 使用平滑曲线 (Ease In)，让后半段加速更明显
            float progress = t * t; 

            // 1. 设置 Shader 参数
            auraMat.SetFloat("_Progress", progress);
            // 亮度随进度从 1 飙升到 6 (产生刺眼感)
            auraMat.SetFloat("_Intensity", 1.0f + progress * 5.0f);
        }
    }


    public void OnMouseEnter() 
    {
        if (Interactions.Instance.PlayerCanHover() && Card.Status != CardStatus.Casting)
        {
            wrapper.SetActive(false);
            Vector3 pos = new(transform.position.x, transform.position.y, 0);
            CardViewHoverSystem.Instance.Show(Card, pos);
        }
    }

    public void OnMouseExit() 
    {
        if (Interactions.Instance.PlayerCanHover() && Card.Status != CardStatus.Casting)
        {
            CardViewHoverSystem.Instance.Hide();
            wrapper.SetActive(true);
        }
    }

    public void ScaleToTarget(float targetScale)
    {
        TargetScale = targetScale;
        transform.localScale = new Vector3(targetScale, targetScale, 1);
    }

    public void ShowCastingEffect()
    {
        wrapper.SetActive(false);
        castingEffect.SetActive(true);
    }

    public void HideCastingEffect()
    {
        wrapper.SetActive(true);
        castingEffect.SetActive(false);
    }

    void OnMouseDown() 
    {
        if (!Interactions.Instance.PlayerCanInteract(Card)) return;

        if (Card.Status == CardStatus.InHand && !Interactions.Instance.PlayerIsCasting)
        {
            Interactions.Instance.PlayerIsDragging = true;
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.SetPositionAndRotation(MouseUtils.GetMouseWorldPosition(-1f), Quaternion.Euler(0, 0, 0));
        }
        else if (Card.Status == CardStatus.CastSuccess || Card.Status == CardStatus.RechooseTarget)
        {
            Interactions.Instance.PlayerIsDragging = true;
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();

            // 总是记录拖拽起始位置和旋转，无论是否有手动目标效果
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;

            if (Card.ManualTargetEffect != null)
            {
                ManualTargetSystem.Instance.StartTargeting(transform.position);
            }
            else
            {
                transform.SetPositionAndRotation(MouseUtils.GetMouseWorldPosition(-1f), Quaternion.Euler(0, 0, 0));
            }
        }
    }

    void OnMouseDrag() 
    {
        if (!Interactions.Instance.PlayerCanInteract(Card)) return;

        if (Card.Status == CardStatus.InHand && !Interactions.Instance.PlayerIsCasting)
        {
            transform.SetPositionAndRotation(MouseUtils.GetMouseWorldPosition(-1f), Quaternion.Euler(0, 0, 0));
        }
        else if (Card.Status == CardStatus.CastSuccess || Card.Status == CardStatus.RechooseTarget)
        {
            if (Card.ManualTargetEffect == null)
            {
                transform.SetPositionAndRotation(MouseUtils.GetMouseWorldPosition(-1f), Quaternion.Euler(0, 0, 0));
            }
        }
    }

    void OnMouseUp() 
    {
        if (!Interactions.Instance.PlayerCanInteract(Card)) return;

        if ((Card.Status == CardStatus.CastSuccess || Card.Status == CardStatus.RechooseTarget) && Card.ManualTargetEffect != null)
        {
            Damageable target = ManualTargetSystem.Instance.EndTargeting(MouseUtils.GetMouseWorldPosition(-1f));
            if (target != null && (target.IsEnemy == Card.ManualTargetEffect.TargetIsEnemy))
            {
                Card.ManualTarget = target;
                PlayCardGA playCardGA = new(Card, target);
                StartCoroutine(PlayCard(playCardGA));
            }
            else
            {
                Debug.Log($"No target selected, target: {target}, target.IsEnemy: {target?.IsEnemy}, Card.ManualTargetEffect.TargetIsEnemy: {Card.ManualTargetEffect.TargetIsEnemy}");
            }
            // 将卡牌位置重置到原始位置
            transform.SetPositionAndRotation(dragStartPosition, dragStartRotation);
        }
        // For card view that doesn't has ManualTargetEffect
        else if (Interactions.Instance.PlayerIsDragging)
        {
            if (Card.Status == CardStatus.InHand && !Interactions.Instance.PlayerIsCasting)
            {
                bool cast = false;
                if ((Physics.Raycast(transform.position, transform.forward,
                        out _, 10f, CastLayer)))
                {
                    if (ManaSystem.Instance.HasEnoughMana(HeroSystem.Instance.HeroView, Card.Mana))
                    {
                        // 启动协程等待CastCardGA执行结束
                        cast = true;
                        StartCoroutine(CastCardAndPauseTime());
                    }
                    else
                    {
                        ManaSystem.Instance.ShowNoManaWarning();
                    }
                }
                

                if (!cast)
                {
                    transform.SetPositionAndRotation(dragStartPosition, dragStartRotation);
                }
            }
            else if (Card.Status == CardStatus.CastSuccess || Card.Status == CardStatus.RechooseTarget)
            {
                Debug.LogError("Should not hit");
                // Interactions.Instance.PlayerIsDragging = false;
                // if (Physics.Raycast(transform.position, transform.forward,
                //         out _, 10f, ActiveLayer))
                // {   
                //     StartCoroutine(PlayCard(new PlayCardGA(Card)));
                // }
                // transform.SetPositionAndRotation(dragStartPosition, dragStartRotation);
            }
        }
        Interactions.Instance.PlayerIsDragging = false;
    }
    
    private IEnumerator PlayCard(PlayCardGA playCardGA)
    {
        // 等待ActiveCardGA执行结束
        yield return ActionSystem.Instance.PerformAndWait(playCardGA);
        
        // TimeSystem.Instance.AddAction(new ResumeTimeGA(), 0);
    }

    private IEnumerator CastCardAndPauseTime()
    {
        CastCardGA castCardGA = new(Card);
        // 等待CastCardGA执行结束
        yield return ActionSystem.Instance.PerformAndWait(castCardGA);
    }

    public void EnableOutline()
    {
        if (backGroundSR == null || outlineMaterial == null) return;

        backGroundSR.material = outlineMaterial;
        outlineEnabled = true;
    }

    public void DisableOutline()
    {
        backGroundSR.material = defaultMaterial;
        outlineEnabled = false;
    }

    public void DisableAndSaveState()
    {
        savedOutlineState = outlineEnabled;
        DisableOutline();
    }

    public void RestoreState()
    {
        if (savedOutlineState)
        {
            EnableOutline();
        }
        else
        {
            DisableOutline();
        }
    }

    protected override void UpdateHealth()
    {
        currentHealth.text = CurrentHealth.ToString();
    }
}
