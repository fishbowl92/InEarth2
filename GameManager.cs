using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conv = System.Convert; 

[System.Serializable]
public class GameManager : MonoBehaviour
{
    public static GameManager Instans;
    //SendManager sm;
    public AudioSource[] btnSound;
    public Transform mapPrefab;
    public int itemCount = 0;
    public int searchCount = 0;
    //public MapItem mapItem;
    public string hashKeyWord;
    public List<byte> MyActionRecordList, EnemyActionRecordList;

    public enum GameMode { Chess, Tetris }
    public GameMode Mode;
    public Character selChar;

    public Character[] charaters;

    public Color[] tearColor;
    public Item noneItem;
    public Item[] nomalItems1;
    public Item[] nomalItems2;
    public Item[] nomalItems3;
    public Item[] nomalItems4;
    public Item[] nomalItems5;
    public Item[] rareItem;
    public Item[] hiddenItem;
    public Sprite[] RuneImgs;
    public Color[] StateColor;

    public Skill[] selectSkillSet;


    public float[] addDoGamState;
    public int myExpData;
    public int myGem;


    void Awake()
    {
        if (GameManager.Instans)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            GameManager.Instans = this;
            GetPlayerExpData(PlayerPrefs.GetString("PlayerExpData"));
            GetPlayerTraitData(PlayerPrefs.GetString("PlayerTraitData"));

            getPlayerData(PlayerPrefs.GetString("NomalGetData"),
                PlayerPrefs.GetString("RareGetData"),
                PlayerPrefs.GetString("HidenGetData"),
                 PlayerPrefs.GetString("PlayerEnemyKillData"));
        }
    }
    public void makePlayerData()
    {
        string sTemp = string.Empty;
        for (int i = 0; i< nomalItems1.Length; ++i)
        {
            sTemp += nomalItems1[i].name + "," +nomalItems1[i].getCount + "#";
        }
        for (int i = 0; i < nomalItems2.Length; ++i)
        {
            sTemp += nomalItems2[i].name + "," + nomalItems2[i].getCount + "#";
        }
        for (int i = 0; i < nomalItems3.Length; ++i)
        {
            sTemp += nomalItems3[i].name + "," + nomalItems3[i].getCount + "#";
        }
        for (int i = 0; i < nomalItems4.Length; ++i)
        {
            sTemp += nomalItems4[i].name + "," + nomalItems4[i].getCount + "#";
        }
        for (int i = 0; i < nomalItems5.Length; ++i)
        {
            sTemp += nomalItems5[i].name + "," + nomalItems5[i].getCount + "#";
        }
        sTemp = sTemp.Substring(0, sTemp.Length - 1);
        PlayerPrefs.SetString("NomalGetData", sTemp);

        sTemp = string.Empty;
        for (int i = 0; i < rareItem.Length; ++i)
        {
            sTemp += rareItem[i].name + "," + rareItem[i].getCount + "#";
        }
        sTemp = sTemp.Substring(0, sTemp.Length - 1);
        PlayerPrefs.SetString("RareGetData", sTemp);

        sTemp = string.Empty;
        for (int i = 0; i < hiddenItem.Length; ++i)
        {
            sTemp += hiddenItem[i].name + "," + hiddenItem[i].getCount + "#";
        }
        sTemp = sTemp.Substring(0, sTemp.Length - 1);
        PlayerPrefs.SetString("HidenGetData", sTemp);

        sTemp = string.Empty;
        for (int i = 0; i < mapData.monsterData.Length; ++i)
        {
            if (i != 0) sTemp += "#";
            sTemp += mapData.monsterData[i].monsterCode + "," + mapData.monsterData[i].count;
        }
        PlayerPrefs.SetString("PlayerEnemyKillData", sTemp);
    }
    public Item returnItemData(int num, string name)
    {
        //0.노말, 1.레어, 2.히든
        Item[] iList;
        switch (num)
        {
            case 0:
                int gab = hashCode(name) % 5 + 1;
                switch (gab)
                {
                    case 1:
                        iList = nomalItems1;
                        break;
                    case 2:
                        iList = nomalItems2;
                        break;
                    case 3:
                        iList = nomalItems3;
                        break;
                    case 4:
                        iList = nomalItems4;
                        break;
                    default:
                        iList = nomalItems5;
                        break;
                }
                break;
            case 1:
                iList = rareItem;
                break;
            default:
                iList = hiddenItem;
                break;
        }
        for(int i = 0; i < iList.Length; ++i)
        {
            if (iList[i].name == name) return iList[i];
        }
        return noneItem;
    }
    public void getPlayerData(string nData, string rData, string hData, string mData)
    {
        string[] dataArr;
        for (int i = 0; i < 3; ++i)
        {
            switch (i)
            {
                case 1:
                    dataArr = rData.Split('#');
                    break;
                case 2:
                    dataArr = hData.Split('#');
                    break;
                default:
                    dataArr = nData.Split('#');
                    break;
            }
            for (int j = 0; j < dataArr.Length; ++j)
            {
                string[] sTemp = dataArr[i].Split(',');
                if (sTemp.Length < 2) break;
                returnItemData(i, sTemp[0]).getCount = conv.ToInt32(sTemp[1]);
            }
        }

        dataArr = mData.Split('#');
        for (int i = 0; i < dataArr.Length; ++i)
        {
            string[] sTemp = dataArr[i].Split(',');
            for (int j = 0; j < mapData.monsterData.Length; ++j)
            {
                if (mapData.monsterData[i].monsterCode == sTemp[0])
                {
                    mapData.monsterData[i].count = conv.ToInt32(sTemp[1]);
                    break;
                }
            }
        }
    }
    public void makePlayerExpData()
    {

        string sTemp = myExpData + "," + myGem;
        for(int i = 0; i < charaters.Length; ++i)
        {
            Character c = charaters[i];
            sTemp += "#" + c.nameC0de + "," + c.expPoint;
        }
        PlayerPrefs.SetString("PlayerExpData", sTemp);
    }
    public void GetPlayerExpData(string sData)
    {

        string[] dataArr = sData.Split('#');
        string[] charData = dataArr[0].Split(',');
        try
        {
            myExpData = conv.ToInt32(charData[0]);
            myGem = conv.ToInt32(charData[1]);
        }
        catch (System.IndexOutOfRangeException)
        {

        }
        for (int i = 1; i < dataArr.Length; ++i)
        {
            charData = dataArr[i].Split(',');
            foreach (Character c in charaters)
            {
                if (c.nameC0de == charData[0])
                {
                    c.expPoint = conv.ToInt32(charData[1]);
                    break;
                }
            }
        }
    }
    public Maptile mapData;
    public int nanido;
    public void makePlayerTraitData()
    {
        string sTemp = string.Empty;
        for (int i = 0; i < charaters.Length; ++i)
        {
            Character c = charaters[i];
            if(i != 0)sTemp += "#";
            sTemp += c.nameC0de;
            for (int j = 0; j < c.myTraitSelectData.Length; ++j)
            {
                sTemp += "," + c.myTraitSelectData[j];
            }
        }
        PlayerPrefs.SetString("PlayerTraitData", sTemp);
    }
    public void GetPlayerTraitData(string tData)
    {
        string[] dataArr = tData.Split('#');
        for (int i = 0; i < dataArr.Length; ++i)
        {
            string[] charData = dataArr[i].Split(',');
            foreach (Character c in charaters)
            {
                if (c.nameC0de == charData[0])
                {
                    try
                    {
                        for (int j = 1; j < charData.Length; ++j)
                        {
                            c.myTraitSelectData[j - 1] = conv.ToInt32(charData[j]);
                        }
                    }catch (System.IndexOutOfRangeException)
                    {

                    }
                    break;
                }
            }
        }
    }
    void Start()
    {
        MyActionRecordList = new List<byte>();
        EnemyActionRecordList = new List<byte>();
    }

    static int hashCode(string s)
    {
        int h = 0;
        for (int i = 0; i < s.Length; ++i)
        {
            h = (h << 5) | (h >> (sizeof(uint) - 5));
            h += s[i];
        }
        return h;
    }

    [ContextMenu("노멀 아이템 셋팅")]
    public void settingNomalItemSet()
    {
        Item[] items = Resources.LoadAll<Item>("Items/Nomal");
        int a = 0;
        int b = 0;
        int c = 0;
        int d = 0;
        int e = 0;
        nomalItems1 = new Item[100];
        nomalItems2 = new Item[100];
        nomalItems3 = new Item[100];
        nomalItems4 = new Item[100];
        nomalItems5 = new Item[100];
        for (int i = 0; i < items.Length; ++i)
        {
            int gab = hashCode(items[i].name) % 5 + 1;
            switch (gab)
            {
                case 1:
                    nomalItems1[a++] = items[i];
                    break;
                case 2:
                    nomalItems2[b++] = items[i];
                    break;
                case 3:
                    nomalItems3[c++] = items[i];
                    break;
                case 4:
                    nomalItems4[d++] = items[i];
                    break;
                case 5:
                    nomalItems5[e++] = items[i];
                    break;
            }
        }
        
    }
    [ContextMenu("아이템 셋팅")]
    public void settingAllItemSet()
    {
        Item[] Normal_items = Resources.LoadAll<Item>("Items/Nomal");
        Item[] rare_items = Resources.LoadAll<Item>("Items/Rare");
        Item[] hidden_items = Resources.LoadAll<Item>("Items/Hidden");
        int a = 0;
        int b = 0;
        int c = 0;
        int d = 0;
        int e = 0;
        nomalItems1 = new Item[50];
        nomalItems2 = new Item[50];
        nomalItems3 = new Item[50];
        nomalItems4 = new Item[50];
        nomalItems5 = new Item[50];
        rareItem = rare_items;
        hiddenItem = hidden_items;
        for (int i = 0; i < Normal_items.Length; ++i)
        {
            int gab = hashCode(Normal_items[i].name) % 5 + 1;
            switch (gab)
            {
                case 1:
                    nomalItems1[a++] = Normal_items[i];
                    break;
                case 2:
                    nomalItems2[b++] = Normal_items[i];
                    break;
                case 3:
                    nomalItems3[c++] = Normal_items[i];
                    break;
                case 4:
                    nomalItems4[d++] = Normal_items[i];
                    break;
                case 5:
                    nomalItems5[e++] = Normal_items[i];
                    break;
            }
        }

    }
}
