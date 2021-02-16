using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
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
//          - DONE      - SPRAWDZIC CZY GRACZ MA W KONCU POPRAWNA HAND LISTE, PRZEPORWADZIC WIECEJ TESTOW, GIGA KURWA WAZNE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//          - DONE      - dodanie reszty battlecryow (DONE)
//          - DONE      - agonie(TAK), pasywki na koniec tury(TAK), pasywki na boardzie(TAK)
//          - DONE      - dwuturowosc (?) -> jednoczesne granie AI+gracz/gracz gra tawerne, potem AI, potem walka - powtorz
//          - DONE      - poprawic wszystkie prefaby oprocz shopslotsow(chyba), bo nie wyswietla sie tribe i skill

//          - NIE DONE  - swapowanie minionow na boardzie               -> dziala tylko zamieniajac miniona po prawej na lewy, niekoniecznie obok siebie
//          - NIE DONE  - aktualizacja hand listy po swapie powyzej tbh
//          - NIE DONE  - swapowanie miniona z blankiem na boardzie     -> analogicznie jak powyzej2x, tylko minion po prawej stronie na blanka po lewej
//          - NIE DONE  - AI (?)
//          - NIE DONE  - wiecej testow czy skillsy dzialaja -> najistotniejsze tera                    (!)
//          - NIE DONE  - licznik zwyciestw, najlepiej zapisywany gdzies do pliku, by sledzic uczenie   (!)
//          - NIE DONE  - mozliwosc wylaczenia uczenia pod klawiszem(?) lub zmiana boola w inspektorze  (!)
//          - NIE DONE  - pokazanie jakos, ze minion jest golden, rozroznienie z DSem

//          BUGI:       
//                      - swapowanie minionow na boardzie, dziala tylko z prawa do lewa + pewnie korekta w playerboard list, choc to i tak jest skurwiale tera
//                      - w przypadku gdy sa dwie te same jednostki na boardzie, zostanie zakupiona 3, inicjalizacja golden mechanic 
//                        -> po zagraniu zlotej jednostki summonuje sie jej kopia -> do naprawy, test kiedy to sie wydarza
//                      - EndTurn wystepuje 2x, przez co buff at end of turn aktywuje sie dwukrotnie
//                      - miniony w walce moga miec nawet ujemne hp, nie odnawiaja sie statsy z jakiegos losowego powodu sadge !!! -> gdy brak akcji, refresh tylko 
//                        przy kupowaniu jednostki, cos sie dzieje potem z lista

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
    private int minionNumber = 44;
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
    public Text tavernTierTextAI;
    public Text tavernTierCostTextAI;

    public GameObject discoverPanel;
    public GameObject[] discoverSlots;

    //ai
    public GameObject[] shopSlotsAI;
    public GameObject[] handSlotsAI;
    public GameObject[] minionSlotsAI;
    //public GameObject[] discoverSlotsAI;
    public Text playerGoldAI;

    public GameObject AllPlayer;
    public GameObject AllAI;

    public int gameNr;
    public int winP1;
    public int winP2;

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
        SetupHandSlots();
        SetupBoardSlots();
        SetupFightSlots();
        player1.Initialize();
        player2.Initialize();
        player1.turn = true;
        player2.turn = false;
        ShowMinionsInTavern(player1, shopSlots, 0);
        ShowMinionsInTavern(player2, shopSlotsAI, 0);
        SetPLayerGoldStatus(player1);
        SetPLayerGoldStatus(player2);
        //freeSpaceInHand = true;
        //freeSpaceOnBoard = true;
        ShowHideFightPanel(false);

        //player1.turnNumber = 1;
        //player2.turnNumber = 1;
        //player1.tavernTierLevel = 1;
        //player2.tavernTierLevel = 1;
        //player1.tavernTierUpgradeGold = 5;
        //player2.tavernTierUpgradeGold = 5;
        //tavernTierText.text = player1.tavernTierLevel.ToString();
        UpdateTavernTierCostText(player1, tavernTierCostText);
        UpdateTavernTierText(player1, tavernTierText);

        UpdateTavernTierCostText(player2, tavernTierCostTextAI);
        UpdateTavernTierText(player2, tavernTierTextAI);

        SetupDiscoverSlots();
        ShowHideDiscoverPanel(false);
        ChangeCanvasObjects("Player");

        gameNr = 1;
        winP1 = 0;
        winP2 = 0;
        SaveHashtagsToFile();
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

        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            /*
            fight.ShowFightBefore(0);
            Fight(player1, player2);
            fight.ShowFightBefore(1);
            ShowHideFightPanel(true);
            */
            EndTurnAI(minionSlotsAI);
        }
        
    }

    public void SaveWinNumberToFile()
    {
        string path = "Assets/Resources/wins.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("Game nr: " + gameNr);
        writer.WriteLine("P1 win counter: " + winP1 + ", P2 win counter: " + winP2);
        writer.Close();
    }

    public void SaveHashtagsToFile()
    {
        string path = "Assets/Resources/wins.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("########################");
        writer.Close();
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

        }

    }

    public void ShowMinionsInTavern(Player player, GameObject[] slots, int numberOfMinionsInTavern)
    {
        int maxTavernTier = player.GetComponent<Player>().GetPlayerTavernTier();
        //int numberOfMinionsInTavern = 0;
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

    public void BuyMinion(Player player, GameObject minion, GameObject[] handSlots, GameObject[] minionSlots)
    {
        if (player.GetPlayerGold() >= 3) //&& freeSpaceInHand == true)
        {
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

            //restore player board
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
                List<int> tokenPos = new List<int>();
                GoldenMinion(player, newHandPos, tempHandPos, tempBoardPos, minion, "simple", "", tokenPos, minionSlots, handSlots);
                
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

    //function for buy button on a scene(for a player)
    public void BuyMinionPlayer(Player player)
    {
        GameObject minionButton = EventSystem.current.currentSelectedGameObject;
        GameObject minion = minionButton.transform.parent.gameObject;
        BuyMinion(player, minion, handSlots, minionSlots);
    }

    //function for buy button on a scene(for a AI)
    public void BuyMinionAI(Player player, GameObject minion)
    {
        //GameObject minionButton = EventSystem.current.currentSelectedGameObject;
        //GameObject minion = minionButton.transform.parent.gameObject;
        BuyMinion(player, minion, handSlotsAI, minionSlotsAI);
    }

    public void SellMinion(Player player, GameObject minion)
    {
        Debug.Log("Pool Size = " + pool.Count);
        
        //MinionData minionInstance = new MinionData();
        MinionData minionInstance = minion.GetComponent<Minion>().GetMinion();
        //minionInstance.Initialize(minionNode, false);
        Debug.Log("przed sellnieciem count = " + player.GetPlayerBoard().Count);
        //remove from the player board
        for(int i = 0; i < player.GetPlayerBoard().Count; i++)
        {
            if(player.GetPlayerBoard()[i].GetMinion().Name == minionInstance.Name)
            {
                player.GetPlayerBoard().RemoveAt(i);
                break;
            }
        }
        Debug.Log("po sellnieciem count = " + player.GetPlayerBoard().Count);
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

    public void SellMinionPlayer(Player player)
    {
        GameObject minionButton = EventSystem.current.currentSelectedGameObject;
        GameObject minion = minionButton.transform.parent.gameObject;
        SellMinion(player, minion);
    }

    public void SellMinionAI(Player player, GameObject minion)
    {
        SellMinion(player, minion);
    }

    public void RefreshMinionsInTavern(Player player, GameObject[] slots)
    {
        if (player.GetPlayerGold() >= 1)
        {
            ShowMinionsInTavern(player, slots, 0);
            player.AddPlayerGold(-1);
            SetPLayerGoldStatus(player);
        }
        else
            Debug.Log("Not enough gold for reroll!");
    }

    //function for a scene (player)
    public void RefreshMinionsInTavernPlayer(Player player)
    {
        if (player.GetPlayerGold() >= 1)
        {
            ShowMinionsInTavern(player, shopSlots, 0);
            player.AddPlayerGold(-1);
            SetPLayerGoldStatus(player);
        }
        else
            Debug.Log("Not enough gold for reroll!");
    }

    //function for a scene(AI)
    public void RefreshMinionsInTavernAI(Player player)
    {
        if (player.GetPlayerGold() >= 1)
        {
            ShowMinionsInTavern(player, shopSlotsAI, 0);
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


        UpdateTavernTierCostText(player1, tavernTierCostText);
        UpdateTavernTierText(player1, tavernTierText);

        UpdateTavernTierCostText(player2, tavernTierCostTextAI);
        UpdateTavernTierText(player2, tavernTierTextAI);
    }

    public void UpgradeTavernLevel(Player player)
    {
        if(player.GetPlayerTavernTier() < 6 && player.GetPlayerGold() >= player.tavernTierUpgradeGold)
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

        UpdateTavernTierCostText(player1, tavernTierCostText);
        UpdateTavernTierText(player1, tavernTierText);

        UpdateTavernTierCostText(player2, tavernTierCostTextAI);
        UpdateTavernTierText(player2, tavernTierTextAI);
    }

    public void UpdateTavernTierText(Player player, Text tavernTierText)
    {
        tavernTierText.text = "Level: " + player.tavernTierLevel.ToString();
    }
    public void UpdateTavernTierCostText(Player player, Text tavernTierCostText)
    {
        tavernTierCostText.text = player.tavernTierUpgradeGold.ToString();

        if (player.tavernTierLevel == 6)
        {
            tavernTierCostText.text = "";
        }
    }

    //problem z buffem u player2(ai) -> slots, dodanie niewidzialnych slotsow dla ai???
    public void EndTurnPlayer(Player player)               // czy na pewno zalezne od playera? jak dwaj gracze naraz to chyba nie, 
                                                            //chyba, ze jakies czekanko jakby sie z bomby skonczylo ture a komp nie zdazylby, do rozkminy
    {
        //end turn effects
        List<int> micromummy = new List<int>();
        List<int> microPos = new List<int>();
        List<int> cobalt = new List<int>();
        List<int> cobaltPos = new List<int>();
        List<int> beasts = new List<int>();
        List<int> mechs = new List<int>();
        List<int> murlocs = new List<int>();
        List<int> demons = new List<int>();
        List<int> dragons = new List<int>();
        List<int> lightPos = new List<int>();
        for (int i = 0; i < player.GetPlayerBoard().Count; i++)
        {
            //MicroMummy
            if(player.GetPlayerBoard()[i].GetMinion().Name == "Micro Mummy")
            {
                microPos.Add(i);
                for(int j = 0; j < player.GetPlayerBoard().Count; j++)
                {
                    if(player.GetPlayerBoard()[j].GetPos() != i)
                    {
                        micromummy.Add(player.GetPlayerBoard()[j].GetPos());
                    }
                }
                //Debug.Log("Micro mummy end turn effect!");
            }
            //Cobalt Scalebane
            if (player.GetPlayerBoard()[i].GetMinion().Name == "Cobalt Scalebane")
            {
                cobaltPos.Add(i);
                for (int j = 0; j < player.GetPlayerBoard().Count; j++)
                {
                    if (player.GetPlayerBoard()[j].GetPos() != i)
                    {
                        cobalt.Add(player.GetPlayerBoard()[j].GetPos());
                    }
                }
                //Debug.Log("Cobalt scalebane end turn effect!");
            }
            //lightfang enforcer
            if (player.GetPlayerBoard()[i].GetMinion().Name == "Lightfang Enforcer")
            {
                lightPos.Add(i);
            }
            if (player.GetPlayerBoard()[i].GetMinion().Tribe == "Beast")
            {
                beasts.Add(i);
            }
            else if (player.GetPlayerBoard()[i].GetMinion().Tribe == "Mech")
            {
                mechs.Add(i);
            }
            else if (player.GetPlayerBoard()[i].GetMinion().Tribe == "Murloc")
            {
                murlocs.Add(i);
            }
            else if (player.GetPlayerBoard()[i].GetMinion().Tribe == "Demon")
            {
                demons.Add(i);
            }
            else if (player.GetPlayerBoard()[i].GetMinion().Tribe == "Dragon")
            {
                dragons.Add(i);
            }
        }
        for(int i = 0; i < cobaltPos.Count; i++)
        {
            if(player.GetPlayerBoard()[cobaltPos[i]].GetMinion().Golden == true)
            {
                int r = Random.Range(0, cobalt.Count);
                int random = cobalt[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 6, 0, "All", player);
                Debug.Log("Cobalt Scalebane effect!");
            }
            else
            {
                int r = Random.Range(0, cobalt.Count);
                int random = cobalt[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 3, 0, "All", player);
                Debug.Log("Cobalt Scalebane effect!");
            }

        }
        for (int i = 0; i < microPos.Count; i++)
        {
            if (player.GetPlayerBoard()[microPos[i]].GetMinion().Golden == true)
            {
                int r = Random.Range(0, micromummy.Count);
                int random = micromummy[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 2, 0, "All", player);
                Debug.Log("Micro Mummy effect!");
            }
            else
            {
                int r = Random.Range(0, micromummy.Count);
                int random = micromummy[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 1, 0, "All", player);
                Debug.Log("Micro Mummy effect!");
            }
        }
        for (int i = 0; i < lightPos.Count; i++)
        {
            if (player.GetPlayerBoard()[lightPos[i]].GetMinion().Golden == true)
            {
                if (beasts.Count > 0)
                {
                    int r = Random.Range(0, beasts.Count);
                    int random = beasts[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Beast", player);
                    Debug.Log("Golden Lightfang Effect on Beast!");
                }
                if (mechs.Count > 0)
                {
                    int r = Random.Range(0, mechs.Count);
                    int random = mechs[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Mech", player);
                    Debug.Log("Golden Lightfang Effect on Mech!");
                }
                if (murlocs.Count > 0)
                {
                    int r = Random.Range(0, murlocs.Count);
                    int random = murlocs[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Murloc", player);
                    Debug.Log("Golden Lightfang Effect on Murloc!");
                }
                if (demons.Count > 0)
                {
                    int r = Random.Range(0, demons.Count);
                    int random = demons[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Demon", player);
                    Debug.Log("Golden Lightfang Effect on Demon!");
                }
                if (dragons.Count > 0)
                {
                    int r = Random.Range(0, dragons.Count);
                    int random = dragons[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Dragon", player);
                    Debug.Log("Golden Lightfang Effect on Dragon!");
                }
            }
            else if (player.GetPlayerBoard()[lightPos[i]].GetMinion().Golden == false)
            {
                if (beasts.Count > 0)
                {
                    int r = Random.Range(0, beasts.Count);
                    int random = beasts[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Beast", player);
                    Debug.Log("Lightfang Effect on Beast!");
                }
                if (mechs.Count > 0)
                {
                    int r = Random.Range(0, mechs.Count);
                    int random = mechs[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Mech", player);
                    Debug.Log("Lightfang Effect on Mech!");
                }
                if (murlocs.Count > 0)
                {
                    int r = Random.Range(0, murlocs.Count);
                    int random = murlocs[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Murloc", player);
                    Debug.Log("Lightfang Effect on Murloc!");
                }
                if (demons.Count > 0)
                {
                    int r = Random.Range(0, demons.Count);
                    int random = demons[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Demon", player);
                    Debug.Log("Lightfang Effect on Demon!");
                }
                if (dragons.Count > 0)
                {
                    int r = Random.Range(0, dragons.Count);
                    int random = dragons[r];
                    //Debug.Log("r: " + r + ", random: " + random);
                    
                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Dragon", player);
                    Debug.Log("Lightfang Effect on Dragon!");
                }
            }
        }


        micromummy.Clear();
        cobalt.Clear();
        beasts.Clear();
        mechs.Clear();
        demons.Clear();
        murlocs.Clear();
        player1.turn = false;
        player2.turn = true;
        ChangeCanvasObjects("AI");
        ShowMinionsInTavern(player2, shopSlotsAI, 0);
    }

    public void EndTurnAI(GameObject[] minionSlots)               // czy na pewno zalezne od playera? jak dwaj gracze naraz to chyba nie, 
                                                           //chyba, ze jakies czekanko jakby sie z bomby skonczylo ture a komp nie zdazylby, do rozkminy
    {
        

        //end turn effects
        List<int> micromummy = new List<int>();
        List<int> microPos = new List<int>();
        List<int> cobalt = new List<int>();
        List<int> cobaltPos = new List<int>();
        List<int> beasts = new List<int>();
        List<int> mechs = new List<int>();
        List<int> murlocs = new List<int>();
        List<int> demons = new List<int>();
        List<int> dragons = new List<int>();
        List<int> lightPos = new List<int>();
        for (int i = 0; i < player2.GetPlayerBoard().Count; i++)
        {
            //MicroMummy
            if (player2.GetPlayerBoard()[i].GetMinion().Name == "Micro Mummy")
            {
                microPos.Add(i);
                for (int j = 0; j < player2.GetPlayerBoard().Count; j++)
                {
                    if (player2.GetPlayerBoard()[j].GetPos() != i)
                    {
                        micromummy.Add(player2.GetPlayerBoard()[j].GetPos());
                    }
                }
                //Debug.Log("Micro mummy end turn effect!");
            }
            //Cobalt Scalebane
            if (player2.GetPlayerBoard()[i].GetMinion().Name == "Cobalt Scalebane")
            {
                cobalt.Add(i);
                for (int j = 0; j < player2.GetPlayerBoard().Count; j++)
                {
                    if (player2.GetPlayerBoard()[j].GetPos() != i)
                    {
                        cobalt.Add(player2.GetPlayerBoard()[j].GetPos());
                    }
                }
                //Debug.Log("Cobalt scalebane end turn effect!");
            }
            //lightfang enforcer
            if (player2.GetPlayerBoard()[i].GetMinion().Name == "Lightfang Enforcer")
            {
                lightPos.Add(i);
                for (int j = 0; j < player2.GetPlayerBoard().Count; j++)
                {
                    if (player2.GetPlayerBoard()[j].GetMinion().Tribe == "Beast")
                    {
                        beasts.Add(j);
                    }
                    else if (player2.GetPlayerBoard()[j].GetMinion().Tribe == "Mech")
                    {
                        mechs.Add(j);
                    }
                    else if (player2.GetPlayerBoard()[j].GetMinion().Tribe == "Murloc")
                    {
                        murlocs.Add(j);
                    }
                    else if (player2.GetPlayerBoard()[j].GetMinion().Tribe == "Demon")
                    {
                        demons.Add(j);
                    }
                    else if (player2.GetPlayerBoard()[j].GetMinion().Tribe == "Dragon")
                    {
                        dragons.Add(j);
                    }
                }
                //Debug.Log("B:" + beasts.Count + ", Me:" + mechs.Count + ", Mu:" + murlocs.Count + ", De:" + demons.Count + ", Dr:" + dragons.Count);

                Debug.Log("Cobalt scalebane end turn effect!");
                //break;
            }
        }
        for (int i = 0; i < cobaltPos.Count; i++)
        {
            if (player2.GetPlayerBoard()[cobaltPos[i]].GetMinion().Golden == true)
            {
                int r = Random.Range(0, cobalt.Count);
                int random = cobalt[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 6, 0, "All", player2);
                Debug.Log("Cobalt Scalebane effect!");
            }
            else
            {
                int r = Random.Range(0, cobalt.Count);
                int random = cobalt[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 3, 0, "All", player2);
                Debug.Log("Cobalt Scalebane effect!");
            }

        }
        for (int i = 0; i < microPos.Count; i++)
        {
            if (player2.GetPlayerBoard()[microPos[i]].GetMinion().Golden == true)
            {
                int r = Random.Range(0, micromummy.Count);
                int random = micromummy[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 2, 0, "All", player2);
                Debug.Log("Micro Mummy effect!");
            }
            else
            {
                int r = Random.Range(0, micromummy.Count);
                int random = micromummy[r];
                //Debug.Log("r: " + r + ", random: " + random);

                BuffSingleMinionBoard(minionSlots[random], 1, 0, "All", player2);
                Debug.Log("Micro Mummy effect!");
            }
        }
        for (int i = 0; i < lightPos.Count; i++)
        {
            if (player2.GetPlayerBoard()[lightPos[i]].GetMinion().Golden == true)
            {
                if (beasts.Count > 0)
                {
                    int r = Random.Range(0, beasts.Count);
                    int random = beasts[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Beast", player2);
                }
                if (mechs.Count > 0)
                {
                    int r = Random.Range(0, mechs.Count);
                    int random = mechs[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Mech", player2);
                }
                if (murlocs.Count > 0)
                {
                    int r = Random.Range(0, murlocs.Count);
                    int random = murlocs[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Murloc", player2);
                }
                if (demons.Count > 0)
                {
                    int r = Random.Range(0, demons.Count);
                    int random = demons[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Demon", player2);
                }
                if (dragons.Count > 0)
                {
                    int r = Random.Range(0, dragons.Count);
                    int random = dragons[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 4, 4, "Dragon", player2);
                }
            }
            else
            {
                if (beasts.Count > 0)
                {
                    int r = Random.Range(0, beasts.Count);
                    int random = beasts[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Beast", player2);
                }
                if (mechs.Count > 0)
                {
                    int r = Random.Range(0, mechs.Count);
                    int random = mechs[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Mech", player2);
                }
                if (murlocs.Count > 0)
                {
                    int r = Random.Range(0, murlocs.Count);
                    int random = murlocs[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Murloc", player2);
                }
                if (demons.Count > 0)
                {
                    int r = Random.Range(0, demons.Count);
                    int random = demons[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Demon", player2);
                }
                if (dragons.Count > 0)
                {
                    int r = Random.Range(0, dragons.Count);
                    int random = dragons[r];
                    //Debug.Log("r: " + r + ", random: " + random);

                    BuffSingleMinionBoard(minionSlots[random], 2, 2, "Dragon", player2);
                }
            }
        }


        micromummy.Clear();
        cobalt.Clear();
        beasts.Clear();
        mechs.Clear();
        demons.Clear();
        murlocs.Clear();

        

        fight.ShowFightBefore(0);
        Fight(player1, player2);
        fight.ShowFightBefore(1);
        player1.fight = true;
        player2.fight = true;
        ShowHideFightPanel(true);

        for (int turn = 1; turn < 99; turn++)
        {
            if (player1.turnNumber == turn && player2.turnNumber < 9)
            {
                //player1.SetPlayerGold(turn + 2);
               // player2.SetPlayerGold(turn + 2);
                if (player1.GetPlayerGold() > 10 || player2.GetPlayerGold() > 10)
                {
                    //player1.SetPlayerGold(10);
                    //player2.SetPlayerGold(10);
                }
                break;
            }
        }
        SetPLayerGoldStatus(player1);
        SetPLayerGoldStatus(player2);

        player1.turnNumber++;
        player2.turnNumber++;

        UpdateTavernPrice();

        if(player1.GetHealth() <= 0 || player2.GetHealth() <= 0)
        {
            ResetGame();
            if (player1.GetHealth() <= 0)
                player1.dead = true;
            else
                player2.dead = true;
        }

        UpdateTavernTierCostText(player1, tavernTierCostText);
        UpdateTavernTierText(player1, tavernTierText);

        UpdateTavernTierCostText(player2, tavernTierCostTextAI);
        UpdateTavernTierText(player2, tavernTierTextAI);
        player1.turn = true;
        player2.turn = false;
        ChangeCanvasObjects("Player");
        ShowMinionsInTavern(player1, shopSlots, 0);
    }

    public void PlayMinionOnBoard(Player player, GameObject handSlot, GameObject minionSlot, GameObject[] handSlots, GameObject[] minionSlots)
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

        /*
        //restore player board
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
        */
        player.GetPlayerBoard().Add(board);
        minionSlot.GetComponent<Minion>().InitializeMinion(minionInstance, handSlot.GetComponent<Minion>().GetMinion().Golden);

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
                SummonTokenBoard("Golden Tabbycat", 1, player, minionSlots);
            else
                SummonTokenBoard("Tabbycat", 1, player, minionSlots);

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
                int newHandPos = 99;
                List<int> temp = new List<int>();
                GameObject useless = new GameObject();
                useless.AddComponent<ShopMinion>();
                GameObject arrow = new GameObject();
                arrow.transform.SetParent(useless.transform);
                useless.GetComponent<ShopMinion>().arrow = arrow;
                useless.GetComponent<ShopMinion>().gc = this.gameObject.GetComponent<GameController>();
                GoldenMinion(player, newHandPos, temp, temp, useless, "token", "Golden Tabbycat", tabbyPos, minionSlots, handSlots);
                
            }
        }
        else if (handSlot.GetComponent<Minion>().minionName.text == "Murloc Tidehunter" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY MURLOC SCOUT");
            if(handSlot.GetComponent<Minion>().golden == false)
                SummonTokenBoard("Murloc Scout", 1, player, minionSlots);
            else
                SummonTokenBoard("Golden Murloc Scout", 1, player, minionSlots);

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
                int newHandPos = 99;
                List<int> temp = new List<int>();
                GameObject useless = new GameObject();
                useless.AddComponent<ShopMinion>();
                GameObject arrow = new GameObject();
                arrow.transform.SetParent(useless.transform);
                useless.GetComponent<ShopMinion>().arrow = arrow;
                useless.GetComponent<ShopMinion>().gc = this.gameObject.GetComponent<GameController>();
                GoldenMinion(player, newHandPos, temp, temp, useless, "token", "Golden Murloc Scout", scoutPos, minionSlots, handSlots);
                //GoldenMinion(player);
                
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
                if(minionSlots[i].GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
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
        //Vulgar Homunculus
        else if (handSlot.GetComponent<Minion>().minionName.text == "Vulgar Homunculus" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY ROCKPOOL HUNTER");
            player.AddHealth(-2);
        }
        //Metaltooth Leaper
        else if(handSlot.GetComponent<Minion>().minionName.text == "Metaltooth Leaper" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY METALTOOTH LEAPER");

            for(int i = 0; i < minionSlots.Length; i++)
            {
                if(minionSlots[i].GetComponent<Minion>().tribe.text == "Mech" && handSlot.GetComponent<HandMinion>().placedSlot != i)
                {
                    if(handSlot.GetComponent<Minion>().golden == false)
                        BuffSingleMinionBoard(minionSlots[i], 2, 0, "Mech", player);
                    else
                        BuffSingleMinionBoard(minionSlots[i], 4, 0, "Mech", player);
                }
            }
        }
        //nathrezim overseer
        else if (handSlot.GetComponent<Minion>().minionName.text == "Nathrezim Overseer" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY NATHREZIM OVERSEER");
            int demonCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Demon" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    demonCounter++;
                    locationIterator.Add(i);
                }
            }

            if (locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 2, 2, "Demon", player);
                else
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 4, 4, "Demon", player);
            }
            else
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[minionNumber], 2, 2, "Demon", player);
                else
                    BuffSingleMinionBoard(minionSlots[minionNumber], 4, 4, "Demon", player);
            }
            locationIterator.Clear();
        }
        //Coldlight Seer
        else if (handSlot.GetComponent<Minion>().minionName.text == "Coldlight Seer" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY COLDLIGHT SEER");

            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<HandMinion>().placedSlot != i)
                {
                    if (handSlot.GetComponent<Minion>().golden == false)
                        BuffSingleMinionBoard(minionSlots[i], 0, 2, "Murloc", player);
                    else
                        BuffSingleMinionBoard(minionSlots[i], 0, 4, "Murloc", player);
                }
            }
        }
        //Crystalweaver
        else if (handSlot.GetComponent<Minion>().minionName.text == "Crystalweaver" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY CRYSTALWEAVER");

            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Demon" && handSlot.GetComponent<HandMinion>().placedSlot != i)
                {
                    if (handSlot.GetComponent<Minion>().golden == false)
                        BuffSingleMinionBoard(minionSlots[i], 1, 1, "Demon", player);
                    else
                        BuffSingleMinionBoard(minionSlots[i], 2, 2, "Demon", player);
                }
            }
        }
        //Felfin Navigator
        else if (handSlot.GetComponent<Minion>().minionName.text == "Felfin Navigator" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY FELFIN NAVIGATOR");

            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<HandMinion>().placedSlot != i)
                {
                    if (handSlot.GetComponent<Minion>().golden == false)
                        BuffSingleMinionBoard(minionSlots[i], 1, 1, "Murloc", player);
                    else
                        BuffSingleMinionBoard(minionSlots[i], 2, 2, "Murloc", player);
                }
            }
        }
        //houndmaster
        else if (handSlot.GetComponent<Minion>().minionName.text == "Houndmaster" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY HOUNDMASTER");
            int beastCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Beast" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    beastCounter++;
                    locationIterator.Add(i);
                }
            }

            if (locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 2, 2, "Beast", player);
                else
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 4, 4, "Beast", player);

                minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion().Taunt = true;
                minionSlots[locationIterator[0]].GetComponent<Minion>().InitializeMinion(minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion(), 
                    minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion().Golden);
            }
            else
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[minionNumber], 2, 2, "Beast", player);
                else
                    BuffSingleMinionBoard(minionSlots[minionNumber], 4, 4, "Beast", player);

                minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion().Taunt = true;
                minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().InitializeMinion(minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion(),
                    minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion().Golden);
            }
            locationIterator.Clear();
        }
        //Screwjank Clunker
        else if (handSlot.GetComponent<Minion>().minionName.text == "Screwjank Clunker" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY SCREWJANK CLUNKER");
            int mechCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Mech" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    mechCounter++;
                    locationIterator.Add(i);
                }
            }

            if (locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 2, 2, "Mech", player);
                else
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 4, 4, "Mech", player);
            }
            else
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[minionNumber], 2, 2, "Mech", player);
                else
                    BuffSingleMinionBoard(minionSlots[minionNumber], 4, 4, "Mech", player);
            }
            locationIterator.Clear();
        }
        //toxfin
        else if (handSlot.GetComponent<Minion>().minionName.text == "Toxfin" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY TOXFIN");
            int murlocCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    murlocCounter++;
                    locationIterator.Add(i);
                }
            }

            if (locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion().Poison = true;
                minionSlots[locationIterator[0]].GetComponent<Minion>().InitializeMinion(minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion(),
                    minionSlots[locationIterator[0]].GetComponent<Minion>().GetMinion().Golden);
            }
            else
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion().Poison = true;
                minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().InitializeMinion(minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion(),
                    minionSlots[locationIterator[minionNumber]].GetComponent<Minion>().GetMinion().Golden);
            }
            locationIterator.Clear();
        }
        //Virmen Sensei
        else if (handSlot.GetComponent<Minion>().minionName.text == "Virmen Sensei" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY VIRMEN SENSEI");
            int beastCounter = 0;
            List<int> locationIterator = new List<int>();
            for (int i = 0; i < minionSlots.Length; i++)
            {
                if (minionSlots[i].GetComponent<Minion>().tribe.text == "Beast" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    beastCounter++;
                    locationIterator.Add(i);
                }
            }

            if (locationIterator.Count == 0)
            {
                Debug.Log("No minions to buff");
            }
            else if (locationIterator.Count == 1)
            {
                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 2, 2, "Beast", player);
                else
                    BuffSingleMinionBoard(minionSlots[locationIterator[0]], 4, 4, "Beast", player);
            }
            else
            {
                int n = Random.Range(0, locationIterator.Count);
                int minionNumber = locationIterator[n];
                Debug.Log("n: " + n + "minionNumber = " + minionNumber);

                if (handSlot.GetComponent<Minion>().golden == false)
                    BuffSingleMinionBoard(minionSlots[minionNumber], 2, 2, "Beast", player);
                else
                    BuffSingleMinionBoard(minionSlots[minionNumber], 4, 4, "Beast", player);
            }
            locationIterator.Clear();
        }
        //Annihilan Battlemaster
        else if (handSlot.GetComponent<Minion>().minionName.text == "Annihilan Battlemaster" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY ANNIHILAN BATTLEMASTER");

            int h = 40 - player.GetHealth();

            if(handSlot.GetComponent<Minion>().golden == false)
                minionSlot.GetComponent<Minion>().GetMinion().Hp = h + 1;
            else
                minionSlot.GetComponent<Minion>().GetMinion().Hp = 2*h + 1;

            minionSlot.GetComponent<Minion>().InitializeMinion(minionSlot.GetComponent<Minion>().GetMinion(), minionSlot.GetComponent<Minion>().GetMinion().Golden);

        }
        //King Bagurgle
        else if (handSlot.GetComponent<Minion>().minionName.text == "King Bagurgle" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY KING BAGURGLE");

            if (handSlot.GetComponent<Minion>().golden == false)
            {
                for(int i = 0; i < minionSlots.Length; i++)
                {
                    if(i != handSlot.GetComponent<HandMinion>().placedSlot)
                    {
                        BuffSingleMinionBoard(minionSlots[i], 2, 2, "Murloc", player);
                    }
                }
            }
            else
            {
                for (int i = 0; i < minionSlots.Length; i++)
                {
                    if (i != handSlot.GetComponent<HandMinion>().placedSlot)
                    {
                        BuffSingleMinionBoard(minionSlots[i], 4, 4, "Murloc", player);
                    }
                }
            }
        }
        //Strongshell Scavenger
        else if (handSlot.GetComponent<Minion>().minionName.text == "Strongshell Scavenger" && handSlot.GetComponent<Minion>().blank == false)
        {
            Debug.Log("BATTLECRY STRONGSHELL SCAVENGER");

            if (handSlot.GetComponent<Minion>().golden == false)
            {
                for (int i = 0; i < minionSlots.Length; i++)
                {
                    if (minionSlots[i].GetComponent<Minion>().GetMinion().Taunt == true)
                    {
                        BuffSingleMinionBoard(minionSlots[i], 2, 2, "All", player);
                    }
                }
            }
            else
            {
                for (int i = 0; i < minionSlots.Length; i++)
                {
                    if (minionSlots[i].GetComponent<Minion>().GetMinion().Taunt == true)
                    {
                        BuffSingleMinionBoard(minionSlots[i], 4, 4, "All", player);
                    }
                }
            }
        }

        //PASSIVES REALIZATION
        //MURLOC TIDECALLER
        if (handSlot.GetComponent<Minion>().tribe.text == "Murloc" && handSlot.GetComponent<Minion>().minionName.text != "Murloc Tidehunter")
        {
            for(int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if(player.GetPlayerBoard()[i].GetMinion().Name == "Murloc Tidecaller" && handSlot.GetComponent<HandMinion>().placedSlot != i)  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 1, 0, "Murloc", player);
                    else
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 2, 0, "Murloc", player);
                }
            }
        }
        //WRATH WEAVER
        if (handSlot.GetComponent<Minion>().tribe.text == "Demon")
        {
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if (player.GetPlayerBoard()[i].GetMinion().Name == "Wrath Weaver")  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 2, 2, "All", player);
                    else
                        BuffSingleMinionBoard(minionSlots[player.GetPlayerBoard()[i].GetPos()], 4, 4, "All", player);

                    player.AddHealth(-1);
                    //hp on scene? update?
                }
            }
        }
        //PACK LEADER & MAMA BEAR
        else if (handSlot.GetComponent<Minion>().tribe.text == "Beast")
        {
            for (int i = 0; i < player.GetPlayerBoard().Count; i++)
            {
                if (player.GetPlayerBoard()[i].GetMinion().Name == "Pack Leader")  //jakos trzeba odciac ostatnio zagrana jednostke, zeby sam sie nei buffowal
                {
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlot, 2, 0, "Beast", player);
                    else
                        BuffSingleMinionBoard(minionSlot, 4, 0, "Beast", player);
                }
                else if (player.GetPlayerBoard()[i].GetMinion().Name == "Mama Bear" && handSlot.GetComponent<HandMinion>().placedSlot != i)
                {
                    if (player.GetPlayerBoard()[i].GetMinion().Golden == false)
                        BuffSingleMinionBoard(minionSlot, 4, 4, "Beast", player);
                    else
                        BuffSingleMinionBoard(minionSlot, 8, 8, "Beast", player);
                }
            }
        }

        //initialize blank on hand
        handSlot.GetComponent<Minion>().InitializeBlank();

    }

    public void SetPLayerGoldStatus(Player player)
    {
        if (player == player1)
            playerGold.text = "Gold: " + player1.GetPlayerGold().ToString();
        else if (player == player2)
            playerGoldAI.text = "Gold: " + player2.GetPlayerGold().ToString();
    }
    /*
    public void SetPLayerGoldStatusAI(Player player)
    {
        playerGoldAI.text = "Gold: " + player.GetPlayerGold().ToString();
    }
    */
    public void SetupHandSlots()
    {
        for (int i = 0; i < handSlots.Length; i++)
        {
            handSlots[i].GetComponent<Minion>().InitializeBlank();
        }
        //freeSpaceInHand = true;

        for (int i = 0; i < handSlotsAI.Length; i++)
        {
            handSlotsAI[i].GetComponent<Minion>().InitializeBlank();
        }
    }

    public void SetupBoardSlots()
    {
        for (int i = 0; i < minionSlots.Length; i++)
        {
            minionSlots[i].GetComponent<Minion>().InitializeBlank();
        }
        //freeSpaceOnBoard = true;
        for (int i = 0; i < minionSlotsAI.Length; i++)
        {
            minionSlotsAI[i].GetComponent<Minion>().InitializeBlank();
        }
    }

    public void SetupDiscoverSlots()
    {
        for (int i = 0; i < discoverSlots.Length; i++)
        {
            discoverSlots[i].GetComponent<Minion>().InitializeBlank();
        }
    }

    public void SetupShopSlots()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i].GetComponent<Minion>().InitializeBlank();
            shopSlots[i].GetComponent<ShopMinion>().arrow.SetActive(false);
        }

        for (int i = 0; i < shopSlotsAI.Length; i++)
        {
            shopSlotsAI[i].GetComponent<Minion>().InitializeBlank();
            shopSlotsAI[i].GetComponent<ShopMinion>().arrow.SetActive(false);
        }
    }

    public void SetupFightSlots()
    {
        fight.SetupFightSlots();
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
        ShowMinionsInTavern(player, slots, 0);
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
        }
        else if(minionA.DivineShield == false && minionB.DivineShield == true)
        {
            minionB.DivineShield = false;
            if (minionB.Poison == true)
            {
                minionA.Hp = 0;
            }
            
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
                    else if (minionA.Name == "Harvest Golem")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                            SummonTokenFight("Damaged Golem", 1, p1);
                        else
                            SummonTokenFight("Golden Damaged Golem", 1, p1);
                    }
                    else if (minionA.Name == "Kindly Grandmother")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Big Bad Wolf", 1, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Big Bad Wolf", 1, p1);
                        }
                    }
                    else if (minionA.Name == "Selfless Hero")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            ChangeRandomOptionFight("ds", p1);
                        }
                        else
                        {
                            ChangeRandomOptionFight("ds", p1);
                            ChangeRandomOptionFight("ds", p1);
                        }
                    }
                    else if (minionA.Name == "Infested Wolf")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Spider", 2, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Spider", 2, p1);
                        }
                    }
                    else if (minionA.Name == "Replicating Menace")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Microbot", 3, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Microbot", 3, p1);
                        }
                    }
                    else if (minionA.Name == "Mechano-Egg")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Robosaur", 1, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Robosaur", 1, p1);
                        }
                    }
                    else if (minionA.Name == "Savannah Highmane")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Hyena", 2, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Hyena", 2, p1);
                        }
                    }
                    else if (minionA.Name == "King Bagurgle")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            for(int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if(p1.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Murloc")
                                {
                                    BuffSingleMinionFight(2, 2, p1);
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Murloc")
                                {
                                    BuffSingleMinionFight(4, 4, p1);
                                }
                            }
                        }      
                    }
                    else if (minionA.Name == "Voidlord")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonTokenFight("Voidwalker", 3, p1);
                        }
                        else
                        {
                            SummonTokenFight("Golden Voidwalker", 3, p1);
                        }
                    }
                    else if (minionA.Name == "Goldrinn, the Great Wolf")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Beast")
                                {
                                    BuffSingleMinionFight(5, 5, p1);
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Beast")
                                {
                                    BuffSingleMinionFight(10, 10, p1);
                                }
                            }
                        }
                    }
                    else if (minionA.Name == "Ghastcoiler")
                    {
                        Debug.Log(minionA.Name + " Deathrattle!");
                        if (minionA.Golden == false)
                        {
                            SummonRandomDHFight(2, p1);
                        }
                        else
                        {
                            SummonRandomDHFight(4, p1);
                        }
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
                    //junkbot
                    else if (minionA.Tribe == "Mech")
                    {
                        Debug.Log("zdechl mech");
                        for (int j = 0; j < p1.GetPlayerCopiedBoard().Count; j++)
                        {
                            if (p1.GetPlayerCopiedBoard()[j].GetMinion().Name == "Junkbot")
                            {
                                Debug.Log(minionA.Name + " Passive!");
                                if (p1.GetPlayerCopiedBoard()[j].GetMinion().Golden == false)
                                    BuffSpecifiedMinionFight(2, 2, p1, j);
                                else
                                    BuffSpecifiedMinionFight(4, 4, p1, j);
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
                    else if (minionB.Name == "Harvest Golem")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                            SummonTokenFight("Damaged Golem", 1, p2);
                        else
                            SummonTokenFight("Golden Damaged Golem", 1, p2);
                    }
                    else if (minionB.Name == "Kindly Grandmother")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Big Bad Wolf", 1, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Big Bad Wolf", 1, p2);
                        }
                    }
                    else if (minionB.Name == "Selfless Hero")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            ChangeRandomOptionFight("ds", p2);
                        }
                        else
                        {
                            ChangeRandomOptionFight("ds", p2);
                            ChangeRandomOptionFight("ds", p2);
                        }
                    }
                    else if (minionB.Name == "Infested Wolf")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Spider", 2, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Spider", 2, p2);
                        }
                    }
                    else if (minionB.Name == "Replicating Menace")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Microbot", 3, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Microbot", 3, p2);
                        }
                    }
                    else if (minionB.Name == "Mechano-Egg")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Robosaur", 1, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Robosaur", 1, p2);
                        }
                    }
                    else if (minionB.Name == "Savannah Highmane")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Hyena", 2, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Hyena", 2, p2);
                        }
                    }
                    else if (minionB.Name == "King Bagurgle")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Murloc")
                                {
                                    BuffSingleMinionFight(2, 2, p2);
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Murloc")
                                {
                                    BuffSingleMinionFight(4, 4, p2);
                                }
                            }
                        }
                    }
                    else if (minionB.Name == "Voidlord")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonTokenFight("Voidwalker", 3, p2);
                        }
                        else
                        {
                            SummonTokenFight("Golden Voidwalker", 3, p2);
                        }
                    }
                    else if (minionB.Name == "Goldrinn, the Great Wolf")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Beast")
                                {
                                    BuffSingleMinionFight(5, 5, p2);
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                            {
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Tribe == "Beast")
                                {
                                    BuffSingleMinionFight(10, 10, p2);
                                }
                            }
                        }
                    }
                    else if (minionB.Name == "Ghastcoiler")
                    {
                        Debug.Log(minionB.Name + " Deathrattle!");
                        if (minionB.Golden == false)
                        {
                            SummonRandomDHFight(2, p2);
                        }
                        else
                        {
                            SummonRandomDHFight(4, p2);
                        }
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
                    //junkbot
                    else if (minionB.Tribe == "Mech")
                    {
                        Debug.Log("zdechl mech");
                        for (int j = 0; j < p2.GetPlayerCopiedBoard().Count; j++)
                        {
                            if (p2.GetPlayerCopiedBoard()[j].GetMinion().Name == "Junkbot")
                            {
                                Debug.Log(minionB.Name + " Passive!");
                                if (p2.GetPlayerCopiedBoard()[j].GetMinion().Golden == false)
                                    BuffSpecifiedMinionFight(2, 2, p2, j);
                                else
                                    BuffSpecifiedMinionFight(4, 4, p2, j);
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
                //save last result: //0 -> lost, 1 -> draw, 2 -> won
                player2.lastResult = 2;
                player1.lastResult = 0;
                winP2++;
                break;
            }
            else if (player2.GetPlayerCopiedBoard().Count == 0 && player1.GetPlayerCopiedBoard().Count > 0)
            {
                Debug.Log("P1 WON!");
                CalculateDamage(player1, player2);
                //save last result: //0 -> lost, 1 -> draw, 2 -> won
                player2.lastResult = 0;
                player1.lastResult = 2;
                winP1++;
                break;
            }
            else if (player1.GetPlayerCopiedBoard().Count == 0 && player2.GetPlayerCopiedBoard().Count == 0)
            {
                Debug.Log("TIE!");
                //save last result: //0 -> lost, 1 -> draw, 2 -> won
                player2.lastResult = 1;
                player1.lastResult = 1;
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

    //option => simple
    //       => token
    public void GoldenMinion(Player player, int newHandPos, List<int> tempHandPos, List<int> tempBoardPos, GameObject shopMinion, 
        string option, string tokenName, List<int>tokenPos, GameObject[] minionSlots, GameObject[] handSlots)
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

        if(temp.Count > 2)
        {
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
        }
        if (temp.Count == 2)
        {
            int r1 = Random.Range(0, temp.Count);
            string m1 = temp[r1].GetName();
            temp.RemoveAt(r1);
            int r2 = Random.Range(0, temp.Count);
            string m2 = temp[r2].GetName();
            temp.RemoveAt(r2);

            XmlNode minionNode1 = minionData.GetMinionByName(m1, minionDataXML);
            discoverSlots[0].GetComponent<Minion>().InitializeMinion(minionNode1);
            XmlNode minionNode2 = minionData.GetMinionByName(m2, minionDataXML);
            discoverSlots[1].GetComponent<Minion>().InitializeMinion(minionNode2);
            discoverSlots[2].GetComponent<Minion>().InitializeBlank();
        }
        if (temp.Count == 1)
        {
            int r1 = Random.Range(0, temp.Count);
            string m1 = temp[r1].GetName();
            temp.RemoveAt(r1);

            XmlNode minionNode1 = minionData.GetMinionByName(m1, minionDataXML);
            discoverSlots[0].GetComponent<Minion>().InitializeMinion(minionNode1);
            discoverSlots[1].GetComponent<Minion>().InitializeBlank();
            discoverSlots[2].GetComponent<Minion>().InitializeBlank();
        }
        if (temp.Count == 0)
        {
            discoverSlots[0].GetComponent<Minion>().InitializeBlank();
            discoverSlots[1].GetComponent<Minion>().InitializeBlank();
            discoverSlots[2].GetComponent<Minion>().InitializeBlank();
        }

        ShowHideDiscoverPanel(true);
        if (temp.Count == 0)
            ShowHideDiscoverPanel(false);

        if (option == "simple")
        {
            //combine minions
            MinionData[] temporaryMinions = new MinionData[3];

            if (tempHandPos.Count == 3)
            {
                temporaryMinions[0] = handSlots[tempHandPos[0]].GetComponent<Minion>().GetMinion();
                temporaryMinions[1] = handSlots[tempHandPos[1]].GetComponent<Minion>().GetMinion();
            }
            else if (tempHandPos.Count == 2 && tempBoardPos.Count == 1)
            {
                temporaryMinions[0] = minionSlots[tempBoardPos[0]].GetComponent<Minion>().GetMinion();
                temporaryMinions[1] = handSlots[tempHandPos[1]].GetComponent<Minion>().GetMinion();
            }
            else if (tempHandPos.Count == 1 && tempBoardPos.Count == 2)
            {
                temporaryMinions[0] = minionSlots[tempBoardPos[0]].GetComponent<Minion>().GetMinion();
                temporaryMinions[1] = minionSlots[tempBoardPos[1]].GetComponent<Minion>().GetMinion();
            }

            bool ds = false;
            bool poison = false;
            bool taunt = false;
            bool golden = true;

            if (temporaryMinions[0].DivineShield == true || temporaryMinions[1].DivineShield == true)
                ds = true;
            if (temporaryMinions[0].Poison == true || temporaryMinions[1].Poison == true)
                poison = true;
            if (temporaryMinions[0].Taunt == true || temporaryMinions[1].Taunt == true)
                taunt = true;
            MinionData newMinion = new MinionData();
            newMinion.Initialize(temporaryMinions[0].Name, temporaryMinions[0].Attack + temporaryMinions[1].Attack, temporaryMinions[0].Hp + temporaryMinions[1].Hp,
                temporaryMinions[0].Tribe, temporaryMinions[0].TavernTier, ds, poison, taunt, temporaryMinions[0].Skill, temporaryMinions[0].GoldenSkill, golden);
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

            shopMinion.GetComponent<ShopMinion>().triple = false;
        }
        else if(option == "token")
        {
            MinionData[] temporaryMinions = new MinionData[3];
            temporaryMinions[0] = minionSlots[tokenPos[0]].GetComponent<Minion>().GetMinion();
            temporaryMinions[1] = minionSlots[tokenPos[1]].GetComponent<Minion>().GetMinion();

            bool ds = false;
            bool poison = false;
            bool taunt = false;
            bool golden = true;

            if (temporaryMinions[0].DivineShield == true || temporaryMinions[1].DivineShield == true)
                ds = true;
            if (temporaryMinions[0].Poison == true || temporaryMinions[1].Poison == true)
                poison = true;
            if (temporaryMinions[0].Taunt == true || temporaryMinions[1].Taunt == true)
                taunt = true;
            MinionData newMinion = new MinionData();
            newMinion.Initialize(tokenName, temporaryMinions[0].Attack + temporaryMinions[1].Attack, temporaryMinions[0].Hp + temporaryMinions[1].Hp,
                temporaryMinions[0].Tribe, temporaryMinions[0].TavernTier, ds, poison, taunt, temporaryMinions[0].Skill, temporaryMinions[0].GoldenSkill, golden);

            //find free hand spot
            int newTempHandPos = 99;
            for (int i = 0; i < handSlots.Length; i++)
            {
                if (handSlots[i].GetComponent<Minion>().blank == true)
                {
                    newTempHandPos = i;
                    Player.Board newM = new Player.Board(handSlots[i].GetComponent<Minion>().GetMinion(), i);
                    player.GetPlayerHand().Add(newM);
                    break;
                }
            }
            handSlots[newTempHandPos].GetComponent<Minion>().InitializeMinion(newMinion, golden);
            handSlots[newTempHandPos].GetComponent<HandMinion>().placed = false;

            minionSlots[tokenPos[0]].GetComponent<Minion>().InitializeBlank();
            minionSlots[tokenPos[1]].GetComponent<Minion>().InitializeBlank();
            minionSlots[tokenPos[2]].GetComponent<Minion>().InitializeBlank();
        }
        player.lastGoldenMinionCounter = player.goldenMinionCounter;
        player.goldenMinionCounter++;
    }

    public void ChooseDiscoveredMinion(Player player)
    {
        if(player.GetPlayerGold() >= 3)
        {
            BuyMinionPlayer(player);
            //BuyMinion(player, );
            player.AddPlayerGold(3);
        }
        else if (player.GetPlayerGold() < 3)
        {
            player.AddPlayerGold(3);
            BuyMinionPlayer(player);
        }
        SetPLayerGoldStatus(player);
        ShowHideDiscoverPanel(false);
    }

    
    public void ChooseDiscoveredMinionAI(Player player, GameObject minion)
    {
        if (player.GetPlayerGold() >= 3)
        {
            BuyMinionAI(player, minion);
            //BuyMinion(player, );
            player.AddPlayerGold(3);
        }
        else if (player.GetPlayerGold() < 3)
        {
            player.AddPlayerGold(3);
            BuyMinionAI(player, minion);
        }
        SetPLayerGoldStatus(player);
        ShowHideDiscoverPanel(false);
    }
    

    //AI        -> show AI canvas & hide player Canvas
    //Player    -> show player canvas & hide ai Canvas
    public void ChangeCanvasObjects(string option)
    {
        if(option == "AI")
        {
            AllPlayer.SetActive(false);
            AllAI.SetActive(true);

            tavernTierCostText.transform.parent.gameObject.SetActive(false);
            tavernTierText.transform.parent.gameObject.SetActive(false);
            tavernTierCostTextAI.transform.parent.gameObject.SetActive(true);
            tavernTierTextAI.transform.parent.gameObject.SetActive(true);
            playerGold.transform.parent.gameObject.SetActive(false);
            playerGoldAI.transform.parent.gameObject.SetActive(true);
        }
        else if (option == "Player")
        {
            AllAI.SetActive(false);
            AllPlayer.SetActive(true);
            
            tavernTierCostTextAI.transform.parent.gameObject.SetActive(false);
            tavernTierTextAI.transform.parent.gameObject.SetActive(false);
            tavernTierCostText.transform.parent.gameObject.SetActive(true);
            tavernTierText.transform.parent.gameObject.SetActive(true);
            playerGoldAI.transform.parent.gameObject.SetActive(false);
            playerGold.transform.parent.gameObject.SetActive(true);
        }
    }

    public void ResetGame()
    {
        if(player1.GetHealth() <= 0 || player2.GetHealth() <= 0)
        {
            SetupBoardSlots();
            SetupHandSlots();
            SetupShopSlots();
            SetupDiscoverSlots();
            SetupFightSlots();

            player1.Initialize();
            player2.Initialize();

            SaveWinNumberToFile();

            winP1 = 0;
            winP2 = 0;
            gameNr++;

        }
    }

    //BATTLECRIES:
    public void SummonTokenBoard(string tokenName, int number, Player player, GameObject[] minionSlots)
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

                    int mama = 0;
                    int g_mama = 0;
                    int pack = 0;
                    int g_pack = 0;
                    if(minionInstance.Tribe == "Beast")
                    {
                        for (int j = 0; j < player.GetPlayerBoard().Count; j++)
                        {
                            if (player.GetPlayerBoard()[j].GetMinion().Name == "Mama Bear" && player.GetPlayerBoard()[j].GetMinion().Golden == false)
                            {
                                mama++;
                            }
                            else if (player.GetPlayerBoard()[j].GetMinion().Name == "Mama Bear" && player.GetPlayerBoard()[j].GetMinion().Golden == true)
                            {
                                g_mama++;
                            }
                            else if (player.GetPlayerBoard()[j].GetMinion().Name == "Pack Leader" && player.GetPlayerBoard()[j].GetMinion().Golden == false)
                            {
                                pack++;
                            }
                            else if (player.GetPlayerBoard()[j].GetMinion().Name == "Pack Leader" && player.GetPlayerBoard()[j].GetMinion().Golden == true)
                            {
                                g_pack++;
                            }
                        }
                    }
                    
                    for(int j = 0; j < mama; j++)
                    {
                        minionInstance.Attack += 4;
                        minionInstance.Hp += 4;
                    }
                    for(int j = 0; j < g_mama; j++)
                    {
                        minionInstance.Attack += 8;
                        minionInstance.Hp += 8;
                    }
                    for (int j = 0; j < pack; j++)
                    {
                        minionInstance.Attack += 2;
                    }
                    for (int j = 0; j < g_pack; j++)
                    {
                        minionInstance.Attack += 4;
                    }

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

            int mama = 0;
            int g_mama = 0;
            int pack = 0;
            int g_pack = 0;
            if (minionInstance.Tribe == "Beast")
            {
                for (int j = 0; j < player.GetPlayerCopiedBoard().Count; j++)
                {
                    if (player.GetPlayerCopiedBoard()[j].GetMinion().Name == "Mama Bear" && player.GetPlayerCopiedBoard()[j].GetMinion().Golden == false)
                    {
                        mama++;
                    }
                    else if (player.GetPlayerCopiedBoard()[j].GetMinion().Name == "Mama Bear" && player.GetPlayerCopiedBoard()[j].GetMinion().Golden == true)
                    {
                        g_mama++;
                    }
                    else if (player.GetPlayerCopiedBoard()[j].GetMinion().Name == "Pack Leader" && player.GetPlayerCopiedBoard()[j].GetMinion().Golden == false)
                    {
                        pack++;
                    }
                    else if (player.GetPlayerCopiedBoard()[j].GetMinion().Name == "Pack Leader" && player.GetPlayerCopiedBoard()[j].GetMinion().Golden == true)
                    {
                        g_pack++;
                    }
                }
            }

            for (int j = 0; j < mama; j++)
            {
                minionInstance.Attack += 4;
                minionInstance.Hp += 4;
            }
            for (int j = 0; j < g_mama; j++)
            {
                minionInstance.Attack += 8;
                minionInstance.Hp += 8;
            }
            for (int j = 0; j < pack; j++)
            {
                minionInstance.Attack += 2;
            }
            for (int j = 0; j < g_pack; j++)
            {
                minionInstance.Attack += 4;
            }

            Player.Board board = new Player.Board(minionInstance, n);
            player.GetPlayerCopiedBoard().Add(board);
        }
    }

    public void ChangeRandomOptionFight(string option, Player player)
    {
        List<int> dsPos = new List<int>();
        List<int> poisonPos = new List<int>();
        List<int> tauntPos = new List<int>();
        for (int i = 0; i < player.GetPlayerCopiedBoard().Count; i++)
        {
            if(player.GetPlayerCopiedBoard()[i].GetMinion().DivineShield == false)
            {
                dsPos.Add(i);
            }
            if (player.GetPlayerCopiedBoard()[i].GetMinion().Poison == false)
            {
                poisonPos.Add(i);
            }
            if (player.GetPlayerCopiedBoard()[i].GetMinion().Taunt == false)
            {
                tauntPos.Add(i);
            }
        }

        if(option == "ds")
        {
            int r = Random.Range(0, dsPos.Count);
            player.GetPlayerCopiedBoard()[dsPos[r]].GetMinion().DivineShield = true;
        }
        else if (option == "poison")
        {
            int r = Random.Range(0, poisonPos.Count);
            player.GetPlayerCopiedBoard()[poisonPos[r]].GetMinion().Poison = true;
        }
        else if (option == "taunt")
        {
            int r = Random.Range(0, tauntPos.Count);
            player.GetPlayerCopiedBoard()[tauntPos[r]].GetMinion().Taunt = true;
        }
    }
    public void SummonRandomDHFight(int number, Player player)
    {
        //get dh minions
        List<string> dh_names = new List<string>();
        List<string> dh = new List<string>();
        XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion");
        for (int i = 0; i < minionNumber; i++)
        {
            //if(minionsList[i]["skill"].InnerText.Substring(0, 11) == "Deathrattle")
            if (minionsList[i]["skill"].InnerText.Contains("Deathrattle"))
            {
                dh_names.Add(minionsList[i].Attributes["name"].Value);
                Debug.Log(minionsList[i].Attributes["name"].Value);
            }
        }
        //update the list using the existing pool
        for(int i = 0; i < pool.Count; i++)
        {
            for(int j = 0; j < dh_names.Count; j++)
            {
                if(pool[i].GetName() == dh_names[j])
                {
                    dh.Add(dh_names[j]);
                }
            }
        }

        for (int n = 0; n < number; n++)
        {
            if (player.GetPlayerCopiedBoard().Count == 7)
            {
                break;
            }

            int r = Random.Range(0, dh.Count);
            string random = dh[r];
            XmlNode minionNode = minionData.GetMinionByName(random, minionDataXML);

            MinionData minionInstance = new MinionData();
            minionInstance.Initialize(minionNode, false);

            Player.Board board = new Player.Board(minionInstance, n);
            player.GetPlayerCopiedBoard().Add(board);
        }
    }

    //PASSIVES:



    //END TURN EFFECTS:

}
