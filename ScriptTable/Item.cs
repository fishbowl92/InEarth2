using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kind = Skill.AtkKind;
using UnityEngine.Events;
using TriggerData = Skill.TriggerData;
using TMPro;

[CreateAssetMenu(fileName = "ItemName", menuName = "Scriptable Object Asset/ItemData")]
public class Item : ScriptableObject
{
    public kind[] myTrigger;    // 아이템 발동 트리거
    public UnityEvent<TriggerData>[] itemEvent; // 아이템 트리거 발동시 실행되는 이벤트
    public int[] code;  // 아이템 코드
    public float[] eventGab;    // 발생 이벤트 수치
    public Sprite Img;  // 아이템 이미지
    public float dmg;   // 아이템 베이스 데미지
    public float def;   // 아이템 베이스 방어력
    public float hp;    // 아이템 베이스 체력
    public int[] getState = new int[3]; // 아이템이 올려주는 힘, 민첩, 지능
    [TextArea(10, 20)]
    public string infoText; // 아이템 설명 텍스트
    public int getCount;
    public int tear;        // 아이템 희귀도
    public int getItemDmg(int lev)  // 해당 아이템 사용시 데미지 계산에 사용될 아이템 데미지 수치
    {
        return (int)(dmg * (1 + lev * 0.3f));
    }
    public int getItemDef(int lev)  // 해당 아이템 사용시 방어력 계산에 사용될 아이템 방어력 수치
    {
        return (int)(def * (1 + lev * 0.3f));
    }
    public int getItemHp(int lev)   // 해당 아이템 사용시 체력 계산에 사용될 아이템 체력 수치
    {
        return (int)(hp * (1 + lev * 0.6f));
    }
    public void skillDmgUp(TriggerData td)  // 스킬 데미지 증가
    {
        MapManager.Instans.player.dmgUp[code[td.ts.y]] += eventGab[code[td.ts.y]];
    }
    public void getKey(TriggerData td)  // 열쇠 획득
    {
        MapManager Mapm = MapManager.Instans;
        Mapm.keyCount += (int)eventGab[code[td.ts.y]];
        if (Mapm.keyCount < 0) Mapm.keyCount = 0;
        Mapm.keyCountText.text = (Mapm.keyCount).ToString();
    }
    public void ItemlevDown(TriggerData td) // 아이템 파괴 or 이벤트로 인한 아이템 레벨 수치 조정시 사용
    {
        Player.ItemData id = MapManager.Instans.player.myItems[td.ts.z];
        int gab = (int)eventGab[code[td.ts.y]];
        if (td.ts.x > gab)
        {
            id.lev -= gab;
        }
        else
        {
            id.lev = 0;
            id.item = GameManager.Instans.noneItem;
        }
        MapManager.Instans.checkItemKanUI();
    }
    public void FixHealItem(TriggerData td) // 플레이어 치유 수치에 영향
    {
        MapManager.Instans.player.PlayerGetHeal((int)eventGab[code[td.ts.y]], true);
    }
    public void LoseShieldAndHealAsItemLev(TriggerData td)  // 쉴드를 잃고 아이템 레벨 비례 회복
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.shild <= 0)
        {
            return;
        }
        p.shild -= 1;
        HealAsItemLev(td);
    }
    public void HealAsItemLev(TriggerData td)   // 아이템 레벨 비례 회복
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.PlayerGetHeal((int)p.myItems[td.ts.z].lev, true);
    }
    public void ChainLighting(TriggerData td)   // 연쇄 공격 이벤트 발생
    {
        td.ts.x *= 5;
        td.ts.y = (int)eventGab[code[td.ts.y]];
        MapManager.Instans.player.StartCoroutine("makeChainLight", td);
    }
    public void getExp(TriggerData td) //경험치 획득
    {
        MapManager.Instans.player.exp += (int)eventGab[code[td.ts.y]];
    }
    public void getShield(TriggerData td) //쉴드 획득
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.shild += (int)eventGab[code[td.ts.y]];
        Mapm.shildCheck();
    }
    public void removeThisItem(TriggerData td) //아이템 파괴
    {
        MapManager Mapm = MapManager.Instans;
        Player.ItemData id = MapManager.Instans.player.myItems[td.ts.z];
        id.lev = 0;
        id.item = GameManager.Instans.noneItem;
        //player.myItems[selectItem].item = GM.noneItem;
        Mapm.checkItemKanUI();
    }
    public void MoveRandomLocation(TriggerData td) //랜덤장소 이동
    {
        MapManager Mapm = MapManager.Instans;
        MonsterManager Monm = MonsterManager.instance;
        Player p = Mapm.player;
        int breakCount = 0;
        Vector2Int pos = new Vector2Int();
        while (true)
        {
            if (++breakCount > 40) return;
            pos.x = Random.Range(1, 19);
            pos.y = Random.Range(1, 19);
            if (Mapm.getTileData(pos.x, pos.y) == MapManager.TileCheck.Empty) break;
        }
        Mapm.playerMove = pos;
        Monm.setTranslucentTile(pos, true);
        Mapm.tileNodeSortSet(pos, MapManager.TileCheck.Player);
        p.transform.position = Mapm.cMap[pos.x, pos.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        p.end = p.transform.position;
    }

    public void Bomb(TriggerData td)    // 플레이어 주변 폭발
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = 1;// = p.GetPlayerDmg(td.ts.z) / 3;
        //d = Mathf.Max(1, d);
        int intState = p.getState(2);
        int ban = (int)eventGab[code[td.ts.y]] / 2;
        for (int i = -ban; i < (int)eventGab[code[td.ts.y]] - ban; ++i)
        {
            for (int j = -ban; j < (int)eventGab[code[td.ts.y]] - ban; ++j)
            {
                if (i == 0 && j == 0) continue;
                Vector2Int point = new Vector2Int(Mapm.playerMove.x + i, Mapm.playerMove.y + j);
                try
                {
                    MapManager.TileCheck tc = Mapm.getTileData(point);
                    if (tc != MapManager.TileCheck.Block)
                    {
                        Transform effect = Instantiate(Mapm.ImpactAnim[10]).transform;
                        effect.position = p.transform.position + new Vector3(0, 0.55f);
                        effect.gameObject.SetActive(true);
                        Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
                        effect.parent = tTemp;
                        if (tc == MapManager.TileCheck.Enemy)
                        {
                            tTemp = tTemp.GetChild(0);
                            int cc = tTemp.childCount;
                            if (cc > 0)
                            {
                                tTemp.GetChild(0).GetComponent<Monster>().APuDa(d);
                                if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                                {
                                    p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                                }
                            }
                        }

                    }
                }
                catch (System.NullReferenceException)
                {

                }
                catch (System.IndexOutOfRangeException)
                {

                }
            }
        }
        Mapm.shildCheck();
    }
    public void DmgUpOneTurn(TriggerData td) //1 턴 데미지 증가
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Mapm.getBuff(0, 0, 0, 1, Img, "", (int)p.myItems[td.ts.z].lev, 0, 0);
    }
    public void temJangPan(TriggerData td)  // 아이템으로 인한 불데미지 장판 소환
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Transform effect = Instantiate(Mapm.ImpactAnim[9]).transform;
        effect.position = p.transform.position + new Vector3(0, 0.55f);
        effect.gameObject.SetActive(true);
        p.dotDmg.Add(new Player.JangPanData(td.ts.z, Mapm.playerMove, (int)p.myItems[td.ts.z].lev, 3, effect));
    }
    public void playerDmged(TriggerData td) // 플레이어 피해량 영향
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.life > 0) p.life -= (int)eventGab[code[td.ts.y]];
        Mapm.lifeBar.value = p.life;
        Mapm.lifeText.text = p.life + "/" + p.checkMyMaxHp();
        Transform temp = Mapm.showDmg((int)eventGab[code[td.ts.y]], 1);
        //temp.position = new Vector3(p.transform.position.x+Random.Range(-0.1f, 0.1f), p.transform.position.y+Random.Range(0.1f, 0.2f) + 1, temp.position.z);
        temp.position = new Vector3(p.transform.position.x + Random.Range(-0.1f, 0.1f),
    p.transform.position.y + Random.Range(0.1f, 0.2f) + 1,
    temp.position.z);
        temp.gameObject.SetActive(true);
    }
    public void playerDmgedPercent(TriggerData td)  // 플레이어 최대체력 피해량에 영향
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int dmg = (int)(eventGab[code[td.ts.y]] / 100f * p.checkMyMaxHp());
        if (p.life > dmg) p.life -= dmg;
        Mapm.lifeBar.value = p.life;
        Mapm.lifeText.text = p.life + "/" + p.checkMyMaxHp();
        Transform temp = Mapm.showDmg((int)eventGab[code[td.ts.y]], 1);
        //temp.position = new Vector3(p.transform.position.x+Random.Range(-0.1f, 0.1f), p.transform.position.y+Random.Range(0.1f, 0.2f) + 1, temp.position.z);
        temp.position = new Vector3(p.transform.position.x + Random.Range(-0.1f, 0.1f),
    p.transform.position.y + Random.Range(0.1f, 0.2f) + 1,
    temp.position.z);
        temp.gameObject.SetActive(true);
    }
    public void buffExtension(TriggerData td)   // 버프 시간 지속 or 갱신
    {
        MapManager Mapm = MapManager.Instans;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code[0].ToString())
            {
                Mapm.buffList[i].count = 1;
                Mapm.buffList[i].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mapm.buffList[i].count.ToString();
            }
        }
    }   // 아이템 버프 시간 연장
    public void playerDmgedAsLv(TriggerData td) // 아이템 레벨에 비례하여 플레이어 피해
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.life > 0) p.life -= p.myItems[td.ts.z].lev;
        Mapm.lifeBar.value = p.life;
        Mapm.lifeText.text = p.life + "/" + p.checkMyMaxHp();
        Transform temp = Mapm.showDmg((int)eventGab[code[td.ts.y]], 1);
    }
    public void addAtkBuff(TriggerData td)  // 공격력 버프
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        string codeS = p.myItems[td.ts.z].ToString() + "atk" + td.ts.y.ToString();
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == codeS)
            {
                Mapm.buffList[i].count = td.ts.y;
                return;
            }
        }
        Mapm.getBuff(0, 0, 0, code[td.ts.y], Img, codeS, (int)eventGab[td.ts.y], 0, 0);
    }
    public void OverHpAtkDmgToHeal(TriggerData td)  // 적 최대체력 초과 공격시 회복
    {
        Player p = MapManager.Instans.player;
        p.PlayerGetHeal(td.num / 10, true);
    }
    public void addDefBuff(TriggerData td)  // 방어력 버프
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        string codeS = p.myItems[td.ts.z].ToString() + "def" + td.ts.y.ToString();
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == codeS)
            {
                Mapm.buffList[i].count = td.ts.y;
                return;
            }
        }
        Mapm.getBuff(0, 0, 0, code[td.ts.y], Img, codeS, 0, (int)eventGab[td.ts.y], 0);
    }
    public void HpAddValUp(TriggerData td) //레벨업시 체력 상승량 증가
    {
        Player p = MapManager.Instans.player;
        p.TaritSpecState[1] += (int)eventGab[td.ts.y];
    }
}
