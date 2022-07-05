using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkillName", menuName = "Scriptable Object Asset/SkillData")]
public class Skill : ScriptableObject
{
    //이름
    //공격 종류
    //범위
    //최대 히트 수
    //데미지
    //에니메이션
    //이펙트
    //스킬사용후 위치
    //정보
    public enum AtkKind { none = 0, acttive, atkTrigger, nonTarget, warfAtk, buffMe, killEnemy, isBuffState, getHeal, moveDetect, noMoveDetect, monsterIsHit, dmgUp, isBuffOff,isLevelUp,isDamaged,isCounter, isGetKey, isBoxOpen, isChangedItem, everyTurn, isAddtionalTurn,isGameStart,isBuffStart, isNormalAtk,isOverHp };
    
    public string skillName;
    public int coolTime;
    public string code;
    public Vector2Int[] atkRange;
    public Vector3Int[] getTimerCode;
    public AtkKind[] kind;
    public MapManager.TileCheck tcTrigger;
    public UnityEvent<object>[] skillEvent;
    public Vector3Int[] atkPoint;
    public int hit;
    public float[] baseDmg = new float[4];
    public float[] dmg = new float[4];

    public AnimationClip anim;
    public GameObject effect;
    public GameObject bombEffect;
    public GameObject bulletEffect;
    public Vector2Int movePlayer;

    [TextArea(10, 20)]
    public string info;
    public Sprite skillIcon;
    public AudioClip sound;

    public Skill changeSkill;

    public string getInfoText(int n, bool inGame = true)
    {
        string[] dataArr = info.Split('@');
        string sTemp = "";
        if (inGame)
        {
            Player p = MapManager.Instans.player;
            for (int i = 0; i < dataArr.Length; ++i)
            {
                switch (dataArr[i])
                {
                    case "dmg":
                        sTemp += "<color=#" + ColorUtility.ToHtmlStringRGB(GameManager.Instans.StateColor[p.mainState]) + ">"
                            + p.GetPlayerDmg(n) + "</color>";
                        break;
                    case "dotDmg":
                        sTemp += "<color=#" + ColorUtility.ToHtmlStringRGB(GameManager.Instans.StateColor[p.mainState]) + ">"
                            + (p.GetPlayerDmg(n) / 3) + "(" + getTimerCode[0] + "턴)" + "</color>";
                        break;
                    case "buff":
                        sTemp += getTimerCode[0].ToString();
                        break;
                    default:
                        sTemp += dataArr[i];
                        break;
                }
            }
        }
        else 
        {
            for (int i = 0; i < dataArr.Length; ++i)
            {
                switch (dataArr[i])
                {
                    case "dmg":
                        sTemp += "<color=#" + ColorUtility.ToHtmlStringRGB(GameManager.Instans.StateColor[GameManager.Instans.selChar.mainState]) + ">"
                            + (int)baseDmg[0] + "+" + dmg[0].ToString("N1") + "*D" + "</color>";
                        break;
                    case "dotDmg":
                        sTemp += "<color=#" + ColorUtility.ToHtmlStringRGB(GameManager.Instans.StateColor[GameManager.Instans.selChar.mainState]) + ">"
                            + Mathf.Max(1, (int)(baseDmg[0] / 3)) + "+" + Mathf.Max(1.0f, dmg[0] / 3).ToString("N1") + "*D" + "(" + getTimerCode[0].x + "턴)" + "</color>";
                        break;
                    case "buff":
                        sTemp += getTimerCode[0].x.ToString();
                        break;
                    default:
                        sTemp += dataArr[i];
                        break;
                }
            }
        }
        return sTemp;
    }

    public static Vector2Int getRotatePoint(Vector2Int p, int rotate)
    {
        //3(0), 1(90), 0(180), 2(270)
        switch (rotate)
        {
            case 1:
                return new Vector2Int(-p.y, p.x);
            case 0:
                //p = new Vector2Int(-p.x, -p.y);
                return p * -1;
            case 2:
                return new Vector2Int(p.y, -p.x);
            default:
                return p;
        }
    }

    public struct TriggerData
    {
        public Vector3Int ts;
        public Monster m;
        public Transform tTemp;
        public int num; // 스킬 이벤트 번호
        public TriggerData(Vector3Int xyz, Monster ms = null, Transform t = null, int number = 0)
        {
            ts = xyz;
            m = ms;
            tTemp = t;
            num = number;
        }
        public TriggerData(int a)
        {
            ts = Vector3Int.zero;
            m = null;
            tTemp = null;
            num = 0;
        }
    }

    public void keyGetPercentageUp(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager.Instans.player.keyGetPercentage +=getTimerCode[td.num].x;
    }
    #region 공격트리거
    public void executionLessLifeEnemy(object m)
    {
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        if (td.m.hp > 0 && td.m.hp < p.getState(p.mainState) * p.lev)
        {
            td.m.APuDa(9999);
        }
    }
    public void atkHeal(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.PlayerGetHeal(p.lev+getTimerCode[td.num].y); 
    }
    public void fixedHeal(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.PlayerGetHeal((int)baseDmg[Mapm.skillLev[td.ts.z]]);
    }
    public void getBuffIfGetKey(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        if (Mapm.keyCount > 0)
        {
            int val = (int)baseDmg[Mapm.skillLev[td.ts.z]] + 1;
            Mapm.getBuff(val, val, val,1, skillIcon, code, 0, 0, 0); 
        }
        else
        {
            Mapm.getBuff(0, 0, 0, 1, skillIcon, code, 0, 0, 0);
        }
    }
    public void aktShield(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.shild = Mathf.Clamp(p.shild + getTimerCode[td.num].x, 0, p.getState(2));
        Mapm.shildCheck();
    }
    public void buffNumIsAtk(object m)
    {
        TriggerData td = (TriggerData)m;
        int counter = 0;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                counter = Mapm.buffList[i].count;
                break;
            }
        }
        p.multipleAtk(this, (TriggerData)m,counter);
    }
    public void probabilityAtk(object m)
    {
        TriggerData td = (TriggerData)m;
        int counter = 1;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int percentage = getTimerCode[td.num].z;
        while (percentage < Random.Range(0, 100))
        {
            ++counter;
        }
        p.multipleAtk(this, (TriggerData)m, counter);
    }
    public void healAsKeyCount(object m)
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if(Mapm.keyCount>4) p.PlayerGetHeal(1,false);
    }
    public void moveRandPos(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int breakCount = 0;
        MonsterManager Monm = p.Monm;
        Monm.setTranslucentTile(Mapm.playerMove, false);
        Mapm.tileNodeSortSet(Mapm.playerMove, MapManager.TileCheck.Empty);
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
    public void killThisSkillGetKey(object m)
    {
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            
            keyCountChange(m);
        }
    }
    public void killThisSkillmakeJangPang(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            int d = Mathf.Max(p.GetPlayerDmg(td.ts.z) / 3, 1);
            Vector2Int pos = td.m.monsterLocatePos;
            if (Mapm.getTileData(pos) == MapManager.TileCheck.Block) return;
            Transform effect = Instantiate(Mapm.ImpactAnim[9]).transform;
            effect.position = Mapm.cMap[pos.x, pos.y].tileTransform.position + new Vector3(0, 0.55f);
            effect.gameObject.SetActive(true);
            p.dotDmg.Add(new Player.JangPanData(td.ts.z, pos, d, getTimerCode[td.num].x, effect));
        }
    }
    public void killThisSkillGetBuff(object m)
    {
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            addMainStateProp(m);
        }
    }
    public void getKey(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        for (int i = 0; i < getTimerCode[td.num].y; ++i) {
            if ((int)baseDmg[Mapm.skillLev[td.ts.z]] > Random.Range(0, 100))
            {
                keyCountChange(m);
            }
        }
    }
    public void keyCountChange(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Mapm.keyCount += getTimerCode[td.num].x;
        if (Mapm.keyCount < 0) Mapm.keyCount = 0;
        Mapm.keyCountText.text = Mapm.keyCount.ToString();
    }
    public void temtemP(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int val = 0;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code) { 
                val = 1;
                if (p.getState(0) < p.getState(1))
                {
                    val = 2;
                    if (p.getState(1) < p.getState(2)) val = 3;
                }
                else if (p.getState(0) < p.getState(2)) val = 3;
                break;
            }
        }

        int d = p.GetPlayerDmg(td.ts.z);

        int intState = p.getState(2);
        switch (val)
        {
            case 1: // 힘이가장 높을경우 : 불장판 생성
                d = Mathf.Max(1, d / 3);
                
                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; ++j) {
                        Vector2Int pos = new Vector2Int(td.ts.x+i, td.ts.y+j);
                        if (Mapm.getTileData(pos)==MapManager.TileCheck.Block) continue;
                        Transform effect = Instantiate(Mapm.ImpactAnim[9]).transform;
                        effect.position = Mapm.cMap[pos.x,pos.y].tileTransform.position + new Vector3(0, 0.55f);
                        effect.gameObject.SetActive(true);
                        p.dotDmg.Add(new Player.JangPanData(td.ts.z, pos, d, getTimerCode[td.num].x, effect));
                    }
                }

                break;
            case 2: // 민첩이 가장 높을경우 : 즉시 폭팔생성
                Vector2Int point = new Vector2Int(td.ts.x, td.ts.y);
                if (Mapm.getTileData(point.x, point.y) != MapManager.TileCheck.Block)
                {
                    Transform temp = Instantiate(Mapm.ImpactAnim[10], Vector2.zero, Quaternion.identity).transform;
                    Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
                    temp.parent = tTemp;
                    temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                    if (Mapm.getTileData(point.x, point.y) == MapManager.TileCheck.Enemy)
                    {
                        tTemp = tTemp.GetChild(0);
                        int cc = tTemp.childCount;
                        if (cc > 0)
                        {
                            int counter = hit;
                            counter += Mapm.skillLev[td.ts.z] / 2;
                            for (int k = 0; k < cc; ++k)
                            {
                                if (counter-- < 0) break;
                                tTemp.GetChild(0).GetComponent<Monster>().APuDa(d);
                                if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                                {
                                    p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                                }
                            }
                        }
                    }
                    temp.gameObject.SetActive(true);
                }
                Mapm.shildCheck();
                break;
            case 3: // 지능이 가장높을경우 : 3번튕기는 연쇄번개 3개 생성
                atk(m);
                ChainLightning(m);
                //td.ts.x *= 5;   // 데미지
                //td.ts.y = 30;    // 튕기는 횟수
                //MapManager.Instans.player.StartCoroutine("makeChainLight", td);
                break;
            default:
                atk(m);
                break;
        }
    }
    public void ChainLightning(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Vector2Int point = new Vector2Int(td.ts.x, td.ts.y);
        MapManager.TileCheck tc = Mapm.getTileData(point);
        int d = p.GetPlayerDmg(td.ts.z);
        if (tc != MapManager.TileCheck.Block)
        {
            Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;

            if (tc == MapManager.TileCheck.Enemy)
            {
                tTemp = tTemp.GetChild(0);
                int cc = tTemp.childCount;
                if (cc > 0)
                {
                    for (int i = 0; i < cc; ++i)
                    {
                        try
                        {
                            Monster monster = tTemp.GetChild(0).GetComponent<Monster>();
                            td.m = monster;

                            td.ts.x = d;   // 데미지
                            td.ts.y = hit + Mapm.skillLev[td.ts.z] / 2; // 튕기는 횟수
                            MapManager.Instans.player.StartCoroutine("makeChainLight", td);
                            break;
                        }
                        catch (System.NullReferenceException)
                        {

                        }
                    }
                }
            }

        }
    }
    public void preemptiveStrike(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Vector2Int point = new Vector2Int(td.ts.x, td.ts.y);
        MapManager.TileCheck tc = Mapm.getTileData(point);
        int d = p.GetPlayerDmg(td.ts.z);
        if (tc != MapManager.TileCheck.Block)
        {
            Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;

            if (tc == MapManager.TileCheck.Enemy)
            {
                tTemp = tTemp.GetChild(0);
                int cc = tTemp.childCount;
                if (cc > 0)
                {
                    for (int i = 0; i < cc; ++i)
                    {
                        try
                        {
                            Monster monster = tTemp.GetChild(0).GetComponent<Monster>();
                            if(monster.sCount == 0){
                                monster.APuDa(d);
                            }
                            break;
                        }
                        catch (System.NullReferenceException)
                        {

                        }
                    }
                }
            }

        }
    }
    public void healIfkeyUseUpgrade(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (Mapm.keyCount > 0) { 
            p.PlayerGetHeal((getTimerCode[td.num].x + p.GetPlayerDmg(td.ts.z))*2);
            Mapm.keyCountText.text = (--Mapm.keyCount).ToString();
        }
        else p.PlayerGetHeal(getTimerCode[td.num].x + p.GetPlayerDmg(td.ts.z));
    }
    public void overHealToBomb(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Mapm.atkSoundPlayer(td.ts.z + 2);
        int d = Mathf.Max(p.GetPlayerDmg(td.ts.z) * p.overHealHp,1);
        int intState = p.getState(2);
        int ban = getTimerCode[td.num].y / 2;
        for (int i = -ban; i < getTimerCode[td.num].y - ban; ++i)
        {
            for (int j = -ban; j < getTimerCode[td.num].y - ban; ++j)
            {
                if (i == 0 && j == 0) continue;
                Vector2Int point = new Vector2Int(td.ts.x + i, td.ts.y + j);
                try
                {
                    if (Mapm.getTileData(point.x,point.y) != MapManager.TileCheck.Block)
                    {
                        Transform temp = Mapm.effectManageToShow(2 + td.ts.z);
                        Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
                        temp.parent = tTemp;
                        temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                        if (Mapm.getTileData(point.x, point.y) == MapManager.TileCheck.Enemy)
                        {
                            tTemp = tTemp.GetChild(0);
                            int cc = tTemp.childCount;
                            if (cc > 0)
                            {
                                int counter = hit;
                                counter += Mapm.skillLev[td.ts.z] / 2;
                                for (int k = 0; k < cc; ++k)
                                {
                                    if (counter-- < 0) break;
                                    tTemp.GetChild(0).GetComponent<Monster>().APuDa(d);
                                    if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                                    {
                                        p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                                    }
                                }
                            }
                        }
                        temp.gameObject.SetActive(true);
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
    public void fortuneCookie(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int val = Random.Range(0, 3);
        switch (val)
        {
            case 0:
                fixedHeal(m);
                break;
            case 1:
                decreasePlayerHp(m);
                break;
            case 2:
                addMainStateProp(m);
                break;
        }
    }
    public void decreasePlayerHp(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.life < (int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]]) p.life = 1;
        else p.life -= (int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]];
        Mapm.lifeBar.value = p.life;
        Mapm.lifeText.text = p.life + "/" + p.checkMyMaxHp();
    }
    public void JangPangByAtkPoint(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z);
        foreach(Vector2Int apt in atkPoint) {
                Vector2Int pos = getRotatePoint(new Vector2Int(apt.x, apt.y), Mapm.saveInput)+Mapm.playerMove;
            if (Mapm.getTileData(pos) == MapManager.TileCheck.Block) continue;
                Transform effect = Instantiate(Mapm.ImpactAnim[9]).transform;
                effect.position = Mapm.cMap[pos.x, pos.y].tileTransform.position + new Vector3(0, 0.55f);
                effect.gameObject.SetActive(true);
                p.dotDmg.Add(new Player.JangPanData(td.ts.z, pos, d, getTimerCode[td.num].x, effect));
        }
    }
    public void crossJangPang(object m){
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z)/3;
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                if (((i>0)?i:-i)+ ((j > 0) ? j : -j) == 2) continue;
                Vector2Int pos = new Vector2Int(td.ts.x + i, td.ts.y + j);
                if (Mapm.getTileData(pos) == MapManager.TileCheck.Block) continue;
                Transform effect = Instantiate(Mapm.ImpactAnim[9]).transform;
                effect.position = Mapm.cMap[pos.x, pos.y].tileTransform.position + new Vector3(0, 0.55f);
                effect.gameObject.SetActive(true);
                p.dotDmg.Add(new Player.JangPanData(td.ts.z, pos, d, getTimerCode[td.num].x, effect));
            }
        }

    }
    #endregion

    #region 쿨타임트리거
    public void Skill_CoolTime_Reduce(object m) //getTimerCode x 번째 스킬을 getTimerCode y만큼 쿨타임 감소
    {                                           //getTimerCode.y==-1이면 전부감소
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        if (p.Mapm.CoolTime[getTimerCode[td.num].x] <= 0) return;
        p.Mapm.CoolTime[getTimerCode[td.num].x] -= (getTimerCode[td.num].y == -1 && p.Mapm.CoolTime[getTimerCode[td.num].x] >= 0) ? coolTime : getTimerCode[td.num].y;
        p.Mapm.coolTimeShowMethod();
    }
    public void Skill_CoolTimeReduceAsCode(object m)//code의 스킬을 getTimerCode y만큼 쿨타임 감소
    {                                           //getTimerCode.y==-1이면 전부감소
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        for (int i=0; i < p.mySkillSet.Length; ++i)
        {
            if (p.mySkillSet[i].code == code)
            {
                if (p.Mapm.CoolTime[i] <= 0) return;
                p.Mapm.CoolTime[i] -= (getTimerCode[td.num].y == -1 && p.Mapm.CoolTime[i] >= 0) ? coolTime : getTimerCode[td.num].y;
                p.Mapm.coolTimeShowMethod();
                break;
            }
        }
    }
    #endregion

    #region 폭팔
    public void CrossBomb3x3(object m)
    {
        //ts.x,y = 타갯위치, tTemp 이팩트
        TriggerData td = (TriggerData)m;

        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        try { 
        GameObject gTemp = Instantiate(bombEffect, Mapm.cMap[td.ts.x, td.ts.y].tileTransform);
        gTemp.transform.localPosition = Vector3.up * 0.9f + Vector3.back * 10;
        Destroy(gTemp, 2);
        }
        catch (System.NullReferenceException) { }
        //Mapm.atkSoundPlayer(td.ts.z + 2);
        int d = p.GetPlayerDmg(td.ts.z);
        int intState = p.getState(2);
        int ban = getTimerCode[td.num].y / 2;
        for (int i = -ban; i < getTimerCode[td.num].y - ban; ++i)
        {
            for (int j = -ban; j < getTimerCode[td.num].y - ban; ++j)
            {
                if (Mapm.getTileData(td.ts.x + i, td.ts.y + j) != MapManager.TileCheck.Block)
                {
                    try
                    {
                        Transform tTemp = Mapm.cMap[td.ts.x + i, td.ts.y + j].tileTransform.GetChild(0);
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
                    catch (System.NullReferenceException)
                    {

                    }
                    catch (System.IndexOutOfRangeException)
                    {

                    }
                }
            }
        }
        Mapm.shildCheck();
    }
    #endregion

    #region 장판남기기
    public void makeBullJangPan3Time(object m)
    {
        TriggerData td = (TriggerData)m;

        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z) / 3;
        d = Mathf.Max(1, d);
        p.dotDmg.Add(new Player.JangPanData(td.ts.z, (Vector2Int)td.ts, d, getTimerCode[td.num].x, td.tTemp));
    }

    #endregion

    #region 이동기
    public void warfAtk3x3(object m)
    {
        TriggerData td = (TriggerData)m;

        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z) / 3;
        d = Mathf.Max(1, d);

        Mapm.atkSoundPlayer(td.ts.z + 2);
        int intState = p.getState(2);
        int ban = getTimerCode[td.num].y / 2;
        for (int i = -ban; i < getTimerCode[td.num].y - ban; ++i)
        {
            for (int j = -ban; j < getTimerCode[td.num].y - ban; ++j)
            {
                if (i == 0 && j == 0) continue;
                Vector2Int point = new Vector2Int(td.ts.x + i, td.ts.y + j);
                try
                {
                    MapManager.TileCheck tc = Mapm.getTileData(point);
                    if (tc != MapManager.TileCheck.Block)
                    {
                        Transform temp = Mapm.effectManageToShow(2 + td.ts.z);
                        Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
                        temp.parent = tTemp;
                        temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                        if (tc == MapManager.TileCheck.Enemy)
                        {
                            tTemp = tTemp.GetChild(0);
                            int cc = tTemp.childCount;
                            if (cc > 0)
                            {
                                int counter = hit;
                                counter += Mapm.skillLev[td.ts.z] / 2;
                                for(int k = 0; k < cc; ++k)
                                {
                                    if (counter-- < 0) break;
                                    tTemp.GetChild(0).GetComponent<Monster>().APuDa(d);
                                    if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                                    {
                                        p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                                    }
                                }
                            }
                        }
                        temp.gameObject.SetActive(true);
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
    public void removeAllBuff(object m) //모든 버프 제거
    {
        MapManager Mapm = MapManager.Instans;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
           Mapm.buffList[i].count = -1;
        }
        Mapm.checkBuffList();
    }

   IEnumerator buffNumberIsAtk(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int counter = 0;
        int d = p.GetPlayerDmg(td.ts.z);
        int intState = p.getState(2);
        int nowMonsterIndex;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code) { 
                counter = Mapm.buffList[i].count;
                Mapm.buffList[i].count= -1;
                break;
            }
        }
        nowMonsterIndex = 0;
        Transform tTemp = Mapm.cMap[td.ts.x, td.ts.y].tileTransform.GetChild(0);
        int cc = tTemp.childCount;
        Monster monster = tTemp.GetChild(nowMonsterIndex).GetComponent<Monster>();
        for (int i=0; i<counter; ++i) {
            p.anim.SetTrigger("Atk"+getTimerCode[0]);
            yield return new WaitForSeconds(0.1f);
            if (cc > 0)
            {
                if (nowMonsterIndex >= cc) break;
                try
                {
                    if (monster.hp < d) monster = tTemp.GetChild(++nowMonsterIndex).GetComponent<Monster>();
                    monster.APuDa(d);
                    if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                    {
                        p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                    }
                }
                catch (System.NullReferenceException)
                {

                }
            }
        }
    }
    public void movePos(object m) //nonTargetArrow로 tcTrigger에 해당하는 타일를 감지하면 그 위치로 플레이어를 이동하기 위함
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        MonsterManager Monm = p.Monm;
        Vector2Int pos = new Vector2Int(td.ts.x, td.ts.y);
        if (Mapm.playerMove == pos) return;
        Monm.setTranslucentTile(Mapm.playerMove, false);
        Mapm.tileNodeSortSet(Mapm.playerMove, MapManager.TileCheck.Empty);
        if (pos.x > Mapm.playerMove.x) --pos.x;
        else if (pos.x < Mapm.playerMove.x) ++pos.x;
        if (pos.y > Mapm.playerMove.y) --pos.y;
        else if (pos.y < Mapm.playerMove.y) ++pos.y;
        Mapm.playerMove = pos;
        Monm.setTranslucentTile(pos, true);
        Mapm.tileNodeSortSet(pos, MapManager.TileCheck.Player);
        p.transform.position = Mapm.cMap[pos.x, pos.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        p.end = p.transform.position;
    }
    public void asHpProportionAtk(object m)
    {
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        float onePercent = p.checkMyMaxHp() / 100f;
        if (p.life <(int)( onePercent* getTimerCode[td.num].x))
        {
            atk(m);
        }
    }
    public void atk_piercing(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int intState = p.getState(2);
        Vector2Int point = new Vector2Int(td.ts.x, td.ts.y);
        int d = p.GetPlayerDmg(td.ts.z);
        MapManager.TileCheck tc = Mapm.getTileData(point);
        if (tc != MapManager.TileCheck.Block)
        {
            Transform temp = Mapm.effectManageToShow(2 + td.ts.z);
            Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
            temp.parent = tTemp;
            temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
            if (tc == MapManager.TileCheck.Enemy)
            {
                tTemp = tTemp.GetChild(0);
                int cc = tTemp.childCount;
                if (cc > 0)
                {
                    for (int i = 0; i < cc; ++i)
                    {
                        try
                        {
                            Monster monster = tTemp.GetChild(0).GetComponent<Monster>();
                            if (d > monster.hp)
                            {
                                int tempN = d;
                                d = d - monster.hp;
                                if (d <= 0) break;
                                monster.APuDa(tempN);
                            }
                            else
                            {
                                monster.APuDa(d);
                                break;
                            }
                            if (td.ts.z > 0 && Random.Range(0, 10) < intState && p.shild < intState)
                            {
                                p.shild = Mathf.Clamp(p.shild + 1, 0, intState);
                            }
                        }
                        catch (System.NullReferenceException)
                        {

                        }
                    }
                }
            }
            temp.gameObject.SetActive(true);
        }

    }
    public void atk(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Vector2Int point = new Vector2Int(td.ts.x, td.ts.y);
        int d = p.GetPlayerDmg(td.ts.z);
        int intState = p.getState(2);
        MapManager.TileCheck tc = Mapm.getTileData(point);
        if (tc != MapManager.TileCheck.Block)
        {
            Transform temp = Mapm.effectManageToShow(2 + td.ts.z);
            Transform tTemp = Mapm.cMap[point.x, point.y].tileTransform;
            temp.parent = tTemp;
            temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
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
            temp.gameObject.SetActive(true);
        }

    }
    public  void dmgUpAsShield(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int addDmg = (int)baseDmg[Mapm.skillLev[td.ts.z]]+p.shild;
        Mapm.getBuff(0, 0, 0, (getTimerCode[td.num].x >= -1) ? getTimerCode[td.num].x : -getTimerCode[td.num].x, skillIcon, code, addDmg, 0, 0);
    }
    public void nothingHitMove(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int a = (Mapm.playerMove.x - td.ts.x>0)? Mapm.playerMove.x - td.ts.x: td.ts.x - Mapm.playerMove.x;
        int b = (Mapm.playerMove.y - td.ts.y > 0) ? Mapm.playerMove.y - td.ts.y : td.ts.y - Mapm.playerMove.y;
        if (a + b == atkPoint.Length) RetreatBack(m);
    }
    public void hitThisSkillGetSheild(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            ++p.shild;
            Mapm.shildCheck();
        }
    }
    public void RetreatBack(object m) // 캐릭터 이동 함수
    {
        // getTimerCode[0].x 의 마이너스 수치만큼 뒤로 후퇴한다
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        MonsterManager Monm = MonsterManager.instance;
        Player pr = Mapm.player;

        Vector2Int endPoint = Mapm.playerMove;
        Vector2Int p;
        if (bombEffect != null)
        {
            Transform slideEffect = Instantiate(bombEffect, pr.transform).transform;
            slideEffect.parent = pr.transform;
            slideEffect.localPosition = new Vector3(0, 0.6f, 0);
            Destroy(slideEffect.gameObject, 1.2f);
        }
        if (getTimerCode[td.num].x < 0) { 
        for (int i = -1; i >= getTimerCode[td.num].x; --i)
        {
            p = getRotatePoint(new Vector2Int(i,0), Mapm.saveInput) + Mapm.playerMove;
            MapManager.TileCheck tc = Mapm.getTileData(p);
            if (tc == MapManager.TileCheck.Enemy || tc == MapManager.TileCheck.Block || tc == MapManager.TileCheck.Item)
            {
                break;
            }
            endPoint = p;
        }
        }
        else{
            for (int i = 1; i <= getTimerCode[td.num].x; ++i)
            {
                p = Skill.getRotatePoint(new Vector2Int(i, 0), Mapm.saveInput) + Mapm.playerMove;
                MapManager.TileCheck tc = Mapm.getTileData(p);
                if (tc == MapManager.TileCheck.Enemy || tc == MapManager.TileCheck.Block || tc == MapManager.TileCheck.Item)
                {
                    break;
                }
                endPoint = p;
            }
        }
        Vector3 end = Mapm.cMap[endPoint.x, endPoint.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        Monm.setTranslucentTile(Mapm.playerMove, false);
        Mapm.tileNodeSortSet(Mapm.playerMove, MapManager.TileCheck.Empty);
        Mapm.playerMove = endPoint;
        Monm.setTranslucentTile(endPoint, true);
        Mapm.tileNodeSortSet(endPoint, MapManager.TileCheck.Player);
        pr.transform.position = Mapm.cMap[endPoint.x, endPoint.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        pr.end = pr.transform.position;
    }
    #endregion

    #region 버프스킬
    public void addMainStateProp(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int[] status;
        status = new int[6] { 0, 0, 0, 0, 0, 0 };
        int addDmg = 0;
        try
        {
            addDmg = (int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]];
        }
        catch (System.IndexOutOfRangeException) { }
        status[getTimerCode[td.num].y] = getTimerCode[td.num].z + addDmg;
        // getTimerCode 
        // x : 지속시간, x가 - 값이면 쌓을수 있는 버프, + 값이면 갱신 형태의 버프
        // y
        // 0 : 힘 , 1 : 민첩, 2 : 지능
        // 3 : 공 , 4 : 방어, 5 : 체력
        /*for (int j = 0; j < Mapm.buffList.Count; ++j)
        {
            if (Mapm.buffList[j].Bfcode == code)
            {
                if (getTimerCode[td.num].x < 0)
                {
                    Mapm.buffList[j].count -= (getTimerCode[td.num].x);
                    Mapm.buffList[j].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mapm.buffList[j].count.ToString();
                    return;
                }
                if (Mapm.buffList[j].count < getTimerCode[td.num].x)
                {
                    Mapm.buffList[j].count = getTimerCode[td.num].x;
                    Mapm.buffList[j].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mapm.buffList[j].count.ToString();
                    return;
                }
                else
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        p.myAddState[k] += status[k];
                        Mapm.buffList[j].addState[k] += status[k];
                    }
                    Mapm.buffList[j].Batk += status[3];
                    Mapm.buffList[j].Bdef += status[4];
                    Mapm.buffList[j].Bhp += status[5];
                    Mapm.checkPlayerState();
                    return;
                }
            }
        }*/
        Mapm.getBuff(status[0], status[1], status[2], getTimerCode[td.num].x, skillIcon, code, status[3], status[4], status[5]);
    }
    public void buff_skillDmgUp(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;   //getTimerCode x = 지속시간 y = 스킬번호 z = 증가 수치 %
        float dmgUpNum = getTimerCode[td.num].z / 100f;
        for (int j = 0; j < Mapm.buffList.Count; ++j)
        {
            if (Mapm.buffList[j].Bfcode == code)
            {
                Mapm.buffList[j].count = getTimerCode[td.num].x;
                return;
            }
        }
        Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, 0, 0, 0, getTimerCode[td.num].y,dmgUpNum);
    }
    public void buffExtension(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code) {
                Mapm.buffList[i].count += (getTimerCode[td.num].x);
                Mapm.buffList[i].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mapm.buffList[i].count.ToString();
            }
         }

    }
    public void buffProportionHeal(object m)
    {
        // getTimerCode 0 번 기초 힐 수치
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int sum = 0;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                sum = Mapm.buffList[i].count;
                break;
            }
        }
        Mapm.checkBuffList();
        p.PlayerGetHeal(getTimerCode[td.num].x + p.GetPlayerDmg(td.ts.z) * sum/5);
    }
    public void getItemRemoveKeyIfOverHealing(object m)
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;       
        if (p.overHealHp > 5 && Mapm.keyCount > 1)
        {
            Mapm.keyCount -= 2;
            Mapm.keyCountText.text = (Mapm.keyCount).ToString();
            Mapm.playerGetRandomItem(10, 50);
            ++Mapm.prbGetItem.item.getCount;
            Mapm.PopupUISetting(1);
        }

    }
    public void removeThisBuff(object m)
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                Mapm.buffList[i].count = -1;
                Mapm.buffList[i].myUI.parent = Mapm.buffSetInvPenal;
                for (int j = 0; j < 3; ++j)
                {
                    // 버프 꺼진것 탐색용
                    p.myAddState[j] -= Mapm.buffList[i].addState[j];
                }
                for (int j = 0; j < 4; ++j)
                {
                    p.dmgUp[j] -= Mapm.buffList[i].addDmg[j];
                }
                p.SkillTrigger(null, AtkKind.isBuffOff);
                Mapm.buffList.RemoveAt(i);
                Mapm.checkPlayerState();
                break;
            }
        }
    }
    public void isBuffStateChangeAtk(object m)
    {
        MapManager Mapm = MapManager.Instans;
        for(int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                warfAtk3x3(m);
                break;
            }
        }
    }
    public void StateUp(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int state = 0;
        switch (getTimerCode[td.num].x)
        {
            case 1:
            case 2:
            case 3:
                state = getTimerCode[td.num].x -1;
                break;
            default:
                state = p.mainState;
                break;
        }
        for(int i = 0; i < p.mySkillSet.Length; ++i)
        {
            if (this == p.mySkillSet[i])
            {
                p.psvState[state] = (int)dmg[Mapm.skillLev[i]];
                break;
            }
        }
        Mapm.checkPlayerState();
    }
    #endregion

    public void HillWind(object m)
    {
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;

        for(int i = 0;i < Mapm.buffList.Count; ++i)
        {
            if(Mapm.buffList[i].Bfcode == code)
            {
                CrossBomb3x3(m);
            }
        }
    }
    public void HealAndDmgUpIfOverHeal(object m)   // 초과회복시 데미지 업
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;

        if(p.checkMyMaxHp() > p.life + p.lev)
        {
            atkHeal(m);
            return;
        }

        p.life = p.checkMyMaxHp();
        Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, getTimerCode[td.num].y, 0, 0);
    }
    public void PercentHeal(object m)   // 일정 퍼센트 만큼 회복한다
    {   // getTimerCode[td.num].x 퍼센트 만큼 회복한다
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        p.PlayerGetHeal((int)(p.checkMyMaxHp() * (getTimerCode[td.num].x / 100f)));
    }
    public void killThisSkillPercentHeal(object m)
    {
        TriggerData td = (TriggerData)m;
        Player p = MapManager.Instans.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            PercentHeal(m);
        }
    }
    public void BuffDeftoDmg(object m)  //  방어력만큼 공격력 증가 버프
    {   // getTimerCode[td.num].x 의 지속시간동안 버프가 유지된다
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, p.getMyDefence(), 0, 0);
    }
    public void BuffStateToDmg(object m)    // 해당 수치만큼 공격력 증가 버프
    {
        // getTimerCode[td.num].x 의 지속시간동안 버프가 유지된다
        // getTimerCode[td.num].y의 수치만큼 공격력이 증가
        // 0 : 힘 , 1 : 민첩, 2 : 지능
        // 3 : 공 , 4 : 방어, 5 : 체력
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        for(int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                Mapm.buffList[i].count = getTimerCode[td.num].x;
                return;
            }
        }
        switch (getTimerCode[td.num].y)
        {
            case 4: // 방어력만큼 공격력 상승
                Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, p.getMyDefence(), 0, 0);
                break;
            case 5:
                float ml = 0f;  // 아이템수치 체력 합 + 힘x2 만큼 공격력 상승
                for (int i = 0; i < 6; ++i)
                {
                    int lev = p.myItems[i].lev;
                    if (lev > 0) ml += p.myItems[i].item.getItemHp(lev);
                }
                ml += p.getState(0) * (int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]];
                Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, (int)ml, 0, 0);
                break;
        }
    }
    public void HealofDmgPercentIfBuff(object m)  // 피흡버프있을때 피흡 시전
    {   //getTimerCode[td.num].x : 몇퍼센트 회복시킬건지 결정
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        for(int i =0; i < Mapm.buffList.Count; ++i)
        {
            if(Mapm.buffList[i].Bfcode == code)
            {
                p.PlayerGetHeal(td.ts.x * ((int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]] / 100));
                return;
            }
        }
    }
    public void HealofDmg(object m) // 피해량의 일부를 체력으로 전환
    {   //getTimerCode[td.num].x : 몇퍼센트 회복시킬건지 결정
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                p.PlayerGetHeal(td.ts.x * ((int)baseDmg[Mapm.skillLev[getTimerCode[td.num].x]] / 100));
                return;
            }
        }
    }
    public void ChainLightningIfBuff(object m)  // 버프 있을때 체인라이트닝 시전
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z);
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == code)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    for (int k = -1; k <= 1; ++k)
                    {
                        if (Mathf.Abs(j) == 1 && Mathf.Abs(k) == 1)
                            continue;
                        if (Mapm.getTileData(td.ts.x + j, td.ts.y + k) == MapManager.TileCheck.Enemy)
                        {
                            td.m = new Monster();
                            td.m = Mapm.cMap[td.ts.x + j, td.ts.y + k].tileTransform.GetChild(0).GetChild(0).GetComponent<Monster>();
                        }
                    }
                }
                //ChainLightning(td);
                td.ts.x = Mathf.Max(1, d / 3);   // 데미지
                td.ts.y = 2;    // 튕기는 횟수
                MapManager.Instans.player.StartCoroutine("makeChainLight", td);

            }
        }
    }
    public void killThisSkillFixedHeal(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        if (p.mySkillSet[td.ts.z].code == code)
        {
            fixedHeal(m);
        }
     }
    public void killThisSkillMakeBomb(object m)
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int d = td.ts.x;
        int intState = p.getState(2);
        
        if (p.mySkillSet[td.ts.z].code == code)
        {
            try { 
            GameObject gTemp = Instantiate(bombEffect, Mapm.cMap[td.m.monsterLocatePos2.x, td.m.monsterLocatePos2.y].tileTransform);
            gTemp.transform.localPosition = Vector3.up * 0.9f + Vector3.back * 10;
            Destroy(gTemp, 2);
            }
            catch (System.NullReferenceException) { }
            int ban = getTimerCode[td.num].y / 2;
            for (int i = -ban; i < getTimerCode[td.num].y - ban; ++i)
            {
                for (int j = -ban; j < getTimerCode[td.num].y - ban; ++j)
                {
                    if (Mapm.getTileData(td.m.monsterLocatePos2.x + i, td.m.monsterLocatePos2.y + j) != MapManager.TileCheck.Block)
                    {
                        try
                        {
                            Transform tTemp = Mapm.cMap[td.m.monsterLocatePos2.x + i, td.m.monsterLocatePos2.y + j].tileTransform.GetChild(0);
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
                        catch (System.NullReferenceException)
                        {

                        }
                        catch (System.IndexOutOfRangeException)
                        {

                        }
                    }
                }
            }
            Mapm.shildCheck();
        }
    }
    public void BombIfOverKill(object m)    // 피해량 초과시 폭발
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;

        Player p = Mapm.player;
        int d = p.GetPlayerDmg(td.ts.z);
        if (td.m.hp > d) return;    // 피해량 초과 체크
        d = d - td.m.hp;    // 초과량
        try { 
        GameObject gTemp = Instantiate(bombEffect, Mapm.cMap[td.ts.x, td.ts.y].tileTransform);
        gTemp.transform.localPosition = Vector3.up * 0.9f + Vector3.back * 10;
        Destroy(gTemp, 2);
        }
        catch (System.NullReferenceException) { }
        //Mapm.atkSoundPlayer(td.ts.z + 2);

        int intState = p.getState(2);
        int ban = getTimerCode[td.num].y / 2;
        for (int i = -ban; i < getTimerCode[td.num].y - ban; ++i)
        {
            for (int j = -ban; j < getTimerCode[td.num].y - ban; ++j)
            {
                if (Mapm.getTileData(td.ts.x + i, td.ts.y + j) != MapManager.TileCheck.Block)
                {
                    try
                    {
                        Transform tTemp = Mapm.cMap[td.ts.x + i, td.ts.y + j].tileTransform.GetChild(0);
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
                    catch (System.NullReferenceException)
                    {

                    }
                    catch (System.IndexOutOfRangeException)
                    {

                    }
                }
            }
        }
        Mapm.shildCheck();
    }
    public void BuffByHightestState(object m)   // 가장높은 스탯에 따라 버프 부여
    {
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;
        int val = 1;
        if (p.getState(0) < p.getState(1))
        {
            val = 2;
            if (p.getState(1) < p.getState(2)) val = 3;
        }
        else if (p.getState(0) < p.getState(2)) val = 3;
        // getTimerCode[td.num].x : 지속시간
        // getTimerCode[td.num].z : 올릴 기본수치
        for (int j = 0; j < Mapm.buffList.Count; ++j)
        {
            if (Mapm.buffList[j].Bfcode == code)
            {
                if (Mapm.buffList[j].count < getTimerCode[td.num].x)
                {
                    Mapm.buffList[j].count = getTimerCode[td.num].x;
                    Mapm.buffList[j].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mapm.buffList[j].count.ToString();
                    return;
                }
            }
        }
        int result = (int)baseDmg[Mapm.skillLev[td.ts.z]] + getTimerCode[td.num].z;
        switch (val)
        {
            case 1:
                Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, 0, 0, result);
                break;
            case 2:
                Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, 0, result, 0);
                break;
            case 3:
                Mapm.getBuff(0, 0, 0, getTimerCode[td.num].x, skillIcon, code, result, 0, 0);
                break;
        }
    }
    public void GetKey(object m)    // 베이스 데미지 확률로 열쇠를 획득한다
    {
        // getTimerCode[td.num].x : 시행 횟수
        // getTimerCode[td.num].y : 성공시 얻는 갯수
        TriggerData td = (TriggerData)m;
        MapManager Mapm = MapManager.Instans;
        Player p = Mapm.player;

        for(int i = 0;i<getTimerCode[td.num].x; ++i)
        {
            if(baseDmg[Mapm.skillLev[td.ts.z]]> Random.Range(0, 100))
            {
                Mapm.keyCountText.text = (++Mapm.keyCount).ToString();
            }
        }
    }
    /*public static RuntimeAnimatorController GetEffectiveController(Animator animator)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        AnimatorOverrideController overrideController = controller as AnimatorOverrideController;
        while (overrideController != null)
        {
            controller = overrideController.runtimeAnimatorController;
            overrideController = controller as AnimatorOver rideController;
        }

        return controller;
    }
    public static void OverrideAnimationClip(Animator anim, string name, AnimationClip clip)
    {
        AnimatorOverrideController overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = GetEffectiveController(anim);
        overrideController[name] = clip;
        anim.runtimeAnimatorController = overrideController;
        anim.runtimeAnimatorController.name = "OverrideRunTimeController";
        Debug.Log(name + ":"+overrideController[name] + "/" + clip.name);
    }*/
    public static void OverrideAnimationClip(Animator anim, AnimatorOverrideController aoc, AnimationClip clip0
        , AnimationClip clip1, AnimationClip clip2, AnimationClip clip3)
    {
        //AnimatorOverrideController aoc = new AnimatorOverrideController(anim.runtimeAnimatorController);
        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in aoc.animationClips)
        {
            switch (a.name)
            {
                case "NomalAtk0":
                    if(clip0 != null)
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip0));
                    break;
                case "NomalAtk1":
                    if (clip1 != null)
                        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip1));
                    break;
                case "NomalAtk2":
                    if (clip2 != null)
                        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip2));
                    break;
                case "NomalAtk3":
                    if (clip3 != null)
                        anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip3));
                    break;
            }
        }
        aoc.ApplyOverrides(anims);
        anim.runtimeAnimatorController = aoc;
    }
    //[System.Serializable]
    //[ContextMenu("getRandValue")]
}
