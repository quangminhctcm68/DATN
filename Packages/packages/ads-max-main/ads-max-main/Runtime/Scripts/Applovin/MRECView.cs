using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH.Ads
{
    public enum MRECPos
    {
        TopCenter = 0,
        BottomCenter = 1,
        CenterLeft = 2,
        CenterRigh = 3,
        Centered = 4,
        TopCenterCenter = 5,
        BottomCenterCenter = 6,
        CenterCenterLeft = 7,
        CenterCenterRigh = 8,
        TopLeft = 9,
        TopRight = 10,
        BottomLeft = 11,
        BottomRight = 12,
    }

    public class MRECView : IAdsView
    {
        private AdManager adManager;
        private string adUnit;

        public MRECView(AdManager adManager, string ID)
        {
            this.adManager = adManager;
            this.adUnit = ID;
            Load();
        }

        public void Load()
        {
            if (!adManager.adsConfig.shouldShowMREC) return;
            if (adUnit == String.Empty) return;
#if BMH_APPLOVIN_MAX
            switch (adManager.adsConfig.mrecPosition)
            {
                case MRECPos.TopLeft:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.TopLeft);
                    break;
                case MRECPos.TopCenter:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.TopCenter);
                    break;
                case MRECPos.TopRight:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.TopRight);
                    break;
                case MRECPos.Centered:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.Centered);
                    break;
                case MRECPos.CenterLeft:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.CenterLeft);
                    break;
                case MRECPos.CenterRigh:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.CenterRight);
                    break;
                case MRECPos.BottomLeft:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.BottomLeft);
                    break;
                case MRECPos.BottomCenter:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.BottomCenter);
                    break;
                case MRECPos.BottomRight:
                    MaxSdk.CreateMRec(adUnit, MaxSdkBase.AdViewPosition.BottomRight);
                    break;
                case MRECPos.TopCenterCenter:
                case MRECPos.BottomCenterCenter:
                case MRECPos.CenterCenterLeft:
                case MRECPos.CenterCenterRigh:
                    var mrec_px =
                        ExtensionApplovinModule.GetPositionBannerExtra(adManager.adsConfig.mrecPosition,
                            new Vector2(300, 250));
                    MaxSdk.CreateMRec(adUnit, mrec_px.Item1, mrec_px.Item2);
                    break;
            }

            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
#endif
        }

        private void OnMRecAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            adManager.OnAdRevenuePaidEvent(adUnit, arg2, AdsType.MREC);
        }

        private void OnMRecAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
        }

        private void OnMRecAdFailedEvent(string arg1, MaxSdkBase.ErrorInfo arg2)
        {
        }

        private void OnMRecAdLoadedEvent(string arg1, MaxSdkBase.AdInfo arg2)
        {
            Debug.Log("Load MREC Success");
        }

        public void CheckLoad()
        {
        }

        public bool IsAdsLoaded()
        {
            return false;
        }

        public void Show(Action callback, string placement)
        {
#if BMH_APPLOVIN_MAX
            if (!adManager.adsConfig.shouldShowMREC) return;
            MaxSdk.ShowMRec(adUnit);
#endif
        }

        public void ShowReplace(Action callback, string placement)
        {
        }

        public void Hide()
        {
#if BMH_APPLOVIN_MAX
            if (!adManager.adsConfig.shouldShowMREC) return;
            MaxSdk.HideMRec(adUnit);
#endif
        }

        public void UpdatePosition()
        {
#if BMH_APPLOVIN_MAX
            switch (adManager.adsConfig.mrecPosition)
            {
                case MRECPos.TopLeft:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.TopLeft);
                    break;
                case MRECPos.TopCenter:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.TopCenter);
                    break;
                case MRECPos.TopRight:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.TopRight);
                    break;
                case MRECPos.Centered:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.Centered);
                    break;
                case MRECPos.CenterLeft:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.CenterLeft);
                    break;
                case MRECPos.CenterRigh:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.CenterRight);
                    break;
                case MRECPos.BottomLeft:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.BottomLeft);
                    break;
                case MRECPos.BottomCenter:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.BottomCenter);
                    break;
                case MRECPos.BottomRight:
                    MaxSdk.UpdateMRecPosition(adUnit, MaxSdkBase.AdViewPosition.BottomRight);
                    break;
                case MRECPos.TopCenterCenter:
                case MRECPos.BottomCenterCenter:
                case MRECPos.CenterCenterLeft:
                case MRECPos.CenterCenterRigh:
                    var mrec_px =
                        ExtensionApplovinModule.GetPositionBannerExtra(adManager.adsConfig.mrecPosition,
                            new Vector2(300, 250));
                    MaxSdk.UpdateMRecPosition(adUnit, mrec_px.Item1, mrec_px.Item2);
                    break;
            }
#endif
        }
    }

    public class ExtensionApplovinModule
    {
        public static (float, float) GetPositionBannerExtra(MRECPos mrecPos, Vector2 size)
        {
            // Lấy kích thước màn hình
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Lấy density (mật độ điểm ảnh)
            float density = MaxSdkUtils.GetScreenDensity();

            // Chuyển đổi từ dp sang pixel
            // float mrecWidth_px = 300 * density;
            // float mrecHeight_px = 250 * density;
            float bannerWith_px = size.x * density;
            float bannerHeight_px = size.y * density;
            float centerX_px = 0, centerY_px = 0, pX = 0, pY = 0;
            switch (mrecPos)
            {
                case MRECPos.TopCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    break;
                case MRECPos.BottomCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight - bannerHeight_px;
                    break;
                case MRECPos.CenterLeft:
                    centerY_px = screenHeight / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case MRECPos.CenterRigh:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth - bannerWith_px;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case MRECPos.Centered:
                    centerX_px = screenWidth / 2;
                    centerY_px = screenHeight / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case MRECPos.TopCenterCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight / 4 - (bannerHeight_px / 2);
                    break;
                case MRECPos.BottomCenterCenter:
                    centerX_px = screenWidth / 2;
                    pX = centerX_px - (bannerWith_px / 2);
                    pY = screenHeight * 3 / 4 - (bannerHeight_px / 2);
                    break;
                case MRECPos.CenterCenterLeft:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth / 4 - bannerWith_px / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case MRECPos.CenterCenterRigh:
                    centerY_px = screenHeight / 2;
                    pX = screenWidth * 3 / 4 - bannerWith_px / 2;
                    pY = centerY_px - (bannerHeight_px / 2);
                    break;
                case MRECPos.TopLeft:
                    pX = 0;
                    pY = 0;
                    break;
                case MRECPos.TopRight:
                    pX = screenWidth - bannerWith_px;
                    pY = 0;
                    break;
                case MRECPos.BottomLeft:
                    pX = 0;
                    pY = screenHeight - bannerHeight_px;
                    break;
                case MRECPos.BottomRight:
                    pY = screenHeight - bannerHeight_px;
                    pX = screenWidth - bannerWith_px;
                    break;
            }

            // mrecX_px += rateX;
            // mrecY_px += rateY;

            // Chuyển đổi tọa độ gốc từ pixel sang dp
            float mrecX_dp = pX / density;
            float mrecY_dp = pY / density;
            return (mrecX_dp, mrecY_dp);
        }
    }
}