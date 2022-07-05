using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "TraitName", menuName = "Scriptable Object Asset/TraitData")]
public class Trait : ScriptableObject
{
    public string tdName;
    public Sprite img;
    public UnityEvent<Player> myEvent;
    public int[] code;
    [TextArea(10, 20)]
    public string info;
    public void getAddDLD(Player p)
    {
        p.TaritDmgDefLife[code[0]] += code[1];
    }
    public void getSpecState(Player p)
    {
        p.TaritSpecState[code[0]] += code[1];
    }
    public void changeMainState(Player p)
    {
        p.mainState = code[0];
    }
    public void addStates(Player p)
    {
        p.myState[code[0]] += code[1];
    }
    public void getKeyWhenStart(Player p)
    {
        p.Mapm.StartCoroutine("getKeyForMonster", p.transform.position + Vector3.up);
    }
    public void getBoxWhenStart(Player p)
    {
        p.Mapm.makeItemBoxInMap(code[0]);
    }
    public void getItemWhenStart(Player p)
    {
        p.Mapm.playerGetRandomItem(0, 30);
    }
}
