using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NumSharp;
using AtkKind = Skill.AtkKind;

public class MapManager : MonoBehaviour
{
    public static MapManager Instans;
    public MonsterManager Mm;
    public GameManager GM;
    public TextMeshProUGUI CenterText;
    public float textShowTime;
    public void ShowCenterText(string t)
    {
        CenterText.text = t;
        textShowTime = Time.time + 1;
        CenterText.gameObject.SetActive(true);
    }
    //0 나만 2 추가기회 4 적과함께
    public int TURN;
    public Image TURNcolor;
    public Color[] colorSet;
    public Transform cameraPoint;

    //0. 레벨업
    public GameObject[] makeEffect;
    public GameObject[] ImpactAnim;
    public List<Transform>[] effectList;
    //0.적공격 1.반격 2~5 스킬 6 열쇠 7 힐팩머금 8라이트닝 9불길 10폭발
    public int[] effectCount;
    public Transform effectManageToShow(int a)
    {
        effectCount[a] %= 27;
        effectCount[a]++;
        Transform temp;
        if (effectList[a].Count < effectCount[a])
        {
            if (ImpactAnim[a] != null)
            {
                temp = Instantiate(ImpactAnim[a], Vector2.zero, Quaternion.identity).transform;
            }
            else
            {
                temp = Instantiate(new GameObject(), Vector2.zero, Quaternion.identity).transform;
            }
            effectList[a].Add(temp);
        }
        else
        {
            temp = effectList[a][effectCount[a] - 1];
        }
        temp.gameObject.SetActive(false);
        return temp;
    }
    IEnumerator getKeyForMonster(Vector3 sPoint)
    {
        Transform tTemp = effectManageToShow(6);
        tTemp.position = sPoint;
        tTemp.gameObject.SetActive(true);
        Vector3 vTemp = Vector3.zero;
        float maxGab = Random.Range(680, 750);
        while(vTemp.y < maxGab)
        {
            yield return null;
            tTemp.eulerAngles = vTemp;
            vTemp.y += Time.deltaTime * maxGab * 1.3f;
        }
        tTemp.eulerAngles = Vector3.zero;
        float far = Vector2.SqrMagnitude(sPoint - player.transform.position - Vector3.up * 0.8f);
        while(far > 0.1f)
        {
            tTemp.position = Vector3.Lerp(tTemp.position, player.transform.position + Vector3.up * 0.8f, Time.deltaTime *9);
            far = Vector2.SqrMagnitude(tTemp.position - player.transform.position - Vector3.up * 0.8f);
            yield return null;
        }
        GM.btnSound[0].Play();
        tTemp.gameObject.SetActive(false);
        keyCountText.text = (++keyCount).ToString();
    }

    public GameObject dmgText;
    public int dmgTextCount;
    public List<Transform> dmgTextList;
    public Color[] dmgTextColor;

    public Transform showDmg(int a,int type)
    {
        dmgTextCount %= 33;
        dmgTextCount++;
        Transform temp;
        if (dmgTextList.Count < dmgTextCount)
        {
            temp = Instantiate(dmgText, Vector3.back * dmgTextCount, Quaternion.identity).transform;
            dmgTextList.Add(temp);
        }
        else
        {
            temp = dmgTextList[dmgTextCount - 1];
        }
        temp.GetChild(0).GetComponent<TextMeshPro>().text = a.ToString();
        temp.GetChild(0).GetComponent<TextMeshPro>().color = dmgTextColor[type];
        temp.gameObject.SetActive(false);
        return temp;
    }
    public Transform buffSetPenal;
    public Transform buffSetInvPenal;
    public GameObject buffUIset;
    public Transform buffUITransform()
    {
        //자버프, 유익한버프, 해로운버프
        //걸린순
        Transform tTemp;
        if (buffSetInvPenal.childCount > 0)
        {
            tTemp = buffSetInvPenal.GetChild(0);
            tTemp.parent = buffSetPenal;
        }
        else
        {
            tTemp = Instantiate(buffUIset, buffSetPenal).transform;
        }
        return tTemp;
    }
    public List<buffInfo> buffList;
    public void getBuff(int s, int d, int i, int time, Sprite img, string code = "", int atk = 0, int def = 0, int addHp = 0, int skillNum = -1, float dmgUp = 0f)
    {
        buffInfo bi = buffList.Find(item => item.Bfcode == code);
        if(bi != null)
        {
            if(time < 0)
            {
                bi.count -= time;
                bi.newBuff = true;
                bi.myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = bi.count.ToString();
            }
            else
            {
                if(bi.count < time)
                {
                    bi.count = time;
                    bi.newBuff = true;
                    bi.myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = bi.count.ToString();
                }
                else
                {
                    int[] addState = new int[3] { s, d, i };
                    for (int k = 0; k < 3; ++k)
                    {
                        player.myAddState[k] += addState[k];
                        bi.addState[k] += addState[k];
                    }
                    bi.Batk += atk;
                    bi.Bdef += def;
                    bi.Bhp += addHp;
                    checkPlayerState();
                }
            }

            return;
        }



        Transform ui = buffUITransform();
        var bf = new buffInfo
        {
            count = Mathf.Abs(time),
            addState = new int[3] { s, d, i },
            myUI = ui,
            Bfcode = code,
            Batk = atk,
            Bdef = def,
            Bhp = addHp,
            newBuff = true
        };
        if (skillNum != -1)
        {
            bf.addDmg[skillNum] = dmgUp;
            player.dmgUp[skillNum] += dmgUp;
        }
        for (int j = 0; j < 3; ++j)
        {
            player.myAddState[j] += bf.addState[j];
        }
        player.life += addHp;
        ui.GetChild(0).GetComponent<Image>().sprite = img;
        ui.GetChild(1).GetComponent<TextMeshProUGUI>().text = bf.count.ToString();
        buffList.Add(bf);
        player.SkillTrigger(null, Skill.AtkKind.isBuffStart);
        checkPlayerState();
    }
    public void checkBuffList()
    {
        for (int i = buffList.Count - 1; i >= 0; --i)
        {
            if (buffList[i].newBuff)
            {
                buffList[i].newBuff = false;
                continue;
            } 
            if (--buffList[i].count < 0)
            {
                buffList[i].myUI.parent = buffSetInvPenal;
                for (int j = 0; j < 3; ++j)
                {
                    // 버프 꺼진것 탐색용
                    player.myAddState[j] -= buffList[i].addState[j];
                }
                for (int j = 0; j < 4; ++j)
                {
                    player.dmgUp[j] -= buffList[i].addDmg[j];
                }
                player.SkillTrigger(null, AtkKind.isBuffOff,new Vector3Int(playerMove.x,playerMove.y));
                buffList.RemoveAt(i);
                checkPlayerState();
            }
            else
            {
                buffList[i].myUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = buffList[i].count.ToString();
                player.SkillTrigger(null, Skill.AtkKind.isBuffState);
            }
        }
    }
    public class buffInfo
    {
        public int count;
        public int[] addState = new int[3] { 0, 0, 0 };
        public float[] addDmg = new float[4] { 0f, 0f, 0f, 0f };
        public Transform myUI;
        public string Bfcode;
        public int Bhp = 0;
        public int Bdef = 0;
        public int Batk = 0;
        public bool newBuff = true;
    }
    public Transform showPopupUI;
    public void PopupUISetting(int a)
    {
        if (showPopupUI.GetChild(2).gameObject.activeSelf) return;
        showPopupUI.gameObject.SetActive(a > 0);
        if(a > 0)
        {
            switch (a)
            {
                //내 아이템, 스텟창
                case 1:
                    showItemInfoInPopui(-1);
                    showItemInfoInPopui(selectItem);
                    PopupUISkillInfoSetting();
                    PopupUiPlayerStateInfoSetting();
                    checkItemKanInPopupUI();
                    break;
                case 2:
                    EndingGame();
                    break;
            }
            GM.btnSound[0].Play();
            for(int i = 1; i < showPopupUI.childCount; ++i)
            {
                showPopupUI.GetChild(i).gameObject.SetActive(i == a);
            }
        }
        else
        {
            GM.btnSound[1].Play();
        }
    }
    public Transform skillInfoUI;
    public void PopupUISkillInfoSetting()
    {
        for(int i = -1; i < 4; ++i)
        {
            Transform tTemp = skillInfoUI.GetChild(i + 1);
            if(i< 0)
            {
                //passive
                tTemp.GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().sprite = player.basicSkill.skillIcon;
                tTemp.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.basicSkill.getInfoText(i);
            }
            else
            {
                tTemp = tTemp.GetChild(0).GetChild(1);
                tTemp.GetChild(0).GetComponent<Image>().sprite = player.mySkillSet[i].skillIcon;
                tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.mySkillSet[i].getInfoText(i);

                for (int j = 0; j < 4; ++j)
                {
                    tTemp.GetChild(2 + j).gameObject.SetActive(skillLev[i] > j);
                }
            }
        }
    }
    public Transform playerStatePopupUI;
    public void PopupUiPlayerStateInfoSetting()
    {
        for(int i = 0; i < 6; ++i)
        {
            string sTemp;
            switch (i)
            {
                case 1:
                    sTemp = player.GetPLayerBaseDmg().ToString();
                    break;
                case 3:
                    sTemp = player.getMyDefence().ToString();
                    break;
                case 5:
                    sTemp = player.checkMyMaxHp().ToString();
                    break;
                default:
                    sTemp = player.getState(i / 2).ToString();
                    break;
            }
            playerStatePopupUI.GetChild(i).GetComponent<TextMeshProUGUI>().text = sTemp;
        }
    }
    public Transform itemKan;
    public void checkItemKanUI()
    {
        for(int i = 0; i < 6; ++i)
        {
            Player.ItemData id = player.myItems[i];
            Transform tTemp = itemKan.GetChild(i);
            tTemp.GetChild(0).GetComponent<Image>().sprite = id.item.Img;
            tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = (id.lev > 0) ? id.lev.ToString() : string.Empty;
        }
    }
    public Transform itemKanUI;
    public int selectItem;
    public void checkItemKanInPopupUI()
    {
        for (int i = 0; i < 6; ++i)
        {
            Player.ItemData id = player.myItems[i];
            Transform tTemp = itemKanUI.GetChild(i);
            tTemp.GetChild(0).GetComponent<Image>().sprite = id.item.Img;
            tTemp.GetChild(0).GetComponent<Outline>().effectColor = (id.item.tear < 0) ? Color.white : GM.tearColor[id.item.tear];
            tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = (id.lev > 0) ? id.lev.ToString() : string.Empty;
            tTemp.GetChild(2).gameObject.SetActive(i == selectItem);
        }
    }
    public Player.ItemData prbGetItem;
    public Transform[] itemInfoKanUI;
    public void showItemInfoInPopui(int order)
    {
        Player.ItemData id;
        Transform tTemp;
        if (order <0)
        {
            id = prbGetItem;
            tTemp = itemInfoKanUI[1];
        }
        else
        {
            id = player.myItems[order];
            tTemp = itemInfoKanUI[0];
        }
        tTemp.GetChild(0).GetComponent<Image>().sprite = id.item.Img;
        tTemp.GetChild(0).GetComponent<Outline>().effectColor = (id.item.tear < 0) ? Color.white : GM.tearColor[id.item.tear];

        tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = (id.lev >0) ? id.lev.ToString() : string.Empty;
        tTemp.GetChild(2).GetComponent<TextMeshProUGUI>().text = (id.lev > 0) ?
              (((id.item.getItemDmg(id.lev) == 0) ? string.Empty : "공:" + id.item.getItemDmg(id.lev) +" ")
              + ((id.item.getItemDef(id.lev) == 0) ? string.Empty : "방:" + id.item.getItemDef(id.lev) + " ")
              + ((id.item.getItemHp(id.lev) == 0) ? string.Empty : "체:" + id.item.getItemHp(id.lev)) + " ")
            : (order < 0) ? "획득 장비": "비어있음";
        tTemp.GetChild(3).GetComponent<TextMeshProUGUI>().text = (id.lev > 0) ?
            id.item.infoText
            : (order < 0) ? "열쇠를 이용하여\n상자로부터 장비를\n획득하여야 합니다" : "착용된 장비가\n없습니다";
    }
    public void changeItemSelect(int a)
    {
        if (selectItem == a) return;
        itemKanUI.GetChild(selectItem).GetChild(2).gameObject.SetActive(false);
        selectItem = a;
        itemKanUI.GetChild(selectItem).GetChild(2).gameObject.SetActive(true);
        showItemInfoInPopui(selectItem);
        GM.btnSound[0].Play();
    }
    public void playerGetRandomItem(int h, int r)
    {
        int gacha = Random.Range(0, 100);
        if (gacha < h)
        {
            prbGetItem.item = GM.hiddenItem[Random.Range(0, GM.hiddenItem.Length)];
        }
        else if (gacha < r)
        {
            prbGetItem.item = GM.rareItem[Random.Range(0, GM.rareItem.Length)];
        }
        else
        {
            switch(Random.Range(0, 5))
            {
                case 0:
                    prbGetItem.item = GM.nomalItems1[Random.Range(0, GM.nomalItems1.Length)];
                    break;
                case 1:
                    prbGetItem.item = GM.nomalItems2[Random.Range(0, GM.nomalItems2.Length)];
                    break;
                case 2:
                    prbGetItem.item = GM.nomalItems3[Random.Range(0, GM.nomalItems3.Length)];
                    break;
                case 3:
                    prbGetItem.item = GM.nomalItems4[Random.Range(0, GM.nomalItems4.Length)];
                    break;
                default:
                    prbGetItem.item = GM.nomalItems5[Random.Range(0, GM.nomalItems5.Length)];
                    break;
            }
        }
        prbGetItem.lev = player.lev;
        Transform tTemp = Instantiate(CountItem.GetChild(0), CountItem);
        tTemp.GetComponent<Image>().sprite = prbGetItem.item.Img;
        tTemp.GetComponent<Outline>().effectColor = (prbGetItem.item.tear < 0) ? Color.white: GM.tearColor[prbGetItem.item.tear];
    }
    public void checkGetItem(Vector2Int vTemp)
    {
        if (keyCount <= 0) return;
        if (vTemp.x < 0) vTemp = playerMove;
        TileCheck tc = getTileData(vTemp);
        if (tc == TileCheck.Block) return;
        int state = cMap[vTemp.x, vTemp.y].tileState;
        if (tileStateCheck(state, TileState.Item))
        {
            Transform item = cMap[vTemp.x, vTemp.y].tileTransform.Find("item");
            if (item == null) return;
            keyCountText.text = (--keyCount).ToString();
            player.SkillTrigger(null, Skill.AtkKind.isBoxOpen);
            int itemGab = getTileStateGab(state, "itemBox");
            //0힐(소), 1힐(대), 2하급아이템 3상급아이템 4최상급
            switch (itemGab)
            {
                case 0:
                case 1:
                    player.PlayerGetHeal(player.checkMyMaxHp() / 5);
                    break;
                default:
                    playerGetRandomItem(0, 30);
                    break;
                case 3:
                    playerGetRandomItem(30, 80);
                    break;
                case 4:
                    playerGetRandomItem(55, 99);
                    break;
            }
            if(itemGab >= 2)
            {
                //prbGetItem.lev = player.lev;
                prbGetItem.item.getCount++;
                PopupUISetting(1);
            }
            cMap[vTemp.x, vTemp.y].tileState = setTileStateGab(state,0, "itemBox");
            cMap[vTemp.x, vTemp.y].tileState &= ~tileStateGab(TileState.Item);
            item.parent = maptiles;
            item.gameObject.SetActive(false);
        }
    }
    public void ChangePrbItemAndInventory()
    {
        if (prbGetItem.lev <= 0) return;
        player.myItems[selectItem].item = prbGetItem.item;
        player.myItems[selectItem].lev = prbGetItem.lev;
        prbGetItem.item = GM.noneItem;
        prbGetItem.lev = 0;
        player.SkillTrigger(null, Skill.AtkKind.isChangedItem, new Vector3Int(player.myItems[selectItem].lev, 0,0));
        showItemInfoInPopui(-1);
        showItemInfoInPopui(selectItem);
        PopupUISkillInfoSetting();
        PopupUiPlayerStateInfoSetting();
        checkItemKanInPopupUI();
        checkItemKanUI();
        checkPlayerState();
        lifeBar.maxValue = player.checkMyMaxHp();
        PlayerStateShow();
    }
    public int monsterRegenTimer;
    public void checkDeathMonster()
    {
        if (Mm.MonsterTomb.Count > 0)
        {
            if (monsterRegenTimer-- > 0) return;
            else monsterRegenTimer = Mm.monsterList.Count / 5;
            for (int i = Mm.MonsterTomb.Count - 1; i >= 0 ; --i)
            {
                if (Random.Range(0, monsterRegenTimer/4 + 1) > 0) continue;
                Monster mTemp = Mm.MonsterTomb[i];
                Mm.MonsterTomb.Remove(mTemp);
                // m.transform.parent = Mm.cMap[ran.x, ran.y].tileTransform.GetChild(0);
                mTemp.MonsterBye();
                if (Mm.monsterList.Count > 15)
                {
                    if (mTemp.monsterTear < maxTear)
                    {
                        Mm.PlaceMonster(mTemp, mTemp.monsterTear + 1);
                    }
                    else
                    {
                        if (++Mm.killPoint > 50 * maxTear)
                        {
                            maxTear++;
                            checkEnemyState();
                            enemyStateText[0].transform.Find("Image").GetComponent<Image>().sprite = GM.mapData.monsterData[(maxTear * 2 - 1) % GM.mapData.monsterData.Length].monsterImage[0];
                        }
                        Mm.PlaceMonster(mTemp, maxTear);
                    }
                    if (Random.Range(0, monsterRegenTimer/2 + 1) == 0) Mm.PlaceMonster(1);
                }
                else
                {
                    Mm.PlaceMonster(1);
                    Mm.PlaceMonster(mTemp, 0);
                }
            }
        }
    }

    public AudioSource[] atkSound;
    public AudioSource[] TurnSound;
    public void atkSoundPlayer(int a)
    {
        atkSound[a].Play();
    }
    public int saveInput;
    public int savePrbBtn = -1;
    public Image[] CoolTimeCheckCircle;
    public TextMeshProUGUI[] CoolTimeCheckText;
    public int[] CoolTime;
    public void coolTimeShowMethod()
    {
        for(int i = 0; i < 4; ++i)
        {
            int time = player.mySkillSet[i].coolTime;
            CoolTimeCheckCircle[i].fillAmount = (float)(time - CoolTime[i]) / time;
            if (i > 0) CoolTimeCheckText[i - 1].text = (CoolTime[i] > 0) ? CoolTime[i].ToString() : "";
        }
    }
    public GameObject rangeCheckPenal;
    public List<GameObject> rangeCheck;
    public bool rStartCheck;
    public void RangeCheckStart(int a)
    {
        rStartCheck = true;
        if (selectActionBtn < 0) return;
        rangeCheckPenal.SetActive(true);
        Skill sTemp = player.mySkillSet[selectActionBtn];
        int i = 0;
        for (; i < sTemp.atkRange.Length; ++i)
        {
            try
            {
                Vector2Int p = Skill.getRotatePoint(sTemp.atkRange[i], a) + Mm.moveVector[a] + playerMove;
                rangeCheck[i].transform.position = cMap[p.x, p.y].tileTransform.position;
                rangeCheck[i].SetActive(true);
            }
            catch (System.IndexOutOfRangeException)
            {
                rangeCheck[i].SetActive(false);
            }
        }
        for(; i < rangeCheck.Count; ++i)
        {
            rangeCheck[i].SetActive(false);
        }
    }

    public void outOfMovePenal(bool a)
    {
        rStartCheck = a;
    }
    public static WaitForSeconds fixWs = new WaitForSeconds(0.0333f);
    public float nextTurnStart;
    public Animator turnAnim;
    public Image turnImg;
    public void updateSetting()
    {
        if(rangeCheckPenal.activeSelf && !rStartCheck)
        {
            rangeCheckPenal.SetActive(false);
        }
        Vector3 vTemp = Vector3.Lerp(cameraPoint.position
            , player.transform.position + Vector3.up * 0.7f
            , 0.025f);
        vTemp.z = -90;
        cameraPoint.position = vTemp; ;
        if (player.life <= 0 && !showPopupUI.GetChild(2).gameObject.activeSelf)
        {
            PopupUISetting(2);
        }
    }
    IEnumerator updateMap()
    {
        while (true)
        {
            if (CenterText.gameObject.activeSelf && Time.time > textShowTime)
            {
                CenterText.gameObject.SetActive(false);
            }
            while(TURN != 9 && nextTurnStart > Time.time)
            {
                updateSetting();
                yield return null;
            }
            updateSetting();
            if (TURN == 0 || TURN == 2 || TURN == 3)
            {
                //0 나만 2 적도 3 추가
                //나만 행동
                int input = findKey();
                if (input >= 0)
                {
                    saveInput = input;
                    savePrbBtn = selectActionBtn;
                    Vector2Int saveP = playerMove + Mm.moveVector[saveInput];
                    TileCheck tc = getTileData(saveP);
                    if (savePrbBtn < 0 && !(tc == TileCheck.Empty))
                    {
                        //Debug.Log(tc);
                        GM.btnSound[3].Play();
                    }
                    else
                    {
                        if(savePrbBtn < 0)
                        {
                            Mm.setTranslucentTile(playerMove, false);
                            tileNodeSortSet(playerMove, TileCheck.Empty);
                            playerMove = saveP;
                            Mm.setTranslucentTile(saveP, true);
                            tileNodeSortSet(saveP, TileCheck.Player);
                        }
                        playerMoveEnd = true;
                        turnImg.color = Color.white;
                        radioBtn(-1);
                        TURN = (TURN != 3) ? nextTurn(TURN) : 6;
                        //turnImg.color = colorSet[1];
                        if (TURN != 8)
                        {
                            turnAnim.SetTrigger("Move");
                            nextTurnStart = Time.time + 0.5f;
                            player.PlayerMove(saveInput, savePrbBtn);
                        }
                        else
                        {
                            turnAnim.SetTrigger("Emove");
                            nextTurnStart = Time.time + 0.8f;
                            //적만행동
                            TURN = 9;
                            Mm.moveMonsters();
                        }
                    }
                }
            }
            else if (TURN == 6)
            {
                if (!playerMoveEnd)
                {
                    checkDeathMonster();
                    checkGetItem(playerMove);
                    TURN = 2;
                    TurnSound[1].Play();
                    TURNcolor.color = colorSet[1];
                }
                else
                {
                    //timerGab += 0.1f * (1.0f - timerGab / (0.5f +timerGab));
                    playerMoveEnd = false;
                    checkDeathMonster();
                    checkGetItem(playerMove);
                    TURN = 2;
                    TurnSound[1].Play();
                    TURNcolor.color = colorSet[1];
                }
                //추가 기회 획득
            }
            else
            {
                switch (TURN)
                {
                    case -1:
                        if (!playerMoveEnd)
                        {
                            //checkDeathMonster();
                            TURN = 13;
                        }
                        else
                        {
                            //timerGab += 0.01f * (1.0f - timerGab * 0.1f / (0.5f + timerGab));
                            playerMoveEnd = false;
                            TURN = 13;
                        }
                        break;
                    case 1:
                        //if ((!playerMoveEnd && !MonsterManager.monstSetting) || timeLimit < 0)
                        if (!playerMoveEnd)
                        {
                            checkDeathMonster();
                            checkGetItem(playerMove);
                            if (Random.Range(0, 10) < player.myState[1])
                            {
                                //추가기회
                                TURN = 3;
                                TurnSound[2].Play();
                                TURNcolor.color = colorSet[2];
                                player.SkillTrigger(null, AtkKind.isAddtionalTurn);
                            }
                            else
                            {
                                TURN = 2;
                                TurnSound[1].Play();
                                TURNcolor.color = colorSet[1];
                            }
                        }
                        else
                        {
                            //timerGab += 0.01f * (1.0f - timerGab / (0.5f + timerGab));
                            playerMoveEnd = false;
                        }
                        break;
                    case 9:
                        //적 행동
                        if (Monster.setCounter <= 0 || nextTurnStart <= Time.time)
                        {
                            TURN = -1;
                            playerMoveEnd = true;
                            player.PlayerMove(saveInput, savePrbBtn);
                        }
                        break;
                    case 13:
                        //적 공격
                        if (Mm.atkEnemy.Count > 0)
                        {
                            try
                            {
                                if (!monsterMoveEnd)
                                {
                                    if (Mm.atkEnemy[0].hp > 0 && Mm.atkEnemy[0].monsterLocatePos == Mm.atkEnemy[0].monsterLocatePos2)
                                    {
                                        monsterMoveEnd = true;
                                        Mm.atkEnemy[0].StartCoroutine("enemyAtkToPlayer");
                                        nextTurnStart = Time.time + 0.1f;
                                    }
                                    else
                                    {
                                        Mm.atkEnemy.RemoveAt(0);
                                        monsterMoveEnd = false;
                                    }
                                }
                                else
                                {
                                    Mm.atkEnemy.RemoveAt(0);
                                    monsterMoveEnd = false;
                                }
                            }
                            catch (System.NullReferenceException)
                            {
                                Mm.atkEnemy.RemoveAt(0);
                            }
                        }
                        else
                        {
                            checkDeathMonster();
                            checkGetItem(playerMove);
                            TURN = 0;
                            TurnSound[0].Play();
                            TURNcolor.color = colorSet[0];
                        }
                        break;
                }
                //타이밍을 기다리는 중
            }
            yield return fixWs;
        }
    }
    
    public Vector2Int playerMove;

    public Player player;
    public Slider lifeBar;
    public Slider expBar;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI levText;
    public int keyCount;
    public TextMeshProUGUI keyCountText;
    public void PlayerStateShow()
    {
        lifeBar.maxValue = player.checkMyMaxHp();
        lifeBar.value = player.life;
        expBar.value = player.exp;
        lifeText.text = player.life + "/" + player.checkMyMaxHp();
        levText.text = player.lev.ToString();
        keyCountText.text = keyCount.ToString();
    }
    public GameObject[] shildImg;
    public void shildCheck()
    {
        for(int i = 0;i < 10; ++i)
        {
            shildImg[i].SetActive(i < player.shild);
        }
    }
    
    public static bool playerMoveEnd;
    public static bool monsterMoveEnd;
    int selectActionBtn = -1;
    public Animator[] skillBtns;
    public void radioBtn(int a)
    {
        if (checkSkillUp)
        {
            if (a < 0) return;
            if (skillLev[a] >= 4)
            {
                ShowCenterText("최대 단계입니다!");
            }
            else
            {
                //스킬 래벨업
                skillLev[a]++;
                skillLevCheck(a);
                checkSkillUp = false;
                skillBtnChange();
                coolTimeShowMethod();
                GM.btnSound[0].Play();
            }
        }
        else
        {
            if (a >= 0)
            {
                if(player.mySkillSet[a].kind[0] == Skill.AtkKind.atkTrigger)
                {
                    ShowCenterText("사용할 수 없는 기술입니다.");
                    return;
                }
                if(CoolTime[a] > 0)
                {
                    ShowCenterText("준비되지 않았습니다!");
                    return;
                }
            }
            if (selectActionBtn == a)
            {
                selectActionBtn = -1;
            }
            else
            {
                selectActionBtn = a;
            }
            for (int i = 0; i < skillBtns.Length; ++i)
            {
                skillBtns[i].SetBool("Mode", i == selectActionBtn);
            }
        }
    }
    public Image skillBtn;
    public Sprite[] skillBtnImg;
    public bool checkSkillUp;
    public void levelupBtn()
    {
        if(player.skillPoint > 0)
        {
            skillBtnChange(true);
            for (int i = 0; i < 4; ++i)
            {
                CoolTimeCheckCircle[i].fillAmount = (skillLev[i] < 4)?1 : 0;
            }
            checkSkillUp = true;
            if (--player.skillPoint <= 0) skillBtn.sprite = skillBtnImg[0];
        }
        else
        {

        }

    }

    public void skillBtnChange(bool up = false)
    {
        for (int i = 0; i < 4; ++i)
        {
            CoolTimeCheckCircle[i].sprite =(up)?skillBtnImg[2] : player.mySkillSet[i].skillIcon;
        }
    }
    public TextMeshProUGUI[] playerStateText;
    public void checkPlayerState()
    {
        for(int i = 0; i < 3; ++i)
        {
            playerStateText[i].text = player.getState(i).ToString();
        }
    }
    public TextMeshProUGUI[] enemyStateText;
    public int enemyLev;
    public int levCount;
    public Sprite[] enemyLevupBackImg;
    public Image enemyLevupBack;
    public int maxTear;
    public void checkEnemyState()
    {
        enemyStateText[0].text = getEnemyDmg(maxTear *2).ToString();
        enemyStateText[1].text = getEnemyLife(maxTear * 2).ToString();
    }
    public int getEnemyDmg(int tear)
    {
        int dmg = (int)((enemyLev * (0.15f + GM.nanido *0.05f) + 1) * (1 + tear * 0.2f));
        return (tear % 2 == 1) ? (int)(dmg * 1.5f) : dmg;
    }
    public int getEnemyLife(int tear)
    {
        int life = (int)((enemyLev * (0.5f + GM.nanido * 0.2f) + 12) * (1 + tear * 0.3f));
        return (tear%2 == 1) ? life *2 : life ;
    }
    public void checkEnemyLevup()
    {
        levCount++;
        if(levCount > 7)
        {
            levCount = 0;
            enemyLev++;
            checkEnemyState();
        }
        enemyLevupBack.sprite = enemyLevupBackImg[levCount];
    }
    //0평타 1Q 2W 3E
    public int[] skillLev;
    public void skillLevCheck(int a)
    {
        int lev = Mathf.Clamp(skillLev[a], 0, 4);
        skillLev[a] = lev;
        for(int i = 0; i < 4; ++i)
        {
            skillBtns[a].transform.Find(i.ToString()).gameObject.SetActive(i < lev);
        }
    }
    public int nextTurn(int a)
    {
        checkEnemyLevup();
        player.checkDotDmg();
        checkBuffList();
        for (int i = 0; i < 4; ++i)
        {
            if (CoolTime[i] > 0)
                CoolTime[i]--;
        }
        coolTimeShowMethod();
        switch (a)
        {
            case 0:
                //나만 행동
                MonsterManager.monstSetting = true;
                Mm.changeMonstersWeight();
                return 1;
            case 2:
                //적도 행동
                Mm.StartCoroutine("mobSetting");
                return 8;
            case 3:
                //추가기회 획득
                return 6;
            default:
                return 8;
        }
    }
    private void Awake()
    {
        Instans = this;
        TURN = -2;
        GM = GameManager.Instans;
        GenerateWorld();
        int len = ImpactAnim.Length;
        buffList = new List<buffInfo>();
        effectList = new List<Transform>[len];
        for(int i =0; i< len; ++i)
        {
            effectList[i] = new List<Transform>();
        }
        dmgTextList = new List<Transform>();
        for(int i = 0; i < 4; ++i)
        {
            skillLevCheck(i);
        }
        iTemBoxList = new List<Transform>();

        GM.selChar.ChangeImgeSet(player.transform);
    }
    private void Start()
    {
        Mm = MonsterManager.instance;
        prbGetItem = new Player.ItemData();
        prbGetItem.item = GM.noneItem;
        Mm.Mm = this;
        for (int i = 0; i < ItemPossiblePoint.Count; ++i)
        {
            int change = Random.Range(0, ItemPossiblePoint.Count);
            if (i != change)
            {
                Vector2Int temp = ItemPossiblePoint[i];
                ItemPossiblePoint[i] = ItemPossiblePoint[change];
                ItemPossiblePoint[change] = temp;
            }
        }
        checkEnemyState();
        StartCoroutine("StageStart1");
        playerStateText[player.mainState].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("MainStateBack");
        for (int i = 10; i < 19; ++i)
        {
            for (int j = 10; j < 19; ++j)
            {
                //Vector2Int vTemp = new Vector2Int(i, j);
                if (ReadCode(i, j, TileCheck.Empty))
                {
                    playerMove = new Vector2Int(i, j);
                    tileNodeSortSet(playerMove, MapManager.TileCheck.Player);
                    player.transform.position = cMap[i, j].tileTransform.position + new Vector3(0, 0.15f, -10.2f);
                    player.PlayerSetting(GM.selChar);
                    shildCheck();
                    StartCoroutine("updateMap");
                    Mm.PlaceMonster(2);
                    makeItemBoxInMap(2);
                    return;
                }
            }
        }
    }
    /*private void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            getMoveKey(2);
        }
        if (Input.GetKey(KeyCode.W))
        {
            getMoveKey(0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            getMoveKey(3);
        }
        if (Input.GetKey(KeyCode.D))
        {
            getMoveKey(1);
        }
        if (Input.GetKey(KeyCode.M))
        {
            playerGetRandomItem(30, 70);
            prbGetItem.item = GM.hiddenItem[0];
            prbGetItem.lev = player.lev;
        }
    }*/
    public void StartGame(int a)
    {
        //0=3, 1=2, 2=1, 3=start
        switch (a)
        {
            case 0:
                ShowCenterText("3");
                break;
            case 1:
                ShowCenterText("2");
                player.gameObject.SetActive(true);
                player.anim.SetTrigger("Stun");
                break;
            case 2:
                ShowCenterText("1");
                break;
            case 3:
                player.anim.SetTrigger("Jump");
                ShowCenterText("시작!");
                TURN = 0;
                break;
        }
    }
    public void exitGame()
    {
        LoadingSceneManager.LoadScene("Start", false);
    }
    public Transform CountEnemy;
    public Transform CountItem;
    public Transform CountPlayerGoods;
    public int[] killCount;
    public void EndingGame()
    {
        int count = 0;
        int hab = 0;
        for(int i = 0; i < CountEnemy.childCount; ++i)
        {
            if(killCount[count] > 0)
            {
                Transform tTemp = CountEnemy.GetChild(i);
                tTemp.gameObject.SetActive(true);
                tTemp.GetChild(0).GetChild(0).GetComponent<Image>().sprite = GM.mapData.monsterData[count].monsterImage[0];
                GM.mapData.monsterData[count].count += killCount[count];
                hab += killCount[count];
                tTemp.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "+" +killCount[count++];
                tTemp.GetChild(1).GetChild(0).GetComponent<Image>().sprite = GM.mapData.monsterData[count].monsterImage[0];
                GM.mapData.monsterData[count].count += killCount[count];
                hab += killCount[count];
                tTemp.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "+" + killCount[count++];
            }
            else
            {
                break;
            }
        }
        hab = (int)(hab * (1 + GM.nanido * 0.1f));
        count = CountItem.childCount;
        count = (int)(count * (1 + GM.nanido * 0.1f));
        CountItem.GetChild(0).GetComponent<Image>().sprite = itemImg[Mathf.Min(itemImg.Length-1, count/30)];
        CountItem.GetChild(0).gameObject.SetActive(true);
        CountPlayerGoods.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + hab;
        GM.myExpData += hab;
        CountPlayerGoods.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + count;
        GM.myGem += count;
        //for (int i = 0; i < 4; ++i) GM.selectSkillSet = null;
        GM.makePlayerExpData();
        Mm.setSaveData();
        StartCoroutine("turnOnItemPenal");
    }
    IEnumerator turnOnItemPenal()
    {
        WaitForSeconds ws = new WaitForSeconds(Mathf.Max(0.05f, 0.5f - (float)CountItem.childCount*0.003f));
        for (int i = 1; i < CountItem.childCount; ++i)
        {
            yield return ws;
            CountItem.GetChild(i).gameObject.SetActive(true);
            GM.btnSound[1].Play();
        }
    }
    public Transform pointerParticle;
    public Vector2 pointerShow;
    public GameObject miniMap;
    IEnumerator StageStart1()
    {
        WaitForSeconds ws = new WaitForSeconds(1);
        StartGame(0);
        yield return ws;
        StartGame(1);
        yield return ws;
        StartGame(2);
        yield return ws;
        StartGame(3);
        yield return ws;
        canMove = true;
    }
    IEnumerator StageStart()
    {
        yield return StartCoroutine("movePointer");
        canMove = true;
        miniMap.SetActive(true);
    }
    IEnumerator movePointer()
    {
        pointerParticle.gameObject.SetActive(true);
        do
        {
            pointerParticle.position = Vector3.MoveTowards(pointerParticle.position, pointerShow, Time.deltaTime * 5);
            yield return null;
        } while (Vector3.Distance(pointerParticle.position, pointerShow) > 0.1f);
        pointerParticle.position = pointerShow;
        pointerParticle.gameObject.SetActive(false);
    }

    [Header("입력 관리")]
    public bool canMove;
    //0, 1, 2, 3 # 4.채크시작한 시간(할지고민)
    public float[] inputQueue = new float[4];
    public float timeGap;
    public void getMoveKey(int a)
    {
        //0  1
        //2  3
        if (!rStartCheck) return;
        inputQueue[a] = Time.time;
        rStartCheck = false;
    }
    public int findKey()
    {
        int gab = -1;
        float findKey = Time.time - timeGap;
        float max = findKey; //최소 유효입력시간
        for(int i = 0; i<4; ++i)
        {
            if(findKey < inputQueue[i])
            {
                if (max < inputQueue[i])
                {
                    max = inputQueue[i];
                    gab = i;
                }
                inputQueue[i] -= timeGap;
            }
        }
        return gab;
    }
    [Header("맵 데이터")]
    [SerializeField]
    public TileNode[,] cMap;
    //public int[,] MapSort;
    public List<Vector2Int> ItemPossiblePoint;

    public class TileNode
    {
        public Transform tileTransform;    // 타일 상태
        public Vector2Int pos;
        public int sort;
        public int tileState;
        public TileNode()
        {
        }
        public TileNode(Vector2Int position)
        {
            pos = position;
            sort = 0;
        }
        public TileNode(Vector2Int position, int s)
        {
            pos = position;
            sort = s;
        }
        public void setValue(Vector2Int position)
        {
            pos = position;
        }
    }
    public Transform maptiles;
    public Transform maptile;
    public GameObject blackStonePrefab;
    #region 맵생성
    //public Maptile MonsterData;
    //0 = 길, 1 = 벽
    void GenerateWorld()
    {
        //int k, val, index;
        //MonsterData = Resources.Load<Maptile>("MapData/Map Data");
        killCount = new int[GM.mapData.monsterData.Length];
        int numberOfSteps = GM.mapData.numberOfSteps;
        int w = GM.mapData.width, h = GM.mapData.height;

        if (ItemPossiblePoint == null)
        {
            ItemPossiblePoint = new List<Vector2Int>();
        }
        else
        {
            ItemPossiblePoint.Clear();
        }

        cMap = new TileNode[w, h];
        //MapSort = new int[w, h];

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                cMap[x, y] = new TileNode(new Vector2Int(x, y));
                cMap[x, y].sort = (Random.Range(1, 101) < GM.mapData.chanceToStartAlive) ? 1 : 0;
                //MapSort[x, y] = (Random.Range(1, 101) < MonsterData.chanceToStartAlive) ? 1 : 0;
            }
        }
        for (int i = 0; i < numberOfSteps; ++i)
        {
            doSimulationStep(cMap, GM.mapData.deathLimit, GM.mapData.birthLimit, w, h);
        }

        // ---set isometic--- //
        //int frequencySum = 0;
        SpriteRenderer ChildOfTile = maptile.transform.GetChild(0).GetComponent<SpriteRenderer>();
        /*for (int i = 0; i < GM.mapData.maptileBlock.Length; ++i)
        {
            frequencySum += GM.mapData.maptileBlock[i].useFreq; //타일 사용빈도 합 구해서 넣음
        }*/
        for (int i = 0; i < w; ++i)
        {
            for (int j = 0; j < h; ++j)
            {
                if (i == 0 || i == w - 1 || j == 0 || j == h - 1) cMap[i,j].sort = 1;
                //Vector3 tileSetting = new Vector3(1.27f / 2 * (j + i), 0.66f / 2 * (j - i) - 0.307f, 1f / 2 * (j - i) + 5); //바닥 생성
                Vector3 tileSetting = new Vector3(0.6f * (j + i), 0.35f * (j - i), 0); //바닥 생성
                tileSetting.z = tileSetting.y;
                cMap[i, j].tileTransform = Instantiate(blackStonePrefab, maptiles).transform;
                cMap[i, j].tileTransform.localPosition = tileSetting;
                if (ReadCode(i, j, TileCheck.Block))
                {
                   /* k = Random.Range(0, frequencySum);
                    index = GM.mapData.maptileBlock[0].useFreq; 
                    val = 0;

                    for (int l = 1; l < GM.mapData.maptileBlock.Length; ++l)
                    {
                        int gab = GM.mapData.maptileBlock[l].useFreq;
                        if (k >= index && k < index + gab) { 
                            val = l; 
                            break; 
                        }
                        else index += gab;
                    }*/
                    //ChildOfTile.sprite = GM.mapData.maptileBlock[val].tileImage;
                    ChildOfTile.sprite = GM.mapData.spriteSet[Random.Range(0, GM.mapData.spriteSet.Length)];
                    //cMap[i, j].sort = MonsterData.maptileBlock[val].tileNum;
                    Transform gTemp = Instantiate(maptile, cMap[i, j].tileTransform);
                    gTemp.localPosition = new Vector3(0, 0.75f, -10); //벽 생성
                    gTemp.gameObject.SetActive(true);
                }
                else
                {
                    if (i > 0 && i < w - 1 && j > 0 && j < h - 1)
                    {
                        int check = countAliveNeighbours(cMap, i, j);
                        if ( check>= 3)
                        {
                            ItemPossiblePoint.Add(new Vector2Int(i, j));
                        }
                        else if (Random.Range(0, 20) == 0)
                        {
                            cMap[i, j].sort = 1;
                            ChildOfTile.sprite = GM.mapData.spriteSet[Random.Range(0, GM.mapData.spriteSet.Length)];
                            Transform gTemp = Instantiate(maptile, cMap[i, j].tileTransform);
                            gTemp.localPosition = new Vector3(0, 0.75f, -10); //벽 생성
                            gTemp.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        itempointCounter = 0;
    }
    public GameObject iTemBox;
    public List<Transform> iTemBoxList;
    public int itempointCounter;
    //0힐(소), 1힐(대), 2하급아이템 3상급아이템 4최상급
    public Sprite[] itemImg;
    public void makeItemBoxInMap(int count)
    {
        Transform tTemp = null;
        for (int i =0; i < count; ++i)
        {
            int j;
            if(tTemp == null)
            {
                for (j = 0; j < iTemBoxList.Count; ++j)
                {
                    if (!iTemBoxList[j].gameObject.activeSelf)
                    {
                        tTemp = iTemBoxList[j];
                        j = -1;
                        break;
                    }
                }
                if (j >= 0)
                {
                    tTemp = Instantiate(iTemBox).transform;
                    iTemBoxList.Add(tTemp);
                }
            }
            bool find = true;
            Vector2Int vTemp = ItemPossiblePoint[itempointCounter];

            while (ReadCode(vTemp, TileCheck.Block) || tileStateCheck(cMap[vTemp.x, vTemp.y].tileState, TileState.Item))
            {
                try
                {
                    vTemp = ItemPossiblePoint[++itempointCounter];
                }
                catch (System.IndexOutOfRangeException)
                {
                    find = false;
                    break;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    find = false;
                    break;
                }
            }
            if (find)
            {
                tTemp.parent = cMap[vTemp.x, vTemp.y].tileTransform;
                tTemp.name = "item";
                tTemp.localPosition = Vector3.up * 0.85f + Vector3.back * 0.1f;
                cMap[vTemp.x, vTemp.y].tileState |= tileStateGab(TileState.Item);
                //0힐(소), 1힐(대 -보류-), 2하급아이템 3상급아이템 4최상급
                int itemBoxGab = 0;
                if(Random.Range(0, 7) > 2)
                {
                    itemBoxGab = 2; 
                    if (Random.Range(0, 4) == 0)
                    {
                        itemBoxGab = 3;
                        if (Random.Range(0, 4) == 0)
                        {
                            itemBoxGab = 4;
                        }
                    }
                }
                cMap[vTemp.x, vTemp.y].tileState = setTileStateGab(cMap[vTemp.x, vTemp.y].tileState, itemBoxGab, "itemBox");
                tTemp.GetComponent<SpriteRenderer>().sprite = itemImg[itemBoxGab];

                Mm.setTranslucentTile(vTemp, true);
                tTemp.gameObject.SetActive(true);
                tTemp = null;
            }
            if (itempointCounter >= ItemPossiblePoint.Count)
            {
                for (j = 0; j < ItemPossiblePoint.Count; ++j)
                {
                    int change = Random.Range(0, ItemPossiblePoint.Count);
                    if (j != change)
                    {
                        Vector2Int temp = ItemPossiblePoint[j];
                        ItemPossiblePoint[j] = ItemPossiblePoint[change];
                        ItemPossiblePoint[change] = temp;
                    }
                }
                itempointCounter = 0;
            }
        }
    }
    public void doSimulationStep(TileNode[,] cMap, int deathLimit, int birthLimit, int w, int h)
    {
        int[,] tempMap = new int[w, h];
        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                int neighbourCount = 0;
                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; ++j)
                    {
                        if (i == 0 && j == 0)
                        {
                            //Do nothing, we dont want the middle to count as a neighbour
                        }
                        else if (x + i < 0 || y + j < 0 || x + i >= w || y + j >= h)
                        {
                            neighbourCount += 1;
                        }
                        else if (!ReadCode(x + i, y + j, TileCheck.Empty))
                        {
                            neighbourCount += 1;
                        }
                    }
                }
                if (!ReadCode(x, y, TileCheck.Empty))
                {
                    tempMap[x, y] = (neighbourCount < deathLimit) ? 0 : 1;
                }
                else
                {
                    tempMap[x, y] = (neighbourCount > birthLimit) ? 1 : 0;
                }
            }
        }
        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                cMap[x, y].sort = (tempMap[x, y] == 1) ? 1 : 0;
            }
        }
    }
    int countAliveNeighbours(TileNode[,] cellMap, int x, int y)
    {
        int count = 0;
        int[] Check = { -1, 0, 1 };
        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                try
                {
                    if (!ReadCode(x + Check[i], y + Check[j], TileCheck.Empty)) ++count;
                }
                catch (System.IndexOutOfRangeException)
                {

                }
            }
        }
        return count;
    }
    #endregion
    #region 타일정보값 읽고/쓰기
    public enum TileState { Item = 0 };
    public int tileStateGab(TileState what)
    {
        switch (what)
        {
            case TileState.Item:
                return 1;
            default:
                return 0;
        }
    }
    //itemBox, 
    public int setTileStateGab(int state,int gab, string a)
    {
        // 값의 위치를 0으로 변경
        // 해당값과 bit or 연산
        Vector2Int filter = tileStateCodeFilter(a);
        state &= ~filter.x;
        return state | (gab << filter.y); 
    }
    public int getTileStateGab(int gab, string a)
    {
        // 해당 위치값을 1과 bit and
        // 다시 원위치 해서 돌려줌
        Vector2Int filter = tileStateCodeFilter(a);
        return (gab & filter.x) >> filter.y;
    }
    public Vector2Int tileStateCodeFilter(string a)
    {
        switch (a)
        {
            case "itemBox":
                return new Vector2Int(0b11111 << 1, 1);
            default:
                return Vector2Int.zero;
        }
    }
    public bool tileStateCheck(int gab, TileState what)
    {
        int iTemp = tileStateGab(what);
        return (gab & iTemp) == iTemp;
    }

    public enum TileCheck { Block, Empty, Player, Enemy, Item, EnemyDie};
    public bool ReadCode(int x, int y, TileCheck tileCheck)
    {
        try
        {
            int tempNode = cMap[x, y].sort;
            switch (tileCheck)
            {
                case TileCheck.Block:
                    if (tempNode > 0 && tempNode < 101) return true;
                    break;
                case TileCheck.Empty:
                    if (tempNode == 0) return true;
                    break;
                case TileCheck.Item:
                    if (tempNode > 100 && tempNode < 201) return true;
                    break;
                case TileCheck.Enemy:
                    if (tempNode > 300 && tempNode < 500) return true;
                    break;
                case TileCheck.Player:
                    if (tempNode == 1000) return true;
                    break;
                case TileCheck.EnemyDie:
                    if (tempNode > -100 && tempNode < -1) return true;
                    break;
            }
            return false;
        }
        catch (System.IndexOutOfRangeException)
        {
            return false;
        }
    }
    public TileCheck getTileData(int x, int y)
    {
        try
        {
            int tempNode = cMap[x, y].sort;
            if (tempNode == 0) return TileCheck.Empty;
            if (tempNode > 100 && tempNode < 201) return TileCheck.Item;
            if (tempNode > 300 && tempNode < 500) return TileCheck.Enemy;
            if (tempNode == 1000) return TileCheck.Player;
            if (tempNode > -100 && tempNode < -1) return TileCheck.EnemyDie;
            return TileCheck.Block;
        }
        catch (System.IndexOutOfRangeException)
        {
            return TileCheck.Block;
        }
    }
    public TileCheck getTileData(Vector2Int v, bool edie = false)
    {
        try
        {
            int tempNode = cMap[v.x, v.y].sort;
            if (tempNode == 0) return TileCheck.Empty;
            if (tempNode > 100 && tempNode < 201) return TileCheck.Item;
            if (tempNode > 300 && tempNode < 500) return TileCheck.Enemy;
            if (tempNode == 1000) return TileCheck.Player;
            if (tempNode > -100 && tempNode < -1)
            {
                if(edie) return TileCheck.EnemyDie;
                if (cMap[v.x, v.y].tileTransform.GetChild(0).childCount <= 0)
                {
                    tileNodeSortSet(v, TileCheck.Empty);
                    return TileCheck.Empty;
                }
                else
                {
                    tileNodeSortSet(v, TileCheck.Enemy);
                    return TileCheck.Enemy;
                }
                
            }
            return TileCheck.Block;
        }
        catch (System.IndexOutOfRangeException)
        {
            return TileCheck.Block;
        }
    }
    public bool ReadCode(int x, TileCheck tileCheck)
    {
        //Debug.Log(code);
        int tempNode = x;
        switch (tileCheck)
        {
            case TileCheck.Block:
                if (tempNode > 0 && tempNode < 101) return true;
                break;
            case TileCheck.Empty:
                if (tempNode == 0) return true;
                break;
            case TileCheck.Item:
                if (tempNode > 100 && tempNode < 201) return true;
                break;
            case TileCheck.Enemy:
                if (tempNode > 300 && tempNode < 500) return true;
                break;
            case TileCheck.Player:
                if (tempNode == 1000) return true;
                break;
            case TileCheck.EnemyDie:
                if (tempNode > -100 && tempNode < -1) return true;
                break;
        }
        return false;
    }
    public bool ReadCode(Vector2Int pos, TileCheck tileCheck)
    {
        return ReadCode(pos.x, pos.y, tileCheck);
    }

    public int tileNodeSortSet(Vector2Int tileNodePos, TileCheck tileSort, bool change = true)
    {
        int gab = cMap[tileNodePos.x, tileNodePos.y].sort;
        switch (tileSort)
        {
            case TileCheck.Block:
                gab = 1;
                break;
            case TileCheck.Empty:
                gab = 0;
                break;
            case TileCheck.Item:
                gab = 101;
                break;
            case TileCheck.Enemy:
                //if (getTileData(tileNodePos) == TileCheck.EnemyDie) Debug.Log(tileNodePos + ":" + playerMove);
                if (gab > 300 && gab < 499) gab++;
                else gab = 301;
                break;
            case TileCheck.Player:
                gab = 1000;
                break;
            case TileCheck.EnemyDie:
                //Debug.Log(tileNodePos + ":>" + playerMove);
                gab = -2;
                break;
            default:
                gab = -505;
                break;
        }
        if (change && gab >= -10)
        {
            cMap[tileNodePos.x, tileNodePos.y].sort = gab;
        }
        return gab;
    }
    #endregion
}
