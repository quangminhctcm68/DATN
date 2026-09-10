using System.Collections;
using NabaGame.Core.Runtime.Singleton;
using UnityEngine;
#if BMH_APP_REVIEW
#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID
using Google.Play.Review;

#endif
#endif

[Singleton("AppReviewManager", true)]
public class AppReviewManager : Singleton<AppReviewManager>
{
    public override void Init()
    {
    }
#if BMH_APP_REVIEW
#if UNITY_ANDROID
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;
    private Coroutine _coroutine;
#endif

    private void Start()
    {
#if UNITY_ANDROID
        _coroutine = StartCoroutine(InitReview());
#endif
    }

    public void RateAndReview()
    {
#if UNITY_IOS
        Device.RequestStoreReview();
#elif UNITY_ANDROID
        StartCoroutine(LaunchReview());
#endif
    }

#if UNITY_ANDROID
    private IEnumerator InitReview(bool force = false)
    {
        Debug.Log("start init review");
        if (_reviewManager == null) _reviewManager = new ReviewManager();

        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            if (force) DirectlyOpen();
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();
        Debug.Log("init review complete");
    }

    public IEnumerator LaunchReview()
    {
        Debug.Log("Open Review");
        if (_playReviewInfo == null)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            yield return StartCoroutine(InitReview(true));
        }

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            DirectlyOpen();
            yield break;
        }
    }
#endif

    private void DirectlyOpen()
    {
        Application.OpenURL($"https://play.google.com/store/apps/details?id={Application.identifier}");
    }
#endif
}