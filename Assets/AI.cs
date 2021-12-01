using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    //jakies bazgroly, rozkminy tak o, o stanach, akcjach i rewardach:
        /*
        akcje(zalezne od posiadanego golda) :
        - kupic miniona(1 lub 2 lub 3 lub 4 lub 5 lub 6) -> wybor dostepny od 3 do 6
        - sprzedac miniona(analogicznie, z tego co gracz posiada na boardzie, od 1 do 7)
        - zagrac miniona na board
        - roll
        - level tawerny
        - freeze(?)
        - swapowanie minionow na boardzie -> ciezko o natychmiastowy reward
        - end turn -> chyba nie samemu, moze w momencie jak sie wykrozysta caly gold?

        -> kiedy konczymy wykonywane akcje? w momencie jak gold zjedzie do 0? jak nie bedzie minionow na lapie(wszystkie juz na boardzie)? jak zostanie wykonana jakas zamiana? jak sprawdzic czy swap na boardzie byl dobry?

        stan:
        - suma att i hp minionow??? -> poisonous traktowany jako giga liczba att???, jak opisac ds?
        - hp chyba nie, moze spadek hp -> im mniejszy spadek lub brak spadku tym lepiej ?

        rewardy:
        - nie stracenie hp -> im wieksza strata tym gorzej?
        - moze zagranie na board jednostki?
        - efektywne zarzadzanie goldem -> tu nie wiadomo jak, bo teoretycznie samo zjechanie do 0 chuja daje, mozna przeciez 10x zakrecic smiglem, moze jak najmniejsza liczba akcji zmniejszajacych golda w procesie wydawania go -> kupno 3 jednostek i 1 roll(4 akcje) za 10 golda jest teoretycznie lepsze od zrobienia 7 rollow i kupna jednej jednostki(8 akcji)
        - triple??? bo jednostki z wyzszego tieru sa lepsze od nizszego(in general)
        - w jakis sposob improvement wymieniajac jednostke 1 tieru na 2/3/etc tieru -> sprzedaz? ale sprzedaz i instant zamiana, tylko w takim przypadku???

        */


    //POSSIBLE ACTIONS:
    //->    show minions in tavern at start of a turn, not using player gold:
    //gc.ShowMinionsInTavern(player, gc.shopSlotsAI, 0);
    //->    buy a chosen minion, choose from shopSlotsAI[]
    //gc.BuyMinionAI(player, minion);
    //->    sell a chosen minion from boardSlotsAI[]
    //gc.SellMinionAI(player, minion);
    //->    roll, use player gold(-1)
    //gc.RefreshMinionsInTavernAI(player); //rolka
    //->    upgrade tavern level with player gold
    //gc.UpgradeTavernLevel(player);
    //->    end turn, probably not a choice, but do at the end of other possible actions
    //gc.EndTurnAI(minionSlotsAI);
    //->    play a chosen minion from handSlotsAI[] to a chosen place at minionSlotsAI[]
    //gc.PlayMinionOnBoard(player, handSlot, minionSlot, gc.handSlotsAI, gc.minionSlotsAI);
    //->    swaps, not working at all tbh
    //gc.SwapNonBlankMinions(minionA, minionB);
    //gc.SwapMinionWithBlank(minion, blank);
    //pick a minion from a discover list, kinda similar to buying, but free
    //gc.ChooseDiscoveredMinionAI(player, minion);

    public struct Action
    {
        string actionName;
        //MinionData minionA;
        //int posMinionA;
        //MinionData minionB;
        //int posMinionB;

        public Action(string newActionName)//, MinionData newMinionA, int a, MinionData newMinionB, int b)
        {
            actionName = newActionName;
            //minionA = newMinionA;
            //posMinionA = a;
            //minionB = newMinionB;
            //posMinionB = b;
        }

        public string GetActionName()
        {
            return actionName;
        }
        public void SetActionName(string newActionName)
        {
            actionName = newActionName;
        }
        /*
        public MinionData GetMinionA()
        {
            return minionA;
        }
        public void SetMinionA(MinionData newMinionA)
        {
            minionA = newMinionA;
        }
        public int GetPosMinionA()
        {
            return posMinionA;
        }
        public void SetPosMinionA(int a)
        {
            posMinionA = a;
        }
        public MinionData GetMinionB()
        {
            return minionB;
        }
        public void SetMinionB(MinionData newMinionB)
        {
            minionB = newMinionB;
        }
        public int GetPosMinionB()
        {
            return posMinionB;
        }
        public void SetPosMinionB(int b)
        {
            posMinionB = b;
        }
        */
    };

    public struct QValue
    {
        public Action action;
        public float value;

        public QValue(Action newAction, float newValue)
        {
            action = newAction;
            value = newValue;
        }

        public Action GetAction()
        {
            return action;
        }
        public void SetAction(Action newAction)
        {
            action = newAction;
        }
        public float GetValue()
        {
            return value;
        }
        public void SetValue(float newValue)
        {
            value = newValue;
        }
    }

    public struct QTable
    {
        public QState state;
        public QValue[] qvalues;
        public string stateStr;

        public QTable(QState newState, QValue[] newQValues, string s)
        {
            state = newState;
            qvalues = newQValues;
            stateStr = s;
        }

        public QState GetState()
        {
            return state;
        }

        public string GetStateStr()
        {
            //stateStr = "EH" + GetState().enemyHealth + "OH" + GetState().ownHealth + "BS" + GetState().boardStats + "G" + GetState().gold + "HC" + GetState().handCounter;

            return stateStr;
        }

        public void SetState(QState newState)
        {
            state = newState;
        }

        public QValue[] GetValues()
        {
            return qvalues;
        }
    };

    public struct History
    {
        public Action pickedAction;
        public QState state;

        public History(Action action, QState s)
        {
            pickedAction = action;
            state = s;
        }
        public Action GetAction()
        {
            return pickedAction;
        }
        public QState GetState()
        {
            return state;
        }
    };

    private GameController gc;
    public Player player;
    private Player enemy;
    private List<Action> allActions;
    private List<Action> possibleActions;
    private GameObject[] minionSlots;
    private GameObject[] handSlots;
    private List<QTable> qTable;
    private Action lastAction;
    private List<History> history;
    private float timerMax = 0;
    private float timer = 0;
    private float learningRate;
    private float discount;

    //public bool learning;
    public float waitTime;
    //public bool readQTable;
    public int gamesToPlay;

    // Start is called before the first frame update
    void Start()
    {
        gc = player.gc;
        allActions = new List<Action>();
        possibleActions = new List<Action>();
        SetupAllActionList();
        qTable = new List<QTable>();

        if (gc.loadQTable == true)
            LoadQTable();

        lastAction = new Action();
        history = new List<History>();
        learningRate = 0.7f;
        discount = 0.5f;

        if (player.name == "Player1")
        {
            minionSlots = gc.minionSlots;
            handSlots = gc.handSlots;
            enemy = gc.player2;
        }
        else if(player.name == "Player2")
        {
            minionSlots = gc.minionSlotsAI;
            handSlots = gc.handSlotsAI;
            enemy = gc.player1;
        }
        else
        {
            Debug.Log("Error with getting minionSlots!");
        }
    }
    private bool Waited(float seconds)
    {
        timerMax = seconds;
        timer += Time.deltaTime;
        if (timer >= timerMax)
        {
            return true; //max reached - waited x - seconds
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        waitTime = gc.waitTimeAI.value;
        gc.waitTimeAIText.text = waitTime.ToString();

        //if(Input.GetKeyDown(KeyCode.C) && player.turn == true)
        if (player.turn == true)// && player.GetPlayerGold() > 0)
        {
            //GeneratePossibleActionsList();
            //Debug.Log("Possible actions number = " + possibleActions.Count);
            //UseRandomGameMechanic();
            //Learn();
            if (Waited(waitTime) == true && gc.learning == true)
                Learn();
            else if (Waited(waitTime) == true && gc.learning == false && gc.loadQTable == true)
                Learned();
            else if (Waited(waitTime) == true && gc.learning == false && gc.loadQTable == false)
                RandomActions();
            else
                return;
            timer = 0.0f;
        }/*
        else if (player.turn == true && player.GetPlayerGold() == 0)
        {
            gc.EndTurnAI(minionSlots);
            Debug.Log("No gold left!");
        }
        }
        if(Input.GetKey(KeyCode.P))
        {
            QState q = new QState();
            q = GetActualQState();
            string s = GetActualQStateStr(q);
            Debug.Log("QSTATE ACT lastR: " + q.lastFightResult + ", CONVERTED lastR: " + GetQStateFromStr(s).lastFightResult +
                "QSTATE ACT gmC: " + q.goldenMinionCounter + ", CONVERTED gmC: " + GetQStateFromStr(s).goldenMinionCounter +
                    "QSTATE ACT BS: " + q.boardCounter + ", CONVERTED bs: " + GetQStateFromStr(s).boardStats +
                        "QSTATE ACT gold: " + q.gold + ", CONVERTED gold: " + GetQStateFromStr(s).gold +
                            "QSTATE ACT HC: " + q.handCounter + ", CONVERTED HC: " + GetQStateFromStr(s).handCounter +
                                "QSTATE ACT BC: " + q.boardCounter + ", CONVERTED bc: " + GetQStateFromStr(s).boardCounter +
                                    "QSTATE ACT TT: " + q.tavernTier + ", CONVERTED TT: " + GetQStateFromStr(s).tavernTier);
        }*/
        if(Input.GetKey(KeyCode.P))
        {
            gc.EndTurnAI(minionSlots);
        }
    }

    public GameObject[] GetShopSlots()
    {
        return gc.shopSlotsAI;
    }

    public GameObject[] GetBoardSlots()
    {
        return gc.minionSlotsAI;
    }

    public GameObject[] GetHandSlots()
    {
        return gc.handSlotsAI;
    }

    public GameObject[] GetDiscoverSlots()
    {
        return gc.discoverSlots;
    }

    public void SetupAllActionList()
    {
        allActions.Clear();
        //if minionA || minionB == null -> useless
        //if posMinionA || posMinionB == 99 -> useless

        //buy
        Action buy = new Action("buy");
        allActions.Add(buy);
        /*
        //buy minion, minionA -> slot pos from shop to buy, minionB -> w/e
        for(int i = 0; i < GetShopSlots().Length; i++)
        {
            Action buy = new Action("buy", GetShopSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            allActions.Add(buy);
        }
        */
        //sell
        Action sell = new Action("sell");
        allActions.Add(sell);
        /*
        //sell minion, minionA -> slot pos on board to sell, minionB -> w/e
        for (int i = 0; i < GetBoardSlots().Length; i++)
        {
            Action sell = new Action("sell", GetBoardSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            allActions.Add(sell);
        }
        */
        //play minion, minionA -> slot pos from hand, minionB -> slot pos on board
        //for (int i = 0; i < GetHandSlots().Length; i++)
        if(GetHandSlots().Length > 0)
        {
            //find first free spot
            int free = 99;
            for (int j = 0; j < GetBoardSlots().Length; j++)
            {
                if(GetBoardSlots()[j].GetComponent<Minion>().blank == true)
                {
                    free = j;
                    break;
                }
            }
            if(free != 99)
            {
                Action play = new Action("play");
                //Action play = new Action("play", GetHandSlots()[i].GetComponent<Minion>().GetMinion(), i, GetBoardSlots()[free].GetComponent<Minion>().GetMinion(), free);
                allActions.Add(play);
            }
            /*
            for(int j = 0; j < GetBoardSlots().Length; j++)
            {
                Action play = new Action("play", GetHandSlots()[i].GetComponent<Minion>().GetMinion(), i, GetBoardSlots()[j].GetComponent<Minion>().GetMinion(), j);
                allActions.Add(play);
            }*/
        }
        //rolka
        //Action roll = new Action("roll", null, 99, null, 99);
        Action roll = new Action("roll");
        allActions.Add(roll);
        //upgrade tavern level
        //Action upgrade = new Action("upgrade", null, 99, null, 99);
        Action upgrade = new Action("upgrade");
        allActions.Add(upgrade);
        //end turn -> chyba nie powinno byc w akcjach, zeby nie konczyl randomowo, chociaz moze?
        //Action endTurn = new Action("end", null, 99, null, 99);
        Action endTurn = new Action("end");
        allActions.Add(endTurn);
        //pick minion from discover list -> only possible when made golden
        //for(int i = 0; i < GetDiscoverSlots().Length; i++)
        if(GetDiscoverSlots().Length > 0)
        {
            //Action pick = new Action("pick", GetDiscoverSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            Action pick = new Action("pick");
            allActions.Add(pick);
        }
        //swapy -> na razie bugged
    }

    public void GeneratePossibleActionsList()
    {
        possibleActions.Clear();

        int goldState = player.GetPlayerGold();
        int numberOfMinionsInTavern = 0;
        if (player.GetPlayerTavernTier() == 1)
        {
            numberOfMinionsInTavern = 3;
        }
        else if (player.GetPlayerTavernTier() == 2)
        {
            numberOfMinionsInTavern = 4;
        }
        else if (player.GetPlayerTavernTier() == 3)
        {
            numberOfMinionsInTavern = 4;
        }
        else if (player.GetPlayerTavernTier() == 4)
        {
            numberOfMinionsInTavern = 5;
        }
        else if (player.GetPlayerTavernTier() == 5)
        {
            numberOfMinionsInTavern = 5;
        }
        else if (player.GetPlayerTavernTier() == 6)
        {
            numberOfMinionsInTavern = 6;
        }
        else
            Debug.Log("MaxTavernTier error!");

        //cos jest pojebanego tutaj chyba
        if(gc.discoverPanel.activeSelf == false)
        {
            //buy
            if (goldState >= 3)
            {
                //check for space on hand
                int blankCounter = 0;
                for (int i = 0; i < GetHandSlots().Length; i++)
                {
                    if (GetHandSlots()[i].GetComponent<Minion>().blank == true)
                    {
                        blankCounter++;
                    }
                }
                if (blankCounter > 0)
                {
                    /*
                    for (int i = 0; i < numberOfMinionsInTavern; i++)
                    {
                        Action buy = new Action("buy", GetShopSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
                        possibleActions.Add(buy);
                    }
                    */
                    Action buy = new Action("buy");
                    possibleActions.Add(buy);
                }
            }
            //sell
            //check for minions on board
            int minionBoardCounter = 0;
            for (int m = 0; m < GetBoardSlots().Length; m++)
            {
                if (GetHandSlots()[m].GetComponent<Minion>().blank == false)
                {
                    minionBoardCounter++;
                }
            }
            if (minionBoardCounter > 0)
            {
                //for (int i = 0; i < GetBoardSlots().Length; i++)
                //{
                //if (GetBoardSlots()[i].GetComponent<Minion>().blank == false)
                //{
                //Action sell = new Action("sell", GetBoardSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
                //}
                //}

                Action sell = new Action("sell");
                possibleActions.Add(sell);
            }
            //play
            //check for a space on board
            //if (minionBoardCounter < GetBoardSlots().Length)
            //{
            //for (int i = 0; i < GetHandSlots().Length; i++)
            int handCounter = 99;
            for (int i = 0; i < GetHandSlots().Length; i++)
            {
                if (GetHandSlots()[i].GetComponent<Minion>().blank == false)
                {
                    handCounter = i;
                    break;
                }
            }
            if (handCounter != 99)
            {
                //find first free spot
                int free = 99;
                for (int j = 0; j < GetBoardSlots().Length; j++)
                {
                    if (GetBoardSlots()[j].GetComponent<Minion>().blank == true)
                    {
                        free = j;
                        break;
                    }
                }
                
                if (free != 99)
                {
                    //Action play = new Action("play", GetHandSlots()[i].GetComponent<Minion>().GetMinion(), i, GetBoardSlots()[free].GetComponent<Minion>().GetMinion(), free);
                    Action play = new Action("play");
                    possibleActions.Add(play);
                }
                    
                }
            //}
            //roll
            if (goldState >= 1)
            {
                //Action roll = new Action("roll", null, 99, null, 99);
                Action roll = new Action("roll");
                possibleActions.Add(roll);
            }
            //upgrade
            if (player.GetPlayerTavernTier() < 6 && goldState >= player.tavernTierUpgradeGold)
            {
                //Action upgrade = new Action("upgrade", null, 99, null, 99);
                Action upgrade = new Action("upgrade");
                possibleActions.Add(upgrade);
            }
            //end turn
            if(player.GetPlayerGold() <= 0)
            {
                //Action end = new Action("end", null, 99, null, 99);
                Action end = new Action("end");
                possibleActions.Add(end);
            }
        }
        //pick
        else if(gc.discoverPanel.activeSelf == true)
        {
            //for(int i = 0; i < gc.discoverSlots.Length; i++)
            //{
            //Action pick = new Action("pick", GetDiscoverSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            //Action pick = new Action("pick");
            //possibleActions.Add(pick);
            //}
            Action pick = new Action("pick");
            possibleActions.Add(pick);
        }

        //if(possibleActions.Count > 0)
            //SavePossibleActions();
    }

    public void UseRandomGameMechanic()
    {
        if(possibleActions.Count > 0)
        {
            int random = Random.Range(0, possibleActions.Count);
            Action chosenAction = possibleActions[random];
            lastAction = chosenAction;
            PlaySelectedAction(chosenAction);
        }
        else
            Debug.Log("No possible actions!");
    }

    public int PlaySelectedAction(Action chosenAction)
    {
        if (chosenAction.GetActionName() != null)
        {
            if (chosenAction.GetActionName() == "buy")
            {
                int shop = 0;
                for (int i = 0; i < GetShopSlots().Length; i++)
                {
                    if (GetShopSlots()[i].GetComponent<Minion>().blank == false)
                    {
                        shop++;
                    }
                }

                int random = 99;
                if (shop > 0)
                {
                    random = Random.Range(0, shop);
                    if (random != 99)
                        return gc.BuyMinionAI(player, GetShopSlots()[random]);
                    else return -1;
                }
                else
                    return -1;
            }
            else if (chosenAction.GetActionName() == "sell")
            {
                List<int> board = new List<int>();
                for (int i = 0; i < GetBoardSlots().Length; i++)
                {
                    if (GetBoardSlots()[i].GetComponent<Minion>().blank == false)
                    {
                        board.Add(i);
                    }
                }

                int random = 99;
                if (board.Count > 0)
                {
                    random = Random.Range(0, board.Count);
                    if (random != 99)
                        return gc.SellMinionAI(player, GetBoardSlots()[board[random]]);
                    else return -1;
                }
                else return -1;
            }
            else if (chosenAction.GetActionName() == "play")
            {
                //look for a minion on hand
                List<int> hand = new List<int>();
                for (int i = 0; i < GetHandSlots().Length; i++)
                {
                    if (GetHandSlots()[i].GetComponent<Minion>().blank == false)
                    {
                        hand.Add(i);
                    }
                }
                if (hand.Count > 0)
                {
                    int free = 99;
                    for (int i = 0; i < GetBoardSlots().Length; i++)
                    {
                        if (GetBoardSlots()[i].GetComponent<Minion>().blank == true)
                        {
                            free = i;
                            break;
                        }
                    }
                    if (free != 99)
                    {
                        int random = 99;
                        random = Random.Range(0, hand.Count);
                        return gc.PlayMinionOnBoard(player, GetHandSlots()[hand[random]], GetBoardSlots()[free], GetHandSlots(), GetBoardSlots());
                    }
                    else
                        return -1;
                }
                else
                    return -1;

                //gc.PlayMinionOnBoard(player, GetHandSlots()[chosenAction.GetPosMinionA()], GetBoardSlots()[chosenAction.GetPosMinionB()],
                //GetHandSlots(), GetBoardSlots());
            }
            else if (chosenAction.GetActionName() == "roll")
            {
                return gc.RefreshMinionsInTavernAI(player);
            }
            else if (chosenAction.GetActionName() == "upgrade")
            {
                return gc.UpgradeTavernLevel(player);
            }
            else if (chosenAction.GetActionName() == "pick")
            {
                int random = Random.Range(0, 3);

                return gc.ChooseDiscoveredMinionAI(player, GetDiscoverSlots()[random]);
            }
            else if (chosenAction.GetActionName() == "end")
            {
                return gc.EndTurnAI(GetBoardSlots());
            }
            else return -1;
            
        }
        else return -1;
    }

    public QState GetActualQState()
    {
        QState actualQState = new QState();
        /*
        int actualBoardStats = 0;
        for (int i = 0; i < minionSlots.Length; i++)
        {
            if (minionSlots[i].GetComponent<Minion>().blank == false)
            {
                int attack = minionSlots[i].GetComponent<Minion>().GetMinion().Attack;
                int health = minionSlots[i].GetComponent<Minion>().GetMinion().Hp;
                actualBoardStats = attack + health;
            }
        }*/
        int actualHandSlotsCounter = 0;
        for (int i = 0; i < handSlots.Length; i++)
        {
            if (handSlots[i].GetComponent<Minion>().blank == false)
            {
                actualHandSlotsCounter++;
            }
        }
        int actualBoardCounter = 0;
        for (int i = 0; i < minionSlots.Length; i++)
        {
            if (minionSlots[i].GetComponent<Minion>().blank == false)
            {
                actualBoardCounter++;
            }
        }
        int lastResult = 1;
        if(player.fight == true)
        {
            lastResult = player.lastResult;
            player.fight = false;
            enemy.fight = false;
        }
        actualQState.Initialize(lastResult, //player.goldenMinionCounter, actualBoardStats, 
            player.GetPlayerGold(), actualHandSlotsCounter, actualBoardCounter, player.GetPlayerTavernTier());

        return actualQState;
    }

    public string GetActualQStateStr(QState state)
    {
        string s = "LR" + state.lastFightResult + //"GC" + state.goldenMinionCounter + "BS" + state.boardStats + 
            "GO" + state.gold  + "HC" + state.handCounter + "BC" + state.boardCounter + "TT" + state.tavernTier;

        return s;
    }


    public QState GetQStateFromStr(string str)
    {
        QState q = new QState();

        
        int lr0 = 2;
        int lr1 = str.IndexOf("GO");
        string lr = str.Substring(lr0, lr1 - lr0);
        int lr_i = int.Parse(lr);
        /*
        int gc0 = lr1 + 2;
        int gc1 = str.IndexOf("BS");
        string gc = str.Substring(gc0, gc1 - gc0);
        int gc_i = int.Parse(gc);

        int bs0 = gc1 + 2;
        int bs1 = str.IndexOf("GO");
        string bs = str.Substring(bs0, bs1 - bs0);
        int bs_i = int.Parse(bs);
        */
        //int go0 = bs1 + 2;
        //int go0 = 2;
        int go0 = lr1 + 2;
        int go1 = str.IndexOf("HC");
        string go = str.Substring(go0, go1 - go0);
        int go_i = int.Parse(go);

        int hc0 = go1 + 2;
        int hc1 = str.IndexOf("BC");
        string hc = str.Substring(hc0, hc1 - hc0);
        int hc_i = int.Parse(hc);

        int bc0 = hc1 + 2;
        int bc1 = str.IndexOf("TT");
        string bc = str.Substring(bc0, bc1 - bc0);
        int bc_i = int.Parse(bc);

        int tt0 = bc1 + 2;
        int tt1 = str.Length;
        string tt = str.Substring(tt0, tt1 - tt0);
        int tt_i = int.Parse(tt);

        q.Initialize(lr_i, //gc_i, bs_i, 
            go_i, hc_i, bc_i, tt_i);

        return q;
    }


    public void Learn()
    {
        Debug.Log("Learning...");
        //StartCoroutine(Wait(1));
        QState actQState = GetActualQState();
        string actQStateStr = GetActualQStateStr(actQState);

        //check if is in table
        bool inTable = false;
        for(int i = 0; i < qTable.Count; i++)
        {
            if(qTable[i].GetStateStr() == actQStateStr)
            {
                inTable = true;
                break;
            }
        }
        

        if (inTable == true)
        {
            //greedy
            float r = Random.Range(0, 1);
            //random action
            if(r < 0.1f)
            {
                GeneratePossibleActionsList();
                UseRandomGameMechanic();
            }
            //best action
            else
            {
                List<float> current = new List<float>();
                int statePos = -1;
                int iter = -1;
                float maxValue = -99.0f;
                for(int i = 0; i < qTable.Count; i++)
                {
                    if(qTable[i].GetStateStr() == actQStateStr)
                    {
                        statePos = i;
                        for (int j = 0; j < qTable[i].GetValues().Length; j++)
                        {
                            if(qTable[i].GetValues()[j].GetValue() != 0.0f)
                                current.Add(qTable[i].GetValues()[j].GetValue());
                        }
                        break;
                    }
                }
                for (int i = 0; i < current.Count; i++)
                {
                    if(current[i] > maxValue)
                    {
                        maxValue = current[i];
                        iter = i;
                    }
                }
                //action with highest qvalue
                Action bestAction = qTable[statePos].GetValues()[iter].GetAction();
                PlaySelectedAction(bestAction);

            }
        }
        else if (inTable == false)
        {
            QState state = actQState;
            string stateStr = actQStateStr;
            QValue[] qvalues = new QValue[allActions.Count];
            GeneratePossibleActionsList();
            UseRandomGameMechanic();
            for (int i = 0; i < allActions.Count; i++)
            {
                if (allActions[i].GetActionName() != lastAction.GetActionName())// && allActions[i].GetMinionA() != lastAction.GetMinionA() &&
                    //allActions[i].GetMinionB() != lastAction.GetMinionB() && allActions[i].GetPosMinionA() != lastAction.GetPosMinionA() &&
                    //allActions[i].GetPosMinionB() != lastAction.GetPosMinionB())
                {
                    QValue q = new QValue(allActions[i], gc.initValueQTable);
                    qvalues[i] = q;
                }
                else
                {
                    QValue q = new QValue(lastAction, gc.initValueQTable);
                    qvalues[i] = q;
                }
            }
            
            QTable qEl = new QTable(state, qvalues, stateStr);
            qTable.Add(qEl);
        }

        //save played action for this state
        History h = new History(lastAction, actQState);
        history.Add(h);
        Debug.Log(history.Count);

        //update table
        History last = history[history.Count - 1];
        int reward = 0;
        for(int i = history.Count - 1; i > 0; i--)
        {
            if (player.dead == true) //negative reward and no next state
            {
                QState state = last.GetState();
                string stateStr = GetActualQStateStr(state);
                Action takenAction = last.GetAction();
                reward = -1;
                for(int j = 0; j < qTable.Count; j++)
                {
                    if(qTable[j].GetStateStr() == stateStr)
                    {
                        for(int x = 0; x < qTable[j].GetValues().Length; x++)
                        {
                            if(qTable[j].GetValues()[x].GetAction().GetActionName() == takenAction.GetActionName())// && 
                                //qTable[j].GetValues()[x].GetAction().GetMinionA() == takenAction.GetMinionA() &&
                                    //qTable[j].GetValues()[x].GetAction().GetMinionB() == takenAction.GetMinionB() && 
                                        //qTable[j].GetValues()[x].GetAction().GetPosMinionA() == takenAction.GetPosMinionA() &&
                                            //qTable[j].GetValues()[x].GetAction().GetPosMinionB() == takenAction.GetPosMinionB())
                            {
                                float newValue = (1 - learningRate) * qTable[j].GetValues()[x].GetValue() + learningRate * reward;
                                qTable[j].GetValues()[x].SetValue(newValue);

                                break;
                            }
                        }

                        break;
                    }
                }

                //clear history, because player dead
                history.Clear();
                player.dead = false;
                enemy.dead = false;
            }
            else if(player.dead == false && player.turnNumber != 1)     //player alive so other states available, rewards for won/lost/drawn fight, making golden minion
            {
                QState currentState = last.GetState();
                QState previousState = history[i - 1].GetState();
                Action previousAction = history[i - 1].GetAction();

                //rewards for winning/losing fight & making golden
                if (currentState.lastFightResult == 0)
                {
                    reward = -1;
                }
                else if (currentState.lastFightResult == 1)
                {
                    reward = 0;
                }
                else if (currentState.lastFightResult == 2)
                {
                    reward = 1;
                }
                /*
                //what about making minion golden(?)
                if (currentState.GetGoldenMinionCounter() > previousState.GetGoldenMinionCounter())
                {
                    reward = 1;
                }
                */
                string previousStateStr = GetActualQStateStr(previousState);
                string currentStateStr = GetActualQStateStr(currentState);
                float maxValueAtCurrentState = -99.0f;
                List<float> temp = new List<float>();
                for (int z = 0; z < qTable.Count; z++)
                {
                    if (qTable[z].GetStateStr() == currentStateStr)
                    {
                        for (int c = 0; c < qTable[z].GetValues().Length; c++)
                        {
                            temp.Add(qTable[z].GetValues()[c].GetValue());
                        }
                        break;
                    }
                }
                for (int z = 0; z < temp.Count; z++)
                {
                    if (temp[z] > maxValueAtCurrentState)
                    {
                        maxValueAtCurrentState = temp[z];
                    }
                }

                for (int j = 0; j < qTable.Count; j++)
                {
                    if (qTable[j].GetStateStr() == previousStateStr)
                    {
                        for (int x = 0; x < qTable[j].GetValues().Length; x++)
                        {
                            if (qTable[j].GetValues()[x].GetAction().GetActionName() == previousAction.GetActionName())// &&
                                //qTable[j].GetValues()[x].GetAction().GetMinionA() == previousAction.GetMinionA() &&
                                    //qTable[j].GetValues()[x].GetAction().GetMinionB() == previousAction.GetMinionB() &&
                                        //qTable[j].GetValues()[x].GetAction().GetPosMinionA() == previousAction.GetPosMinionA() &&
                                            //qTable[j].GetValues()[x].GetAction().GetPosMinionB() == previousAction.GetPosMinionB())
                            {
                                float newValue = (1 - learningRate) * qTable[j].GetValues()[x].GetValue() + learningRate * (reward + discount * maxValueAtCurrentState);
                                qTable[j].GetValues()[x].SetValue(newValue);

                                break;
                            }
                        }

                        break;
                    }
                }
            }
            //Debug.Log("Updated Q Table!");
        }
    }

    struct ActionPick {
        string actionName;
        float value;

        public ActionPick(string newActionName, float newValue)
        {
            actionName = newActionName;
            value = newValue;
        }

        public string GetActionName()
        {
            return actionName;
        }
        public void SetActionName(string a)
        {
            actionName = a;
        }
        public float GetValue()
        {
            return value;
        }
        public void SetValue(float a)
        {
            value = a;
        }
    };

    int SortByValue(ActionPick a1, ActionPick a2)
    {
        return a2.GetValue().CompareTo(a1.GetValue());
    }

    public void Learned()
    {
        Debug.Log("Learning finished, making best moves!");
        //StartCoroutine(Wait(1));
        QState actQState = GetActualQState();
        string actQStateStr = GetActualQStateStr(actQState);

        //check if is in table
        bool inTable = false;
        for (int i = 0; i < qTable.Count; i++)
        {
            if (qTable[i].GetStateStr() == actQStateStr)
            {
                inTable = true;
                break;
            }
        }


        if (inTable == true)
        {
            List<ActionPick> actionPicks = new List<ActionPick>();
            actionPicks.Clear();
            int statePos = -1;
            //best action
            for (int i = 0; i < qTable.Count; i++)
            {
                if (qTable[i].GetStateStr() == actQStateStr)
                {
                    statePos = i;
                    for (int j = 0; j < qTable[i].GetValues().Length; j++)
                    {
                        if(qTable[i].GetValues()[j].GetValue() != 0)
                        {
                            ActionPick a = new ActionPick(qTable[i].GetValues()[j].GetAction().GetActionName(), qTable[i].GetValues()[j].GetValue());
                            actionPicks.Add(a);
                        }
                    }
                }
            }
            actionPicks.Sort(SortByValue);
            for(int i = 0; i < actionPicks.Count; i++)
            {
                Debug.Log("actionPicks[" + actionPicks[i].GetActionName() + "]: " + actionPicks[i].GetValue());
            }

            //action with highest qvalue
            int test = 99;

            if (actionPicks.Count == 0)
                gc.EndTurnAI(minionSlots);

            for(int i = 0; i < actionPicks.Count; i++)
            {
                Action bestAction = new Action(actionPicks[i].GetActionName());
                test = PlaySelectedAction(bestAction);

                if (test == 0)
                {
                    gc.actionsEachTurn += bestAction.GetActionName() + ", ";
                    break;
                }
                else
                    continue;
            }
            
        }
        else if (inTable == false)
        {
            //QState state = actQState;
            //string stateStr = actQStateStr;
            //QValue[] qvalues = new QValue[allActions.Count];
            GeneratePossibleActionsList();
            UseRandomGameMechanic();
        }
    }

    public void RandomActions()
    {
        if(gc.loadQTable == false && gc.learning == false)
        {
            GeneratePossibleActionsList();
            UseRandomGameMechanic();
        }
    }

    /*
    public void SaveQTable()
    {
        //1 element: QState stan (lastFightResult, goldenMinionCounter, boardStats, gold, handCounter, boardCounter, tavernTier),
        //           lista QValues[] -> QValue to: Action action i float Value,
        //           string QStateStr.

        string path = "Assets/Resources/q.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);

        for(int i = 0; i < qTable.Count; i++)
        {
            writer.WriteLine(qTable[i].GetStateStr());
            for(int j = 0; j < qTable[i].GetValues().Length; j++)
            {
                
                if (qTable[i].GetValues()[j].GetAction().GetMinionA() != null)
                {
                    writer.WriteLine(qTable[i].GetValues()[j].GetAction().GetActionName() + "," + qTable[i].GetValues()[j].GetAction().GetMinionA().Name + "," +
                    qTable[i].GetValues()[j].GetAction().GetPosMinionA() + "," + qTable[i].GetValues()[j].GetValue());
                }
                else
                {
                    writer.WriteLine(qTable[i].GetValues()[j].GetAction().GetActionName() + "," + 
                    qTable[i].GetValues()[j].GetAction().GetPosMinionA() + "," + qTable[i].GetValues()[j].GetValue());
                }
                
                writer.WriteLine(qTable[i].GetValues()[j].GetAction().GetActionName() + ": " + qTable[i].GetValues()[j].GetValue());
            }
        }

        writer.Close();
    }
    */
    public void LoadQTable()
    {
        if(gc.loadQTable == true)
        {
            TextAsset t = Resources.Load<TextAsset>("qBEST");
            //string path = "Assets/Resources/qBEST.txt";
             //File.ReadAllLines(path).ToList();
            string [] qlines = t.text.Split("\n"[0]);
            //Debug.Log(qlines.Count);
            QState q = new QState();
            string s = "";
            QValue buy = new QValue();
            QValue sell = new QValue();
            QValue play = new QValue();
            QValue roll = new QValue();
            QValue upgrade = new QValue();
            QValue end = new QValue();
            QValue pick = new QValue();

            for (int i = 0; i < qlines.Length; i++)
            {
                if (i%8 == 0)
                {
                    s = qlines[i];
                    q.Initialize(GetQStateFromStr(s).lastFightResult, GetQStateFromStr(s).gold, GetQStateFromStr(s).handCounter, GetQStateFromStr(s).boardCounter, 
                        GetQStateFromStr(s).tavernTier);
                    
                }
                else if(i%8 == 1)
                {
                    Action abuy = new Action("buy");
                    buy = new QValue(abuy, float.Parse(qlines[i].Substring(5)));
                }
                else if (i % 8 == 2)
                {
                    Action asell = new Action("sell");
                    sell = new QValue(asell, float.Parse(qlines[i].Substring(6)));
                }
                else if (i % 8 == 3)
                {
                    Action aplay = new Action("play");
                    play = new QValue(aplay, float.Parse(qlines[i].Substring(6)));
                }
                else if (i % 8 == 4)
                {
                    Action aroll = new Action("roll");
                    roll = new QValue(aroll, float.Parse(qlines[i].Substring(6)));
                }
                else if (i % 8 == 5)
                {
                    Action aupgrade = new Action("upgrade");
                    upgrade = new QValue(aupgrade, float.Parse(qlines[i].Substring(9)));
                }
                else if (i % 8 == 6)
                {
                    Action aend = new Action("end");
                    end = new QValue(aend, float.Parse(qlines[i].Substring(5)));
                }
                else if (i % 8 == 7)
                {
                    Action apick = new Action("pick");
                    pick = new QValue(apick, float.Parse(qlines[i].Substring(6)));

                    QValue[] newQValues = new QValue[]{ buy, sell, play, roll, upgrade, end, pick };
                    QTable newQTable = new QTable(q, newQValues, s);

                    qTable.Add(newQTable);
                }
            }
            Debug.Log(qTable.Count);

            /*
            //edit winsNR for stats
            string pathW = "Assets/Resources/winsX.txt";
            List<string> wins = File.ReadAllLines(pathW).ToList();
            string str = wins[wins.Count - 1];
            int str1 = 0;
            int str2 = str.IndexOf(" ");
            string gameNR = str.Substring(str1, str2 - str1);
            int gameNR_i = int.Parse(gameNR);
            gc.gameNr = gameNR_i + 1;
            */
        }
    }

    public void SavePossibleActions()
    {
        string path = "Assets/Resources/a.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("##################");

        for (int i = 0; i < possibleActions.Count; i++)
        {
            writer.WriteLine(possibleActions[i].GetActionName());
        }

        writer.Close();
    }

    public void ResetHistory()
    {
        history.Clear();
    }
}
