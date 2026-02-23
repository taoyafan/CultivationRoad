using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyGA : GameAction, IHaveCaster
{
    public EnemyView EnemyView { get; private set; }
    public CardView Caster { get; private set; }
    
    public KillEnemyGA(EnemyView enemyView, CardView caster)
    {
        EnemyView = enemyView;
        Caster = caster;
    }
}
