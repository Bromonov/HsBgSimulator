using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBot : MonoBehaviour
{
    public struct ShopPos
    {
        public MinionData minion;
        public int pos;

        public ShopPos(MinionData newM, int newP)
        {
            minion = newM;
            pos = newP;
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


    private Player p;
    private GameController gc;
    int turnNr = 0;
    private float timerMax = 0;
    private float timer = 0;
    public float waitTime;
    bool acted;
    List<ShopPos> minionsInTavern; 

    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<Player>();
        gc = p.gc;
        turnNr = p.turnNumber;
        acted = false;
        minionsInTavern = new List<ShopPos>();
        UpdateMinionsShopList();
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
        waitTime = gc.waitTimeBot.value;
        gc.waitTimeBotText.text = waitTime.ToString();

        turnNr = p.turnNumber;
        Debug.Log("bot jest " + gc.botTurnedOn);

        if(gc.botTurnedOn == true)
        {
            if (p.turn == true && acted == false)
            {
                if (Waited(waitTime) == false)
                    return;
                else
                    timer = 0.0f;

                acted = true;
                UpdateMinionsShopList();

                if (turnNr == 1)
                {
                    //find token
                    for (int i = 0; i < minionsInTavern.Count; i++)
                    {
                        //find free spot on hand
                        int free = 99;
                        for (int j = 0; j < gc.handSlots.Length; j++)
                        {
                            if (gc.handSlots[j].GetComponent<Minion>().blank == false)
                            {
                                free = j;
                                break;
                            }
                        }
                        /*
                        if (minionsInTavern[i].GetMinion().Name == "Alleycat" && p.GetPlayerGold() == 3)
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[i].GetPos()], gc.handSlots, gc.minionSlots);
                        else if (minionsInTavern[i].GetMinion().Name == "Murloc Tidehunter" && p.GetPlayerGold() == 3)
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[i].GetPos()], gc.handSlots, gc.minionSlots);
                        */
                        UpdateMinionsShopList();
                    }
                    if (p.GetPlayerGold() == 3)
                    {
                        //int o = FindBestUnit();
                        int o = FindRandomUnit();
                        if (o != 99)
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                        UpdateMinionsShopList();
                    }
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[0], gc.handSlots, gc.minionSlots);

                    //end turn
                    //gc.EndTurnPlayer(p);

                }
                else if (turnNr == 2)
                {
                    UpdateMinionsShopList();
                    gc.UpgradeTavernLevel(p);

                    //end turn
                    //gc.EndTurnPlayer(p);
                }
                else if (turnNr == 3)
                {
                    UpdateMinionsShopList();
                    /*
                    //if token bought
                    if ((gc.minionSlots[0].GetComponent<Minion>().GetMinion().Name == "Alleycat" &&
                        gc.minionSlots[1].GetComponent<Minion>().GetMinion().Name == "Tabbycat") ||
                            (gc.minionSlots[0].GetComponent<Minion>().GetMinion().Name == "Murloc Tidehunter" &&
                                gc.minionSlots[1].GetComponent<Minion>().GetMinion().Name == "Murloc Scout"))
                    {
                        gc.SellMinion(p, gc.minionSlots[1]);
                    }
                    else*/

                    gc.SellMinion(p, gc.minionSlots[0]);

                    //find best unit
                    //int o1 = FindBestUnit();
                    int o1 = FindRandomUnit();
                    if (o1 != 99)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                    //find first free spot on board
                    int free = 99;
                    free = FindFreeSpotOnBoard();

                    if (free != 99)
                        gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                    UpdateMinionsShopList();
                    //find second best unit
                    //o1 = FindBestUnit();
                    o1 = FindRandomUnit();
                    if (o1 != 99)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                    //find first free spot on board
                    free = FindFreeSpotOnBoard();


                    if (free != 99)
                        gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                    //end turn
                    //gc.EndTurnPlayer(p);
                }
                else if (turnNr == 4)
                {
                    UpdateMinionsShopList();
                    //find best unit
                    //int o1 = FindBestUnit();
                    int o1 = FindRandomUnit();
                    if (o1 != 99)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                    //find first free spot on board
                    int free = 99;
                    free = FindFreeSpotOnBoard();

                    if (free != 99)
                        gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                    UpdateMinionsShopList();
                    //find second best unit
                    //o1 = FindBestUnit();
                    o1 = FindRandomUnit();
                    if (o1 != 99)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                    free = FindFreeSpotOnBoard();

                    if (free != 99)
                        gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                    //end turn
                    //gc.EndTurnPlayer(p);
                }
                else if (turnNr == 5)
                {
                    gc.UpgradeTavernLevel(p);

                    int free = 99;
                    free = FindFreeSpotOnBoard();

                    UpdateMinionsShopList();
                    //int o = FindBestUnit();
                    int o = FindRandomUnit();
                    if (o != 99)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);

                    if (free != 99)
                    {
                        gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);
                    }

                    //end turn
                    //gc.EndTurnPlayer(p);
                }
                else if (turnNr == 6)
                {
                    //5 minions on board
                    if (gc.minionSlots[5].GetComponent<Minion>().blank == true)
                    {
                        UpdateMinionsShopList();
                        //int o = FindBestUnit();
                        int o = FindRandomUnit();
                        if (o != 99)
                        {
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                            gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[5], gc.handSlots, gc.minionSlots);
                        }
                        UpdateMinionsShopList();
                        //o = FindBestUnit();
                        o = FindRandomUnit();
                        if (o != 99)
                        {
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                            gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[6], gc.handSlots, gc.minionSlots);
                        }
                    }
                    //6 minions on board
                    else if (gc.minionSlots[6].GetComponent<Minion>().blank == true)
                    {
                        UpdateMinionsShopList();
                        //int o = FindBestUnit();
                        int o = FindRandomUnit();
                        if (o != 99)
                        {
                            gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                            gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[6], gc.handSlots, gc.minionSlots);
                        }
                    }
                    //end turn
                    //gc.EndTurnPlayer(p);
                }
                else if (turnNr > 6)
                {
                    UpdateMinionsShopList();
                    gc.UpgradeTavernLevel(p);
                    if (p.GetPlayerGold() >= 1)
                    {
                        gc.RefreshMinionsInTavern(p, gc.shopSlots);
                    }

                    //end turn
                    //gc.EndTurnPlayer(p);
                }

            }
            //jakies pcozekanie na koniec tury by sie przydalo, jakos osobno rozpatrywanie end turn
            if (acted == true && Waited(waitTime) == true)
            {
                acted = false;
                gc.EndTurnPlayer(p);
            }
            else
                return;

            //timer = 0.0f;
        }


    }

    public int FindBestUnit()
    {
        UpdateMinionsShopList();
        /*
        //remove triple option(for now)
        for(int i = 0; i < minionsInTavern.Count; i++)
        {
            for(int j = 0; j < p.GetPlayerBoard().Count; j++)
            {
                if(minionsInTavern[i].GetMinion().Name == p.GetPlayerBoard()[j].GetMinion().Name)
                {
                    minionsInTavern.RemoveAt(i);
                }
            }
        }
        */
        int max = 0;
        int pos = 99;
        for(int i = 0; i < minionsInTavern.Count; i++)
        {
            int stat = minionsInTavern[i].GetMinion().Attack + minionsInTavern[i].GetMinion().Hp;
            if(stat>max)
            {
                max = stat;
                pos = i;
            }
        }

        return pos;
    }

    public int FindRandomUnit()
    {
        UpdateMinionsShopList();
        int r = 99;
        r = Random.Range(0, minionsInTavern.Count);
        return r;
    }

    public int FindFreeSpotOnBoard()
    {
        //UpdateMinionsShopList();
        //find first free spot on board
        int free = 99;
        for (int i = 0; i < gc.minionSlots.Length; i++)
        {
            if (gc.minionSlots[i].GetComponent<Minion>().blank == true)
            {
                free = i;
                break;
            }
        }

        return free;
    }

    public void UpdateMinionsShopList()
    {
        minionsInTavern.Clear();
        for (int i = 0; i < gc.shopSlots.Length; i++)
        {
            if (gc.shopSlots[i].GetComponent<Minion>().blank == false)
            {
                ShopPos s = new ShopPos(gc.shopSlots[i].GetComponent<Minion>().GetMinion(), i);
                minionsInTavern.Add(s);
            }
        }
    }
}
