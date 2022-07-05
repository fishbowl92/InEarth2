using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;
    public static bool hi;
    [SerializeField]
    SpriteRenderer progressBar;
    Color curtain = Color.black;
    public Animator loadingAnim;
    public Text Tip;
    public string[] Tips;
    private void Start()
    {
        Time.timeScale = 1;
        Tip.text = Tips[UnityEngine.Random.Range(0, Tips.Length)];
        progressBar.color = curtain;
        //GetComponent<Utilleti>().setSprite();
        StartCoroutine(LoadScene());
    }

    //string nextSceneName;
    public static void LoadScene(string sceneName, bool bye)
    {
        hi = bye;
        nextScene = sceneName;
        //BGMManager.instans.SetBGM("LoadingScene", 1);
        SceneManager.LoadScene("LoadingScene");
    }
    public void Doun(bool hehe)
    {
        hi = hehe;
    }
    IEnumerator LoadScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            if (op.progress >= 0.9f)
            {
                curtain.a = Mathf.Lerp(curtain.a, 0f, timer);
                progressBar.color = curtain;

                if (curtain.a == 0 && !op.allowSceneActivation)
                {
                    //if (GameManager.instans != null)
                    //{
                    //    GameManager GM = GameManager.instans;
                    //    BGMManager.instans.SetBGM(nextScene, 1);
                   // }
                    op.allowSceneActivation = true;
                }
            }
            else
            {
                curtain.a = Mathf.Lerp(curtain.a, op.progress, timer);
                if (curtain.a >= op.progress)
                {
                    timer = 0f;
                }
            }
        }

        /*while (progressBar.color.a > 0.05f)
        {
            yield return null;
            if (op.progress >= 0.85f)
            {
                curtain.a = Mathf.Lerp(curtain.a, 0f, Time.deltaTime);
                progressBar.color = curtain;  
            }
            else
            {
                curtain.a = Mathf.Lerp(curtain.a, 1f - op.progress, Time.deltaTime * 0.3f);
                progressBar.color = curtain;
            }
        }*/
        /*if (!hi)loadingAnim.SetTrigger("Start");
        while (!hi)
        {
            yield return null;
        }*/
        //op.allowSceneActivation = true;
    }
}