using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fight : MonoBehaviour
{
    //TODO:     - skonfigurowac i sprawdzic czy dziala

    public GameObject[] fightSlotsAIBefore;
    public GameObject[] fightSlotsPlayerBefore;
    public GameObject[] fightSlotsAIAfter;
    public GameObject[] fightSlotsPlayerAfter;
    public GameController gc;
    public Text healthBeforeAI;
    public Text healthAfterAI;
    public Text healthBeforePlayer;
    public Text healthAfterPlayer;
    public Player AI;
    public Player human;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            fightSlotsAIBefore[i].GetComponent<Minion>().InitializeBlank();
            fightSlotsPlayerBefore[i].GetComponent<Minion>().InitializeBlank();
            fightSlotsAIAfter[i].GetComponent<Minion>().InitializeBlank();
            fightSlotsPlayerAfter[i].GetComponent<Minion>().InitializeBlank();
        }
        healthBeforeAI.text = AI.GetHealth().ToString();
        healthAfterAI.text = AI.GetHealth().ToString();
        healthBeforePlayer.text = human.GetHealth().ToString();
        healthAfterPlayer.text = human.GetHealth().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //choice == 0 -> before; choice == 1 -> after
    public void ShowFightBefore(int choice)
    {
        //before
        if (choice == 0)
        {
            //AI
            for (int i = 0; i < AI.GetPlayerBoard().Count; i++)
            {
                MinionData minionData = AI.GetPlayerBoard()[i].GetMinion();
                fightSlotsAIBefore[i].GetComponent<Minion>().InitializeMinion(minionData, minionData.Golden);
            }
            //human
            for (int i = 0; i < human.GetPlayerBoard().Count; i++)
            {
                MinionData minionData = human.GetPlayerBoard()[i].GetMinion();
                fightSlotsPlayerBefore[i].GetComponent<Minion>().InitializeMinion(minionData, minionData.Golden);
            }
            //update health status
            healthBeforeAI.text = AI.GetHealth().ToString();
            healthBeforePlayer.text = human.GetHealth().ToString();
        }
        //after
        else if (choice == 1)
        {
            //AI
            for (int i = 0; i < AI.GetPlayerCopiedBoard().Count; i++)
            {
                MinionData minionData = AI.GetPlayerCopiedBoard()[i].GetMinion();
                fightSlotsAIAfter[i].GetComponent<Minion>().InitializeMinion(minionData, minionData.Golden);
            }
            //human
            for (int i = 0; i < human.GetPlayerCopiedBoard().Count; i++)
            {
                MinionData minionData = human.GetPlayerCopiedBoard()[i].GetMinion();
                fightSlotsPlayerAfter[i].GetComponent<Minion>().InitializeMinion(minionData, minionData.Golden);
            }
            //update health status
            healthAfterAI.text = AI.GetHealth().ToString();
            healthAfterPlayer.text = human.GetHealth().ToString();
        }
        else
            Debug.Log("Wrong choice number!");

    }

}
