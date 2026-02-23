using UnityEngine;
using TMPro;

public class CombatantView : Damageable
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private CardsView _cardsView;
    public CardsView CardsView => _cardsView;

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = health;
        CurrentHealth = health;
        spritesRender.sprite = image;
        UpdateHealthText();
        ManaSystem.Instance.RegisterCombatant(this);
    }

    private void UpdateHealthText()
    {
        // 使用 Rich Text 语法将数字部分设置为加粗红色
        healthText.text = $"血量: <color=red><b>{CurrentHealth}/{MaxHealth}</b></color>";
    }

    public void UpdateManaText(int mana, int maxMana)
    {
        // 使用 Rich Text 语法将数字部分设置为加粗蓝色
        manaText.text = $"灵力: <color=blue><b>{mana}/{maxMana}</b></color>";
    }

    protected override void UpdateHealth()
    {
        UpdateHealthText();
    }
}
