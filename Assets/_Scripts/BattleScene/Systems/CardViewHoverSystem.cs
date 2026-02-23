using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView _cardViewHoverPrefab;
    [SerializeField] private float rightBoundary;
    [SerializeField] private float topBoundary;
    
    public void Show(Card card, Vector3 pos)
    {
        if (_cardViewHoverPrefab == null) return;
        _cardViewHoverPrefab.SetCard(card);
        _cardViewHoverPrefab.gameObject.SetActive(true);
        
        // 检测边界并调整位置
        Vector3 adjustedPos = ClampToScreenBounds(pos);
        _cardViewHoverPrefab.transform.position = adjustedPos;
    }
    
    private Vector3 ClampToScreenBounds(Vector3 pos)
    {        
        float leftBoundary = -rightBoundary;
        float bottomBoundary = -topBoundary;
        
        // 检测右边界（如果超出，调整到右边界内）
        if (pos.x > rightBoundary)
        {
            pos.x = rightBoundary;

        }
        
        // 检测上边界（如果超出，调整到上边界内）
        if (pos.y > topBoundary)    
        {
            pos.y = topBoundary;
        }
        
        // 检测左边界（如果超出，调整到左边界内）
        if (pos.x < leftBoundary)
        {
            pos.x = leftBoundary;
        }
        
        // 检测下边界（如果超出，调整到下边界内）
        if (pos.y < bottomBoundary)
        {
            pos.y = bottomBoundary;
        }
        
        return pos;
    }

    public void Hide()
    {
        if (_cardViewHoverPrefab == null) return;
        _cardViewHoverPrefab.gameObject.SetActive(false);
    }
}
