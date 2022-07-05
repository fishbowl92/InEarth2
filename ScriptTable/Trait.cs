using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "TraitName", menuName = "Scriptable Object Asset/TraitData")]
public class Trait : ScriptableObject
{
    public string tdName;   // 맵타일이름
    public Sprite img;  // 맵타일 이미지
    public UnityEvent<Player> myEvent;  // 플레이어 이벤트 체크
    public int[] code;  // 맵타일 코드
    [TextArea(10, 20)]
    public string info; // 맵타일 설명
    public void getAddDLD(Player p) // 맵타일에 가해지는 dmgDef 반영
    {
        p.TaritDmgDefLife[code[0]] += code[1];
    }
    public void getSpecState(Player p)  // 맵타일 특수상태 반영
    {
        p.TaritSpecState[code[0]] += code[1];
    }
    public void changeMainState(Player p)   // 맵타일 근본 자체 수정
    {
        p.mainState = code[0];
    }
    public void addStates(Player p) // 맵타일 상태 변화
    {
        p.myState[code[0]] += code[1];
    }
    public void getKeyWhenStart(Player p)   // 게임 시작시 열쇠 존재하는경우
    {
        p.Mapm.StartCoroutine("getKeyForMonster", p.transform.position + Vector3.up);
    }
    public void getBoxWhenStart(Player p)   // 게임 시작시 아이템 박스 존재하는경우
    {
        p.Mapm.makeItemBoxInMap(code[0]);
    }
    public void getItemWhenStart(Player p)  // 게임 시작시 아이템 존재하는경우
    {
        p.Mapm.playerGetRandomItem(0, 30);
    }
} 
