using System;
using System.Collections;
using System.Collections.Generic;
using BMH.Ads;
using BrickEscape;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField] private Image loadingFillImage;
    [SerializeField] List<string> loadingTexts = new List<string>();
    [SerializeField] private TextMeshProUGUI loadingText;
    private float timer;
    private float textTimer;

    private int textIndex;
    private Tweener loadingAnimation;
    IEnumerator Start()
    {
        var adManager = AdManager.Instance;
        timer = 0;
        textTimer = 0;
        loadingText.text = loadingTexts[0];
        loadingFillImage.fillAmount = 0;
        //yield return new WaitUntil(() => adManager.IsLoaded(AdsType.AppOpen) || timer > 15);
        //CheckFirstSession();
        adManager.InitAdsInGame(false);
        yield return new WaitForSeconds(1);
        loadingAnimation = DOVirtual.Float(0, 1, 2, (x) => loadingFillImage.fillAmount = x)
            .OnComplete(delegate
            {
                loadingAnimation = null;
                SceneManager.LoadScene(StringConsts.SCENE_GAMEPLAY);
            });
        //loadingAnimation.Kill();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        textTimer += Time.deltaTime;
        if (textTimer >= 0.5f)
        {
            textTimer = 0;
            textIndex++;
            if (textIndex >= loadingTexts.Count) textIndex = 0;
            loadingText.text = loadingTexts[textIndex];
        }
    }

    void CheckFirstSession()
    {
        bool isFirstSession = PlayerPrefs.GetInt(StringConsts.FIRST_SESSION, 1) == 1;
        if (isFirstSession)
        {
            PlayerPrefs.SetInt(StringConsts.FIRST_SESSION, 0);
            PlayerPrefs.Save();
        }
        else
        {
            //AdManager.Instance.Show(AdsType.AppOpen);
        }
    }
}
