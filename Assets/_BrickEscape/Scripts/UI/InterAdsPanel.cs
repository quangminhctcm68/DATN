using BMH.Ads;
using BrickEscape;
using DG.Tweening;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.UI;
using System.Collections;
using TMPro;
using UnityEngine;

public class InterAdsPanel : MonoBehaviour
{


    [SerializeField] bool interIsReady = false;
    //[SerializeField] bool watchAdsInProgress = false;
    bool isSkipInter = false;
    public Coroutine showInterCoroutine;
    WaitForSeconds waitForSeconds;
    public int levelToShowInter = 15;
    public void SetInfo()
    {
        isSkipInter = false;
        waitForSeconds = new WaitForSeconds(90f);
        InterCount = 0;

        if (GameManager.Instance.PlayerProfile.isNoAds == 0)
            showInterCoroutine = StartCoroutine(ShowInterPopup());


        EventManager.Instance.AddListener<LevelChange>(StartShowingInter);
    }

    IEnumerator ShowInterPopup()
    {
        yield return waitForSeconds;          
#if !UNITY_EDITOR
            yield return new WaitUntil(() => AdManager.Instance.IsLoaded(AdsType.Interstitial));
#endif

        //SetupWaitInter();
        interIsReady = true;
        showInterCoroutine = null;
    }

    public void ResetShowInter()
    {
        interIsReady = false;
        if (showInterCoroutine != null)
        {
            StopCoroutine(showInterCoroutine);
            showInterCoroutine = null;
        }

        DOVirtual.DelayedCall(Time.deltaTime, () =>
        {
            if (isSkipInter) return;
            if (showInterCoroutine != null) StopCoroutine(showInterCoroutine);
            showInterCoroutine = StartCoroutine(ShowInterPopup());
        });
    }


    public int InterCount = 0;


    public void WatchAdsInter()
    {
        if (GameManager.Instance.PlayerProfile.isNoAds == 1) return;
        if (!interIsReady || GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel < levelToShowInter) return;
        AdManager.Instance.ShowInterNow("ads_break");
        OnCompletedInter();

    }

    void OnCompletedInter()
    {
        //Close();
        InterCount++;
        Debug.Log("Show Inter1");
        ResetShowInter();
        Debug.Log("Show Inter");
    }

    public void CheckOnShowNoAds() 
    {
        if (GameManager.Instance.PlayerProfile.isNoAds == 1) return;
        if (InterCount >= 5)
        {
            UIMainManager.Instance.NoAdsPanel.Show();
            InterCount = 0;
        }
    }
    public void StopInterCooldown()
    {
        if (showInterCoroutine != null)
        {
            StopCoroutine(showInterCoroutine);
            showInterCoroutine = null;
            //Debug.Log("Inter stopped");
        }

        interIsReady = false;
        isSkipInter = true;
    }

    public void StartInterCooldown()
    {
        if (showInterCoroutine == null && !isSkipInter)
        {
            showInterCoroutine = StartCoroutine(ShowInterPopup());
        }
    }

    public void RestartInterCooldown()
    {
        isSkipInter = false;
        interIsReady = false;
        if (showInterCoroutine == null)
        {
            showInterCoroutine = StartCoroutine(ShowInterPopup());
        }
        else
        {
            StopCoroutine(showInterCoroutine);
            showInterCoroutine = null;

            showInterCoroutine = StartCoroutine(ShowInterPopup());
        }

        Debug.Log("Inter restarted");
    }

    //public bool IsAdsRunning => watchAdsInProgress;
    public bool IsInterReady => interIsReady;

    void StartShowingInter(LevelChange e)
    {
        //WatchAdsInter();
    }
}
