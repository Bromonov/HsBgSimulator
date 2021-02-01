using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMinion : MonoBehaviour
{
    public GameObject arrow;
    public GameController gc;
    public bool triple;

    // Start is called before the first frame update
    void Start()
    {
        arrow.gameObject.SetActive(false);
        triple = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (triple == true)
        {
            arrow.gameObject.SetActive(true);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }
}
