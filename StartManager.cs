using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartManager : MonoBehaviour
{
    [Header("셋팅, 시작 버튼 조율")]
    public Image back;
    Material backMaterial;
    //0, 셋팅창 1, 시작창
    public GameObject[] settingAndStart;
    public GameManager GM;
    private void Awake()
    {
        backMaterial = back.material;
        Application.targetFrameRate = 60;
        GM = GameManager.Instans;
        //Screen.SetResolution(800, 360, true);
        Screen.SetResolution(1600, 720, true);
        changeCharacter(0);
        getItemsData(PlayerPrefs.GetString("TileItmeData"));
    }
    private void Start()
    {
        checkItemDataGage();
        checkEnemyDataGage();
        checkItemList(PlayerPrefs.GetString("PlayerSkillItemData"));
        myGemCounter.text = GM.myGem + "<color=#f29786>(-120)</color>";
    }
   /* private void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            //playerGetRandomItem(30, 70);
            getNewSkillItem(PublicSkills[Random.Range(0, PublicSkills.Length)]);
        }
    }*/


    public GameObject GameStartPenal;
    public void clickStart(bool setting)
    {
        GameStartPenal.SetActive(true);
        StartCoroutine("CanvasSetWave", setting);
        if (setting)
        {
            //settingAndStart[1].SetActive(false);
            //StartCoroutine("CanvasSetChange", -1f);
        }
        else
        {
            //settingAndStart[0].SetActive(false);
            //StartCoroutine("CanvasSetChange", 1f);
        }
    }
    IEnumerator CanvasSetWave(bool setting)
    {
        yield return StartCoroutine("backOnOff", 1.5f);
        settingAndStart[(setting) ? 1 : 0].SetActive(false);
        settingAndStart[(!setting) ? 1 : 0].SetActive(true);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine("backOnOff", -1f);
        GameStartPenal.SetActive(false);
    }
    IEnumerator CanvasSetChange(float on)
    {
        yield return StartCoroutine("backOnOff", on);
        //settingAndStart[(on > 0) ? 1 : 0].SetActive(true);
    }
    IEnumerator backOnOff(float on)
    {

        float fade;
        if (on > 0)
        {
            //배경 덮기
            fade = 0;
        }
        else
        {
            fade = 1;
        }
        while(fade >=0 && fade <=1)
        {
            backMaterial.SetFloat("_Fade", fade);
            fade += Time.deltaTime * on;
            yield return null;
        }
        yield break;
    }
    public void StartAIPlay()
    {
        setItemsData();
        GM.nanido = 0;
        for (int i = 0; i < selectTileList.Length; ++i)
        {
            if (selectTileList[i] > 0) GM.nanido++;
            GM.mapData.spriteSet[i] = tileImg[selectTileList[i]];
        }
        LoadingSceneManager.LoadScene("Map", false);
        //SceneManager.LoadScene(1);
    }
    public int selNumber;
    public Transform playerCharShow;
    public void changeCharacter(int a)
    {
        if (!playerCharShow.gameObject.activeSelf) return;
        GM.btnSound[2].Play();
        selNumber += a;
        if (selNumber >= GM.charaters.Length) selNumber = 0;
        if (selNumber < 0) selNumber = GM.charaters.Length - 1;
        GM.charaters[selNumber].ChangeImgeSet(playerCharShow);
        playerCharShow.GetComponent<Animator>().runtimeAnimatorController = GM.charaters[selNumber].anim;
        Invoke("delayChangeChar", 0.03f);
    }
    public Skill[] PublicSkills;
    public Transform SkillItemList;
    public GameObject skillKan;
    public void checkItemList(string data)
    {
        for (int i = SkillItemList.childCount - 1; i >= 1 ; ++i)
        {
            Destroy(SkillItemList.GetChild(i).gameObject);
        }
        if (data == "") return;
        string[] dataArr = data.Split('#');

        for(int i = 0; i < dataArr.Length; ++i)
        {
            string[] getData = dataArr[i].Split(',');
            if (getData.Length < 2 || System.Convert.ToInt32(getData[1]) < 1) continue;
            Skill sTemp = returnSkillToName(getData[0]);
            if (sTemp == null) continue;

            Transform tTemp = Instantiate(skillKan, SkillItemList).transform;
            tTemp.gameObject.SetActive(true);
            tTemp.name = getData[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = System.Convert.ToInt32(getData[1]) + 0.3f;
            tTemp.position = vTemp;

            tTemp.GetChild(0).GetComponent<Image>().sprite = sTemp.skillIcon;
            tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "x"+System.Convert.ToInt32(getData[1]);
        }
    }
    public string getMyItemContens()
    {
        string sTemp = string.Empty;

        for(int i = 1; i < SkillItemList.childCount; ++i)
        {
            if (i != 0) sTemp += "#";
            Transform tTemp = SkillItemList.GetChild(i);
            sTemp += tTemp.name + "," + (int)tTemp.position.z;
        }

        return sTemp;
    }
    public Skill returnSkillToName(string name)
    {
        if (name == "default") return GM.selChar.skillSetting[selSkillKanNum];
        for (int i = 0; i < PublicSkills.Length; ++i)
        {
            if (PublicSkills[i].code == name) return PublicSkills[i];
        }
        return null;
    }
    public Skill getNewSkillItem(Skill sTemp)
    {
        Transform tTemp;
        Vector3 vTemp;
        for (int i = 0; i < SkillItemList.childCount; ++i)
        {
            tTemp = SkillItemList.GetChild(i);
            if (tTemp.name == sTemp.code)
            {
                vTemp = tTemp.position;
                vTemp.z++;
                tTemp.position = vTemp;
                tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "x" + (int)vTemp.z;
                PlayerPrefs.SetString("PlayerSkillItemData",getMyItemContens());
                return sTemp;
            }
        }
        tTemp = Instantiate(skillKan, SkillItemList).transform;
        tTemp.gameObject.SetActive(true);
        tTemp.name = sTemp.code;
        vTemp = tTemp.position;
        vTemp.z = 1.3f;


        tTemp.GetChild(0).GetComponent<Image>().sprite = sTemp.skillIcon;
        tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "x1";
        PlayerPrefs.SetString("PlayerSkillItemData", getMyItemContens());
        return sTemp;
    }
    public Transform[] selectSkillInfo;
    public int selSkillKanNum;
    public string selSkillName;
    public void clickNewSkill(Transform t)
    {

        selSkillName = t.name;
        Skill sTemp = returnSkillToName(selSkillName);

        selectSkillInfo[1].GetChild(0).GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
        selectSkillInfo[1].GetChild(2).GetComponent<Image>().sprite = sTemp.skillIcon;
        selectSkillInfo[1].GetChild(3).GetComponent<TextMeshProUGUI>().text = sTemp.getInfoText(1, false);
    }
    public void changeSkillSetting()
    {
        Skill sTemp = returnSkillToName(selSkillName);

        if (selSkillName == "default")
        {
            GM.selectSkillSet[selSkillKanNum] = null;

            sTemp = (GM.selectSkillSet[selSkillKanNum] == null) ? GM.selChar.skillSetting[selSkillKanNum] : GM.selectSkillSet[selSkillKanNum];
            selectSkillInfo[0].GetChild(0).GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
            selectSkillInfo[0].GetChild(2).GetComponent<Image>().sprite = sTemp.skillIcon;
            clickNewSkill(SkillItemList.GetChild(0));
        }
        else
        {
            for (int i = 1; i < SkillItemList.childCount; ++i)
            {
                Transform tTemp = SkillItemList.GetChild(i);
                if (tTemp.name == sTemp.code)
                {
                    Vector3 vTemp = tTemp.position;
                    vTemp.z--;
                    tTemp.position = vTemp;
                    if (vTemp.z < 1) Destroy(tTemp.gameObject);
                    else tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = "x" + (int)vTemp.z;
                    PlayerPrefs.SetString("PlayerSkillItemData", getMyItemContens());

                    GM.selectSkillSet[selSkillKanNum] = sTemp;

                    sTemp = (GM.selectSkillSet[selSkillKanNum] == null) ? GM.selChar.skillSetting[selSkillKanNum] : GM.selectSkillSet[selSkillKanNum];
                    selectSkillInfo[0].GetChild(0).GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
                    selectSkillInfo[0].GetChild(2).GetComponent<Image>().sprite = sTemp.skillIcon;
                    clickNewSkill(SkillItemList.GetChild(0));
                    break;
                }
            }
        }
        Transform trTemp = skillInfoUI.GetChild(selSkillKanNum);
        trTemp.GetChild(0).Find("icon").GetComponent<Image>().sprite = sTemp.skillIcon;
        trTemp.GetChild(0).Find("name").GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
        trTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = sTemp.getInfoText(1, false);
        saveCharSkillData();

        GM.btnSound[2].Play();
    }
    public void saveCharSkillData()
    {
        string sData = string.Empty;
        for (int i = 1; i < 4; ++i)
        {
            if (i != 1) sData += "#";
            Skill sTemp = GM.selectSkillSet[i];
            if (sTemp == null) sData += "default";
            else sData += sTemp.code;
        }
        PlayerPrefs.SetString(GM.selChar.nameC0de + "CharSkillSettingData", sData);
    }
    public void setCharSkillData()
    {
        string[] sData = PlayerPrefs.GetString(GM.selChar.nameC0de + "CharSkillSettingData").Split('#');
        if (sData.Length < 3)
        {
            for (int i = 1; i < 4; ++i)
            {
                GM.selectSkillSet[i] = null;
            }
        }
        else
        {
            for (int i = 1; i < 4; ++i)
            {
                if (sData[i - 1] == "default")
                {
                    GM.selectSkillSet[i] = null;
                }
                else GM.selectSkillSet[i] = returnSkillToName(sData[i - 1]);
            }
        }
    }
    public GameObject SkillPenalUi;
    public void popupSkillPenal(int a)
    {
        if((a != 0 && selSkillKanNum != a) || !SkillPenalUi.activeSelf)
        {
            SkillPenalUi.SetActive(true);
            if(selSkillKanNum > 0) skillInfoUI.GetChild(selSkillKanNum).GetChild(0).GetChild(3).gameObject.SetActive(false);
            selSkillKanNum = a;
            Skill sTemp = (GM.selectSkillSet[selSkillKanNum] == null) ? GM.selChar.skillSetting[selSkillKanNum] : GM.selectSkillSet[selSkillKanNum];
            selectSkillInfo[0].GetChild(0).GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
            selectSkillInfo[0].GetChild(2).GetComponent<Image>().sprite = sTemp.skillIcon;
            clickNewSkill(SkillItemList.GetChild(0));

            skillInfoUI.GetChild(a).GetChild(0).GetChild(3).gameObject.SetActive(true);
            GM.btnSound[0].Play();
        }
        else
        {
            if (selSkillKanNum > 0) skillInfoUI.GetChild(selSkillKanNum).GetChild(0).GetChild(3).gameObject.SetActive(false);
            SkillPenalUi.SetActive(false);
            selSkillKanNum = 0;
            GM.btnSound[1].Play();
        }
    }
    public void delayChangeChar()
    {
        GM.selChar = GM.charaters[selNumber];
        setCharSkillData();
        playerCharShow.gameObject.SetActive(true);
        playerCharShow.GetComponent<Animator>().SetTrigger("Stun");
        expCheck();
        skillInfoSetting();
        StateSliderSetting();
        settingTraitKans();
        showTraitInfo(0, 0);
    }
    public Transform skillInfoUI;
    public Transform baseSkillInfoUI;
    public void skillInfoSetting()
    {

        baseSkillInfoUI.GetChild(0).Find("icon").GetComponent<Image>().sprite = GM.selChar.basicSkill.skillIcon;
        baseSkillInfoUI.GetChild(0).Find("name").GetComponent<TextMeshProUGUI>().text = GM.selChar.basicSkill.skillName;
        baseSkillInfoUI.GetChild(1).GetComponent<TextMeshProUGUI>().text = GM.selChar.basicSkill.getInfoText(1, false);
        for (int i = 0; i < 4; ++i)
        {
            Transform tTemp = skillInfoUI.GetChild(i);
            Skill sTemp = (GM.selectSkillSet[i] == null) ? GM.selChar.skillSetting[i] : GM.selectSkillSet[i];
            tTemp.GetChild(0).Find("icon").GetComponent<Image>().sprite = sTemp.skillIcon;
            tTemp.GetChild(0).Find("name").GetComponent<TextMeshProUGUI>().text = sTemp.skillName;
            tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = sTemp.getInfoText(1, false);
        }
    }
    public Slider[] stateBar;
    public TextMeshProUGUI mainStateText;
    public void StateSliderSetting()
    {
        for(int i = 0; i < 3; ++i)
        {
            stateBar[i].value = GM.selChar.baseState[i] * 0.1f;
        }
        string sTemp = "주스텟: ";
        switch (GM.selChar.mainState)
        {
            case 1:
                sTemp += "'민첩'";
                break;
            case 2:
                sTemp += "'지능'";
                break;
            default:
                sTemp += "'근력'";
                break;
        }

        mainStateText.text = sTemp;
    }
    public Transform[] settingPenal;
    public void chageSettingPeanl(int a)
    {
        GM.btnSound[0].Play();
        for(int i =0; i < settingPenal.Length; ++i)
        {
            settingPenal[i].gameObject.SetActive(i == a);
        }
    }
    public Sprite[] onAndOffBtnSprite;
    public Image[] pageBtn;
    public GameObject[] penal;
    public void selcetillustratedPage(int a)
    {
        for(int i = 0; i < pageBtn.Length; ++i)
        {
            pageBtn[i].sprite = (a == i) ? onAndOffBtnSprite[0] : onAndOffBtnSprite[1];
            penal[i].SetActive(i == a);
        }

        GM.btnSound[0].Play();
    }
    public Transform selItemInfo;
    public void selectContens(Transform kan)
    { 
        if (kan.position.z < 0) return;
        GM.btnSound[0].Play();
        Item id = GM.returnItemData((int)(kan.position.z), kan.name);
        selItemInfo.GetChild(0).GetComponent<Image>().sprite = id.Img;
        selItemInfo.GetChild(1).GetComponent<TextMeshProUGUI>().text =
              (((id.getItemDmg(1) == 0) ? string.Empty : "공:" + id.getItemDmg(1) + " ")
              + ((id.getItemDef(1) == 0) ? string.Empty : "방:" + id.getItemDef(1) + " ")
              + ((id.getItemHp(1) == 0) ? string.Empty : "체:" + id.getItemHp(1)) + " ");
        selItemInfo.GetChild(2).GetComponent<TextMeshProUGUI>().text = id.infoText;

    }
    public void selectEnemyContes(Transform kan)
    {
        GM.btnSound[0].Play();
        Maptile.MonsterData md = GM.mapData.monsterData[(int)kan.position.z];
        selItemInfo.GetChild(0).GetComponent<Image>().sprite = md.monsterImage[0];
        selItemInfo.GetChild(1).GetComponent<TextMeshProUGUI>().text = md.monsterName;
        selItemInfo.GetChild(2).GetComponent<TextMeshProUGUI>().text = md.infoText;
    }
    public void checkItemDataGage()
    {
        int gab = 0;
        for (int i = 0; i < have.childCount; ++i)
        {
            Transform tTemp = have.GetChild(i);
            Vector3 vTemp = tTemp.position;
            Item iData = GM.returnItemData((int)(vTemp.z), tTemp.name);
            try
            {
                if (iData.getCount <= 0 || iData == GM.noneItem) vTemp.z = -1;
            }
            catch (System.IndexOutOfRangeException)
            {
                vTemp.z = -1;
            }
            if (vTemp.z < 0)
            {
                tTemp.position = vTemp;
                tTemp.GetComponent<Image>().color = Color.gray;
            }
            else
            {
                gab++;
            }
        }
        GM.addDoGamState[0] = (float)gab / have.childCount;
        addDoGamStateSlider[0].value = GM.addDoGamState[0];
        addDoGamStateText[0].text = "장비 도감 달성률 보너스 : 피해량 +" + (int)(GM.addDoGamState[0] * 50) + "%";
        addDoGamStateTextPerSent[0].text = (int)(GM.addDoGamState[0] * 100) + "%";
    }
    public void checkEnemyDataGage()
    {
        int hab = 0;
        int i = 0;
        for (; i < ehave.childCount; ++i)
        {
            Transform tTemp = ehave.GetChild(i);
            int gab = GM.mapData.monsterData[i].count;
            tTemp.GetChild(1).GetComponent<TextMeshProUGUI>().text = gab.ToString();
            //최대값 10000000
            hab += Mathf.Min(6, (int)Mathf.Log10(gab + 1));
        }
        GM.addDoGamState[1] = (float)hab / (i *  6);
        addDoGamStateSlider[1].value = GM.addDoGamState[1];
        addDoGamStateText[1].text = "적군 도감 달성률 보너스 : 방어력 +" + (int)(GM.addDoGamState[1] * 50) + "%";
        addDoGamStateTextPerSent[1].text = (int)(GM.addDoGamState[1] * 100) + "%";
    }
    public Slider[] addDoGamStateSlider;
    public TextMeshProUGUI[] addDoGamStateText;
    public TextMeshProUGUI[] addDoGamStateTextPerSent;
    public GameObject kans;
    public Transform have;
    [ContextMenu("항목 생성")]
    public void makeKans()
    {
        for(int i =0; i < GM.nomalItems1.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.nomalItems1[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i+0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.nomalItems1[i].name;
        }
        for (int i = 0; i < GM.nomalItems2.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.nomalItems2[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.nomalItems2[i].name;
        }
        for (int i = 0; i < GM.nomalItems3.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.nomalItems3[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.nomalItems3[i].name;
        }
        for (int i = 0; i < GM.nomalItems4.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.nomalItems4[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.nomalItems4[i].name;
        }
        for (int i = 0; i < GM.nomalItems5.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.nomalItems5[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.nomalItems5[i].name;
        }
        for (int i = 0; i < GM.rareItem.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.rareItem[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[1];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f + 1;
            tTemp.position = vTemp;
            tTemp.name = GM.rareItem[i].name;
        }
        for (int i = 0; i < GM.hiddenItem.Length; ++i)
        {
            Transform tTemp = Instantiate(kans, have).transform;
            tTemp.GetComponent<Image>().sprite = GM.hiddenItem[i].Img;
            tTemp.GetComponent<Outline>().effectColor = GM.tearColor[2];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f + 2;
            tTemp.position = vTemp;
            tTemp.name = GM.hiddenItem[i].name;
        }
    }

    public GameObject ekans;
    public Transform ehave;
    [ContextMenu("적항목 생성")]
    public void makeEnemyKans()
    {
        for(int i = 0; i < GM.mapData.monsterData.Length; ++i)
        {
            Transform tTemp = Instantiate(ekans, ehave).transform;
            tTemp.GetChild(0).GetComponent<Image>().sprite = GM.mapData.monsterData[i].monsterImage[0];
            Vector3 vTemp = tTemp.position;
            vTemp.z = i + 0.3f;
            tTemp.position = vTemp;
            tTemp.name = GM.mapData.monsterData[i].monsterCode;
        }
    }
    [System.Serializable]
    public struct NPCscripts
    {
        [TextArea(10, 20)]
        public string[] texts;
    }
    public int[] talkCount;
    public Transform[] talkBox;
    public NPCscripts[] NpcScripts;

    public void talkNPC(int a)
    {
        if (talkBox[a].gameObject.activeSelf) return;
        talkBox[a].gameObject.SetActive(true);
        StartCoroutine("startTalk", a);
    }
    IEnumerator startTalk(int a)
    {
        string sTemp = NpcScripts[a].texts[talkCount[a]];
        if (++talkCount[a] >= NpcScripts[a].texts.Length) talkCount[a] = 0;
        TextMeshProUGUI text = talkBox[a].GetChild(0).GetComponent<TextMeshProUGUI>();

        GameObject check = (a == 0) ? settingPenal[2].gameObject : ((a == 1) ? settingPenal[0].gameObject : settingPenal[3].gameObject);
        WaitForSeconds ws = new WaitForSeconds(0.07f);
        text.text = "";
        //위에서부터 1, 2, 0, 3
        for (int i = 0; i < sTemp.Length; ++i)
        {
            if (!check.activeSelf) break;
            text.text += sTemp[i];
            if (sTemp[i] != ' ') GM.btnSound[3].Play();
            else yield return null;
            yield return ws;
        }
        if (check.activeSelf) yield return new WaitForSeconds(1.8f);
        talkBox[a].gameObject.SetActive(false);
    }
    #region 특성칸
    public GameObject expPoint;
    public Transform expStartPoint;
    public Transform expGetPoint;
    public bool startGetExp;
    public List<GameObject> expBubblePolling;
    public int pollingCounter;
    public Transform getExpBubble()
    {
        if (expBubblePolling.Count > 0)
        {
            if (!expBubblePolling[0].activeSelf)
            {
                pollingCounter = 1;
                return expBubblePolling[0].transform;
            }
            for (; pollingCounter < expBubblePolling.Count; ++pollingCounter)
            {
                if (!expBubblePolling[pollingCounter].activeSelf)
                {
                    return expBubblePolling[pollingCounter].transform;
                }
            }
        }
        Transform tTemp = Instantiate(expPoint, expStartPoint).transform;
        expBubblePolling.Add(tTemp.gameObject);
        return tTemp;
    }
    public TextMeshProUGUI namPlayerEXPData;
    public void checkPlayerEXPdata()
    {
        namPlayerEXPData.text = "소유중인 경험치 : " + GM.myExpData;
    }
    public void getExpPoint()
    {
        startGetExp = true;
        StartCoroutine("startGetExpPointGGug");
    }
    IEnumerator startGetExpPointGGug()
    {
        WaitForSeconds ws = new WaitForSeconds(0.03f);
        bool expGetSound = false;
        while (startGetExp && GM.myExpData > 0)
        {
            Transform tTemp = getExpBubble();
            tTemp.GetComponent<Image>().sprite = GM.RuneImgs[Random.Range(0, GM.RuneImgs.Length)];
            tTemp.gameObject.SetActive(true);
            tTemp.GetComponent<MakeAnimForUI>().starting(expStartPoint.position, expGetPoint.position);
            if (!expGetSound) GM.btnSound[0].Play();
            else GM.btnSound[1].Play();
            expGetSound = !expGetSound;
            GM.selChar.expPoint++;
            GM.myExpData--;
            expCheck();
            yield return ws;
        }
        GM.makePlayerExpData();
    }
    public void endGetExpPoint()
    {
        startGetExp = false;
    }
    public TextMeshProUGUI CharName;
    public TextMeshProUGUI namEXPtext;
    public Slider expSliderBar;
    public void expCheck()
    {
        checkPlayerEXPdata();
        Vector3Int eTemp = GM.selChar.getCharLevAndExp();
        expSliderBar.maxValue = eTemp.z;
        expSliderBar.value = eTemp.y;
        CharName.text = "Lv." + eTemp.x + " " + GM.selChar.CharacterName;
        namEXPtext.text = eTemp.y + "/" + eTemp.z;
        if (eTemp.y < 2) settingTraitKans();
    }

    public Transform traitKans;
    public GameObject traitContens;
    public void settingTraitKans()
    {
        Vector3Int eTemp = GM.selChar.getCharLevAndExp();
        for (int i = 0; i < traitKans.childCount; ++i)
        {
            Transform tTemp = traitKans.GetChild(i).GetChild(0);
            int j = 0;
            int sel = GM.selChar.myTraitSelectData[i] - 1;
            bool manjok = eTemp.x >= (i + 1) * 10;
            for (; j < tTemp.childCount; ++j)
            {
                Transform tKan = tTemp.GetChild(j);

                if (j >= GM.selChar.myTraitData[i].tds.Length)
                {
                    //존재하지 않는 
                    tKan.gameObject.SetActive(false);
                }
                else
                {
                    tKan.gameObject.SetActive(true);
                    tKan.GetChild(0).GetComponent<Image>().sprite = GM.selChar.myTraitData[i].tds[j].img;
                    if (manjok)
                    {
                        //요구레벨을 만족
                        if (sel < 0)
                        {
                            //선택된게 없음
                            tKan.GetComponent<Image>().color = Color.yellow;
                            tKan.GetChild(0).GetComponent<Image>().color = Color.white;
                        }
                        else
                        {
                            tKan.GetComponent<Image>().color = (sel == j) ? Color.red : Color.black;
                            tKan.GetChild(0).GetComponent<Image>().color = (sel == j) ? Color.white : Color.gray;
                        }
                    }
                    else
                    {
                        tKan.GetComponent<Image>().color = Color.black;
                        tKan.GetChild(0).GetComponent<Image>().color = Color.gray;
                    }
                }
                Vector3 vTemp = tKan.position;
                vTemp.z = i * 10 + j + 0.3f;
                tKan.position = vTemp;
            }
            for(; j < GM.selChar.myTraitData[i].tds.Length; ++j)
            {
                Transform tKan = Instantiate(traitContens, tTemp).transform; 
                tKan.gameObject.SetActive(true);
                tKan.GetChild(0).GetComponent<Image>().sprite = GM.selChar.myTraitData[i].tds[j].img;
                if (manjok)
                {
                    //요구레벨을 만족
                    if (sel < 0)
                    {
                        //선택된게 없음
                        tKan.GetComponent<Image>().color = Color.yellow;
                        tKan.GetChild(0).GetComponent<Image>().color = Color.white;
                    }
                    else
                    {
                        tKan.GetComponent<Image>().color = (sel == j) ? Color.red : Color.black;
                        tKan.GetChild(0).GetComponent<Image>().color = (sel == j) ? Color.white : Color.gray;
                    }
                }
                else
                {
                    tKan.GetComponent<Image>().color = Color.black;
                    tKan.GetChild(0).GetComponent<Image>().color = Color.gray;
                }
                Vector3 vTemp = tKan.position;
                vTemp.z = i * 10 + j + 0.3f;
                tKan.position = vTemp;
            }
        }
    }
    public void selectTrait(Transform t)
    {
        //Vector3Int eTemp = GM.selChar.getCharLevAndExp();
        //bool manjok = eTemp.x >= (t.position.x + 1) * 5;
        showTraitInfo((int)(t.position.z * 0.1f), (int)(t.position.z % 10));
        GM.btnSound[0].Play();
    }
    public Transform TraitInfoPenal;
    public Vector2Int prbTraitSelect;
    public void showTraitInfo(int lev, int n)
    {
        prbTraitSelect.x = lev;
        prbTraitSelect.y = n;
        Trait td = GM.selChar.myTraitData[lev].tds[n];
        TraitInfoPenal.GetChild(0).GetComponent<Image>().sprite = td.img;
        TraitInfoPenal.GetChild(1).GetComponent<TextMeshProUGUI>().text = td.tdName;
        TraitInfoPenal.GetChild(2).GetComponent<TextMeshProUGUI>().text = td.info;
    }
    public void changeTraitSelect()
    {
        if (GM.selChar.getCharLevAndExp().x < (prbTraitSelect.x + 1) * 10)
        {
            GM.btnSound[1].Play();
            return;
        }
        GM.btnSound[2].Play();
        GM.selChar.myTraitSelectData[prbTraitSelect.x] = prbTraitSelect.y +1;
        settingTraitKans();
        GM.makePlayerTraitData();
    }
    #endregion
    #region 상점칸
    public Sprite[] tileImg;
    public int[] myItemCount;
    public int[] selectTileList;
    public Transform Tileitems;
    public Transform ItemsPenal;
    public TextMeshProUGUI nanidoText;
    public void getItemsData(string sData)
    {
        string[] dataArr = sData.Split(',');
        if (dataArr.Length < 10) return;
        for(int i = 0; i < myItemCount.Length; ++i)
        {
            try
            {
                myItemCount[i] = System.Convert.ToInt32(dataArr[i]);
            }
            catch (System.IndexOutOfRangeException)
            {
                myItemCount[i] = 0;
            }
        }
    }
    public void setItemsData()
    {
        string sTemp = myItemCount[0].ToString();
        for (int i = 0; i < myItemCount.Length; ++i)
        {
            sTemp += "," + myItemCount[i];
        }
        PlayerPrefs.SetString("TileItmeData", sTemp);
    }
    public void selectNewTileBlock(Transform t)
    {
        int index = (int)t.position.z;
        if (myItemCount[index] <= 0)
        {
            GM.btnSound[1].Play();
            return;
        }
        int i = 0;
        for(; i < selectTileList.Length; ++i)
        {
            if (selectTileList[i] <= 0) break;
        }
        if (selectTileList.Length == i)
        {
            GM.btnSound[1].Play();
            return;
        }

        GM.btnSound[0].Play();
        selectTileList[i] = index;
        t.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + (--myItemCount[index]);
        nanidoText.text = "난이도(" + (i + 1) + ") // 결과보상 획득 : + " + 10* (i + 1) + "%";
        Tileitems.GetChild(i).GetChild(0).GetComponent<Image>().sprite = tileImg[index];
    }
    public void resetSellTileBlock()
    {
        for(int i = 0; i < selectTileList.Length; ++i)
        {
            int gab = selectTileList[i];
            if (gab <= 0) return;
            Tileitems.GetChild(i).GetChild(0).GetComponent<Image>().sprite = tileImg[0];
            selectTileList[i] = 0;
            ItemsPenal.GetChild(gab - 1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + ++myItemCount[gab];
        }
        nanidoText.text = "난이도(0) // 결과보상 획득 : + 10%";
    }
    public TextMeshProUGUI myGemCounter;
    public Sprite[] itemSprites;
    public Image getItemImage;
    public void getNewItem()
    {
        if (GM.myGem < 120)
        {
            GM.btnSound[1].Play();
            return;
        }
        GM.myGem -= 120;
        int get = Random.Range(-6, 13);
        if (get > 0)
        {
            get = Random.Range(1, myItemCount.Length);
            getItemImage.sprite = tileImg[get];
            ItemsPenal.GetChild(get - 1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + ++myItemCount[get];
            setItemsData();
        }
        else
        {
            switch (get)
            {
                case 0:
                case -1:
                case -2:
                case -3:
                case -4:
                    //경험치 획득
                    GM.myExpData += 1000;
                    expCheck();
                    getItemImage.sprite = itemSprites[0];
                    break;
                default:
                    getItemImage.sprite = getNewSkillItem(PublicSkills[Random.Range(0, PublicSkills.Length)]).skillIcon;
                    break;
            }
        }
        GM.btnSound[2].Play();
        GM.makePlayerExpData();
        myGemCounter.text = GM.myGem +"<color=#f29786>(-120)</color>";
    }
    public void byeGem(int a)
    {
        GM.myGem += a;
        myGemCounter.text = GM.myGem + "<color=#f29786>(-120)</color>";
        GM.makePlayerExpData();
    }

    [ContextMenu("아이템 타일 항목 생성")]
    public void makeItemTileContens()
    {
        myItemCount = new int[tileImg.Length];
        GameObject gTemp = ItemsPenal.GetChild(0).gameObject;
        for(int i = 1; i < tileImg.Length; ++i)
        {
            Transform make = Instantiate(gTemp, ItemsPenal).transform;
            Vector3 vTemp = make.position;
            vTemp.z = i + 0.3f;
            make.position = vTemp;
            make.GetComponent<Image>().sprite = tileImg[i];
            make.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x0";
        }
    }
    #endregion
}
