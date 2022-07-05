using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AtkKind = Skill.AtkKind;

public class Player : MonoBehaviour
{
    public MapManager Mapm;
    public MonsterManager Monm;
    public Transform Img;
    public Animator anim;

    public int mainState;   // 주스탯
    public int[] myState;   // 스탯 : 힘(Strenght), 민첩(Dexterity), 지능(Intelligence)
    public int[] myAddState;
    public int[] psvState;  // 기본 스탯
    public Skill basicSkill;
    //0은 무조건 평타
    public Skill[] mySkillSet;
    public int skillPoint;
    public int keyGetPercentage;
    public int overHealHp = 0;
    public ItemData[] myItems = new ItemData[6];
    [System.Serializable]
    public class ItemData
    {
        public Item item;
        public int lev;
        public int counter;
        public ItemData()
        {
            //item = GameManager.Instans.noneItem;
            lev = 0;
            counter = 0;
        }
    }
    private void Start()
    {
        Mapm = MapManager.Instans;
        Monm = MonsterManager.instance;
        dotDmg = new List<JangPanData>();
        keyGetPercentage = 20;
    }

    public int[] TaritDmgDefLife;
    public int[] TaritSpecState;
    public void PlayerSetting(Character c)
    {
        maxLife = c.life;
        for (int i = 0; i < 6; ++i)
        {
            myItems[i] = new ItemData();
            myItems[i].item = GameManager.Instans.noneItem;
            if(i < 2)
            {
                switch (Random.Range(0, 5))
                {
                    case 0:
                        myItems[i].item = Mapm.GM.nomalItems1[Random.Range(0, Mapm.GM.nomalItems1.Length)];
                        break;
                    case 1:
                        myItems[i].item = Mapm.GM.nomalItems2[Random.Range(0, Mapm.GM.nomalItems2.Length)];
                        break;
                    case 2:
                        myItems[i].item = Mapm.GM.nomalItems3[Random.Range(0, Mapm.GM.nomalItems3.Length)];
                        break;
                    case 3:
                        myItems[i].item = Mapm.GM.nomalItems4[Random.Range(0, Mapm.GM.nomalItems4.Length)];
                        break;
                    default:
                        myItems[i].item = Mapm.GM.nomalItems5[Random.Range(0, Mapm.GM.nomalItems5.Length)];
                        break;
                }
                myItems[i].lev = 1;
            }
        }
        basicSkill = c.basicSkill;
        int maxRange = 0;
        for (int i = 0; i < 4; ++i)
        {
            mySkillSet[i] = (GameManager.Instans.selectSkillSet[i] == null) ? c.skillSetting[i] : GameManager.Instans.selectSkillSet[i];
            if (maxRange < mySkillSet[i].atkRange.Length) maxRange = mySkillSet[i].atkRange.Length;
            Mapm.ImpactAnim[2 + i] = mySkillSet[i].effect;
            Mapm.atkSound[2 + i].clip = mySkillSet[i].sound;
            Mapm.skillBtns[i].transform.Find("ImgBack").GetComponent<Image>().sprite = mySkillSet[i].skillIcon;
            Mapm.CoolTimeCheckCircle[i].sprite = mySkillSet[i].skillIcon;
        }
        Skill.OverrideAnimationClip(anim, c.anim, mySkillSet[0].anim, mySkillSet[1].anim, mySkillSet[2].anim, mySkillSet[3].anim);
        TaritDmgDefLife = new int[6];
        TaritSpecState = new int[3];
        for(int i = 0; i < 3; ++i)
        {
            myState[i] = c.baseState[i];
        }
        c.checkTaritData(this);
        life = checkMyMaxHp();
        PlayerGetExp(1);
        Mapm.checkItemKanUI();
        transform.GetComponent<AudioSource>().clip = c.myBGM;
        transform.GetComponent<AudioSource>().Play();
        SkillTrigger(null, AtkKind.isGameStart);
        GameObject gTemp = Mapm.rangeCheck[0];
        for (int i = 1; i < maxRange; ++i)
        {
            Mapm.rangeCheck.Add(Instantiate(gTemp, Mapm.rangeCheckPenal.transform));
        }
    }
    public void PlayerGetExp(int a)
    {
        exp += a;
        if (exp % 3 == 0) Mapm.makeItemBoxInMap(1);
        if (exp > requestEXP(lev))
        {
            exp = 0;
            levUP(1);
            Mapm.checkPlayerState();
            GameObject gTemp = Instantiate(Mapm.makeEffect[0]);
            gTemp.transform.position = transform.position + Vector3.up * 0.2f;
            Destroy(gTemp, 2);
            SkillTrigger(null, Skill.AtkKind.isLevelUp);
        }
        Mapm.PlayerStateShow();
    }
    public void levUP(int a)
    {
        lev += a;
        skillPoint += a;
        Mapm.expBar.maxValue = requestEXP(lev);
        Mapm.skillBtn.sprite = Mapm.skillBtnImg[1];
        int gab = 15 + TaritSpecState[1];
        life += gab;
        maxLife += gab;
        for(int i = 0; i < TaritSpecState[2]; ++i)
        {
            Mapm.StartCoroutine("getKeyForMonster", transform.position + Vector3.up);
        }
        Mapm.PlayerStateShow();
    }
    public int requestEXP(int lev)
    {
        return 7 * lev;
    }
    Vector3 start;
    public Vector3 end;
    public void PlayerMove(int r, int n)
    {
        Vector2Int p = Mapm.playerMove;
        string sTemp = "Move";
        switch (n)
        {
            case 0:
                sTemp = "Nomal";
                p += Monm.moveVector[r];
                SkillTrigger(null, AtkKind.isNormalAtk,new Vector3Int(p.x,p.y,0));
                break;
            case 1:
            case 2:
            case 3:
                sTemp = "Atk" + n;
                p += Monm.moveVector[r];
                break;
        }
        start = end = transform.position;
        Img.localScale = (r == 0 || r == 2) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        if (n >= 0)
        {
            PlayerSkill(p, r, n);
            Mapm.CoolTime[n] += mySkillSet[n].coolTime;
            Mapm.coolTimeShowMethod();
            SkillTrigger(null, AtkKind.noMoveDetect);
        }
        else
        {
            SkillTrigger(null, AtkKind.moveDetect, new Vector3Int(p.x,p.y));
            end = Mapm.cMap[p.x, p.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        }
        anim.SetTrigger(sTemp);
        SkillTrigger(null, Skill.AtkKind.everyTurn, new Vector3Int(n, 0, 0));
    }
    public void endMove(float point)
    {
        if (point < 1)
        {
            transform.position = Vector3.Lerp(start, end, point);
        }
        else
        {
            transform.position = end;
            MapManager.playerMoveEnd = false;
        }
    }

    public int GetPLayerBaseDmg()
    {
        float dmg = (2 * getState(mainState) + TaritDmgDefLife[3] * 0.2f) * lev + TaritDmgDefLife[0];

        for (int i = 0; i < 6; ++i)
        {
            int lev = myItems[i].lev;
            if (lev > 0) dmg += myItems[i].item.getItemDmg(lev);
        }
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            dmg += Mapm.buffList[i].Batk;
        }
        dmg = Mathf.Max(1, dmg * (1 + Mapm.GM.addDoGamState[0] * 0.005f));
        return (int)dmg ;
    }
    public float[] dmgUp = new float[5];
    public int GetPlayerDmg(int n, bool tCheck = false)
    {
        dmgUp = new float[5];
        if (!tCheck) SkillTrigger(null, AtkKind.dmgUp);
        int dmg;
        switch (n)
        {
            case -1:
                //반격
                SkillTrigger(null, Skill.AtkKind.isCounter,new Vector3Int(Mapm.playerMove.x,Mapm.playerMove.y,0));
                dmg = (int)((1 + GetPLayerBaseDmg() / 2) * (1 + dmgUp[4]));
                break;
            default:
                //평타
                //스킬
                dmg = (int)(mySkillSet[n].baseDmg[Mapm.skillLev[n]] * lev + mySkillSet[n].dmg[Mapm.skillLev[n]] * GetPLayerBaseDmg() * (1 + dmgUp[n]));
                break;
        }
        return dmg;
    }
    public int getState(int n)
    {
        int state = myState[n] + myAddState[n] +psvState[n];

        for (int i = 0; i < 6; ++i)
        {
            int lev = myItems[i].lev;
            if (lev > 0) state += myItems[i].item.getState[n];
        }
        return Mathf.Max(0, state);
    }
    public int getMyDefence()
    {
        float def = (getState(1) * 2  + TaritDmgDefLife[4] * 0.2f) * lev + TaritDmgDefLife[1];
        for (int i = 0; i < 6; ++i)
        {
            int lev = myItems[i].lev;
            if (lev > 0) def += myItems[i].item.getItemDef(lev);
        }
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            def += Mapm.buffList[i].Bdef;
        }
        if(def > 0) def = def *(1 + Mapm.GM.addDoGamState[1] * 0.005f);
        return (int)def;
    }
    public int checkMyMaxHp()
    {
        float ml = maxLife + TaritDmgDefLife[2];
        for (int i = 0; i < 6; ++i)
        {
            int lev = myItems[i].lev;
            if (lev > 0) ml += myItems[i].item.getItemHp(lev);
        }
        ml += (getState(0) * 5 + TaritDmgDefLife[5] * 0.2f) * lev;

        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            ml += Mapm.buffList[i].Bhp;
        }
        return (int)Mathf.Max(5, ml);
    }
    #region 트리거채크
    public void SkillTrigger(Monster m, AtkKind a, Vector3Int Data = new Vector3Int())
    {
        if(a == AtkKind.killEnemy)
        {
            if(TaritSpecState[0] > 0) PlayerGetHeal(TaritSpecState[0]);
        }
        for (int i = -1; i < 4; ++i)
        {
            Skill sk;
            if (i < 0)
            {
                sk = basicSkill;
            }
            else
            {
                sk = mySkillSet[i];
            }
            for (int j = 0; j < sk.kind.Length; ++j)
            {
                if (sk.kind[j] == a)
                {
                    sk.skillEvent[j].Invoke(new Skill.TriggerData(Data, m,null,j));
                }
            }
        }
        for (int i = 0; i < 6; ++i)
        {
            int lev = myItems[i].lev;
            if (lev <= 0) continue;
            Item id = myItems[i].item;
            for (int j = 0; j < id.myTrigger.Length; ++j)
            {
                if (id.myTrigger[j] == a)
                {
                    id.itemEvent[j].Invoke(new Skill.TriggerData(new Vector3Int(lev, j, i), m, null, Data.x));
                }
            }
        }

    }
    public List<JangPanData> dotDmg;
    public void checkDotDmg()
    {
        //number, bp, dmg, count, effect(transform)
        int intState = getState(2);
        for (int i = dotDmg.Count - 1; i >= 0; --i)
        {
            JangPanData jpd = dotDmg[i];
            try
            {
                MapManager.TileCheck tc = Mapm.getTileData(jpd.bp);
                if (tc == MapManager.TileCheck.Enemy)
                {
                    Transform tTemp = Mapm.cMap[jpd.bp.x, jpd.bp.y].tileTransform.GetChild(0);
                    if (tTemp.childCount > 0)
                    {
                        tTemp.GetChild(0).GetComponent<Monster>().APuDa(jpd.dmg);
                    }
                }
            }
            catch (System.NullReferenceException)
            {

            }
            catch (System.IndexOutOfRangeException)
            {

            }
            if (--jpd.count < 0)
            {
                dotDmg.RemoveAt(i);
                jpd.effect.gameObject.SetActive(false);
            }
        }
    }

    public void PlayerSkill(Vector2Int bp, int r, int n)
    {
        //r:방향 n:스킬번호 0(평타)
        //0  1
        //2  3
        Skill sTemp = mySkillSet[n];
        for (int i = 0; i < sTemp.kind.Length; ++i)
        {
            switch (sTemp.kind[i])
            {
                case AtkKind.acttive:
                    milleAtk(new SkillData(sTemp, bp, r, n,i));
                    break;
                case AtkKind.nonTarget:
                    nonTargetArrow(new SkillData(sTemp, bp, r, n,i));
                    break;
                case AtkKind.warfAtk:
                    warpAtk(new SkillData(sTemp, bp, r, n,i));
                    break;
                case AtkKind.buffMe:
                    buffMe(new SkillData(sTemp, bp, r, n,i));
                    break;
            }
        }
    }
    public void multipleAtk(Skill sk, Skill.TriggerData td, int count)
    {
        int r = Mapm.saveInput;
        StartCoroutine(multipleAtk(new SkillData(sk, Mapm.playerMove+ Monm.moveVector[r],r, sk.getTimerCode[td.num].y,td.num), count));
    }
    public struct SkillData
    {
        public Skill sTemp;
        public Vector2Int bp;
        public int r;
        public int n;
        public int num;
        public SkillData(Skill s, Vector2Int a, int b, int c,int number)
        {
            sTemp = s;
            bp = a;
            r = b;
            n = c;
            num = number;
        }
    }
    public class JangPanData
    {
        public int number;
        public Vector2Int bp;
        public int dmg;
        public int count;
        public Transform effect;
        public JangPanData(int n, Vector2Int p, int d, int c, Transform tTemp)
        {
            number = n;
            bp = p;
            dmg = d;
            count = c;
            effect = tTemp;
        }
    }
    #endregion
    #region 기술함수
    public void milleAtk(SkillData sData)
    {
        Mapm.atkSoundPlayer(sData.n + 2);
        List<Vector2Int> ap = new List<Vector2Int>();   // ap 어택포인트 : 때림 시작하는곳(여기서 때림이 퍼져나간다)
        for (int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
        {
            // r : 누른 이동키 방향
            // bp : 스킬 시전 방향
            // p : 공격 되는 곳
            // n : 몇번째 스킬
            Vector2Int p = Skill.getRotatePoint((Vector2Int)sData.sTemp.atkPoint[i], sData.r) + sData.bp;
            MapManager.TileCheck tc = Mapm.getTileData(p);
            if(tc == MapManager.TileCheck.Empty || tc == MapManager.TileCheck.Enemy )
            {
                Transform temp = Mapm.effectManageToShow(2 + sData.n);
                temp.parent = Mapm.cMap[p.x, p.y].tileTransform;
                temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                if(tc == MapManager.TileCheck.Enemy) ap.Add(p);
                temp.gameObject.SetActive(true);
                sData.sTemp.skillEvent[sData.num].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, temp, sData.num));
            }
        }
        int d = GetPlayerDmg(sData.n);
        foreach (Vector2Int hi in ap)
        {
            Transform tTemp = Mapm.cMap[hi.x, hi.y].tileTransform.GetChild(0);
            int cc = tTemp.childCount;
            if (cc > 0)
            {
                int counter = sData.sTemp.hit;
                counter += Mapm.skillLev[sData.n] / 2;
                for (int i = 0; i < cc; ++i)
                {
                    if (counter-- < 0) break;
                    try
                    {
                        tTemp.GetChild(0).GetComponent<Monster>().APuDa(d, sData.n);
                        if (sData.n > 0 && Random.Range(0, 10) < getState(2) && shild < getState(2))
                        {
                            shild = Mathf.Clamp(shild + 1, 0, getState(2));
                            Mapm.shildCheck();
                        }
                    }
                    catch (System.NullReferenceException)
                    {

                    }
                }
            }
        }
        
    }
    
    public void nonTargetArrow(SkillData sData)
    {
        //r:방향 n:스킬번호 0(평타)
        StartCoroutine("nonTargetArrowStart", sData);
        if (sData.sTemp.bulletEffect)
        {
            Vector2Int endPoint = Mapm.playerMove;
            for (int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
            {
                Vector2Int p = Skill.getRotatePoint((Vector2Int)sData.sTemp.atkPoint[i], sData.r) + sData.bp;
                MapManager.TileCheck tc = Mapm.getTileData(p);
                endPoint = p;
                if (tc == MapManager.TileCheck.Block || tc == sData.sTemp.tcTrigger)
                {
                    break;
                }
            }
            Transform effect;
            if (sData.sTemp.bulletEffect != null) effect = Instantiate(sData.sTemp.bulletEffect).transform;
            else effect = Instantiate(new GameObject()).transform;
            effect.position = transform.position + new Vector3(0, 0.55f);

            int hab = 0;
            for(int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
            {
                hab += sData.sTemp.atkPoint[i].z;
            }

            StartCoroutine("bulletThrow", new JangPanData(sData.n, endPoint, 0, hab, effect));
            
        }
    }
    IEnumerator nonTargetArrowStart(SkillData sData)
    {
        //bool check = false;
        int i;
        Vector3Int pt;
        Vector2Int p;
        MapManager.TileCheck tc;
        bool blockBomb = sData.sTemp.tcTrigger == MapManager.TileCheck.Block;
        for (i = 0; i < sData.sTemp.atkPoint.Length; ++i)
        {
            pt = sData.sTemp.atkPoint[i];
            p = Skill.getRotatePoint((Vector2Int)pt, sData.r) + sData.bp;
            yield return new WaitForSeconds(pt.z * 0.01f);
            tc = Mapm.getTileData(p);
            if (!blockBomb && tc != MapManager.TileCheck.Block)
            {
                Transform temp = Mapm.effectManageToShow(2 + sData.n);
                temp.parent = Mapm.cMap[p.x, p.y].tileTransform;
                temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                int counter = sData.sTemp.hit;
                counter += Mapm.skillLev[sData.n] / 2;
                try
                {
                    if (tc == sData.sTemp.tcTrigger)
                    {
                        sData.sTemp.skillEvent[0].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, temp,sData.num));
                        Mapm.atkSoundPlayer(sData.n + 2);
                        if (blockBomb || --counter < 0) break;
                    }
                    else
                    {
                        temp.gameObject.SetActive(true);
                        sData.sTemp.skillEvent[1].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, temp,sData.num));
                    }
                }
                catch (System.IndexOutOfRangeException)
                {

                }
            }
            else if (blockBomb)
            {
                Transform temp = Mapm.effectManageToShow(2 + sData.n);
                temp.parent = Mapm.cMap[p.x, p.y].tileTransform;
                temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
                int counter = sData.sTemp.hit;
                counter += Mapm.skillLev[sData.n] / 2;
                try
                {
                    if (tc == sData.sTemp.tcTrigger)
                    {
                        sData.sTemp.skillEvent[0].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, temp,sData.num));
                        Mapm.atkSoundPlayer(sData.n + 2);
                        if (blockBomb || --counter < 0) break;
                    }
                    else
                    {
                        temp.gameObject.SetActive(true);
                        sData.sTemp.skillEvent[1].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, temp,sData.num));
                    }
                }
                catch (System.IndexOutOfRangeException)
                {

                }
            }
            else
            {
                //check = true;
                break;
            }
        }
    }
    public void warpAtk(SkillData sData)
    {
        Vector2Int endPoint = Mapm.playerMove;
        Vector2Int p;
        for (int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
        {
            p = Skill.getRotatePoint((Vector2Int)sData.sTemp.atkPoint[i], sData.r) + sData.bp;
            MapManager.TileCheck tc = Mapm.getTileData(p);
            if (tc == MapManager.TileCheck.Empty || tc == MapManager.TileCheck.EnemyDie)
            {
                endPoint = p;
                break;
            }
        }
        end = Mapm.cMap[endPoint.x, endPoint.y].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
        if (Mapm.playerMove != endPoint)
        {
            Monm.setTranslucentTile(Mapm.playerMove, false);
            Mapm.tileNodeSortSet(Mapm.playerMove, MapManager.TileCheck.Empty);
            Mapm.playerMove = endPoint;
            Monm.setTranslucentTile(endPoint, true);
            Mapm.tileNodeSortSet(endPoint, MapManager.TileCheck.Player);
        }
        transform.position = end;
        if (sData.sTemp.bombEffect != null)
        {
            GameObject gTemp = Instantiate(sData.sTemp.bombEffect, Mapm.cMap[endPoint.x, endPoint.y].tileTransform);
            gTemp.transform.localPosition = Vector3.up * 0.9f + Vector3.back * 10;
            Destroy(gTemp, 2);
        }
        sData.sTemp.skillEvent[sData.num].Invoke(new Skill.TriggerData(new Vector3Int(endPoint.x, endPoint.y, sData.n), null, null, sData.num));
    }
    public void buffMe(SkillData sData)
    {
        Vector2Int p = Mapm.playerMove;
        Mapm.atkSoundPlayer(sData.n + 2);
        Transform temp = Mapm.effectManageToShow(2 + sData.n);
        temp.parent = Mapm.cMap[p.x, p.y].tileTransform;
        temp.localPosition = Vector3.up * 0.85f + Vector3.back * 10;
        temp.gameObject.SetActive(true);
        sData.sTemp.skillEvent[sData.num].Invoke(new Skill.TriggerData(new Vector3Int(p.x, p.y, sData.n), null, null, sData.num));
    }
    #endregion
    public int life;
    public int maxLife;
    public int exp;
    public int maxExp;
    public int lev;
    public int shild;
    public void PlayerAtk(Monster m, int c)
    {
        //c. 0, 반격
        m.APuDa(GetPlayerDmg(-1));
    }
    public bool PlayerGetDmg(Monster m)
    {
        if (shild > 0)
        {
            shild--;
            Mapm.shildCheck();
        }
        else
        {
            int CalDmg = Mathf.Max(1, Mapm.getEnemyDmg(m.monsterTear) - getMyDefence());
            life -= CalDmg;
            Transform temp = Mapm.showDmg(CalDmg,1);
            temp.position = new Vector3(transform.position.x + Random.Range(-0.1f, 0.1f),
                transform.position.y + Random.Range(0.1f, 0.2f) + 1.1f,
                temp.position.z);
            temp.gameObject.SetActive(true);
            temp.GetComponent<Animator>().SetTrigger("Player");
            Mapm.lifeBar.value = life;
            Mapm.lifeText.text = life + "/" + checkMyMaxHp();
        }
        Img.localScale = (transform.position.x - m.transform.position.x > 0) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        //MapManager.Instans.PlayerStateShow();
        if (life <= 0)
        {
            Mapm.PopupUISetting(2);
            return false;
        }
        SkillTrigger(null, Skill.AtkKind.isDamaged);
        return Random.Range(0, 10) < getState(0);
    }
    public void PlayerGetHeal(int a, bool tCheck = false)
    {
        Transform tTemp = Mapm.effectManageToShow(7);
        tTemp.position = transform.position + Vector3.up ;
        tTemp.GetChild(0).GetComponent<ParticleSystem>().maxParticles = 3 + a / 20;
        tTemp.gameObject.SetActive(true);
        int myMaxHp = checkMyMaxHp();
        Transform temp = Mapm.showDmg(a,2);
        temp.position = new Vector3(transform.position.x + Random.Range(-0.1f, 0.1f),
            transform.position.y + Random.Range(0.1f, 0.2f) + 1,
            temp.position.z);
        temp.gameObject.SetActive(true);
        temp.GetComponent<Animator>().SetTrigger("Enemy");
        if (life + a > myMaxHp)
        {
            overHealHp = life + a - myMaxHp;
            SkillTrigger(null, AtkKind.isOverHp,new Vector3Int(Mapm.playerMove.x, Mapm.playerMove.y,0));
            life = myMaxHp;
        }
        else
        {
            overHealHp = 0;
            life += a;
        }
        Mapm.lifeBar.value = life;
        Mapm.lifeText.text = life + "/" + checkMyMaxHp();
        if (!tCheck) SkillTrigger(null, AtkKind.getHeal);
    }
    IEnumerator bulletThrow(JangPanData jp)
    {
        //int number, Vector2Int bp, int dmg, int count, Transform effect;
        Vector3 endPoint = (Vector2)(Mapm.cMap[jp.bp.x, jp.bp.y].tileTransform.position + new Vector3(0, 0.8f));
        Vector3 startPoint = (Vector2)(endPoint - transform.position - new Vector3(0, 0.5f));
        float far = Vector2.SqrMagnitude(startPoint);
        jp.effect.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(startPoint.y, startPoint.x) * Mathf.Rad2Deg + 90);
        startPoint = jp.effect.transform.position;
        float timeCheck = 0;
        float timeGab = 1 / (jp.count * 0.01f);
        while (far > 0.1)
        {
            timeCheck += Time.deltaTime * timeGab;
            jp.effect.transform.position = Vector3.Lerp(startPoint, endPoint, timeCheck);
            far = Vector2.SqrMagnitude(endPoint - jp.effect.transform.transform.position);
            yield return null;
        }
        Destroy(jp.effect.gameObject, 0.03f);
    }
    IEnumerator multipleAtk(SkillData sData,int counter)
    {
        int d = GetPlayerDmg(sData.n);
        int intState = getState(2);
        ++counter;
        int hab = 0;
        for (int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
        {
            hab += sData.sTemp.atkPoint[i].z;
        }
        string animName = sData.sTemp.anim.name;
        for (int i = 0; i < sData.sTemp.atkPoint.Length; ++i)
        {
            if (counter < 1) break;
            Vector2Int p = Skill.getRotatePoint((Vector2Int)sData.sTemp.atkPoint[i], sData.r) + sData.bp;
            MapManager.TileCheck tc = Mapm.getTileData(p);
            if (tc == MapManager.TileCheck.Block) break;
            if (tc == MapManager.TileCheck.Enemy)
            {
                Transform tTemp = Mapm.cMap[p.x, p.y].tileTransform.GetChild(0);
                int cc = tTemp.childCount;
                Monster monster = tTemp.GetChild(0).GetComponent<Monster>();
                while (cc >= 0)
                {

                    anim.SetTrigger(animName);
                    Mapm.atkSoundPlayer(sData.n + 2);
                    yield return new WaitForSeconds(0.2f);
                    try
                    {
                        Transform effect = Instantiate(sData.sTemp.bulletEffect).transform;
                        effect.position = transform.position + new Vector3(0, 0.55f);
                        StartCoroutine("bulletThrow", new JangPanData(sData.n, p, 0, hab, effect));
                    }//StartCoroutine("b", sData.sTemp.getTimerCode[0]);
                    catch (System.NullReferenceException) { }
                    try
                    {
                        monster.APuDa(d, sData.n);
                        if (--counter < 1)
                        {
                            break;
                        }
                        if (monster.hp < 1)
                        {
                            if (--cc == 0) break;
                            monster = tTemp.GetChild(0).GetComponent<Monster>();
                        }
                        if (sData.n > 0 && Random.Range(0, 10) < intState && shild < intState)
                        {
                            shild = Mathf.Clamp(shild + 1, 0, intState);
                            Mapm.shildCheck();
                        }
                    }
                    catch (System.NullReferenceException)
                    {

                    }
                }

            }

        }
        for (int i = 0; i < Mapm.buffList.Count; ++i)
        {
            if (Mapm.buffList[i].Bfcode == sData.sTemp.code)
            {
                Mapm.buffList[i].count=-1;
                Mapm.buffList[i].myUI.parent = Mapm.buffSetInvPenal;
                for (int j = 0; j < 3; ++j)
                {
                    // 버프 꺼진것 탐색용
                    myAddState[j] -= Mapm.buffList[i].addState[j];
                }
                SkillTrigger(null, AtkKind.isBuffOff);
                Mapm.buffList.RemoveAt(i);
                Mapm.checkPlayerState();
                break;
            }
        }

    }
    
    IEnumerator makeChainLight(Skill.TriggerData td)
    {
        Vector2Int start = td.m.monsterLocatePos2;
        Transform tTemp = Mapm.effectManageToShow(8);
        Vector3 sPoint = td.m.transform.position;
        tTemp.position = sPoint;
        tTemp.gameObject.SetActive(true);
        for (int i = 0; i < td.ts.y + 1; ++i)
        {
            Vector2Int end = returnNearEnemy(start);
            if (end == Vector2Int.zero) break;
            Monster target = Mapm.cMap[end.x, end.y].tileTransform.GetChild(0).GetChild(0).GetComponent<Monster>();
            float persent = 0;
            while (persent < 1)
            {
                yield return null;
                persent += Time.deltaTime *8;
                tTemp.position = Vector3.MoveTowards(sPoint, target.transform.position, persent);
            }
            target.APuDa(td.ts.x,td.ts.z, true);
            start = end;
            sPoint = tTemp.position;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.25f);
        tTemp.gameObject.SetActive(false);
    }
    public Vector2Int returnNearEnemy(Vector2Int vi)
    {
        int start = Random.Range(0, 4);
        for (int i = 0; i < 4; ++i)
        {
            Vector2Int vTemp = vi + Monm.moveVector[(i + start) % 4];
            MapManager.TileCheck tc = Mapm.getTileData(vTemp);
            if (tc == MapManager.TileCheck.Enemy) return vTemp;
            if (tc == MapManager.TileCheck.EnemyDie)
            {
                if (Mapm.cMap[vTemp.x, vTemp.y].tileTransform.GetChild(0).childCount <= 0)
                {
                    Mapm.tileNodeSortSet(vTemp, MapManager.TileCheck.Empty);
                }
                else
                {
                    Mapm.tileNodeSortSet(vTemp, MapManager.TileCheck.Enemy);
                    return vTemp;
                }
            }
        }
        return Vector2Int.zero;
    }

}
