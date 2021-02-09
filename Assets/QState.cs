using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QState : MonoBehaviour
{
    public int lastFightResult;
    public int goldenMinionCounter;
    //public int ownHealth;
    public int boardStats;          //hp+att (co z DSem i poisonem?)
    public int gold;                //wanna use all possible gold???
    public int handCounter;
    public int boardCounter;
    
    public void Initialize(int newLastFightResult, int newGoldenMinionCounter, int newBoardStats, int newGold, int newHandCounter, int newBoardCounter)
    {
        lastFightResult = newLastFightResult;
        goldenMinionCounter = newGoldenMinionCounter;
        boardStats = newBoardStats;
        gold = newGold;
        handCounter = newHandCounter;
        boardCounter = newBoardCounter;
    }

    public int GetLastFightResult()
    {
        return lastFightResult;
    }

    public int GetGoldenMinionCounter()
    {
        return goldenMinionCounter;
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

    public int GetBoardCounter()
    {
        return boardCounter;
    }

}
