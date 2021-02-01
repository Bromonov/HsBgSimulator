using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Minion : MonoBehaviour
{
    public Text att;
    public Text hp;
    public Text tribe;
    public Text minionName;
    public Text tavernTier;
    public GameObject poison;
    public Color ds;
    public GameObject taunt;
    public Text skill;

    XmlDocument minionDataXML;
    public XmlNode curNode;
    //public Button buyButton;
    public bool blank;
    public bool golden;
    private MinionData minion;
    //public Vector2 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);
        //XmlNode curNode = minionDataXML.SelectSingleNode("minions/minion");
        //InitializeMinion(curNode);
        //buyButton.gameObject.SetActive(false);
        blank = true;
        golden = false;
        minion = this.gameObject.AddComponent<MinionData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeMinion(XmlNode curNode)
    {
        Debug.Log("Initializing minion on " + this.gameObject.name);
        SetActiveAllChildren(this.gameObject.transform, true);
        //MinionData alleycat = new MinionData(curNode);
        //GameObject gameObject = new GameObject();
        //MinionData minion = this.gameObject.AddComponent<MinionData>();
        minion.Initialize(curNode, false);
        att.text = "ATT: " + minion.Attack.ToString();
        hp.text = "HP: " + minion.Hp.ToString();
        tribe.text = minion.Tribe;
        minionName.text = minion.Name;
        tavernTier.text = minion.TavernTier.ToString();
        skill.text = minion.Skill;

        if (minion.DivineShield == true)
            this.gameObject.GetComponent<Image>().color = ds;
        else this.gameObject.GetComponent<Image>().color = Color.white;

        if (minion.Poison == true)
            poison.SetActive(true);
        else poison.SetActive(false);

        if (minion.Taunt == true)
            taunt.SetActive(true);
        else taunt.SetActive(false);

        blank = false;
        golden = false;
    }

    public void InitializeMinion(MinionData curNode, bool newGolden)
    {
        Debug.Log("Initializing minion on " + this.gameObject.name);
        SetActiveAllChildren(this.gameObject.transform, true);
        //MinionData alleycat = new MinionData(curNode);
        //GameObject gameObject = new GameObject();
        //MinionData minion = this.gameObject.AddComponent<MinionData>();
        minion.Initialize(curNode);
        att.text = "ATT: " + minion.Attack.ToString();
        hp.text = "HP: " + minion.Hp.ToString();
        tribe.text = minion.Tribe;
        minionName.text = minion.Name;
        tavernTier.text = minion.TavernTier.ToString();
        if (newGolden == false)
            skill.text = minion.Skill;
        else if (newGolden == true)
            skill.text = minion.GoldenSkill;

        if (minion.DivineShield == true)
            this.gameObject.GetComponent<Image>().color = ds;
        else this.gameObject.GetComponent<Image>().color = Color.white;

        if (minion.Poison == true)
            poison.SetActive(true);
        else poison.SetActive(false);

        if (minion.Taunt == true)
            taunt.SetActive(true);
        else taunt.SetActive(false);

        blank = false;
        golden = newGolden;
    }

    public void InitializeBlank()
    {
        /*
        att.gameObject.SetActive(false);
        hp.gameObject.SetActive(false);
        tribe.gameObject.SetActive(false);
        minionName.gameObject.SetActive(false);
        tavernTier.gameObject.SetActive(false);
        poison.SetActive(false);
        taunt.SetActive(false);
        this.gameObject.GetComponent<Image>().color = Color.white;
        */
        //minionName.text = "BLANK";
        SetActiveAllChildren(this.gameObject.transform, false);
        blank = true;
        golden = false;
        /*
        if(this.gameObject.GetComponent<BoardMinion>() != null)
        {
            Debug.Log("Blanking /w handminion " + this.gameObject.name);
        }
        else
            Debug.Log("Blanking " + this.gameObject.name);
        */
    }

    public void SetActiveAllChildren(Transform transform, bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);

            SetActiveAllChildren(child, value);
        }
        //buyButton.gameObject.SetActive(false);
    }
    /*
    public void ShowAndHideBuyButton()
    {
        if (buyButton.gameObject.activeSelf == false)
            buyButton.gameObject.SetActive(true);
        else buyButton.gameObject.SetActive(false);
    }
    */
    public MinionData GetMinion()
    {
        return minion;
    }
}
