using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Object Asset/Character Data")]
public class Character : ScriptableObject
{
    public string CharacterName;
    public string nameC0de;
    public int life;
    public int mainState;
    public int[] baseState;
    public Skill basicSkill;
    public Skill[] skillSetting;
    public GameObject ImgSet;


    public AnimatorOverrideController anim;
    public TraitDatas[] myTraitData;
    public int[] myTraitSelectData;
    public AudioClip myBGM;
    [System.Serializable]
    public struct TraitDatas
    {
        public Trait[] tds;
    }
    public void checkTaritData(Player p)
    {
        int lev = getCharLevAndExp().x;
        for (int i = 0; i < myTraitSelectData.Length; ++i)
        {
            if ((i + 1) * 10 < lev) break;
            if(myTraitSelectData[i] > 0)
            {
                myTraitData[i].tds[myTraitSelectData[i] - 1].myEvent.Invoke(p);
            }
        }
    }

    public int expPoint;
    // 몬스터 정보
    //[System.Serializable]
    public void ChangeImgeSet(Transform target)
    {
        target.gameObject.SetActive(false);
        Transform t = target.Find("Img");
        //Transfomr (IMG
            //effect
            //Sprites
            //bone_2
        try {
            Destroy(t.Find("Sprites").gameObject);
        }
        catch (System.NullReferenceException)
        {

        }
        try
        {
            Destroy(t.Find("bone_2").gameObject);
        }
        catch (System.NullReferenceException)
        {

        }
        GameObject gTemp = Instantiate(ImgSet, t);
        gTemp.transform.localPosition = Vector3.zero;
        gTemp.transform.Find("bone_2").parent = t;
        gTemp.name = "Sprites";
    }
    public Vector3Int getCharLevAndExp()
    {
        //x레벨, y경험치, z요구경험치
        int gab = expPoint;
        Vector3Int vTemp = Vector3Int.zero; 
        vTemp.x++;
        for (int i = 0; i < 100; ++i)
        {
            if(gab > DemandEXP(i))
            {
                gab -= DemandEXP(i);
                vTemp.x++;
            }
            else
            {
                vTemp.y = gab;
                break;
            }
        }
        vTemp.z = DemandEXP(vTemp.x);
        return vTemp;
    }
    public int DemandEXP(int l)
    {
        return 5 + 5 * l;
    }
}
