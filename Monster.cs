using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
public class Monster : MonoBehaviour
{
    MonsterManager Monm;
    MapManager Mapm;

    public SpriteRenderer img;

    public string monsterName;
    public int code;        // 몬스터 번호
    public int hp;
    public int Maxhp;
    public int sCount;
    public int lifeCycle;  // 몬스터 생명주기
    public Vector2Int monsterLocatePos;
    public Vector2Int monsterLocatePos2;
    //public NDArray monsterAndPlayerMoveLimit = np.zeros(29, 29);
    public NDArray kernelWeight = np.random.rand(new int[2] { 5, 4});
    public NDArray fullyConnectedArr = np.random.rand(new int[2] { 4, 6 });
    public NDArray fullyConnectedArr2 = np.random.rand(new int[2] { 5, 13 });
    public NDArray subData = np.ones(7);
    public NDArray layer2 = np.ones(6);
    public List<sortVector2> result;
    public class sortVector2
    {
        public int num;
        public double gab;
        public sortVector2()
        {
            num = 0;
            gab = 0;
        }
        public sortVector2(int i, double g)
        {
            num = i;
            gab = g;
        }
    }
    private void Awake()
    {
        //subData = np.zeros(10);
        Monm = MonsterManager.instance;
        Mapm = MapManager.Instans;
        CMWlist = new List<int>();
        result = new List<sortVector2>(4);
        for(int i = 0; i < 4; ++i) result.Add(new sortVector2());
    }
    IEnumerator softMaxBlock(bool hi)
    {
        if (hi)
        {
            if (setCounter % 2 == 0) yield return MapManager.fixWs;
            layer2["0:5"] = np.maximum(0.0, (fullyConnectedArr2 *
                np.concatenate((subData, MonsterManager.supSubData)))
                .sum(axis: 1, false, dtype: np.float64), outType: np.float64);
            yield return MapManager.fixWs;

            NDArray nTemp = (fullyConnectedArr * layer2).sum(axis: 1, false, dtype: np.float64);
           // Debug.Log(nTemp);
            for (int i = 0; i < 4; ++i)
            {
                //vTemp.y = (float)fullyConnectedArr[i, 0];
                //vTemp.gab = ().sum(axis: 0, false, dtype: np.float64);
                //vTemp.y *= 0.001f;
                result[i].num = i;
                result[i].gab = nTemp[i];
            }
            //softMax();
            //prebOutPut, 반환값을 통한 움직임 시작
        }
        result.Sort(delegate (sortVector2 a, sortVector2 b)
        {
            if (a.gab < b.gab) return 1;
            else if (a.gab > b.gab) return -1;
            else return 0;
        });
        MapManager.TileCheck tc = MoveMonster(hi);
        if(tc != MapManager.TileCheck.Block)
        {
            Vector3 endPoint = Mapm.cMap[monsterLocatePos2.x, monsterLocatePos2.y].tileTransform.position + new Vector3(0, 0.85f, -10);
            img.transform.localRotation = (img.flipX) ? Quaternion.Euler(0, 0, 15) : Quaternion.Euler(0, 0, -15);
            transform.position = Vector3.Lerp(transform.position, endPoint, 0.25f) + new Vector3(0, 0.03f, 0);
            yield return new WaitForSeconds(0.06f);
            img.transform.localRotation = (img.flipX) ? Quaternion.Euler(0, 0, 35) : Quaternion.Euler(0, 0, -35);
            transform.position = Vector3.Lerp(transform.position, endPoint, 0.5f) + new Vector3(0, 0.07f, 0);
            yield return new WaitForSeconds(0.06f);
            if (tc == MapManager.TileCheck.Player || monsterLocatePos2 == Mapm.playerMove)
            {
                monsterLocatePos2 = monsterLocatePos;
                Monm.atkEnemy.Add(this);
                if (!Monm.changeMobPointer.Contains(monsterLocatePos)) Monm.changeMobPointer.Contains(monsterLocatePos);
                //transform.localPosition = Monm.MobChangePoint(monsterLocatePos, monsterLocatePos) + new Vector3(0, 0.85f, -10);
            }
            else
            {
                img.transform.localRotation = (img.flipX) ? Quaternion.Euler(0, 0, -15) : Quaternion.Euler(0, 0, 15);
                transform.position = Vector3.Lerp(transform.position, endPoint, 0.75f) + new Vector3(0, 0.03f, 0);
                yield return new WaitForSeconds(0.06f);
                img.transform.localRotation = Quaternion.Euler(0, 0, 0);
                transform.parent = Mapm.cMap[monsterLocatePos2.x, monsterLocatePos2.y].tileTransform.GetChild(0);
                transform.localPosition = Monm.MobChangePoint(monsterLocatePos, monsterLocatePos2) + new Vector3(0, 0.85f, -10);
                monsterLocatePos = monsterLocatePos2;
            }
            if (--setCounter == 1) setCounter = 0;
        }
    }
    IEnumerator enemyAtkToPlayer()
    {
        WaitForSeconds ws = new WaitForSeconds(0.05f);
        score += 0.1f;
        img.transform.localRotation = (img.flipX) ? Quaternion.Euler(0, 0, -15) : Quaternion.Euler(0, 0, 15);
        Vector3 endPoint = Mapm.cMap[Mapm.playerMove.x, Mapm.playerMove.y].tileTransform.position + new Vector3(0, 0.85f, -10);
        transform.position = Vector3.Lerp(transform.position, endPoint, 0.8f);
        yield return ws;

        bool counter = Mapm.player.PlayerGetDmg(this);
        Transform temp = Mapm.effectManageToShow((counter) ? 1 : 0);
        temp.position = transform.position;
        temp.gameObject.SetActive(true);

        img.transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.position = Vector3.Lerp(transform.position, endPoint, 0.6f);
        yield return ws;
        Mapm.atkSoundPlayer(0);
        //transform.position = Mapm.cMap[monsterLocatePos.x, monsterLocatePos.y].tileTransform.position + new Vector3(0, 0.85f, -10);
        transform.localPosition = new Vector3(0, 0.85f, -10);
        if (counter)
        {
            Mapm.player.anim.SetTrigger("Counter");
            Mapm.atkSoundPlayer(1);
            Mapm.player.PlayerAtk(this, 0);
            yield return ws;
        }
        Monm.atkEnemy.Remove(this);
        MapManager.monsterMoveEnd = false;
    }
    public int prbMove;
    public float prbFar;
    int chamCount;
    float far;
    public List<int> CMWlist;
    public MapManager.TileCheck MoveMonster(bool hi)
    {
        //go monsterLocatePos2
        //0  1
        //2  3
        int i, gab = 0;
        MapManager.TileCheck tc = MapManager.TileCheck.Block;
        //  Debug.Log(result[0].gab +"("+result[0].num+")" + "," + result[1].gab + "(" + result[1].num + ")" + "," 
        //     + result[2].gab + "(" + result[2].num + ")" + "," + result[3].gab + "(" + result[3].num + ")");
        if (hi)
        {
            far = Vector2.SqrMagnitude(Mapm.playerMove - monsterLocatePos2 - Monm.moveVector[result[0].num]);
            if (far < 3) score += 0.025f; //
            if (prbFar > far) score += 0.01f;
            else if (result[0].num == prbMove)
            {
                if (++chamCount > 2)
                {
                    //changeMyWeight(prbMove, 0.2);
                    CMWlist.Add(prbMove);
                    chamCount = 0;
                }
            }
            else
            {
                //changeMyWeight(result[0].num, 0.1);
                Vector2Int vTemp = Mapm.playerMove - monsterLocatePos2;
                if (vTemp.x > 0) CMWlist.Add(-4);
                else if (vTemp.x < 0) CMWlist.Add(-1);
                if (vTemp.y > 0) CMWlist.Add(-2);
                else if (vTemp.y < 0) CMWlist.Add(-3);
                //if (vTemp.x > 0) changeMyWeight(3, -0.2);
                //else if (vTemp.x < 0) changeMyWeight(0, -0.2);
                //if (vTemp.y > 0) changeMyWeight(1, -0.2);
                //else if (vTemp.y < 0) changeMyWeight(2, -0.2);
                score -= 0.01f;
            }
            prbMove = result[0].num;
        }

        int a = -1;
        for (i = 0; i < 4; ++i)
        {
            gab = result[i].num;
            tc = Mapm.getTileData(monsterLocatePos + Monm.moveVector[gab]);
            if(tc != MapManager.TileCheck.Block)
            {
                if (a < 0) a = gab;
            }
            if(hi &&tc == MapManager.TileCheck.Player)
            {
                if (i != 0)
                {
                    //double p = np.abs(result[i].gab) + 1;
                    //changeMyWeight(gab, -0.5);
                    CMWlist.Add(-gab - 1);
                    //changeMyWeight(gab, ((result[i].gab + p) / (result[0].gab + p)) *0.5 - 0.5);
                }
                a = gab;
                break;
            }
        }
        if (a < 0)
        {
            if (--setCounter == 1) setCounter = 0;

            hp = -1;
            prbFar = 0;
            gameObject.SetActive(false);
            Monm.MonsterTomb.Add(this);

            return MapManager.TileCheck.Block;
        }
        gab = Mathf.Clamp(a, 0, 3);
        tc = Mapm.getTileData(monsterLocatePos + Monm.moveVector[gab]);
        monsterLocatePos2 = monsterLocatePos + Monm.moveVector[gab];
        prbFar = Vector2.SqrMagnitude(Mapm.playerMove - monsterLocatePos2);

        img.flipX = gab == 1 || gab == 3;
        return tc;
    }
    /*public void SetMyData()
    {
        Vector2Int p = monsterLocatePos2;
        for (int i = -1; i < 2; ++i)
        {
            for (int j = -1; j < 2; ++j)
            {
                try
                {
                    float gab = (i == 0 && j == 0) ? 3 : 2;
                    monsterAndPlayerMoveLimit[p.x + i, p.y + j] = gab;
                    monsterAndPlayerMoveLimit[monsterLocatePos.x + i, monsterLocatePos.y + j] = 0;
                }
                catch (System.IndexOutOfRangeException)
                {

                }
            }
        }
        monsterLocatePos = p;
    }*/
    //kernelWeight = Monm.kwDNA + (kernelWeight - Monm.kwDNA) * score * 0.003f;
    //fullyConnectedArr = Monm.fwDNA + (fullyConnectedArr - Monm.fwDNA) * score * 0.005f;

    public void changeMyWeight(int a, double b)
    {
        //result  :4

        //         4
        //fullyca 10  W

        //layer2  10

        //         9
        //fullyca2 13  w

        //subData 13  x

        //w1 -= x1W1, sub*co
        //w      x     co2      co    (b = 0 ~ 9) (sub = ~13) 
        // w1 = sub[0][b,0] -> [a, b]
        // w3 = sub[1][b,1] -> [a, b]
        // w5 = sub[2][b,2] -> [a, b]

        //b *= 0.1;
        fullyConnectedArr2 -= (np.ones(13, 5) * fullyConnectedArr[a + ",0:5"]).T
            * np.concatenate((subData, MonsterManager.supSubData)) * b;//subData * fullyConnectedArr[a]
        //Debug.Log(layer2);
        fullyConnectedArr[a + ",0:5"] -= layer2["0:5"] * b;
    }
    public int monsterTear;
    public int myCodeGab;
    public void MonsterSetting(Vector2Int p, int tear)
    {
        tear *= 2;
        if (Random.Range(0, 10) == 0) tear++;
        score = 0;
        monsterTear = tear;
        myCodeGab = tear % GameManager.Instans.mapData.monsterData.Length;
        monsterName = GameManager.Instans.mapData.monsterData[myCodeGab].monsterName;
        Maxhp = hp = Mapm.getEnemyLife(tear);
        Maxhp /= 2;
        sCount = 0;
        img.sprite = GameManager.Instans.mapData.monsterData[myCodeGab].monsterImage[0];
        monsterLocatePos2 = monsterLocatePos = p;
        //SetMyData();
        Monm.setTranslucentTile(p, true);
        Mapm.tileNodeSortSet(p, MapManager.TileCheck.Enemy);
        kernelWeight = np.random.rand(new int[2] { 5, 4 }) * 0.1 + Monm.kwDNA;
        fullyConnectedArr = (tear % 2 == 0) ? np.random.rand(new int[2] { 4, 6 }) * 0.1 + Monm.fwDNA :
            np.maximum(0.0, np.random.rand(new int[2] { 4, 6 }) * 0.2 + Monm.fwDNA - 0.1, outType: np.float64);
        fullyConnectedArr2 =np.random.rand(new int[2] { 5, 13 }) * 0.1 + Monm.fwDNA2;
    }
    public float score;
    public static int setCounter;
    public void MonsterBye()
    {
        if (Mathf.Abs(score) < 0.01f) return;
        Monm.kwDNA += (kernelWeight - Monm.kwDNA) * score * 0.0025;
        Monm.fwDNA += (fullyConnectedArr - Monm.fwDNA) * score * 0.015;
        Monm.fwDNA2 += (fullyConnectedArr2 - Monm.fwDNA2) * score * 0.01;
    }
    public void APuDa(int a,int skillNum=0, bool tCheck = false)
    {
        if (hp < 0)
        {
            if (gameObject.activeSelf)
            {
                hp = -1;
                prbFar = 0;

                gameObject.SetActive(false);

                transform.parent = Mapm.maptiles;
                Monm.MonsterTomb.Add(this);
            }
            return;
        }
        a = Mathf.Max(1, a);
        Transform temp = Mapm.showDmg(a,0); //0 -데미지 줌, 1- 데미지 받음 2- 회복
        //Vector3 p = transform.position + Vector3.forward * temp.position.z;
        Mapm.player.SkillTrigger(this, Skill.AtkKind.monsterIsHit, new Vector3Int(a, 0, skillNum));
        temp.position = new Vector3(transform.position.x + Random.Range(-0.1f, 0.1f),
            transform.position.y + Random.Range(0.1f, 0.2f) + 0.1f,
            temp.position.z);

        temp.gameObject.SetActive(true);
        temp.GetComponent<Animator>().SetTrigger("Enemy");
        hp -= a;
        if (sCount < 2 && Maxhp >= hp)
        {
            img.sprite = GameManager.Instans.mapData.monsterData[myCodeGab].monsterImage[++sCount];
            Maxhp /= 2;
        }
        if (hp <= 0)
        {
            if (Random.Range(0, 100) <= Mapm.player.keyGetPercentage)
            {
                Mapm.StartCoroutine("getKeyForMonster", transform.position);
                Mapm.player.SkillTrigger(null, Skill.AtkKind.isGetKey);
            }
            if (!tCheck) Mapm.player.SkillTrigger(this, Skill.AtkKind.killEnemy, new Vector3Int(-hp, 0, skillNum));
            hp = -1;
            prbFar = 0;
            

            GameManager.Instans.mapData.monsterData[myCodeGab].count++;

            Mapm.killCount[myCodeGab]++;
            gameObject.SetActive(false);
            Mapm.player.PlayerGetExp(1);

            transform.parent = Mapm.maptiles;
            Mapm.tileNodeSortSet(monsterLocatePos2, MapManager.TileCheck.EnemyDie);
            Mapm.tileNodeSortSet(monsterLocatePos, MapManager.TileCheck.EnemyDie);

            Monm.MonsterTomb.Add(this);
        }
        else
        {
            if (!tCheck) Mapm.player.SkillTrigger(this, Skill.AtkKind.atkTrigger);
        }
    }

}
