using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandMinion : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public GameController gc;
    private float deltaX, deltaY; //offset for drag'n'drop minions in board
    public bool locked; //true->minion in board in place
    private Vector2 mousePosition;
    private GameObject[] minionSlots;
    private Vector2 initialPosition;
    //private Button OnClickButton;
    public bool placed;
    public int placedSlot;

    // Start is called before the first frame update
    void Start()
    {
        minionSlots = gc.minionSlots;
        initialPosition = this.transform.position;
        //OnClickButton = this.gameObject.transform.Find("OnClickButton").gameObject.GetComponent<Button>();
        //OnClickButton.gameObject.SetActive(false);
        placed = false;
        placedSlot = 99;
    }

    // Update is called once per frame
    void Update()
    {
        //OnClickButton.gameObject.SetActive(false);
    }

    #region IDragHandler implementation
    public void OnDrag(PointerEventData eventData)
    {
        if (GetComponent<Minion>().blank == false && placed == false)
        {
            this.transform.position += (Vector3)eventData.delta;
        } 
        else
            Debug.Log("You cannot move blank!");
    }

    #endregion

    #region IEndDragHandler implementation
    public void OnEndDrag(PointerEventData eventData)
    {
        if(GetComponent<Minion>().blank == false && placed == false)
        {
            if (minionSlots[0].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[0].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[0].transform.position.y) <= 15.0f))
            {
                //transform.position = new Vector2(minionSlots[i].transform.position.x, minionSlots[i].transform.position.y);
                //this.transform.position = (Vector3)eventData.delta;
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO!0");
                placed = true;
                placedSlot = 0;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[0], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[1].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[1].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[1].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO1!");
                placed = true;
                placedSlot = 1;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[1], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[2].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[2].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[2].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO2!");
                placed = true;
                placedSlot = 2;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[2], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[3].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[3].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[3].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO3!");
                placed = true;
                placedSlot = 3;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[3], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[4].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[4].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[4].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO4!");
                placed = true;
                placedSlot = 4;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[4], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[5].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[5].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[5].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO5!");
                placed = true;
                placedSlot = 5;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[5], gc.handSlots, gc.minionSlots);
            }
            else if (minionSlots[6].GetComponent<Minion>().blank == true && Mathf.Abs(this.transform.position.x - minionSlots[6].transform.position.x) <= 10.0f &&
                (Mathf.Abs(this.transform.position.y - minionSlots[6].transform.position.y) <= 15.0f))
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                Debug.Log("NO HEJ, UDALO CI SIE MORDO6!");
                placed = true;
                placedSlot = 6;
                gc.PlayMinionOnBoard(gc.player1, this.gameObject, minionSlots[6], gc.handSlots, gc.minionSlots);
            }
            else
            {
                transform.position = new Vector2(initialPosition.x, initialPosition.y);
                placed = false;
                placedSlot = 99;
            }
            
        }

        
    }

    #endregion

}
