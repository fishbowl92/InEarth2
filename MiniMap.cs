using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public void StageStart()
    {
        MapManager.Instans.StartGame(3);
    }
    public Transform myMap;
    public void changeMapSize(int a)
    {
        Vector2 size = new Vector2(880, 0);
        size.x += a * 240;
        myMap.GetComponent<RectTransform>().sizeDelta = size;
        MapManager.Instans.StartGame(a);
    }
}
