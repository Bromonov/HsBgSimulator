using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QState : MonoBehaviour
{
    public int lastFightResult;         //(0, 1, 2)
    //public int goldenMinionCounter;     //(0-10, ale prawdopodobnie 0-1-2)
    //public int ownHealth;
    //public int boardStats;              //(0-100(?), duza liczba, duze wahania, moze powinien byc lcizony przyrost w caigu tury?)    //hp+att (co z DSem i poisonem?)
    public int gold;                    //(0-10)
    public int handCounter;             //(0-10)
    public int boardCounter;            //(0-7)
    public int tavernTier;              //(1-6)
    
    public void Initialize(int newLastFightResult, //int newGoldenMinionCounter, int newBoardStats, 
        int newGold, int newHandCounter, int newBoardCounter, int newTT)
    {
        lastFightResult = newLastFightResult;
        //goldenMinionCounter = newGoldenMinionCounter;
        //boardStats = newBoardStats;
        gold = newGold;
        handCounter = newHandCounter;
        boardCounter = newBoardCounter;
        tavernTier = newTT;
    }
    
    public int GetLastFightResult()
    {
        return lastFightResult;
    }
    /*
    public int GetGoldenMinionCounter()
    {
        return goldenMinionCounter;
    }

    public int GetBoardStats()
    {
        return boardStats;
    }
    */
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

    public int GetTavernTier()
    {
        return tavernTier;
    }

}
