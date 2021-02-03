using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QState : MonoBehaviour
{
    public int enemyHealth;
    public int ownHealthDifference;
    public int boardStats;          //hp+att (co z DSem i poisonem?)
    public int gold;                //wanna use all possible gold???
    
    
    public void Initialize(int newEnemyHealth, int newOwnHealthDifference, int newBoardStats, int newGold)
    {
        enemyHealth = newEnemyHealth;
        ownHealthDifference = newOwnHealthDifference;
        boardStats = newBoardStats;
        gold = newGold;
    }

    public int GetEnemyHealth()
    {
        return enemyHealth;
    }

    public int GetOwnHealthDifference()
    {
        return ownHealthDifference;
    }

    public int GetBoardStats()
    {
        return boardStats;
    }

    public int GetGold()
    {
        return gold;
    }
 
}
