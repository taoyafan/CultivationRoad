using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToCoolDownGA : GameAction
{
    public CardView CardView { get; }
    public float CoolDownTime { get; }

    public MoveToCoolDownGA(CardView cardView, float coolDownTime)
    {
        CardView = cardView;
        CoolDownTime = coolDownTime;
    }
}
