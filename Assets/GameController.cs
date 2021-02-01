using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TODO:     - DONE      - naprawienie, zeby mozna bylo sprzedawac wszystkie jednostki, nie tylko tokeny
//          - DONE      - poprawienie dodawania do listy board i hand playera
//
//          - DONE      - sprawdzic skrypt Fight czy dziala i jak dziala to pogchamp+dokonczyc, jak nei to fixnac -> DZIALA
//          - DONE      - miniony w tawernie nie zaleza od tavern tieru gracza
//          - DONE      - walka
//          - DONE      - taunty w walce
//          - DONE (?)  - poprawic wielkosc sceny, minionow, zeby to wszystko bylo czytelne w koncu
//          - DONE      - obsluga golden minionow dla tokenow
//          - DONE      - golden miniony i discover

//          - NIE DONE  - SPRAWDZIC CZY GRACZ MA W KONCU POPRAWNA HAND LISTE, PRZEPORWADZIC WIECEJ TESTOW, GIGA KURWA WAZNE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//          - NIE DONE  - swapowanie minionow na boardzie               -> dziala tylko zamieniajac miniona po prawej na lewy, niekoniecznie obok siebie
//          - NIE DONE  - aktualizacja hand listy po swapie powyzej tbh
//          - NIE DONE  - swapowanie miniona z blankiem na boardzie     -> analogicznie jak powyzej2x, tylko minion po prawej stronie na blanka po lewej
//          - NIE DONE  - dodanie reszty battlecryow (DONE) + pasywki (TROCHE DONE, TYLKO BUG Z TIDECALLEREM)
//          - NIE DONE  - agonie(TAK), pasywki na koniec tury(TAK), pasywki na boardzie(NIE -> te w walce done poki co, nie dziala ciagle tidecaller na boardzie)
//          - NIE DONE  - dwuturowosc (?) -> jednoczesne granie AI+gracz/gracz gra tawerne, potem AI, potem walka - powtorz
//          - NIE DONE  - AI (?)
//          - NIE DONE  - wiecej testow czy skillsy dzialaja
//          - NIE DONE  - poprawic wszystkie prefaby oprocz shopslotsow(chyba), bo nie wyswietla sie tribe i skill

//          BUGI:       - skurwiale dodawanei do player handu/boardu, trzeba refreszowac na pdostawie gameobjectow na scenie -> AI tez trzeba bedzie dac gameobjecty na scene jakos
//                      - tidecaller buffuje sam siebie, rockpool tak samo moze
//                      - swapowanie minionow na boardzie, dziala tylko z prawa do lewa + pewnie korekta w playerboard list, choc to i tak jest skurwiale tera

public class GameController : MonoBehaviour
{
    public GameObject[] shopSlots;
    public GameObject[] handSlots;

    private XmlDocument minionDataXML;
    private XmlDocument tokenMinionsDataXML;
    //pool
    //public Minion.MinionData[] pool;
    private int poolSize = 8307;
    //private int minionNumber = 117;
    private int minionNumber = 10;
    public MinionData minionData;
    public Player player1;          //TODO: 2 PLAYERS PLAYING SIMULTANEOUSLY
    public Player player2;

    public struct Pool
    {
        string name;
        int tavernTier;
        //int minionsCounter;

        public Pool(string name, int tavernTier)//, int minionsCounter)
        {
            this.name = name;
            this.tavernTier = tavernTier;
            //this.minionsCounter = minionsCounter;
        }
        public string GetName()
        {
            return name;
        }
        public int GetTavernTier()
        {
            return tavernTier;
        }
        /*
        public int GetMinionsCounter()
        {
            return minionsCounter;
        }
        */
    };
    public List<Pool> pool;
    public List<Pool> copiedPool;

    public Text playerGold;
    //public bool freeSpaceInHand;
    //public bool freeSpaceOnBoard;
    public GameObject[] minionSlots;
    public GameObject fightPanel;
    public Fight fight;

    public Text tavernTierText;
    public Text tavernTierCostText;

    public GameObject discoverPanel;
    public GameObject[] discoverSlots;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);

        TextAsset textAsset2 = Resources.Load<TextAsset>("token_minions");
        tokenMinionsDataXML = new XmlDocument();
        tokenMinionsDataXML.LoadXml(textAsset2.text);

        pool = new List<Pool>();
        copiedPool = new List<Pool>();
        CreatePool();
        SetupHandSlots(player1);
        SetupBoardSlots(player1);
        player1.Initialize();
        player2.Initialize();
        ShowMinionsInTavern(player1, shopSlots);
        SetPLayerGoldStatus(player1);
        //freeSpaceInHand = true;
        //freeSpaceOnBoard = true;
        ShowHideFightPanel(false);

        player1.turnNumber = 1;
        player2.turnNumber = 1;
        player1.tavernTierLevel = 1;
        player2.tavernTierLevel = 1;
        player1.tavernTierUpgradeGold = 5;
        player2.tavernTierUpgradeGold = 5;
        //tavernTierText.text = player1.tavernTierLevel.ToString();
        UpdateTavernTierCostText();
        UpdateTavernTierText();

        SetupDiscoverSlots();
        ShowHideDiscoverPanel(false);
    }

    // Update is called once per frame
    void Update()
    {
        //when lpm is up, then check if u dont play minion here
        //if (Input.GetMouseButtonUp(0))
        //{
        //PlayMinionOnBoard(player1);
        //RefreshBlanks(minionSlots);
        //}

        /*
        if(Input.GetKeyDown(KeyCode.Space))
        {
            fight.ShowFightBefore(0);
            Fight(player1, player2);
            fight.ShowFightBefore(1);
            ShowHideFightPanel(true);
        }
        */
    }

    public void CreatePool()
    {
        XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion");
        for (int i = 0; i < minionNumber; i++)
        {
            int tavernTemp = int.Parse(minionsList[i].Attributes["tavernTier"].Value);
            int minionsCounterTemp = 0;
            if (tavernTemp == 1)
                minionsCounterTemp = 17;
            else if (tavernTemp == 2)
                minionsCounterTemp = 23;
            else if (tavernTemp == 3)
                minionsCounterTemp = 26;
            else if (tavernTemp == 4)
                minionsCounterTemp = 23;
            else if (tavernTemp == 5)
                minionsCounterTemp = 22;
            else if (tavernTemp == 6)
                minionsCounterTemp = 16;
            else
                Debug.Log("TavernTier Error!");

            for (int j = 0; j < minionsCounterTemp; j++)
            {
                Pool singleMinion = new Pool(minionsList[i].Attributes["name"].Value, int.Parse(minionsList[i].Attributes["tavernTier"].Value));
                pool.Add(singleMinion);
                copiedPool.Add(singleMinion);
            }

            //Pool singleMinion = new Pool(minionsList[i].Attributes["name"].Value, int.Parse(minionsList[i].Attributes["tavernTier"].Value), minionsCounterTemp);
            //pool[i] = singleMinion;
            //pool.Add(singleMinion);
            //copiedPool.Add(singleMinion);
        }

    }

    public void ShowMinionsInTavern(Player player, GameObject[] slots)
    {
        int maxTavernTier = player.GetComponent<Player>().GetPlayerTavernTier();
        int numberOfMinionsInTavern = 0;
        //int minionNumber = 0;

        if (maxTavernTier == 1)
        {
            numberOfMinionsInTavern = 3;
        }
        else if (maxTavernTier == 2)
        {
            numberOfMinionsInTavern = 4;
        }
        else if (maxTavernTier == 3)
        {
            numberOfMinionsInTavern = 4;
        }
        else if (maxTavernTier == 4)
        {
            numberOfMinionsInTavern = 5;
        }
        else if (maxTavernTier == 5)
        {
            numberOfMinionsInTavern = 5;
        }
        else if (maxTavernTier == 6)
        {
            numberOfMinionsInTavern = 6;
        }
        else
            Debug.Log("MaxTavernTier error!");

        //remove minions with unmatching player tavern tier
        List<Pool> newCopied = new List<Pool>();
        for (int i = 0; i < copiedPool.Count; i++)
        {
            if (copiedPool[i].GetTavernTier() <= player.GetPlayerTavernTier())
            {
                newCopied.Add(copiedPool[i]);
            }
        }

        //initializing minions in tavern shop
        for (int i = 0; i < numberOfMinionsInTavern; i++)
        {
            int n = Random.Range(0, newCopied.Count);
            Pool random = newCopied[n];
            string randomName = random.GetName();
            XmlNode randomMinionNode = minionData.GetMinionByName(randomName, minionDataXML);
            //slots[i].GetComponent<Minion>().SetActiveAllChildren(slots[i].transform, true);
            slots[i].GetComponent<Minion>().InitializeMinion(randomMinionNode);
            newCopied.RemoveAt(n);

            //checking for possible triple to make golden minion
            int temp = 0;

            for (int j = 0; j < player.GetPlayerBoard().Count; j++)
            {
                if (player.GetPlayerBoard()[j].GetMinion().Name == randomName && player.GetPlayerBoard()[j].GetMinion().Golden == false)
                {
                    temp++;
                }
            }
            for (int j = 0; j < player.GetPlayerHand().Count; j++)
            {
                if (player.GetPlayerHand()[j].GetMinion().Name == randomName && player.GetPlayerHand()[j].GetMinion().Golden == false)
                {
                    temp++;
                }
            }

            if (temp == 2)
            {
                slots[i].GetComponent<ShopMinion>().triple = true;
            }
            else
            {
                slots[i].GetComponent<ShopMinion>().triple = false;
            }
        }

        /*
        //initializing minions in tavern shop
        for (int i = 0; i < numberOfMinionsInTavern; i++)
        {
            int n = Random.Range(0, copiedPool.Count);
            Pool random = copiedPool[n];
            string randomName = random.GetName();
            XmlNode randomMinionNode = minionData.GetMinionByName(randomName, minionDataXML);
            slots[i].GetComponent<Minion>().SetActiveAllChildren(slots[i].transform, true);
            slots[i].GetComponent<Minion>().InitializeMinion(randomMinionNode);
            copiedPool.RemoveAt(n);
        }
        */

        //blanks
        if(numberOfMinionsInTavern < slots.Length)
        {
            for (int i = numberOfMinionsInTavern; i < slots.Length; i++)
            {
                slots[i].GetComponent<Minion>().InitializeBlank();
            }
        }

        //rewriting the array
        copiedPool.Clear();
        for(int i = 0; i < pool.Count; i++)
        {
            copiedPool.Add(pool[i]);
        }
    }

    public void BuyMinion(Player player)
    {
        if (player.GetPlayerGold() >= 3) //&& freeSpaceInHand == true)
        {
            GameObject minionButton = EventSystem.current.currentSelectedGameObject;
            GameObject minion = minionButton.transform.parent.gameObject;
            //string minionName = minion.GetComponent<Minion>().minionName.text;
            //XmlNode minionNode = minionData.GetMinionByName(minionName, minionDataXML);
            //MinionData minionInstance = new MinionData();
            MinionData minionInstance = minion.GetComponent<Minion>().GetMinion();
            minionInstance.Initialize(minionInstance);

            //adding to hand,finding first spot
            int newHandPos = 0;
            for (int i = 0; i < handSlots.Length; i++)
            {
                if (handSlots[i].GetComponent<Minion>().blank == true)
                {
                    handSlots[i].GetComponent<Minion>().InitializeMinion(minionInstance, false);
                    //handSlots[i].GetComponent<Minion>().initialPosition = this.transform.position;
                    handSlots[i].GetComponent<HandMinion>().placed = false;
                    newHandPos = i;

                    Player.Board board = new Player.Board(minionInstance, newHandPos);
                    player.GetPlayerHand().Add(board);
                    Debug.Log("You have bought: " + minionInstance.Name);
                    //if (i == handSlots.Length - 1)
                    //freeSpaceInHand = false;
                    break;
                }
                else
                {
                    continue;
                }
            }
            //restore player hand
            Debug.Log("Refreshing player hand list...");
            player.GetPlayerHand().Clear();
            Debug.Log("Cleared! Player hand list count = " + player.GetPlayerHand().Count);
            for(int i = 0; i < handSlots.Length; i++)
            {
                if (handSlots[i].GetComponent<Minion>().blank == false)
                {
                    Player.Board newMinion = new Player.Board(handSlots[i].GetComponent<Minion>().GetMinion(), i);
                    player.GetPlayerHand().Add(newMinion);
                }
            }
            Debug.Log("Refreshed! Player hand list count = " + player.GetPlayerHand().Count);

            //restore player hand
            Debug.Log("Refreshing player board list...");
            player.GetPlayerBoard().Clear();
            Debug.Log("Cleared! Player board list count = " + player.GetPlayerHand().Count);
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().blank == false)
                {
                    Player.Board newMinion = new Player.Board(minionSlots[i].GetComponent<Minion>().GetMinion(), i);
                    player.GetPlayerBoard().Add(newMinion);
                }
            }
            Debug.Log("Refreshed! Player board list count = " + player.GetPlayerHand().Count);

            Debug.Log("PRZED CHECKIEM TRIPLA");
            //check if triple
            List<int> tempHandPos = new List<int>();
            List<int> tempBoardPos = new List<int>();
            tempHandPos.Clear();
            tempBoardPos.Clear();

            for (int i = 0; i < player.GetPlayerHand().Count; i++)
            {
                Debug.Log(i + ". " + player.GetPlayerHand()[i].GetMinion().Name);
                if (player.GetPlayerHand()[i].GetMinion().Name == minionInstance.Name && player.GetPlayerHand()[i].GetMinion().Golden == false)
                {
                    tempHandPos.Add(i);
                    Debug.Log("Added to tempHandPos minion " + player.GetPlayerHand()[i].GetMinion().Name);
                }
            }
            /*
            for (int i = 0; i < player.GetPlayerHand().Count; i++)
            {
                Debug.Log("hand " + i + " name: " + player.GetPlayerHand()[i].Name);
            }
            */
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if (player.GetPlayerBoard()[i].GetMinion().Name == minionInstance.Name && player.GetPlayerBoard()[i].GetMinion().Golden == false)
                {
                    tempBoardPos.Add(player.GetPlayerBoard()[i].GetPos());
                    Debug.Log("Added to tempBoardPos minion " + player.GetPlayerBoard()[i].GetMinion().Name);
                }
            }
            
            for(int i = 0; i < tempHandPos.Count; i++)
                Debug.Log("tempHand[" + i + "]: " + tempHandPos[i]);

            for (int i = 0; i < tempBoardPos.Count; i++)
                Debug.Log("tempBoard[" + i + "]: " + tempBoardPos[i]);

            Debug.Log("tempHand: " + tempHandPos.Count + ", tempBoard: " + tempBoardPos.Count);

            if ((tempHandPos.Count + tempBoardPos.Count) == 3)
            {
                GoldenMinion(player);
                //combine minions
                MinionData[] tempMinions = new MinionData[3];

                if (tempHandPos.Count == 3)
                {
                    tempMinions[0] = handSlots[tempHandPos[0]].GetComponent<Minion>().GetMinion();
                    tempMinions[1] = handSlots[tempHandPos[1]].GetComponent<Minion>().GetMinion();
                }
                else if (tempHandPos.Count == 2 && tempBoardPos.Count == 1)
                {
                    tempMinions[0] = minionSlots[tempBoardPos[0]].GetComponent<Minion>().GetMinion();
                    tempMinions[1] = handSlots[tempHandPos[1]].GetComponent<Minion>().GetMinion();
                }
                else if (tempHandPos.Count == 1 && tempBoardPos.Count == 2)
                {
                    tempMinions[0] = minionSlots[tempBoardPos[0]].GetComponent<Minion>().GetMinion();
                    tempMinions[1] = minionSlots[tempBoardPos[1]].GetComponent<Minion>().GetMinion();
                }

                bool ds = false;
                bool poison = false;
                bool taunt = false;
                bool golden = true;

                if (tempMinions[0].DivineShield == true || tempMinions[1].DivineShield == true)
                    ds = true;
                if (tempMinions[0].Poison == true || tempMinions[1].Poison == true)
                    poison = true;
                if (tempMinions[0].Taunt == true || tempMinions[1].Taunt == true)
                    taunt = true;
                MinionData newMinion = new MinionData();
                newMinion.Initialize(tempMinions[0].Name, tempMinions[0].Attack + tempMinions[1].Attack, tempMinions[0].Hp + tempMinions[1].Hp,
                    tempMinions[0].Tribe, tempMinions[0].TavernTier, ds, poison, taunt, tempMinions[0].GoldenSkill, golden);
                handSlots[newHandPos].GetComponent<Minion>().InitializeMinion(newMinion, golden);

                if (tempBoardPos.Count == 2)
                {
                    minionSlots[tempBoardPos[0]].GetComponent<Minion>().InitializeBlank();
                    minionSlots[tempBoardPos[1]].GetComponent<Minion>().InitializeBlank();
                }
                else if (tempHandPos.Count == 3)
                {
                    handSlots[tempHandPos[0]].GetComponent<Minion>().InitializeBlank();
                    handSlots[tempHandPos[1]].GetComponent<Minion>().InitializeBlank();
                }
                else if (tempHandPos.Count == 2 && tempBoardPos.Count == 1)
                {
                    minionSlots[tempBoardPos[0]].GetComponent<Minion>().InitializeBlank();
                    handSlots[tempHandPos[0]].GetComponent<Minion>().InitializeBlank();
                }

                minion.GetComponent<ShopMinion>().triple = false;
            }

            minion.GetComponent<Minion>().InitializeBlank();
            player.AddPlayerGold(-3);
            SetPLayerGoldStatus(player);

            //remove from the pool
            for (int i = 0; i < pool.Count; i++)
            {
                if(pool[i].GetName() == minionInstance.Name)
                {
                    pool.RemoveAt(i);
                    copiedPool.RemoveAt(i);
                    break;
                }
            }

            
        }
        else if (player.GetPlayerGold() >= 3) //&& freeSpaceInHand == false)
            Debug.Log("No space in hand!");
        else 
            Debug.Log("Not enough gold!");
    }

    public void SellMinion(Player player)
    {
        Debug.Log("Pool Size = " + pool.Count);
        GameObject minionButton = EventSystem.current.currentSelectedGameObject;
        GameObject minion = minionButton.transform.parent.gameObject;
        
        //string minionName = minion.GetComponent<Minion>().minionName.text;
        //XmlNode minionNode;
        //MinionData minionek = minion.GetComponent<Minion>().GetMinion();
        /*
        if(minionName == "Murloc Scout" || minionName == "Tabbycat")
        {
            minionNode = minionData.GetMinionByName(minionName, tokenMinionsDataXML);
        }
        else
        {
            minionNode = minionData.GetMinionByName(minionName, minionDataXML);
        } 
        */
        //MinionData minionInstance = new MinionData();
        MinionData minionInstance = minion.GetComponent<Minion>().GetMinion();
        //minionInstance.Initialize(minionNode, false);

        //remove from the player board
        for(int i = 0; i < player.GetPlayerBoard().Count; i++)
        {
            if(player.GetPlayerBoard()[i].GetMinion().Name == minionInstance.Name)
            {
                player.GetPlayerBoard().RemoveAt(i);
                break;
            }
        }

        //add gold, but check if player has no more than 10 gold
        if (player.GetPlayerGold() < 10)
        {
            player.AddPlayerGold(1);
        }
        SetPLayerGoldStatus(player);

        //add minion to the pool
        Pool newMinion = new Pool(minionInstance.Name, minionInstance.TavernTier);
        if(minionInstance.Name != "Tabbycat" && minionInstance.Name != "Golden Tabbycat" 
            && minionInstance.Name != "Murloc Scout" && minionInstance.Name != "Golden Murloc Scout")
        {
            pool.Add(newMinion);
            copiedPool.Add(newMinion);
            Debug.Log(newMinion.GetName());
        }

        //removing from board, pushing everything to the left(?) -> na razie sam remove
        minion.GetComponent<Minion>().InitializeBlank();
        Debug.Log("gameobject name after blanking" + minion.name + ", blank: " + minion.GetComponent<Minion>().blank);
        //freeSpaceOnBoard = true;
        Debug.Log("Pool Size = " + pool.Count);
    }

    public void RefreshMinionsInTavern(Player player, GameObject[] slots)
    {
        if (player.GetPlayerGold() >= 1)
        {
            ShowMinionsInTavern(player, slots);
            player.AddPlayerGold(-1);
            SetPLayerGoldStatus(player);
        }
        else
            Debug.Log("Not enough gold for reroll!");
    }

    //function for a scene
    public void RefreshMinionsInTavern(Player player)
    {
        if (player.GetPlayerGold() >= 1)
        {
            ShowMinionsInTavern(player, shopSlots);
            player.AddPlayerGold(-1);
            SetPLayerGoldStatus(player);
        }
        else
            Debug.Log("Not enough gold for reroll!");
    }

    public void FreezeMinionsInTavern(Player player)
    {

    }

    public void UpdateTavernPrice()
    {
        if (player1.tavernTierLevel <= 6 && player1.tavernTierUpgradeGold > 0)
            player1.tavernTierUpgradeGold--;
        if (player2.tavernTierLevel <= 6 && player2.tavernTierUpgradeGold > 0)
            player2.tavernTierUpgradeGold--;
            
        UpdateTavernTierText();
        UpdateTavernTierCostText();
    }

    public void UpgradeTavernLevel(Player player)
    {
        if(player.GetPlayerTavernTier() < 6)
        {
            player.tavernTierLevel++;
            player.AddPlayerGold(-player.tavernTierUpgradeGold);
            SetPLayerGoldStatus(player);

            if (player.tavernTierLevel == 1)
            {
                player.tavernTierUpgradeGold = player.tavernCost1;
            }
            else if (player.tavernTierLevel == 2)
            {
                player.tavernTierUpgradeGold = player.tavernCost2;
                //tavernTierText.text = "Level: 2";
                //tavernTierCostText.text = "7";
            }
            else if (player.tavernTierLevel == 3)
            {
                player.tavernTierUpgradeGold = player.tavernCost3;
                //tavernTierText.text = "Level: 3";
                //tavernTierCostText.text = "8";
            }
            else if (player.tavernTierLevel == 4)
            {
                player.tavernTierUpgradeGold = player.tavernCost4;
                //tavernTierText.text = "Level: 4";
                //tavernTierCostText.text = "9";
            }
            else if (player.tavernTierLevel == 5)
            {
                player.tavernTierUpgradeGold = player.tavernCost5;
                //tavernTierText.text = "Level: 5";
                //tavernTierCostText.text = "10";
            }

            //UpdateTavernTierText();
            //UpdateTavernTierCostText();
        }
        if (player.tavernTierLevel == 6)
        {
            player.tavernTierUpgradeGold = player.tavernCost6;
            //tavernTierText.text = "Level: 5";
            tavernTierCostText.text = "0";
        }

        UpdateTavernTierCostText();
        UpdateTavernTierText();
    }

    public void UpdateTavernTierText()
    {
        tavernTierText.text = "Level: " + player1.tavernTierLevel.ToString();
    }
    public void UpdateTavernTierCostText()
    {
        tavernTierCostText.text = player1.tavernTierUpgradeGold.ToString();

        if (player1.tavernTierLevel == 6)
        {
            tavernTierCostText.text = "";
        }

        /*
        if (player1.tavernTierLevel == 1)
            tavernTierCostText.text = "5";
        else if (player1.tavernTierLevel == 2)
            tavernTierCostText.text = "7";
        else if (player1.tavernTierLevel == 3)
            tavernTierCostText.text = "8";
        else if (player1.tavernTierLevel == 4)
            tavernTierCostText.text = "9";
        else if (player1.tavernTierLevel == 5)
            tavernTierCostText.text = "10";
        else if (player1.tavernTierLevel == 6)
            tavernTierCostText.text = "";
        */
    }

    //problem z buffem u player2(ai) -> slots, dodanie niewidzialnych slotsow dla ai???
    public void EndTurn()               // czy na pewno zalezne od playera? jak dwaj gracze naraz to chyba nie, 
                                        //chyba, ze jakies czekanko jakby sie z bomby skonczylo ture a komp nie zdazylby, do rozkminy
    {
        //end turn effects
        List<int> temp = new List<int>();
        for (int i = 0; i < player1.GetPlayerBoard().Count; i++)
        {
            //MicroMummy
            if(player1.GetPlayerBoard()[i].GetMinion().Name == "Micro Mummy")
            {
                for(int j = 0; j < player1.GetPlayerBoard().Count; j++)
                {
                    if(player1.GetPlayerBoard()[j].GetPos() != i)
                    {
                        temp.Add(player1.GetPlayerBoard()[j].GetPos());
                    }
                }
                int r = Random.Range(0, temp.Count);
                int random = temp[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 1, 0, "All", player1);

                Debug.Log("Micro mummy end turn effect!");
                break;
            }
        }

        temp.Clear();
        for (int i = 0; i < player2.GetPlayerBoard().Count; i++)
        {
            //MicroMummy
            if (player1.GetPlayerBoard()[i].GetMinion().Name == "Micro Mummy")
            {
                for (int j = 0; j < player2.GetPlayerBoard().Count; j++)
                {
                    if (player2.GetPlayerBoard()[j].GetPos() != j)
                    {
                        temp.Add(j);
                    }
                }
                int r = Random.Range(0, temp.Count);
                int random = temp[r];

                //BuffSingleMinionBoard(minionSlots[random], 1, 0, "All", player2);
                break;
            }
        }

        fight.ShowFightBefore(0);
        Fight(player1, player2);
        fight.ShowFightBefore(1);
        ShowHideFightPanel(true);

        for(int turn = 1; turn < 99; turn++)
        {
            if (player1.turnNumber == turn && player1.turnNumber < 9)
            {
                //player1.SetPlayerGold(turn + 2);
                //player2.SetPlayerGold(turn + 2);
                if (player1.GetPlayerGold() > 10 || player2.GetPlayerGold() > 10)
                {
                    //player1.SetPlayerGold(10);
                    //player2.SetPlayerGold(10);
                }
                break;
            }
        }

        player1.turnNumber++;
        player2.turnNumber++;
        UpdateTavernPrice();
        UpdateTavernTierCostText();
    }

    public void PlayMinionOnBoard(Player player, GameObject handSlot, GameObject minionSlot)
    {
        //initialize minion on board
        //string minionName = handSlot.GetComponent<Minion>().minionName.text;
        //XmlNode minionNode = minionData.GetMinionByName(minionName, minionDataXML);
        //MinionData minionInstance = new MinionData();
        MinionData minionInstance = handSlot.GetComponent<Minion>().GetMinion();
        minionInstance.Initialize(minionInstance);
        int iter = 99;
        for (int i = 0; i < minionSlots.Length; i++)
        {
            if (minionSlots[i] == minionSlot)
                iter = i;
        }
        Player.Board board = new Player.Board(minionInstance, iter);
        player.GetPlayerBoard().Add(board);
        minionSlot.GetComponent<Minion>().InitializeMinion(minionInstance, handSlot.GetComponent<Minion>().GetMinion());

        //remove minion from hand -> probably golden parameter
        //player.GetPlayerHand().RemoveAt(handSlot.GetComponent<HandMinion>().placedSlot);
        for (int x = 0; x < player.GetPlayerHand().Count; x++)
        {
            if (player.GetPlayerHand()[x].GetMinion().Name == minionInstance.Name)
            {
                player.GetPlayerHand().RemoveAt(x);
                break;
            }
        }
        

        //BATTLECRIES REALIZATION
        if (handSlot.GetComponent<Minion>().minionName.text == "Alleycat" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY ALLEYCAT");
            if(handSlot.GetComponent<Minion>().golden == true)
                SummonTokenBoard("Golden Tabbycat", 1, player);
            else
                SummonTokenBoard("Tabbycat", 1, player);

            //check for triple
            List<int> tabbyPos = new List<int>();
            //int tabbyCounter = 0;
            for(int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if (player.GetPlayerBoard()[i].GetMinion().Name == "Tabbycat")
                    tabbyPos.Add(player.GetPlayerBoard()[i].GetPos());
            }
            if(tabbyPos.Count == 3)
            {
                GoldenMinion(player);
                MinionData[] tempMinions = new MinionData[3];
                tempMinions[0] = minionSlots[tabbyPos[0]].GetComponent<Minion>().GetMinion();
                tempMinions[1] = minionSlots[tabbyPos[1]].GetComponent<Minion>().GetMinion();

                bool ds = false;
                bool poison = false;
                bool taunt = false;
                bool golden = true;

                if (tempMinions[0].DivineShield == true || tempMinions[1].DivineShield == true)
                    ds = true;
                if (tempMinions[0].Poison == true || tempMinions[1].Poison == true)
                    poison = true;
                if (tempMinions[0].Taunt == true || tempMinions[1].Taunt == true)
                    taunt = true;
                MinionData newMinion = new MinionData();
                newMinion.Initialize("Golden Tabbycat", tempMinions[0].Attack + tempMinions[1].Attack, tempMinions[0].Hp + tempMinions[1].Hp,
                    tempMinions[0].Tribe, tempMinions[0].TavernTier, ds, poison, taunt, tempMinions[0].GoldenSkill, golden);

                //find free hand spot
                int newHandPos = 99;
                for(int i = 0; i < handSlots.Length; i++)
                {
                    if(handSlots[i].GetComponent<Minion>().blank == true)
                    {
                        newHandPos = i;
                        Player.Board newM = new Player.Board(handSlots[i].GetComponent<Minion>().GetMinion(), i);
                        player.GetPlayerHand().Add(newM);
                        break;
                    }
                }
                handSlots[newHandPos].GetComponent<Minion>().InitializeMinion(newMinion, golden);
                handSlots[newHandPos].GetComponent<HandMinion>().placed = false;

                minionSlots[tabbyPos[0]].GetComponent<Minion>().InitializeBlank();
                minionSlots[tabbyPos[1]].GetComponent<Minion>().InitializeBlank();
                minionSlots[tabbyPos[2]].GetComponent<Minion>().InitializeBlank();
            }
        }
        else if (handSlot.GetComponent<Minion>().minionName.text == "Murloc Tidehunter" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY MURLOC SCOUT");
            if(handSlot.GetComponent<Minion>().golden == false)
                SummonTokenBoard("Murloc Scout", 1, player);
            else
                SummonTokenBoard("Golden Murloc Scout", 1, player);

            //check for triple
            List<int> scoutPos = new List<int>();
            //int tabbyCounter = 0;
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if (player.GetPlayerBoard()[i].GetMinion().Name == "Murloc Scout")
                    scoutPos.Add(player.GetPlayerBoard()[i].GetPos());
            }
            if (scoutPos.Count == 3)
            {
                GoldenMinion(player);
                MinionData[] tempMinions = new MinionData[3];
                tempMinions[0] = minionSlots[scoutPos[0]].GetComponent<Minion>().GetMinion();
                tempMinions[1] = minionSlots[scoutPos[1]].GetComponent<Minion>().GetMinion();

                bool ds = false;
                bool poison = false;
                bool taunt = false;
                bool golden = true;

                if (tempMinions[0].DivineShield == true || tempMinions[1].DivineShield == true)
                    ds = true;
                if (tempMinions[0].Poison == true || tempMinions[1].Poison == true)
                    poison = true;
                if (tempMinions[0].Taunt == true || tempMinions[1].Taunt == true)
                    taunt = true;
                MinionData newMinion = new MinionData();
                newMinion.Initialize("Golden Murloc Scout", tempMinions[0].Attack + tempMinions[1].Attack, tempMinions[0].Hp + tempMinions[1].Hp,
                    tempMinions[0].Tribe, tempMinions[0].TavernTier, ds, poison, taunt, tempMinions[0].GoldenSkill, golden);

                //find free hand spot
                int newHandPos = 99;
                for (int i = 0; i < handSlots.Length; i++)
                {
                    if (handSlots[i].GetComponent<Minion>().blank == true)
                    {
                        newHandPos = i;
                        Player.Board newM = new Player.Board(handSlots[i].GetComponent<Minion>().GetMinion(), i);
                        player.GetPlayerHand().Add(newM);
                        break;
                    }
                }
                handSlots[newHandPos].GetComponent<Minion>().InitializeMinion(newMinion, golden);
                handSlots[newHandPos].GetComponent<HandMinion>().placed = false;

                minionSlots[scoutPos[0]].GetComponent<Minion>().InitializeBlank();
                minionSlots[scoutPos[1]].GetComponent<Minion>().InitializeBlank();
                minionSlots[scoutPos[2]].GetComponent<Minion>().InitializeBlank();
            }

            //murloc tidecaller buff after summoning murloc scout
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                Debug.Log("i: " + i + ", minion name: " + player.GetPlayerBoard()[i].GetMinion().Name);

                if (player.GetPlayerBoard()[i].GetMinion().Name == "Murloc Tidecaller")
                {
                    Debug.Log("murloc tidecaller na boardzie");
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 2, 0, "Murloc", player);
                    else
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 4, 0, "Murloc", player);
                }
            }
            
        }
        else if (handSlot.GetComponent<Minion>().minionName.text == "Rockpool Hunter" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY ROCKPOOL HUNTER");
            int murlocCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if(minionSlots[i].GetComponent<Minion>().tribe.text == "Murloc")  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    murlocCounter++;
                    locationIterator.Add(i);
                }
            }

            if(locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                if(handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 1, 1, "Murloc", player);
                else
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 2, 2, "Murloc", player);
            }
            else 
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[minionNumber], 1, 1, "Murloc", player);
                else
                    BuffSingleMinionBoard(minionSlots[minionNumber], 2, 2, "Murloc", player);
            }
            locationIterator.Clear();
        }

        //PASSIVES REALIZATION

        //MURLOC TIDECALLER
        if(handSlot.GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<Minion>().minionName.text != "Murloc Tidehunter")
        {
            for(int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if(player.GetPlayerBoard()[i].GetMinion().Name == "Murloc Tidecaller")  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 1, 0, "Murloc", player);
                    else
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 2, 0, "Murloc", player);
                }
            }
        }

        //initialize blank on hand
        handSlot.GetComponent<Minion>().InitializeBlank();

    }

    public void SetPLayerGoldStatus(Player player)
    {
        playerGold.text = "Gold: " + player.GetPlayerGold().ToString();
    }
    
    public void SetupHandSlots(Player player)
    {
        for (int i = 0; i < handSlots.Length; i++)
        {
            handSlots[i].GetComponent<Minion>().InitializeBlank();
        }
        //freeSpaceInHand = true;
    }

    public void SetupBoardSlots(Player player)
    {
        for (int i = 0; i < minionSlots.Length; i++)
        {
            minionSlots[i].GetComponent<Minion>().InitializeBlank();
        }
        //freeSpaceOnBoard = true;
    }

    public void SetupDiscoverSlots()
    {
        for (int i = 0; i < discoverSlots.Length; i++)
        {
            discoverSlots[i].GetComponent<Minion>().InitializeBlank();
        }
    }

    public void RefreshBlanks(GameObject[] slots)
    {
        Debug.Log("Refreshing " + slots[0].name + "...");
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetComponent<Minion>().blank == true)
            {
                slots[i].GetComponent<Minion>().InitializeBlank();
            }
        }
    }

    public void RefreshMinionSlots(GameObject[] slots, Player player)
    {
        int temp = 0;
        for(int i = 0; i < player.GetPlayerBoard().Count; i++)
        {
            MinionData newMinion = player.GetPlayerBoard()[i].GetMinion();
            slots[i].GetComponent<Minion>().InitializeMinion(newMinion, newMinion.Golden);
            temp = i;
        }
        for (int i = temp; i < slots.Length; i++)
        {
            slots[i].GetComponent<Minion>().InitializeBlank();
        }
    }

    public void RefreshShopSlots(GameObject[] slots, Player player)
    {
        ShowMinionsInTavern(player, slots);
    }
    /*
    public void RefreshHandSlots(GameObject[] slots, Player player)
    {
        int temp = 0;
        for (int i = 0; i < player.GetPlayerHand().Count; i++)
        {
            MinionData newMinion = player.GetPlayerHand()[i];
            slots[i].GetComponent<Minion>().InitializeMinion(newMinion);
            temp = i;
        }
        for (int i = temp; i < slots.Length; i++)
        {
            slots[i].GetComponent<Minion>().InitializeBlank();
        }
    }
    */
    //TODO: - COS NIE DIZALA ZE SWAPOWANIEM JEDNOSTEK, NIE WIEM CO JESZCZE
    public void SwapNonBlankMinions(GameObject minionA, GameObject minionB)
    {
        MinionData minionDataA = minionA.GetComponent<Minion>().GetMinion();
        MinionData minionDataB = minionB.GetComponent<Minion>().GetMinion();

        string nameMinionA = minionDataA.Name;
        string nameMinionB = minionDataB.Name;
        Debug.Log("A: " + nameMinionA + ", B: " + nameMinionB);

        XmlNode nodeMinionA = minionData.GetMinionByName(nameMinionA, minionDataXML);
        XmlNode nodeMinionB = minionData.GetMinionByName(nameMinionB, minionDataXML);
        if (minionDataA.Name == "Rat" || minionDataA.Name == "Tabbycat" || minionDataA.Name == "Murloc Scout" || minionDataA.Name == "Imp")
            nodeMinionA = minionData.GetMinionByName(nameMinionA, tokenMinionsDataXML);
        else
            nodeMinionA = minionData.GetMinionByName(nameMinionA, minionDataXML);

        if (minionDataB.Name == "Rat" || minionDataB.Name == "Tabbycat" || minionDataB.Name == "Murloc Scout" || minionDataB.Name == "Imp")
            nodeMinionB = minionData.GetMinionByName(nameMinionB, tokenMinionsDataXML);
        else
            nodeMinionB = minionData.GetMinionByName(nameMinionB, minionDataXML);

        Debug.Log("A: " + nodeMinionA.Attributes["name"].Value + ", B: " + nodeMinionB.Attributes["name"].Value);

        Debug.Log("Minion0: " + minionSlots[0].GetComponent<Minion>().minionName.text + ", Minion1: " + minionSlots[1].GetComponent<Minion>().minionName.text);
        minionA.GetComponent<Minion>().InitializeMinion(nodeMinionB);
        minionB.GetComponent<Minion>().InitializeMinion(nodeMinionA);
        Debug.Log("minion1: " + minionA.GetComponent<Minion>().minionName.text + ", minion2: " + minionB.GetComponent<Minion>().minionName.text);
        Debug.Log("Minion0: " + minionSlots[0].GetComponent<Minion>().minionName.text + ", Minion1: " + minionSlots[1].GetComponent<Minion>().minionName.text);
    }

    public void SwapMinionWithBlank(GameObject minion, GameObject blank)
    {
        MinionData minionDataObj = minion.GetComponent<Minion>().GetMinion();
        string nameMinion = minionDataObj.Name;
        XmlNode nodeMinion = minionData.GetMinionByName(nameMinion, minionDataXML);

        blank.GetComponent<Minion>().InitializeMinion(nodeMinion);
        minion.GetComponent<Minion>().InitializeBlank();
    }

    //minionA -> player1, minionB -> player2
    public void FightBetweenTwoMinions(MinionData minionA, MinionData minionB, Player p1, Player p2)
    {
        //double ds
        if (minionA.DivineShield == true && minionB.DivineShield == true)
        {
            minionA.DivineShield = false;
            minionB.DivineShield = false;
        }
        else if (minionA.DivineShield == true && minionB.DivineShield == false)
        {
            minionA.DivineShield = false;
            if (minionA.Poison == true)
            {
                minionB.Hp = 0;
            }

            /*
            if(minionB.Hp <= 0)
            {
                for(int i = 0; i < p2.GetPlayerCopiedBoard().Count; i++)
                {
                    if(p2.GetPlayerCopiedBoard()[i].GetMinion().Name == minionB.Name)
                    {
                        p2.GetPlayerCopiedBoard().RemoveAt(i);

                        //DEATHRATTLES REALIZATION
                        if(minionB.Name == "Spawn of NZoth")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            BuffAllMinions(1, 1, p2);
                        }
                        else if(minionB.Name == "Rat Pack")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            SummonTokenFight("Rat", minionB.Attack, p2);
                        }
                        else if(minionB.Name == "Fiendish Servant")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            if (p2.GetPlayerCopiedBoard().Count > 0)
                            {
                                BuffSingleMinionFight(minionB.Attack, 0, p2);
                            }
                        }
                        else if (minionB.Name == "Imprisoner")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            SummonTokenFight("Imp", 1, p2);
                        }
                        //PASSIVES REALIZATION
                        //scavenging hyena
                        if (minionB.Tribe == "Beast")
                        {
                            Debug.Log("zdechla bestia");
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if(p2.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                                {
                                    Debug.Log(minionB.Name + " Passive!");
                                    BuffSpecifiedMinionFight(2, 1, p2, j);
                                }
                            }
                        }

                        break;
                    }
                }
                
                Debug.Log(minionB.Name + " is dead!");
            }*/
        }
        else if(minionA.DivineShield == false && minionB.DivineShield == true)
        {
            minionB.DivineShield = false;
            if (minionB.Poison == true)
            {
                minionA.Hp = 0;
            }
            /*
            if (minionA.Hp <= 0)
            {
                for (int i = 0; i < p1.GetPlayerCopiedBoard().Count; i++)
                {
                    if (p1.GetPlayerCopiedBoard()[i].GetMinion().Name == minionA.Name)
                    {
                        p1.GetPlayerCopiedBoard().RemoveAt(i);

                        //DEATHRATTLES REALIZATION
                        if (minionA.Name == "Spawn of NZoth")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            BuffAllMinions(1, 1, p1);
                        }
                        else if (minionA.Name == "Rat Pack")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            SummonTokenFight("Rat", minionA.Attack, p1);
                        }
                        else if (minionA.Name == "Fiendish Servant")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            if (p1.GetPlayerCopiedBoard().Count > 0)
                            {
                                BuffSingleMinionFight(minionA.Attack, 0, p1);
                            }
                        }
                        else if (minionA.Name == "Imprisoner")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            SummonTokenFight("Imp", 1, p1);
                        }

                        //PASSIVES REALIZATION
                        //scavenging hyena
                        if (minionA.Tribe == "Beast")
                        {
                            Debug.Log("zdechla bestia");
                            for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                                {
                                    Debug.Log(minionA.Name + " Passive!");
                                    BuffSpecifiedMinionFight(2, 1, p1, j);
                                }
                            }
                        }

                        break;
                    }
                }

                Debug.Log(minionA.Name + " is dead!");
            }*/
        }
        else if (minionA.DivineShield == false && minionB.DivineShield == false)
        {
            if(minionA.Poison == true && minionB.Poison == true)
            {
                minionA.Hp = 0;
                minionB.Hp = 0;
            }
            else if(minionA.Poison == true && minionB.Poison == false)
            {
                minionA.Hp -= minionB.Attack;
                minionB.Hp = 0;
            }
            else if(minionA.Poison == false && minionB.Poison == true)
            {
                minionA.Hp = 0;
                minionB.Hp -= minionA.Attack;
            }
            else if (minionA.Poison == false && minionB.Poison == false)
            {
                minionA.Hp -= minionB.Attack;
                minionB.Hp -= minionA.Attack;
            }
            /*
            if (minionA.Hp <= 0)
            {
                for (int i = 0; i < p1.GetPlayerCopiedBoard().Count; i++)
                {
                    if (p1.GetPlayerCopiedBoard()[i].GetMinion().Name == minionA.Name)
                    {
                        p1.GetPlayerCopiedBoard().RemoveAt(i);

                        //DEATHRATTLES REALIZATION
                        if (minionA.Name == "Spawn of NZoth")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            BuffAllMinions(1, 1, p1);
                        }
                        else if (minionA.Name == "Rat Pack")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            SummonTokenFight("Rat", minionA.Attack, p1);
                        }
                        else if (minionA.Name == "Fiendish Servant")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            if (p1.GetPlayerCopiedBoard().Count > 0)
                            {
                                BuffSingleMinionFight(minionA.Attack, 0, p1);
                            }
                        }
                        else if (minionA.Name == "Imprisoner")
                        {
                            Debug.Log(minionA.Name + " Deathrattle!");
                            SummonTokenFight("Imp", 1, p1);
                        }

                        //PASSIVES REALIZATION
                        //scavenging hyena
                        if (minionA.Tribe == "Beast")
                        {
                            Debug.Log("zdechla bestia");
                            for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                                {
                                    Debug.Log(minionA.Name + " Passive!");
                                    BuffSpecifiedMinionFight(2, 1, p1, j);
                                }
                            }
                        }
                        break;
                    }
                }

                Debug.Log(minionA.Name + " is dead!");
            }

            if (minionB.Hp <= 0)
            {
                for (int i = 0; i < p2.GetPlayerCopiedBoard().Count; i++)
                {
                    if (p2.GetPlayerCopiedBoard()[i].GetMinion().Name == minionB.Name)
                    {
                        p2.GetPlayerCopiedBoard().RemoveAt(i);

                        //DEATHRATTLES REALIZATION
                        if (minionB.Name == "Spawn of NZoth")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            BuffAllMinions(1, 1, p2);
                        }
                        else if (minionB.Name == "Rat Pack")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            SummonTokenFight("Rat", minionB.Attack, p2);
                        }
                        else if (minionB.Name == "Fiendish Servant")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            if (p2.GetPlayerCopiedBoard().Count > 0)
                            {
                                BuffSingleMinionFight(minionB.Attack, 0, p2);
                            }
                        }
                        else if (minionB.Name == "Imprisoner")
                        {
                            Debug.Log(minionB.Name + " Deathrattle!");
                            SummonTokenFight("Imp", 1, p2);
                        }

                        //PASSIVES REALIZATION
                        //scavenging hyena
                        if (minionB.Tribe == "Beast")
                        {
                            Debug.Log("zdechla bestia");
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                                {
                                    Debug.Log(minionB.Name + " Passive!");
                                    BuffSpecifiedMinionFight(2, 1, p2, j);
                                }
                            }
                        }
                        break;
                    }
                }

                Debug.Log(minionB.Name + " is dead!");
            }*/
        }
        if (minionA.Hp <= 0)
        {
            for (int i = 0; i < p1.GetPlayerCopiedBoard().Count; i++)
            {
                if (p1.GetPlayerCopiedBoard()[i].GetMinion().Name == minionA.Name)
                {
                    p1.GetPlayerCopiedBoard().RemoveAt(i);

                    //DEATHRATTLES REALIZATION
                    if (minionA.Name == "Spawn of NZoth")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if(minionA.Golden == false)
                            BuffAllMinions(1, 1, p1);
                        else
                            BuffAllMinions(2, 2, p1);
                    }
                    else if (minionA.Name == "Rat Pack")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if(minionA.Golden == false)
                            SummonTokenFight("Rat", minionA.Attack, p1);
                        else
                            SummonTokenFight("Golden Rat", minionA.Attack, p1);
                    }
                    else if (minionA.Name == "Fiendish Servant")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (p1.GetPlayerCopiedBoard().Count > 0)
                        {
                            if (minionA.Golden == false)
                                BuffSingleMinionFight(minionA.Attack, 0, p1);
                            else
                            {
                                BuffSingleMinionFight(minionA.Attack, 0, p1);
                                BuffSingleMinionFight(minionA.Attack, 0, p1);
                            }   
                        }
                    }
                    else if (minionA.Name == "Imprisoner")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                            SummonTokenFight("Imp", 1, p1);
                        else
                            SummonTokenFight("Golden Imp", 1, p1);
                    }

                    //PASSIVES REALIZATION
                    //scavenging hyena
                    if (minionA.Tribe == "Beast")
                    {
                        Debug.Log("zdechla bestia");
                        for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                        {
                            if (p1.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                            {
                                Debug.Log(minionA.Name + " Passive!");
                                if(p1.GetPlayerCopiedBoard()[j].GetMinion().Golden == false)
                                    BuffSpecifiedMinionFight(2, 1, p1, j);
                                else
                                    BuffSpecifiedMinionFight(4, 2, p1, j);
                            }
                        }
                    }
                    break;
                }
            }

            Debug.Log(minionA.Name + " is dead!");
        }

        if (minionB.Hp <= 0)
        {
            for (int i = 0; i < p2.GetPlayerCopiedBoard().Count; i++)
            {
                if (p2.GetPlayerCopiedBoard()[i].GetMinion().Name == minionB.Name)
                {
                    p2.GetPlayerCopiedBoard().RemoveAt(i);

                    //DEATHRATTLES REALIZATION
                    if (minionB.Name == "Spawn of NZoth")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if(minionB.Golden == false)
                            BuffAllMinions(1, 1, p2);
                        else
                            BuffAllMinions(2, 2, p2);
                    }
                    else if (minionB.Name == "Rat Pack")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                            SummonTokenFight("Rat", minionB.Attack, p2);
                        else
                            SummonTokenFight("Golden Rat", minionB.Attack, p2);
                    }
                    else if (minionB.Name == "Fiendish Servant")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (p2.GetPlayerCopiedBoard().Count > 0)
                        {
                            if (minionB.Golden == false)
                                BuffSingleMinionFight(minionB.Attack, 0, p2);
                            else
                            {
                                BuffSingleMinionFight(minionB.Attack, 0, p2);
                                BuffSingleMinionFight(minionB.Attack, 0, p2);
                            }
                        }
                    }
                    else if (minionB.Name == "Imprisoner")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                            SummonTokenFight("Imp", 1, p2);
                        else
                            SummonTokenFight("Golden Imp", 1, p2);
                    }
                    //PASSIVES REALIZATION
                    //scavenging hyena
                    if (minionB.Tribe == "Beast")
                    {
                        Debug.Log("zdechla bestia");
                        for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                        {
                            if (p2.GetPlayerCopiedBoard()[j].GetMinion().Name == "Scavenging Hyena")
                            {
                                Debug.Log(minionB.Name + " Passive!");
                                if (minionB.Golden == false)
                                    BuffSpecifiedMinionFight(2, 1, p2, j);
                                else
                                    BuffSpecifiedMinionFight(4, 2, p2, j);
                            }
                        }
                    }

                    break;
                }
            }

            Debug.Log(minionB.Name + " is dead!");
        }
    }

    public void Fight(Player player1, Player player2)
    {
        List<Player.Board> taunts1 = new List<Player.Board>();
        List<Player.Board> taunts2 = new List<Player.Board>();

        //update copiedBoard
        player1.GetPlayerCopiedBoard().Clear();
        for (int i = 0; i < player1.GetPlayerBoard().Count; i++)
        {
            player1.GetPlayerCopiedBoard().Add(player1.GetPlayerBoard()[i]);
        }
        player2.GetPlayerCopiedBoard().Clear();
        for (int i = 0; i < player2.GetPlayerBoard().Count; i++)
        {
            player2.GetPlayerCopiedBoard().Add(player2.GetPlayerBoard()[i]);
        }
        Debug.Log("P1: " + player1.GetPlayerCopiedBoard().Count);
        Debug.Log("P2: " + player2.GetPlayerCopiedBoard().Count);

        //List<Player.Board> player1Board = player1.GetPlayerBoard();
        //List<Player.Board> player2Board = player2.GetPlayerBoard();
        int firstAttack = 0;    // 1->player1 starts, 2->player2 starts
        //Debug.Log("P1 count: " + player1.GetPlayerBoard().Count + ", P2 count: " + player2.GetPlayerBoard().Count);
        if (player1.GetPlayerCopiedBoard().Count > player2.GetPlayerCopiedBoard().Count)
            firstAttack = 1;
        else if (player1.GetPlayerCopiedBoard().Count < player2.GetPlayerCopiedBoard().Count)
            firstAttack = 2;
        else if (player1.GetPlayerCopiedBoard().Count == player2.GetPlayerCopiedBoard().Count)
            firstAttack = Random.Range(1, 3);
        
        int temp1 = 0;
        int temp2 = 0;
        for(int i = 0; i < 999; i++)
        {
            //Debug.Log("P1.Count = " + player1.GetPlayerBoard().Count + ", P2.Count = " + player2.GetPlayerBoard().Count);
            if (player1.GetPlayerCopiedBoard().Count == 0 && player2.GetPlayerCopiedBoard().Count > 0)
            {
                Debug.Log("P2 WON!");
                CalculateDamage(player2, player1);
                break;
            }
            else if (player2.GetPlayerCopiedBoard().Count == 0 && player1.GetPlayerCopiedBoard().Count > 0)
            {
                Debug.Log("P1 WON!");
                CalculateDamage(player1, player2);
                break;
            }
            else if (player1.GetPlayerCopiedBoard().Count == 0 && player2.GetPlayerCopiedBoard().Count == 0)
            {
                Debug.Log("TIE!");
                break;
            }

            if (firstAttack == 1)
            {
                if (temp1 == player1.GetPlayerCopiedBoard().Count)
                    temp1 = 0;
                taunts1.Clear();
                for(int x = 0; x < player2.GetPlayerCopiedBoard().Count; x++)
                {
                    if(player2.GetPlayerCopiedBoard()[x].GetMinion().Taunt == true)
                    {
                        Player.Board taunt = new Player.Board(player2.GetPlayerCopiedBoard()[x].GetMinion(), x);
                        taunts1.Add(taunt);
                    }
                }
                int n = 0;
                if (taunts1.Count == 0)
                {
                    n = Random.Range(0, player2.GetPlayerCopiedBoard().Count);
                }
                else if (taunts1.Count > 0)
                {
                    int r = Random.Range(0, taunts1.Count);
                    n = taunts1[r].GetPos();
                }
                else
                    Debug.Log("Taunts List Count cannot be negative!");

                //Debug.Log("temp1: " + temp1 + ", p1.count: " + player1.GetPlayerCopiedBoard().Count + ", n: " + n + ", p2.count: " + player2.GetPlayerCopiedBoard().Count);
                FightBetweenTwoMinions(player1.GetPlayerCopiedBoard()[temp1].GetMinion(), player2.GetPlayerCopiedBoard()[n].GetMinion(), player1, player2);
                temp1++;
                firstAttack = 2;
            }
            else if (firstAttack == 2)
            {
                if (temp2 == player2.GetPlayerCopiedBoard().Count)
                    temp2 = 0;
                taunts2.Clear();
                for (int x = 0; x < player1.GetPlayerCopiedBoard().Count; x++)
                {
                    if (player1.GetPlayerCopiedBoard()[x].GetMinion().Taunt == true)
                    {
                        Player.Board taunt = new Player.Board(player1.GetPlayerCopiedBoard()[x].GetMinion(), x);
                        taunts2.Add(taunt);
                    }
                }
                int n = 0;
                if (taunts2.Count == 0)
                {
                    n = Random.Range(0, player1.GetPlayerCopiedBoard().Count);
                }
                else if (taunts2.Count > 0)
                {
                    int r = Random.Range(0, taunts2.Count);
                    n = taunts2[r].GetPos();
                }
                else
                    Debug.Log("Taunts List Count cannot be negative!");

                //Debug.Log("temp2: " + temp2 + ", p2.count: " + player2.GetPlayerCopiedBoard().Count + ", n: " + n + ", p1.count: " + player1.GetPlayerCopiedBoard().Count);
                FightBetweenTwoMinions(player2.GetPlayerCopiedBoard()[temp2].GetMinion(), player1.GetPlayerCopiedBoard()[n].GetMinion(), player2, player1);
                temp2++;
                firstAttack = 1; 
            }
        }
    }

    public void CalculateDamage(Player playerWon, Player playerLost)
    {
        int temp = 0;
        for (int i = 0; i < playerWon.GetPlayerCopiedBoard().Count; i++)
        {
            temp += playerWon.GetPlayerCopiedBoard()[i].GetMinion().TavernTier;
        }
        temp += playerWon.GetPlayerTavernTier();

        playerLost.AddHealth(-temp);
    }

    public void ShowHideFightPanel(bool flag)
    {
        fightPanel.gameObject.SetActive(flag);
    }

    public void ShowHideDiscoverPanel(bool flag)
    {
        discoverPanel.gameObject.SetActive(flag);
    }

    //rozpatrzony tylko jeden przypadek -> 2 miniony na boardzie, kupiony 3
    //zostaja przypadki:                -> 1 minion na boardzie a 1 na reku
    //                                  -> 2 miniony na reku
    public void GoldenMinion(Player player)
    {
        List<Pool> temp = new List<Pool>();
        XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion");
        List<Pool> tempMinions = new List<Pool>();
        for (int i = 0; i < minionsList.Count; i++)
        {
            Pool pool = new Pool(minionsList[i].Attributes["name"].Value, int.Parse((minionsList[i].Attributes["tavernTier"].Value)));
            tempMinions.Add(pool);
        }

        for (int i = 0; i < tempMinions.Count; i++)
        {
            if(tempMinions[i].GetTavernTier() == (player.GetPlayerTavernTier() + 1))
            {
                temp.Add(tempMinions[i]);
            }
        }

        
        int r1 = Random.Range(0, temp.Count);
        string m1 = temp[r1].GetName();
        temp.RemoveAt(r1);
        int r2 = Random.Range(0, temp.Count);
        string m2 = temp[r2].GetName();
        temp.RemoveAt(r2);
        int r3 = Random.Range(0, temp.Count);
        string m3 = temp[r3].GetName();
        temp.RemoveAt(r3);

        XmlNode minionNode1 = minionData.GetMinionByName(m1, minionDataXML);
        discoverSlots[0].GetComponent<Minion>().InitializeMinion(minionNode1);
        XmlNode minionNode2 = minionData.GetMinionByName(m2, minionDataXML);
        discoverSlots[1].GetComponent<Minion>().InitializeMinion(minionNode2);
        XmlNode minionNode3 = minionData.GetMinionByName(m3, minionDataXML);
        discoverSlots[2].GetComponent<Minion>().InitializeMinion(minionNode3);

        ShowHideDiscoverPanel(true);
    }

    public void ChooseDiscoveredMinion(Player player)
    {
        if(player.GetPlayerGold() >= 3)
        {
            BuyMinion(player);
            player.AddPlayerGold(3);
        }
        else if (player.GetPlayerGold() < 3)
        {
            player.AddPlayerGold(3);
            BuyMinion(player);
        }
        ShowHideDiscoverPanel(false);
    }

    //BATTLECRIES:
    public void SummonTokenBoard(string tokenName, int number, Player player)
    {
        for (int n = 0; n < number; n++)
        {
            //if (freeSpaceOnBoard == true)
            //{
            XmlNode minionNode = minionData.GetMinionByName(tokenName, tokenMinionsDataXML);
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().blank == true)
                {
                    MinionData minionInstance = minionSlots[i].GetComponent<Minion>().GetMinion();
                    minionInstance.Initialize(minionNode, false);
                    minionSlots[i].GetComponent<Minion>().InitializeMinion(minionNode);
                    if (tokenName == "Golden Tabbycat" || tokenName == "Golden Murloc Scout")
                        minionInstance.Golden = true;
                    minionSlots[i].GetComponent<Minion>().InitializeMinion(minionInstance, minionInstance.Golden);
                    //minionSlots[i].GetComponent<Minion>().golden = minionInstance;

                    //if (i == minionSlots.Length - 1)
                    //{
                    //    freeSpaceInHand = false;
                    //}

                    Player.Board board = new Player.Board(minionInstance, i);
                    player.GetPlayerBoard().Add(board);

                    break;
                }
            }
            //}
        }
    }

    public void BuffSingleMinionBoard(GameObject selectedMinion, int attack, int health, string tribe, Player player)
    {
        if (selectedMinion.GetComponent<Minion>().tribe.text == tribe || tribe == "All")
        {
            /*
            string selectedMinionName = selectedMinion.GetComponent<Minion>().minionName.text;
            XmlNode minionNode;
            
            if (selectedMinion.GetComponent<Minion>().minionName.text == "Tabbycat" || selectedMinion.GetComponent<Minion>().minionName.text == "Murloc Scout")
                minionNode = minionData.GetMinionByName(selectedMinionName, tokenMinionsDataXML);
            else
                minionNode = minionData.GetMinionByName(selectedMinionName, minionDataXML);
            */
            MinionData minionInstance = selectedMinion.GetComponent<Minion>().GetMinion();
            minionInstance.Attack += attack;
            minionInstance.Hp += health;
            selectedMinion.GetComponent<Minion>().InitializeMinion(minionInstance, minionInstance.Golden);
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if(player.GetPlayerBoard()[i].GetMinion().Name == minionInstance.Name && 
                    player.GetPlayerBoard()[i].GetMinion().Attack == (minionInstance.Attack - attack) &&
                     player.GetPlayerBoard()[i].GetMinion().Hp == (minionInstance.Hp - health))
                {
                    int pos = player.GetPlayerBoard()[i].GetPos();
                    player.GetPlayerBoard().RemoveAt(i);
                    Player.Board newBoard = new Player.Board(minionInstance, pos);
                    player.GetPlayerBoard().Add(newBoard);

                    Debug.Log("Player board database has been updated!");
                    break;
                }
            }
        }
        else
            Debug.Log("Choose correct minion!");
    }

    public void BuffAllMinions(int attack, int health, Player player)
    {
        for(int i = 0; i < player.GetPlayerCopiedBoard().Count; i++)
        {
            MinionData minionInstance = player.GetPlayerCopiedBoard()[i].GetMinion();
            minionInstance.Attack += attack;
            minionInstance.Hp += health;
            player.GetPlayerCopiedBoard()[i].GetMinion().Initialize(minionInstance);
        }
    }

    public void BuffSingleMinionFight(int attack, int health, Player player)
    {
        int r = Random.Range(0, player.GetPlayerCopiedBoard().Count);
        MinionData minionInstance = player.GetPlayerCopiedBoard()[r].GetMinion();
        minionInstance.Attack += attack;
        minionInstance.Hp += health;
        player.GetPlayerCopiedBoard()[r].GetMinion().Initialize(minionInstance);
    }

    public void BuffSpecifiedMinionFight(int attack, int health, Player player, int minionPos)
    {
        MinionData minionInstance = player.GetPlayerCopiedBoard()[minionPos].GetMinion();
        minionInstance.Attack += attack;
        minionInstance.Hp += health;
        player.GetPlayerCopiedBoard()[minionPos].GetMinion().Initialize(minionInstance);
    }

    //DEATHRATTLES:
    public void SummonTokenFight(string tokenName, int number, Player player)
    {
        for (int n = 0; n < number; n++)
        {
            if(player.GetPlayerCopiedBoard().Count == 7)
            {
                break;
            }

            XmlNode minionNode = minionData.GetMinionByName(tokenName, tokenMinionsDataXML);

            MinionData minionInstance = new MinionData();
            minionInstance.Initialize(minionNode, false);

            Player.Board board = new Player.Board(minionInstance, n);
            player.GetPlayerCopiedBoard().Add(board);
        }
    }

    //PASSIVES:



    //END TURN EFFECTS:

}
