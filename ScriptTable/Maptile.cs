using UnityEngine;

// 맵 구성요소 / 맵을 정의하기위한 ScriptableObject
[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Object Asset/Map Data")]
public class Maptile : ScriptableObject
{
    // 맵 정보 및 설정
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
        public Sprite tileImage;   // 맵타일 이미지
        public int useFreq;         // 맵타일 이미지 사용 빈도
        public int tileNum; // 맵타일 고유 숫자
    }

    //public Map[] maptileBlock;
   // public Map[] maptileWall;
    public int width, height, chanceToStartAlive;   // 배치된 좌표
    public Sprite[] spriteSet;  // 스프라이트 설정용


    // 몬스터 정보
    [System.Serializable]
    public struct MonsterData
    {
        public string monsterName;     // 몬스터 이름
        public string monsterCode;

        public Sprite[] monsterImage;     // 몬스터 이미지
        public int count;
        [TextArea(10, 20)]
        public string infoText;
    }

    public MonsterData[] monsterData;
}
