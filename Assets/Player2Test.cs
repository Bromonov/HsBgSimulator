using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class Player2Test : MonoBehaviour
{
    public Player player;
    private List<Player.Board> board;
    private XmlDocument minionDataXML;

    public MinionData minionData;

    // Start is called before the first frame update
    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("test_minion");
        minionDataXML = new XmlDocument();
        minionDataXML.LoadXml(textAsset.text);

        board = player.GetPlayerBoard();
        //board = new List<Player.Board>();
        //XmlNode minionNode1 = minionData.GetMinionByName("Alleycat", minionDataXML);
        MinionData minionData1 = new MinionData();
        //minionData1.Initialize("Alleycat", 1, 1, "Beast", 1, false, false, false);
        Player.Board minion1 = new Player.Board(minionData1, 0);
        board.Add(minion1);

        Debug.Log(board[0].GetMinion().Name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
