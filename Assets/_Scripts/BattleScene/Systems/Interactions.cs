using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    public bool PlayerIsCasting { get; set; } = false;

    public void Reset()
    {
        PlayerIsDragging = false;
        PlayerIsCasting = false;
    }

    public bool PlayerCanInteract(Card card)
    {
        if (!ActionSystem.Instance.IsPerforming && 
            !TimeSystem.Instance.IsPlaying && 
            !card.IsEnemy)
        {
            return true;
        }
        return false;
    }

    public bool PlayerCanHover()
    {
        if (!ActionSystem.Instance.IsPerforming && 
            !PlayerIsDragging && 
            !TimeSystem.Instance.IsPlaying)
        {
            return true;
        }
        return false;
    }
}
