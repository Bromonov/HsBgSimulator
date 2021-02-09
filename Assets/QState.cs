using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QState : MonoBehaviour
{
    public int enemyHealth;
    public int ownHealth;
    public int boardStats;          //hp+att (co z DSem i poisonem?)
    public int gold;                //wanna use all possible gold???
    public int handCounter;
    
    public void Initialize(int newEnemyHealth, int newOwnHealth, int newBoardStats, int newGold, int newHandCounter)
    {
        enemyHealth = newEnemyHealth;
        ownHealth = newOwnHealth;
        boardStats = newBoardStats;
        gold = newGold;
        handCounter = newHandCounter;
    }

    public int GetEnemyHealth()
    {
        return enemyHealth;
    }

    public int GetOwnHealth()
    {
        return ownHealth;
    }

    public int GetBoardStats()
    {
        return boardStats;
    }

    public int GetGold()
    {
        return gold;
    }

    public int GetHandCounter()
    {
        return handCounter;
    }

}
