using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeAnimForUI : MonoBehaviour
{
    public Vector3 sPoint;
    public Vector3 ePoint;
    public string what;
    public void starting(Vector3 s, Vector3 e)
    {
        sPoint = s;
        ePoint = e;
        transform.position = sPoint;
        StartCoroutine("startAnim");
    }
    IEnumerator startAnim()
    {
        float r = Random.Range(85, 200);
        float rad = 0;
        float maxR = r * 1.5f - Random.Range(-10, 50);
        float timer = 0;
        float rSpeed = (Random.Range(0, 2) == 0)? Random.Range(2, 7) : Random.Range(-2, -7);
        bool goFor = false;
        float seed = Random.Range(-Mathf.PI, Mathf.PI);
        while (timer < 1 || rad > 0)
        {
            yield return null;
            transform.position = Vector3.Lerp(sPoint, ePoint, Mathf.Min(1, timer)) 
                + new Vector3(Mathf.Sin((timer + seed) * rSpeed), Mathf.Cos((timer + seed) * rSpeed), 0) * rad;
            if (!goFor)
            {
                rad += Time.deltaTime * maxR * 2;
                if (rad > r) goFor = true;
            }
            else if(rad > 0)
            {
                rad -= Time.deltaTime * maxR;
            }
            
            timer += Time.deltaTime * 2;
        }
        yield return null;
        gameObject.SetActive(false);
    }
}
