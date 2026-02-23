using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCardViewGA : GameAction
{
    public CardView CardView { get; private set; }
    
    public DestroyCardViewGA(CardView cardView)
    {
        CardView = cardView;
    }
}