using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private EnemyView _enemyViewPrefab;
    
    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        EnemyView enemyView = Instantiate(_enemyViewPrefab, position, rotation); 
        enemyView.Setup(enemyData);
        return enemyView;
    }
}
