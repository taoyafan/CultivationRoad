using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayCardGA : GameAction
{
    public Card Card { get; private set; }
    public EnemyView EnemyView { get; private set; }

    public EnemyPlayCardGA(Card card, EnemyView enemyView)
    {
        Card = card;
        EnemyView = enemyView;
    }
}