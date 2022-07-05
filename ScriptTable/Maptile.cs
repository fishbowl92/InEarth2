using UnityEngine;

// �� ������� / ���� �����ϱ����� ScriptableObject
[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Object Asset/Map Data")]
public class Maptile : ScriptableObject
{
    // �� ���� �� ����
    public GameObject tile;
    [Range(0, 10)]
    public int numberOfItem = 2;
    [Range(0, 10)]
    public int numberOfSteps = 2;
    [Range(0, 10)]
    public int deathLimit = 4;
    [Range(0, 10)]
    public int birthLimit = 3;

    [System.Serializable]
    public struct Map
    {
        public Sprite tileImage;   // ��Ÿ�� �̹���
        public int useFreq;         // ��Ÿ�� �̹��� ��� ��
        public int tileNum;
    }

    //public Map[] maptileBlock;
   // public Map[] maptileWall;
    public int width, height, chanceToStartAlive;
    public Sprite[] spriteSet;


    // ���� ����
    [System.Serializable]
    public struct MonsterData
    {
        public string monsterName;     // ���� �̸�
        public string monsterCode;

        public Sprite[] monsterImage;     // ���� �̹���
        public int count;
        [TextArea(10, 20)]
        public string infoText;
    }

    public MonsterData[] monsterData;
}