using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int gold;
    public int tavernTierLevel;
    public int tavernTierUpgradeGold;
    public int turnNumber;
    public int health;
    //hero choice, hero power ???

    public GameController gc;
    public int phaseNumber;         // 1->tavern, 2->fight
    public List <Board> board;
    public List<Board> copiedBoard;
    public List<Board> hand;
    private XmlDocument minionDataXML;
    private MinionData minionData;

    public int tavernCost1;
    public int tavernCost2;
    public int tavernCost3;
    public int tavernCost4;
    public int tavernCost5;
    public int tavernCost6;

    public bool turn;
    public int lastResult;  //0 -> lost, 1 -> draw, 2 -> won
    public int lastGoldenMinionCounter;
    public int goldenMinionCounter;
    public bool dead;
    public bool fight;

    public struct Board
    {
        MinionData minion;
        int pos;

        public Board(MinionData newMinion, int newPos)
        {
            minion = newMinion;
            pos = newPos;
        }

        public MinionData GetMinion()
        {
            return minion;
        }

        public int GetPos()
        {
            return pos;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        //gold = 3;
        //tavernTierLevel = 1;
        //turnNumber = 1;
        health = 40;
        board = new List<Board>();
        copiedBoard = new List<Board>();
        hand = new List<MinionData>();

        minionData = gc.minionData;
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);

        tavernCost1 = 5;
        tavernCost2 = 7;
        tavernCost3 = 8;
        tavernCost4 = 9;
        tavernCost5 = 10;
        tavernCost6 = 0;
        */
    }

    public void Initialize()
    {
        gold = 99;
        //gold = 3;
        tavernTierLevel = 1;
        turnNumber = 1;
        health = 40;
        board = new List<Board>();
        copiedBoard = new List<Board>();
        hand = new List<Board>();

        minionData = gc.minionData;
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);

        tavernCost1 = 5;
        tavernCost2 = 7;
        tavernCost3 = 8;
        tavernCost4 = 9;
        tavernCost5 = 10;
        tavernCost6 = 0;

        tavernTierUpgradeGold = tavernCost1;
        lastResult = -1;
        goldenMinionCounter = 0;
        lastGoldenMinionCounter = goldenMinionCounter;
        dead = false;
        fight = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Q))
        {
            if (this.gameObject.name == "Player2")
            {
                Debug.Log("no hej graczu 2");
                MinionData minionData1 = new MinionData();
                minionData1.Initialize("Fiendish Servant", 2, 1, "Demon", 1, false, false, false, "", "", false);
                Player.Board minion1 = new Player.Board(minionData1, 0);
                board.Add(minion1);

                Debug.Log(board[0].GetMinion().Name);
            }
        }
        //Debug.Log(this.gameObject.name + copiedBoard.Count);
    }

    public int GetPlayerGold()
    {
        return gold;
    }

    public void AddPlayerGold(int newGold)
    {
        this.gold += newGold;
        Debug.Log("Adding " + newGold + " gold...");
    }

    public void SetPlayerGold(int newGold)
    {
        this.gold = newGold;
    }

    public int GetPlayerTavernTier()
    {
        return tavernTierLevel;
    }
    public void AddPlayerTavernTier(int newTavernTier)
    {
        this.tavernTierLevel += newTavernTier;
    }

    public List<Board> GetPlayerBoard()
    {
        return board;
    }
    public List<Board> GetPlayerCopiedBoard()
    {
        return copiedBoard;
    }

    public List<Board> GetPlayerHand()
    {
        return hand;
    }

    public int GetHealth()
    {
        return health;
    }

    public void AddHealth(int newHealth)
    {
        health += newHealth;
    }
}
