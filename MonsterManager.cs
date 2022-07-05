using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using UnityEngine.UI;
using TileNode = MapManager.TileNode;
using TileCheck = MapManager.TileCheck;
public class MonsterManager : MonoBehaviour
{
    public static MonsterManager instance = null;
    public MapManager Mm;
    int mapSize = 21;

    public GameObject monsterPrefab;
    public List<Monster> monsterList = new List<Monster>();
    //public List<Vector2Int> deathMonsterLocList = new List<Vector2Int>();
    NDArray mapTileAllData = np.zeros(2,19, 19);
    //NDArray mapTileBlockArray = np.zeros(29, 29);
    //?NDArray monsterLiveDeathArray = np.zeros(19, 19);
    //NDArray playerAndMonsterArray = np.zeros(29, 29);
    public NDArray featureLayer = np.zeros(4, 9, 9);
    public NDArray fLayerHab = np.zeros(4);
    //NDArray step1AfterConv = np.zeros(2, 14, 14);
    //NDArray kernelWeight = np.array(new float[5] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
    //NDArray sumArr = np.zeros(5);
    NDArray DsFilter = np.array(new double[2, 3, 3] {
                                              { {0,0.2,0 },
                                                {0.2,0,0.2 },
                                                {0,0.2,0 }},
                                             { {0.2, 0, 0.2 },
                                               {0,0.2,0 },
                                               {0.2,0,0.2 }} });

    public float bestScore;
    //public List<NDArray> kwDNAs;
    //public List<NDArray> fwDNAs;
    public NDArray kwDNA = np.random.rand(new int[2] { 5, 4 });
    public NDArray fwDNA = np.random.rand(new int[2] { 4, 6 });
    public NDArray fwDNA2 = np.random.rand(new int[2] { 5, 13 });
    public WeightTkinter save;
    public void setSaveData()
    {
        //NDArray kwDNA = kwDNAs[0];
        //NDArray fwDNA = fwDNAs[0];
        string sTemp = string.Empty;
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                sTemp += (int)(System.Math.Truncate((double)kwDNA[i, j] * 1000)) + ",";
                //save.kwDNA[i].myWeight[j] = kwDNA[i, j];
            }
        }
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 6; ++j)
            {
                sTemp += (int)(System.Math.Truncate((double)fwDNA[i, j] * 1000)) + ",";
                //save.fwDNA[i].myWeight[j] = fwDNA[i, j];
            }
        }
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 13; ++j)
            {
                sTemp += (int)(System.Math.Truncate((double)fwDNA2[i, j] * 1000)) + ",";
                //save.fwDNA[i].myWeight[j] = fwDNA[i, j];
            }
        }
        PlayerPrefs.SetString("LocalDNAdata", sTemp);
    }
    public void getSaveData()
    {
        //NDArray kwDNA = np.zeros(5, 5);
        //NDArray fwDNA = np.zeros(4, 6);
        string[] dataArr = PlayerPrefs.GetString("LocalDNAdata").Split(',');
        if (dataArr.Length < 10) return;
        int count = 0;
        try
        {
            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    kwDNA[i, j] = System.Convert.ToInt32(dataArr[count++]) * 0.001f;
                }
            }
            //kwDNAs.Add(kwDNA);
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 6; ++j)
                {
                    fwDNA[i, j] = System.Convert.ToInt32(dataArr[count++]) * 0.001f;
                }
            }
            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 13; ++j)
                {
                    fwDNA2[i, j] = System.Convert.ToInt32(dataArr[count++]) * 0.001f;
                }
            }
        }
        catch (System.IndexOutOfRangeException)
        {

        }
        //fwDNAs.Add(fwDNA);
    }

    private void Awake()
    {
        //kwDNAs = new List<NDArray>();
        //fwDNAs = new List<NDArray>();
        changeMobPointer = new List<Vector2Int>();
        kwDNA = np.random.rand(new int[2] { 5, 4 });
        fwDNA = np.random.rand(new int[2] { 4, 6 });
        fwDNA2 = np.random.rand(new int[2] { 5, 13 });
        mapTileAllData = np.zeros((2, 19, 19)).astype(NPTypeCode.Float);
        //?monsterLiveDeathArray = np.zeros(19, 19);
        instance = this;
        getSaveData();
        //setSaveData();
    }
    void convolutionAdvanced(NDArray map, NDArray kernel, int order, int stride = 2, int padding = 0) // �Է� : 29x29 ��� : 14x14
    {
        int kernel_height = kernel.shape[0];
        int kernel_width = kernel.shape[1];
        //int out_h = (map.shape[0] + 2 * padding - kernel_height) / stride + 1;
        //int out_w = (map.shape[1] + 2 * padding - kernel_width) / stride + 1;

        for (int j = 0; j < 9; ++j)
        {
            if ((j * stride + kernel_height) <= map.shape[0])
            {
                for (int k = 0; k < 9; ++k)
                {
                    if ((k * stride + kernel_width) <= map.shape[1])
                    {
                        //step1AfterConv
                        featureLayer[order, j, k] = np.sum((map[(j * stride).ToString() + ":" +
                         (j * stride + kernel_height).ToString() + ", " +
                         (k * stride).ToString() + ":"
                         + (k * stride + kernel_width).ToString()] * kernel).astype(NPTypeCode.Float));
                    }
                }
            }
        }
    }
    float sumNPArr1D(NDArray arr)
    {
        float sum = 0;
        for (int i = 0; i < arr.shape[0]; ++i)
        {
            sum += float.Parse(arr[i].ToString());
        }
        return sum;
    }


    void poolingAdvancedFucA(NDArray arr, int order, int strideSize = 1, int poolingSize = 6) //�Է� : 14x14 / ��� : 9x9 / Ǯ�������� 6x6 //stride=1
    {                                                                           //�Է� : 29x29 / ��� : 9x9 / Ǯ�������� 5x5 //stride=3
        for (int i = 0; i < 9; ++i)
        {
            for (int j = 0; j < 9; ++j)
            {
                //  / (float)(arr.shape[0] * arr.shape[1])).astype(NPTypeCode.Float)); 
                featureLayer[order, i, j] = np.sum(arr[(i * strideSize).ToString() + ":" + (i * strideSize + poolingSize).ToString() + ", " + (j * strideSize).ToString() + ":" + (j * strideSize + poolingSize).ToString()].astype(NPTypeCode.Float));
            }
        }
        featureLayer[order] *= 0.005;
    }
    public List<Vector2Int> changeMobPointer;
    public List<Monster> atkEnemy;
    public Vector3 MobChangePoint(Vector2Int s, Vector2Int e)
    {
        if (!changeMobPointer.Contains(s))
        {
            Mm.tileNodeSortSet(s, TileCheck.Empty);
            setTranslucentTile(s, false);
        }
        if (!changeMobPointer.Contains(e))
        {
            setTranslucentTile(e, true);
            changeMobPointer.Add(e);
        }
        int gab = Mm.tileNodeSortSet(e, TileCheck.Enemy) - 301;
        float x = 0;
        if (gab < 3)
        {
            switch (gab)
            {
                case 1:
                    x = -0.3f;
                    break;
                case 2:
                    x = 0.3f;
                    break;
            }
        }
        else
        {
            //3
            gab -= 3;
            //0
            x = 0.3f;
            int temp = 1;
            while (gab > 2)
            {
                int iTemp = (int)Mathf.Pow(2, temp);
                if (gab - iTemp < 0)
                {
                    break;
                }
                else
                {
                    temp++;
                    gab -= iTemp;
                }
            }
            x /= Mathf.Pow(2, temp);
            x = x * (gab + 1) - 0.3f;
        }
        return new Vector3(x, 0, -x);
    }
    public Vector2Int[] moveVector;
    public int TestTimer;
    public static bool monstSetting = false;
    IEnumerator mobSetting()
    {
        for (int i = 1; i < mapSize - 1; ++i)
        {
            for (int j = 1; j < mapSize - 1; ++j)
            {
                mapTileAllData[0, i - 1, j - 1] = setUniqueNum(Mm.getTileData(i, j));
                Vector2Int hi = new Vector2Int(i, j) - Mm.playerMove;
                int far =  hi.x * hi.x + hi.y * hi.y;
                mapTileAllData[1, i - 1, j - 1] = (far > 50) ? 2.5 - far * 0.02 : 0;
            }
        }
        yield return null;
        for (int j = 0; j < 9; ++j)
        {
            for (int k = 0; k < 9; ++k)
            {
                NDArray ndTemp2 = mapTileAllData[":," + (j * 2) + ":" + (j * 2 + 3) + ", " + (k * 2) + ":" + (k * 2 + 3)];
                featureLayer[":", j, k] = np.sum(np.sum(np.concatenate((ndTemp2 * DsFilter, ndTemp2 * 0.1), axis: 0).astype(NPTypeCode.Float), axis: -1).astype(NPTypeCode.Float), axis: -1);
            }
            yield return null;
        }
        monstSetting = false;
        fLayerHab = featureLayer.sum(axis: 2, false, dtype: np.float64).sum(axis: 1, false, dtype: np.float64) *0.0127;
    }
    public static NDArray supSubData = np.ones(6);
    public void moveMonsters()
    {
        if (atkEnemy == null) atkEnemy = new List<Monster>();
        else atkEnemy.Clear();
        changeMobPointer.Clear();
        Monster.setCounter = 1;
        supSubData[0] = Mm.playerMove.x * 0.03;
        supSubData[1] = Mm.playerMove.y * 0.03;
        supSubData[2] = Mm.player.mySkillSet[1].coolTime * 0.05;
        supSubData[3] = Mm.player.mySkillSet[2].coolTime * 0.05;
        supSubData[4] = Mm.player.mySkillSet[3].coolTime * 0.05;
        NDArray addSubData = (fLayerHab * kwDNA).sum(axis: 1, false, dtype: np.float64);
        foreach (Monster mTemp in monsterList)
        {
            if (mTemp.gameObject.activeSelf && mTemp.hp >= 0)
            {
                Monster.setCounter++;
                if (mTemp.prbFar <= 21)
                {
                    mTemp.subData["0:5"] = addSubData + (fLayerHab * mTemp.kernelWeight).sum(axis: 1, false, dtype: np.float64);
                    mTemp.subData[5] = mTemp.monsterLocatePos.x * 0.05;
                    mTemp.subData[6] = mTemp.monsterLocatePos.y * 0.05;
                    //mTemp.getNetValue();
                    mTemp.StartCoroutine("softMaxBlock", true); 
                }
                else
                {
                    Vector2 vTemp = Mm.playerMove - mTemp.monsterLocatePos;
                    if (Monster.setCounter%2 == 0) vTemp.x *= 0.5f;
                    else vTemp.y *= 0.5f;
                    //      1
                    //0     2      3
                    mTemp.result[0].num = 0;
                    mTemp.result[0].gab = -vTemp.x;

                    mTemp.result[1].num = 1;
                    mTemp.result[1].gab = vTemp.y;

                    mTemp.result[2].num = 2;
                    mTemp.result[2].gab = -vTemp.y;

                    mTemp.result[3].num = 3;
                    mTemp.result[3].gab = vTemp.x;

                    mTemp.StartCoroutine("softMaxBlock", false);
                }
            }
        }
    }
    public void changeMonstersWeight()
    {
        foreach (Monster mTemp in monsterList)
        {
            if(mTemp.CMWlist.Count > 0)
            {
                foreach(int i in mTemp.CMWlist)
                {
                    if (i >= 0) mTemp.changeMyWeight(i, 0.2);
                    else mTemp.changeMyWeight(-i - 1, -0.2);
                }
                mTemp.CMWlist.Clear();
            }
        }
        monstSetting = false;
    }
    float setUniqueNum(TileCheck x)
    {
        /*
            �� =0
            �÷��̾� = 1
            ���� = 2
            ������ = 3
         */
        switch (x)
        {
            case TileCheck.Block:
                return -0.06f;
            case TileCheck.Player:
                return 1.1f;
            case TileCheck.Enemy:
                return 0.25f;
            case TileCheck.Item:
                return 0.1f;
            case TileCheck.EnemyDie:
                return -0.02f;
            default:
                return 0;
        }
    }

    public List<Monster> MonsterTomb;
    public int killPoint;
    public void PlaceMonster(int numMonster)
    {
        int h = Mm.cMap.GetLength(0) - 1;
        int w = Mm.cMap.GetLength(1) - 1;
        Vector2Int rand = Vector2Int.zero;
        int breakCnt;
        for (int i = 0; i < numMonster; ++i)    // �Էµ� ���ڸ�ŭ ������ Ÿ�Ͽ� ���� ��ġ
        {
            breakCnt = 0;
            while (++breakCnt < 200)
            {
                Vector2Int vTemp = new Vector2Int(Random.Range(1, w), Random.Range(1, h));
                TileCheck tc = Mm.getTileData(vTemp);
                if (tc == TileCheck.Empty || (breakCnt >= 80 && tc == TileCheck.Enemy))
                {
                    rand = vTemp;
                    break;
                }
            }
            if (rand == Vector2Int.zero) continue;
            Monster monster = Instantiate(monsterPrefab, Mm.cMap[rand.x, rand.y].tileTransform.GetChild(0)).GetComponent<Monster>();
            monster.transform.localPosition = new Vector3(0, 0.85f, -10);
            monster.MonsterSetting(rand, 0);
            monsterList.Add(monster);
        }
    }
    public void PlaceMonster(Monster m, int t)
    {
        int h = Mm.cMap.GetLength(0) - 1;
        int w = Mm.cMap.GetLength(1) - 1;
        Vector2Int ran = Vector2Int.zero;
        int breakCnt = 0;
        while (++breakCnt < 200)
        {
            Vector2Int vTemp = new Vector2Int(Random.Range(1, w), Random.Range(1, h));
            TileCheck tc = Mm.getTileData(vTemp);
            if (tc == TileCheck.Empty || (breakCnt >= 80 && tc == TileCheck.Enemy))
            {
                ran = vTemp;
                break;
            }
        }
        m.transform.parent = Mm.cMap[ran.x, ran.y].tileTransform.GetChild(0);
        m.transform.localPosition = new Vector3(0, 0.85f, -10);

        for (int i = 0; i < 2; ++i)
        {
            Vector2Int vTemp;
            if (i == 0) vTemp = m.monsterLocatePos2;
            else if (m.monsterLocatePos2 == m.monsterLocatePos) break;
            else vTemp = m.monsterLocatePos;
            if (Mm.cMap[vTemp.x, vTemp.y].tileTransform.GetChild(0).childCount == 0)
            {
                Mm.tileNodeSortSet(vTemp, MapManager.TileCheck.Empty);
            }
        }

        m.MonsterSetting(ran, t);
        m.gameObject.SetActive(true);
        //monsterList.Add(m);
    }
    public void setTranslucentTile(Vector2Int pos, bool invisible)
    {   // �ڿ� 
        try
        {
            if (Mm.getTileData(pos.x + 1, pos.y - 1)==TileCheck.Block)
            {
                if (Mm.cMap[pos.x + 1, pos.y - 1].tileTransform.childCount > 1)
                {
                    Transform temp = Mm.cMap[pos.x + 1, pos.y - 1].tileTransform.GetChild(1).GetChild(0);
                    //Color color = temp.GetComponent<SpriteRenderer>().color;
                    Color color = Color.white;
                    color.a = invisible ? 0.35f : 1;
                    temp.GetComponent<SpriteRenderer>().color = color;
                }
            }
        }
        catch (System.NullReferenceException)
        {

        }
    }
    public Monster getMonster(Vector2Int pos)
    {
        for (int i = 0; i < monsterList.Count; ++i)
        {
            if (monsterList[i].monsterLocatePos == pos)
                return monsterList[i];
        }
        return null;
    }

    /*
    1. �Է�
    �ʵ����� : tileNode�� int code, ���� �ִ´� (GameManager.instance.cMap)
    ���� ü��, ����, ��ų �ڽ�Ʈ
    ���� ��ų ���� : �ֺ� Ÿ�� ��ȭ��Ű�°ɷ� üũ
    */
}


/*
�� �� ����
1. ���� �ൿ 1ȸ
2. ����, �� ���� �ൿ 1ȸ
�� �����Ѵ�

�⺻ ���� �ൿ - �÷��̾�� �ٰ�����

������ �ൿ
1. �Ϲݰ���
2. ��ų���
- ��ų ��Ÿ��or����or��ų��밡�� Ƚ�� �� ���� ��ų ��� ��
3. �̵�

<��ǥ>
1. ���鰣�� ��ġ������ ���� �˰��ִ� ���¿��� ����
2. ������ ���¿� ���� ������ ������� ã�� �ൿ
- ���� ��ų(���ѵ�����, ���� Ÿ��) ��� �����Ұ�� ����
- ���� ��ų(���� ���� ������, �ټ� Ÿ��) ��� �����Ұ�� ���� ������ ȥ�� �������� ����
- ü�� �� ����/��ų���Ƚ�� ������ ��� �ش� �ڿ��� ȸ���� �� �ִ� ������ ��ó�� ���� �̵�
3. ������ �ൿ�� �н�
- ������ ��ų��� �󵵸� �н��ϰ� 2���� ���� ������ ����
- ������ ��ġ�κ��� ���� ���� �ȿ� �ټ��� ���� �����Ҷ����� ���� �� ����� �ٰ��� ����
- ������ �ֱ� �̵���ο� ���� �̵������� �����ϰ� �ش������ ���θ� ���� �������� ���� �̵�


1. �Է�
�ʵ����� : tileNode�� int code, ���� �ִ´� (GameManager.cMap)
���� ü��, ����, ��ų �ڽ�Ʈ
���� ��ų ���� : �ֺ� Ÿ�� ��ȭ��Ű�°ɷ� üũ

2. �Ű��
�ۼ�Ʈ���� �Ұ���

3. ���
�������� �����¿� ����, ���� 5���� �ൿ�� �ϳ��� �ǽ��� // 5�� �� �ϳ� ����, �����¿� �� �ϳ��� �ൿ�ϰ� ���� ������ �ִٸ� ������ ��� ���� ���

4. �� 
�����ֱ⿡ �Է��� ����������
�������� �ٰ����� 1��
�־����� 0��
����Ÿ�ݽ� 5��
-> ��ȭ�н� ���
=> �׾����� �������� ��

5. �н�
������ ���
*/