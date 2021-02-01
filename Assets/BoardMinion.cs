using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Xml;
using System.Xml.Serialization;

public class BoardMinion : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private XmlDocument minionDataXML;
    public GameController gc;
    private MinionData minionData;
    private GameObject[] minionSlots;
    private Vector2 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);
        minionData = gc.minionData;
        minionSlots = gc.minionSlots;
        initialPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region IDragHandler implementation
    public void OnDrag(PointerEventData eventData)
    {
        if (GetComponent<Minion>().blank == false)
            this.transform.position += (Vector3)eventData.delta;
        else
            Debug.Log("You cannot move blank!");
    }

    #endregion

    #region IEndDragHandler implementation
    public void OnEndDrag(PointerEventData eventData)
    {       
        if (Mathf.Abs(this.transform.position.x - minionSlots[0].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[0].transform.position.y) <= 15.0f))
        {
            //transform.position = new Vector2(minionSlots[i].transform.position.x, minionSlots[i].transform.position.y);
            //this.transform.position = (Vector3)eventData.delta;
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA0");
            //if (minionSlots[0].GetComponent<Minion>().blank == false)
            if (minionSlots[0].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[0]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[0]);
        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[1].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[1].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA1");
            if (minionSlots[1].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[1]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[1]);
            ;
        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[2].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[2].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA2");
            if (minionSlots[2].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[2]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[2]);

        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[3].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[3].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA3");
            if (minionSlots[3].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[3]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[3]);

        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[4].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[4].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA4");
            if (minionSlots[4].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[4]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[4]);

        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[5].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[5].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA5");
            if (minionSlots[5].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[5]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[5]);

        }
        else if (Mathf.Abs(this.transform.position.x - minionSlots[6].transform.position.x) <= 10.0f &&
            (Mathf.Abs(this.transform.position.y - minionSlots[6].transform.position.y) <= 15.0f))
        {
            transform.position = new Vector2(initialPosition.x, initialPosition.y);
            Debug.Log("ZAMIANA6");
            if (minionSlots[6].GetComponent<Minion>().blank == false)
                gc.SwapNonBlankMinions(this.gameObject, minionSlots[6]);
            else
                gc.SwapMinionWithBlank(this.gameObject, minionSlots[6]);

        }
        else
        {
            Debug.Log("ZAMIANA BNIE WYSZLA!");
            transform.position = new Vector2(initialPosition.x, initialPosition.y);

        }
        
    }
    
        
    #endregion
}
