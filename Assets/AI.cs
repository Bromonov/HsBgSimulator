using System.Collections;
using System.Collections.Generic;
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
        MinionData minionA;
        int posMinionA;
        MinionData minionB;
        int posMinionB;

        public Action(string newActionName, MinionData newMinionA, int a, MinionData newMinionB, int b)
        {
            actionName = newActionName;
            minionA = newMinionA;
            posMinionA = a;
            minionB = newMinionB;
            posMinionB = b;
        }

        public string GetActionName()
        {
            return actionName;
        }
        public void SetActionName(string newActionName)
        {
            actionName = newActionName;
        }
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
        public string stateStr;

        public History(Action action, string s)
        {
            pickedAction = action;
            stateStr = s;
        }
        public Action GetAction()
        {
            return pickedAction;
        }
        public string GetStateStr()
        {
            return stateStr;
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

    // Start is called before the first frame update
    void Start()
    {
        gc = player.gc;
        allActions = new List<Action>();
        possibleActions = new List<Action>();
        SetupAllActionList();
        qTable = new List<QTable>();
        lastAction = new Action();
        history = new List<History>();

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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            //GeneratePossibleActionsList();
            //Debug.Log("Possible actions number = " + possibleActions.Count);
            //UseRandomGameMechanic();
            Learn();
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
        //if minionA || minionB == null -> useless
        //if posMinionA || posMinionB == 99 -> useless
        //buy minion, minionA -> slot pos from shop to buy, minionB -> w/e
        for(int i = 0; i < GetShopSlots().Length; i++)
        {
            Action buy = new Action("buy", GetShopSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            allActions.Add(buy);
        }
        //sell minion, minionA -> slot pos on board to sell, minionB -> w/e
        for (int i = 0; i < GetBoardSlots().Length; i++)
        {
            Action sell = new Action("sell", GetBoardSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
            allActions.Add(sell);
        }
        //play minion, minionA -> slot pos from hand, minionB -> slot pos on board
        for (int i = 0; i < GetHandSlots().Length; i++)
        {
            for(int j = 0; j < GetBoardSlots().Length; j++)
            {
                Action play = new Action("play", GetHandSlots()[i].GetComponent<Minion>().GetMinion(), i, GetBoardSlots()[j].GetComponent<Minion>().GetMinion(), j);
                allActions.Add(play);
            }
        }
        //rolka
        Action roll = new Action("roll", null, 99, null, 99);
        allActions.Add(roll);
        //upgrade tavern level
        Action upgrade = new Action("upgrade", null, 99, null, 99);
        allActions.Add(upgrade);
        //end turn -> chyba nie powinno byc w akcjach, zeby nie konczyl randomowo, chociaz moze?
        //Action endTurn = new Action("end", 99, 99);
        //allActions.Add(endTurn);
        //pick minion from discover list -> only possible when made golden
        for(int i = 0; i < GetDiscoverSlots().Length; i++)
        {
            Action pick = new Action("pick", GetDiscoverSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
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
                    for (int i = 0; i < numberOfMinionsInTavern; i++)
                    {
                        Action buy = new Action("buy", GetShopSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
                        possibleActions.Add(buy);
                    }
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
                for (int i = 0; i < GetBoardSlots().Length; i++)
                {
                    if (GetBoardSlots()[i].GetComponent<Minion>().blank == false)
                    {
                        Action sell = new Action("sell", GetBoardSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
                        possibleActions.Add(sell);
                    }

                }
            }
            //play
            //check for a space on board
            if (minionBoardCounter < GetBoardSlots().Length)
            {
                for (int i = 0; i < GetHandSlots().Length; i++)
                {
                    if (GetHandSlots()[i].GetComponent<Minion>().blank == false)
                    {
                        for (int j = 0; j < GetBoardSlots().Length; j++)
                        {
                            if (GetBoardSlots()[j].GetComponent<Minion>().blank == true)
                            {
                                Action play = new Action("play", GetHandSlots()[i].GetComponent<Minion>().GetMinion(), i,
                                    GetBoardSlots()[j].GetComponent<Minion>().GetMinion(), j);
                                possibleActions.Add(play);
                            }
                        }
                    }
                }
            }
            //roll
            if (goldState >= 1)
            {
                Action roll = new Action("roll", null, 99, null, 99);
                possibleActions.Add(roll);
            }
            //upgrade
            if (player.GetPlayerTavernTier() < 6 && goldState <= player.tavernTierUpgradeGold)
            {
                Action upgrade = new Action("upgrade", null, 99, null, 99);
                possibleActions.Add(upgrade);
            }
        }
        //pick
        else if(gc.discoverPanel.activeSelf == true)
        {
            for(int i = 0; i < gc.discoverSlots.Length; i++)
            {
                Action pick = new Action("pick", GetDiscoverSlots()[i].GetComponent<Minion>().GetMinion(), i, null, 99);
                possibleActions.Add(pick);
            }
        }
    }

    public void UseRandomGameMechanic()
    {
        int random = Random.Range(0, possibleActions.Count);
        Action chosenAction = possibleActions[random];
        lastAction = chosenAction;
        PlaySelectedAction(chosenAction);
    }

    public void PlaySelectedAction(Action chosenAction)
    {
        if (chosenAction.GetActionName() != null)
        {
            if (chosenAction.GetActionName() == "buy")
            {
                gc.BuyMinionAI(player, GetShopSlots()[chosenAction.GetPosMinionA()]);
            }
            else if (chosenAction.GetActionName() == "sell")
            {
                gc.SellMinionAI(player, GetBoardSlots()[chosenAction.GetPosMinionA()]);
            }
            else if (chosenAction.GetActionName() == "play")
            {
                gc.PlayMinionOnBoard(player, GetHandSlots()[chosenAction.GetPosMinionA()], GetBoardSlots()[chosenAction.GetPosMinionB()],
                    GetHandSlots(), GetBoardSlots());
            }
            else if (chosenAction.GetActionName() == "roll")
            {
                gc.RefreshMinionsInTavernAI(player);
            }
            else if (chosenAction.GetActionName() == "upgrade")
            {
                gc.UpgradeTavernLevel(player);
            }
            else if (chosenAction.GetActionName() == "pick")
            {
                gc.ChooseDiscoveredMinionAI(player, GetDiscoverSlots()[chosenAction.GetPosMinionA()]);
            }
        }
    }

    public QState GetActualQState()
    {
        QState actualQState = new QState();
        int actualBoardStats = 0;
        for (int i = 0; i < minionSlots.Length; i++)
        {
            if (minionSlots[i].GetComponent<Minion>().blank == false)
            {
                int attack = minionSlots[i].GetComponent<Minion>().GetMinion().Attack;
                int health = minionSlots[i].GetComponent<Minion>().GetMinion().Hp;
                actualBoardStats = attack + health;
            }
        }
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
        actualQState.Initialize(enemy.GetHealth(), player.GetHealth(), actualBoardStats, player.GetPlayerGold(), actualHandSlotsCounter, actualBoardCounter);

        return actualQState;
    }

    public string GetActualQStateStr(QState state)
    {
        string s = "EH" + state.enemyHealth + "OH" + state.ownHealth + "BS" + state.boardStats + "G" + state.gold + "HC" + state.handCounter;

        return s;
    }


    public void Learn()
    {
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
                if (allActions[i].GetActionName() != lastAction.GetActionName() && allActions[i].GetMinionA() != lastAction.GetMinionA() &&
                    allActions[i].GetMinionB() != lastAction.GetMinionB() && allActions[i].GetPosMinionA() != lastAction.GetPosMinionA() &&
                    allActions[i].GetPosMinionB() != lastAction.GetPosMinionB())
                {
                    QValue q = new QValue(allActions[i], 0.0f);
                    qvalues[i] = q;
                }
                else
                {
                    QValue q = new QValue(lastAction, 0.0f);
                    qvalues[i] = q;
                }
            }
            
            QTable qEl = new QTable(state, qvalues, stateStr);
            qTable.Add(qEl);
        }

        //save played action for this state
        History h = new History(lastAction, actQStateStr);
        history.Add(h);

        //update table



   
    }
}
