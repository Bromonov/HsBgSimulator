using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    private GameController gc;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        gc = player.gc;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetShopSlots()
    {

    }
}
