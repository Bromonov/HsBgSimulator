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
    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<Player>();
        gc = p.gc;
        turnNr = p.turnNumber;
    }

    // Update is called once per frame
    void Update()
    {
        turnNr = p.turnNumber;
        if (p.turn == true)
        {
            List<ShopPos> minionsInTavern = new List<ShopPos>();
            for (int i = 0; i < gc.shopSlots.Length; i++)
            {
                if (gc.shopSlots[i].GetComponent<Minion>().blank == false)
                {
                    ShopPos s = new ShopPos(gc.shopSlots[i].GetComponent<Minion>().GetMinion(), i);
                    minionsInTavern.Add(s);
                }
            }

            if (turnNr == 1)
            {
                //find token
                for(int i = 0; i < minionsInTavern.Count; i++)
                {
                    //find free spot on hand
                    int free = 99;
                    for(int j = 0; j < gc.handSlots.Length; j++)
                    {
                        if(gc.handSlots[j].GetComponent<Minion>().blank == false)
                        {
                            free = j;
                            break;
                        }    
                    }

                    if (minionsInTavern[i].GetMinion().Name == "Alleycat" && p.GetPlayerGold() == 3)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[i].GetPos()], gc.handSlots, gc.minionSlots);
                    else if(minionsInTavern[i].GetMinion().Name == "Murloc Tidehunter" && p.GetPlayerGold() == 3)
                        gc.BuyMinion(p, gc.shopSlots[minionsInTavern[i].GetPos()], gc.handSlots, gc.minionSlots);
                }
                if(p.GetPlayerGold() == 3)
                {
                    int o = FindBestUnit();

                    gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                }
                gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[0], gc.handSlots, gc.minionSlots);

                //end turn
                gc.EndTurnPlayer(p);

            }
            else if(turnNr == 2)
            {
                gc.UpgradeTavernLevel(p);

                //end turn
                gc.EndTurnPlayer(p);
            }
            else if(turnNr == 3)
            {
                //if token bought
                if ((gc.minionSlots[0].GetComponent<Minion>().GetMinion().Name == "Alleycat" &&
                    gc.minionSlots[1].GetComponent<Minion>().GetMinion().Name == "Tabbycat") ||
                        (gc.minionSlots[0].GetComponent<Minion>().GetMinion().Name == "Murloc Tidehunter" &&
                            gc.minionSlots[1].GetComponent<Minion>().GetMinion().Name == "Murloc Scout"))
                {
                    gc.SellMinion(p, gc.minionSlots[1]);
                }
                else
                    gc.SellMinion(p, gc.minionSlots[0]);

                //find best unit
                int o1 = FindBestUnit();
                gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                //find first free spot on board
                int free = 99;
                free = FindFreeSpotOnBoard();

                if (free != 99)
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                //find second best unit
                o1 = FindBestUnit();
                gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                //find first free spot on board
                free = FindFreeSpotOnBoard();


                if (free != 99)
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                //end turn
                gc.EndTurnPlayer(p);
            }
            else if(turnNr == 4)
            {
                //find best unit
                int o1 = FindBestUnit();
                gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                //find first free spot on board
                int free = 99;
                free = FindFreeSpotOnBoard();

                if (free != 99)
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                //find second best unit
                o1 = FindBestUnit();
                gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o1].GetPos()], gc.handSlots, gc.minionSlots);

                free = FindFreeSpotOnBoard();

                if (free != 99)
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);

                //end turn
                gc.EndTurnPlayer(p);
            }
            else if (turnNr == 5)
            {
                gc.UpgradeTavernLevel(p);

                int free = 99;
                free = FindFreeSpotOnBoard();

                int o = FindBestUnit();
                gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);

                if(free != 99)
                {
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[free], gc.handSlots, gc.minionSlots);
                }

                //end turn
                gc.EndTurnPlayer(p);
            }
            else if (turnNr == 6)
            {
                //5 minions on board
                if (gc.minionSlots[5].GetComponent<Minion>().blank == true)
                {
                    int o = FindBestUnit();
                    gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[5], gc.handSlots, gc.minionSlots);

                    o = FindBestUnit();
                    gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[6], gc.handSlots, gc.minionSlots);
                }
                //6 minions on board
                else if (gc.minionSlots[6].GetComponent<Minion>().blank == true)
                {
                    int o = FindBestUnit();
                    gc.BuyMinion(p, gc.shopSlots[minionsInTavern[o].GetPos()], gc.handSlots, gc.minionSlots);
                    gc.PlayMinionOnBoard(p, gc.handSlots[0], gc.minionSlots[6], gc.handSlots, gc.minionSlots);
                }

                //end turn
                gc.EndTurnPlayer(p);
            }
            else if(turnNr > 6)
            {
                gc.UpgradeTavernLevel(p);
                if(p.GetPlayerGold() >= 1)
                {
                    gc.RefreshMinionsInTavern(p, gc.shopSlots);
                }

                //end turn
                gc.EndTurnPlayer(p);
            }
        }
    }

    public int FindBestUnit()
    {
        List<ShopPos> minionsInTavern = new List<ShopPos>();
        for (int i = 0; i < gc.shopSlots.Length; i++)
        {
            if (gc.shopSlots[i].GetComponent<Minion>().blank == false)
            {
                ShopPos s = new ShopPos(gc.shopSlots[i].GetComponent<Minion>().GetMinion(), i);
                minionsInTavern.Add(s);
            }
        }

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

    public int FindFreeSpotOnBoard()
    {
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
}
