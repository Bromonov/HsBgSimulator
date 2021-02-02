using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class MinionData : MonoBehaviour
{
    public int numberOfMinionsTier1 = 17;
    public int numberOfMinionsTier2 = 23;
    public int numberOfMinionsTier3 = 26;
    public int numberOfMinionsTier4 = 23;
    public int numberOfMinionsTier5 = 22;
    public int numberOfMinionsTier6 = 16;

    public string Name { get; set; }
    public int Attack { get; set; }
    public int Hp { get; set; }
    public string Tribe { get; set; }
    public int TavernTier { get; set; }
    public bool DivineShield { get; set; }
    public bool Poison { get; set; }
    public bool Taunt { get; set; }
    public string Skill { get; set; }
    public bool Golden { get; set; }
    public string GoldenSkill { get; set; }
    //public costam Battlecry { get; set; }     !!! DO PRZEMYSLENIA !!!
    //public costam Deathrattle { get; set; }   !!! DO PRZEMYSLENIA !!!

    public void Initialize(XmlNode curMinionNode, bool golden)
    {
        Name = curMinionNode.Attributes["name"].Value;
        Attack = int.Parse(curMinionNode["attack"].InnerText);
        Hp = int.Parse(curMinionNode["hp"].InnerText);
        Tribe = curMinionNode["tribe"].InnerText;
        TavernTier = int.Parse(curMinionNode.Attributes["tavernTier"].Value);

        string tempDS = curMinionNode["ds"].InnerText;
        if (tempDS == "true")
            DivineShield = true;
        else if (tempDS == "false")
            DivineShield = false;
        else Debug.Log("DivineShield boolean parsing problem!");

        string tempPoison = curMinionNode["poison"].InnerText;
        if (tempPoison == "true")
            Poison = true;
        else if (tempPoison == "false")
            Poison = false;
        else Debug.Log("Poison boolean parsing problem!");

        string tempTaunt = curMinionNode["taunt"].InnerText;
        if (tempTaunt == "true")
            Taunt = true;
        else if (tempTaunt == "false")
            Taunt = false;
        else Debug.Log("Taunt boolean parsing problem!");

        Skill = curMinionNode["skill"].InnerText;
        GoldenSkill = curMinionNode["goldenSkill"].InnerText;

        Golden = golden;
    }

    
    public void Initialize(MinionData curMinionNode)
    {
        Name = curMinionNode.Name;
        Attack = curMinionNode.Attack;
        Hp = curMinionNode.Hp;
        Tribe = curMinionNode.Tribe;
        TavernTier = curMinionNode.TavernTier;
        DivineShield = curMinionNode.DivineShield;
        Poison = curMinionNode.Poison;
        Taunt = curMinionNode.Taunt;
        Skill = curMinionNode.Skill;
        GoldenSkill = curMinionNode.GoldenSkill;
        Golden = curMinionNode.Golden;
    }
    
    public void Initialize(string newName, int newAttack, int newHP, string newTribe, int newTavernTier, bool newDS, bool newPoison, 
         bool newTaunt, string skill, string goldenSkill, bool newGolden)
    {
        Name = newName;
        Attack = newAttack;
        Hp = newHP;
        Tribe = newTribe;
        TavernTier = newTavernTier;
        DivineShield = newDS;
        Poison = newPoison;
        Taunt = newTaunt;
        Skill = skill;
        GoldenSkill = goldenSkill;
        Golden = newGolden;
    }
    /*
    public void InitializeBlank()
    {
        Name = "NULL";
        Attack = 0;
        Hp = 0;
        Tribe = "NULL";
        TavernTier = 0;
        DivineShield = false;
        Poison = false;
        Taunt = false;
    }
    */
    public XmlNode[] FindMinionsOfTier(int tavernTier, XmlDocument minionDataXML)//, MinionData[] minions)
    {
        int numberOfMinions = 0;

        if (tavernTier == 1)
            numberOfMinions = numberOfMinionsTier1;
        else if (tavernTier == 2)
            numberOfMinions = numberOfMinionsTier2;
        else if (tavernTier == 3)
            numberOfMinions = numberOfMinionsTier3;
        else if (tavernTier == 4)
            numberOfMinions = numberOfMinionsTier4;
        else if (tavernTier == 5)
            numberOfMinions = numberOfMinionsTier5;
        else if (tavernTier == 6)
            numberOfMinions = numberOfMinionsTier6;
        else
            Debug.Log("TavernTier Error!");

        //MinionData[] minions = new MinionData[numberOfMinions];
        XmlNode[] xmlNodes = new XmlNode[numberOfMinions];
        XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + tavernTier + "']");

        for (int i = 0; i < numberOfMinions; i++)
        {
            //MinionData minion = new MinionData();
            //minion.Initialize(minionsList[i]);
            //minions[i] = minion;
            xmlNodes[i] = minionsList[i];
        }

        //return minions;
        return xmlNodes;
    }

    public XmlNode[] FindMinionsOfMaxTiers(int tavernTier, XmlDocument minionDataXML)//, MinionData[] minions)
    {
        int numberOfMinions = 0;

        XmlNode[] xmlNodes = new XmlNode[numberOfMinions];
        //MinionData[] minions = new MinionData[numberOfMinions];
        if (tavernTier == 1)
        {
            XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 1 + "']");
            numberOfMinions = numberOfMinionsTier1;
            for (int i = 0; i < numberOfMinions; i++)
            {
                //MinionData minion = new MinionData();
                //minion.Initialize(minionsList[i]);
                //minions[i] = minion;
                xmlNodes[i] = minionsList[i];
            }
        }

        else if (tavernTier == 2)
        {
            XmlNodeList minionsList1 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 1 + "']");
            XmlNodeList minionsList2 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 2 + "']");
            numberOfMinions = numberOfMinionsTier1 + numberOfMinionsTier2;
            for (int i = 1; i <= numberOfMinions; i++)
            {
                MinionData minion = new MinionData();
                if (i <= numberOfMinionsTier1)
                {
                    //minion.Initialize(minionsList1[i]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList1[i];
                }
                else
                {
                    //minion.Initialize(minionsList2[i - numberOfMinionsTier1]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList2[i - numberOfMinionsTier1];
                }
            }
        }

        else if (tavernTier == 3)
        {
            XmlNodeList minionsList1 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 1 + "']");
            XmlNodeList minionsList2 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 2 + "']");
            XmlNodeList minionsList3 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 3 + "']");
            numberOfMinions = numberOfMinionsTier1 + numberOfMinionsTier2 + numberOfMinionsTier3;
            for (int i = 1; i <= numberOfMinions; i++)
            {
                MinionData minion = new MinionData();
                if (i <= numberOfMinionsTier1)
                {
                    //minion.Initialize(minionsList1[i]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList1[i];
                }
                else if (i > numberOfMinionsTier1 && i <= numberOfMinionsTier2)
                {
                    //minion.Initialize(minionsList2[i - numberOfMinionsTier1]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList2[i - numberOfMinionsTier1];
                }
                else
                {
                    //minion.Initialize(minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2];
                }
            }
        }

        else if (tavernTier == 4)
        {
            XmlNodeList minionsList1 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 1 + "']");
            XmlNodeList minionsList2 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 2 + "']");
            XmlNodeList minionsList3 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 3 + "']");
            XmlNodeList minionsList4 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 4 + "']");
            numberOfMinions = numberOfMinionsTier1 + numberOfMinionsTier2 + numberOfMinionsTier3 + numberOfMinionsTier4;
            for (int i = 1; i <= numberOfMinions; i++)
            {
                MinionData minion = new MinionData();
                if (i <= numberOfMinionsTier1)
                {
                    //minion.Initialize(minionsList1[i]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList1[i];
                }
                else if (i > numberOfMinionsTier1 && i <= numberOfMinionsTier2)
                {
                    //minion.Initialize(minionsList2[i - numberOfMinionsTier1]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList2[i - numberOfMinionsTier1];
                }
                else if (i > numberOfMinionsTier2 && i <= numberOfMinionsTier3)
                {
                    //minion.Initialize(minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2];
                }
                else
                {
                    //minion.Initialize(minionsList4[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList4[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3];
                }
            }
        }

        else if (tavernTier == 5)
        {
            XmlNodeList minionsList1 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 1 + "']");
            XmlNodeList minionsList2 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 2 + "']");
            XmlNodeList minionsList3 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 3 + "']");
            XmlNodeList minionsList4 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 4 + "']");
            XmlNodeList minionsList5 = minionDataXML.SelectNodes("/minions/minion[@tavernTier='" + 5 + "']");
            numberOfMinions = numberOfMinionsTier1 + numberOfMinionsTier2 + numberOfMinionsTier3 + numberOfMinionsTier4 + numberOfMinionsTier5;
            for (int i = 1; i <= numberOfMinions; i++)
            {
                MinionData minion = new MinionData();
                if (i <= numberOfMinionsTier1)
                {
                    //minion.Initialize(minionsList1[i]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList1[i];
                }
                else if (i > numberOfMinionsTier1 && i <= numberOfMinionsTier2)
                {
                    //minion.Initialize(minionsList2[i - numberOfMinionsTier1]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList2[i - numberOfMinionsTier1];
                }
                else if (i > numberOfMinionsTier2 && i <= numberOfMinionsTier3)
                {
                    //minion.Initialize(minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList3[i - numberOfMinionsTier1 - numberOfMinionsTier2];
                }
                else if (i > numberOfMinionsTier3 && i <= numberOfMinionsTier4)
                {
                    //minion.Initialize(minionsList4[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList4[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3];
                }
                else
                {
                    //minion.Initialize(minionsList5[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3 - numberOfMinionsTier4]);
                    //minions[i] = minion;
                    xmlNodes[i] = minionsList5[i - numberOfMinionsTier1 - numberOfMinionsTier2 - numberOfMinionsTier3 - numberOfMinionsTier4];
                }
            }
        }

        else if (tavernTier == 6)
        {
            XmlNodeList minionsList = minionDataXML.SelectNodes("/minions/minion");
            numberOfMinions = numberOfMinionsTier1 + numberOfMinionsTier2 + numberOfMinionsTier3 + numberOfMinionsTier4 + numberOfMinionsTier5 + numberOfMinionsTier6;
            for (int i = 0; i < numberOfMinions; i++)
            {
                //MinionData minion = new MinionData();
                //minion.Initialize(minionsList[i]);
                //minions[i] = minion;
                xmlNodes[i] = minionsList[i];
            }
        }
        else
            Debug.Log("TavernTier Error!");

        //return minions;
        return xmlNodes;
    }

    public XmlNode GetMinionByName(string name, XmlDocument minionDataXML)
    {
        XmlNode minionSingle = minionDataXML.SelectSingleNode("/minions/minion[@name='" + name + "']");
        //MinionData minion = new MinionData();
        //minion.Initialize(minionSingle);
        //return minion;

        return minionSingle;
    }
}
