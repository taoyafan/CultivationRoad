using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCoolDownGA : GameAction
{
    public CardView CardView { get; }

    public MoveToCoolDownGA(CardView cardView)
    {
        CardView = cardView;
    }
}
