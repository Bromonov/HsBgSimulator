using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    //jakies bazgroly, rozkminy tak o, o stanach, akcjach i rewardach:
        /*
        akcje(zalezne od posiadanego golda) :
        - kupic miniona(1 lub 2 lub 3 lub 4 lub 5 lub 6) -> wybor dostepny od 3 do 6
        - sprzedac miniona(analogicznie, z tego co gracz posiada na boardzie, od 1 do 7)
        - zagrac miniona na board
        - roll
        - level tawerny
        - freeze(?)
        - swapowanie minionow na boardzie -> ciezko o natychmiastowy reward
        - end turn -> chyba nie samemu, moze w momencie jak sie wykrozysta caly gold?

        -> kiedy konczymy wykonywane akcje? w momencie jak gold zjedzie do 0? jak nie bedzie minionow na lapie(wszystkie juz na boardzie)? jak zostanie wykonana jakas zamiana? jak sprawdzic czy swap na boardzie byl dobry?

        stan:
        - suma att i hp minionow??? -> poisonous traktowany jako giga liczba att???, jak opisac ds?
        - hp chyba nie, moze spadek hp -> im mniejszy spadek lub brak spadku tym lepiej ?

        rewardy:
        - nie stracenie hp -> im wieksza strata tym gorzej?
        - moze zagranie na board jednostki?
        - efektywne zarzadzanie goldem -> tu nie wiadomo jak, bo teoretycznie samo zjechanie do 0 chuja daje, mozna przeciez 10x zakrecic smiglem, moze jak najmniejsza liczba akcji zmniejszajacych golda w procesie wydawania go -> kupno 3 jednostek i 1 roll(4 akcje) za 10 golda jest teoretycznie lepsze od zrobienia 7 rollow i kupna jednej jednostki(8 akcji)
        - triple??? bo jednostki z wyzszego tieru sa lepsze od nizszego(in general)
        - w jakis sposob improvement wymieniajac jednostke 1 tieru na 2/3/etc tieru -> sprzedaz? ale sprzedaz i instant zamiana, tylko w takim przypadku???

        */


    //POSSIBLE ACTIONS:
    //->    show minions in tavern at start of a turn, not using player gold:
    //gc.ShowMinionsInTavern(player, gc.shopSlotsAI);
    //->    buy a chosen minion, choose from shopSlotsAI[]
    //gc.BuyMinionAI(player, minion);
    //->    sell a chosen minion from boardSlotsAI[]
    //gc.SellMinionAI(player, minion);
    //->    roll, use player gold(-1)
    //gc.RefreshMinionsInTavernAI(player); //rolka
    //->    upgrade tavern level with player gold
    //gc.UpgradeTavernLevel(player);
    //->    end turn, probably not a choice, but do at the end of other possible actions
    //gc.EndTurnAI(minionSlotsAI);
    //->    play a chosen minion from handSlotsAI[] to a chosen place at minionSlotsAI[]
    //gc.PlayMinionOnBoard(player, handSlot, minionSlot, gc.handSlotsAI, gc.minionSlotsAI);
    //->    swaps, not working at all tbh
    //gc.SwapNonBlankMinions(minionA, minionB);
    //gc.SwapMinionWithBlank(minion, blank);
    //pick a minion from a discover list, kinda similar to buying, but free
    //gc.ChooseDiscoveredMinionAI(player, minion);

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
