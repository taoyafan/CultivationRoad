using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public void Set(Sprite sprite)
    {
        Debug.Log($"StatusEffectUI.Set: Setting sprite for {gameObject.name}, sprite={(sprite != null ? sprite.name : "null")}");
        
        if (image == null)
        {
            Debug.LogError("StatusEffectUI.Set: image component is null!");
            return;
        }
        
        image.sprite = sprite;
        Debug.Log($"StatusEffectUI.Set: Sprite set successfully for {gameObject.name}");
    }
}
