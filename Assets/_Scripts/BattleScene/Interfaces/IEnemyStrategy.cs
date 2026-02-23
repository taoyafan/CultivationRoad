using System.Collections.Generic;

public interface IEnemyStrategy
{
    /// <summary>
    /// 选择要释放的卡牌和目标
    /// </summary>
    /// <param name="enemyView">敌人视图</param>
    /// <param name="enemyCards">敌人手牌</param>
    /// <param name="playerCards">玩家场上的卡牌</param>
    /// <returns>选中的卡牌索引和目标，如果没有合适的卡牌返回null</returns>
    CardReleaseDecision SelectCardToRelease(EnemyView enemyView, List<Card> enemyCards, List<CardView> playerCards);
}