using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

[CreateAssetMenu(fileName = "WeightData", menuName = "Scriptable Object Asset/Weight Data")]
public class WeightTkinter : ScriptableObject
{

    [System.Serializable]
    public class weightSet
    {
        public double[] myWeight;
    }
    public weightSet[] kwDNA = new weightSet[5];  // 사망 가중치
    public weightSet[] fwDNA = new weightSet[4];    // 공격 가중치
    public weightSet[] fwDNA2 = new weightSet[9];   // 이동 가중치
    [ContextMenu("getRandValue")]
    public void setSaveData()
    {
        for (int i = 0; i < 5; ++i)
        {
            kwDNA[i].myWeight = new double[4];
            for (int j = 0; j < 4; ++j)
            {
                kwDNA[i].myWeight[j] = Random.Range(-1.0f, 1.0f);
            }
        }
        for (int i = 0; i < 4; ++i)
        {
            fwDNA[i].myWeight = new double[6];
            for (int j = 0; j < 6; ++j)
            {
                fwDNA[i].myWeight[j] = Random.Range(-1.0f, 1.0f);
            }
        }
        for (int i = 0; i < 5; ++i)
        {
            fwDNA2[i].myWeight = new double[13];
            for (int j = 0; j < 13; ++j)
            {
                fwDNA2[i].myWeight[j] = Random.Range(-1.0f, 1.0f);
            }
        }
    }
}
