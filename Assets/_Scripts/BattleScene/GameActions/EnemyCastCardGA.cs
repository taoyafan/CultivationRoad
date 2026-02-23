using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCastCardGA : GameAction
{
    public Card Card { get; set; }
    public EnemyView EnemyView { get; set; }
    public EnemyCastCardGA(Card card, EnemyView enemyView)
    {
        Card = card;
        EnemyView = enemyView;
    }
}