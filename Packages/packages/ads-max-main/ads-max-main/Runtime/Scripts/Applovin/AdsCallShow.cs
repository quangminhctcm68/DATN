using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH.Ads
{
    public enum AsdStatus
    {
        None = 0,
        Loading = 1,
        Loaded = 2,
        Showing = 3,
    }

    public abstract class AdsCallShow
    {
        protected AdManager adManager;
        protected string adUnit;
        protected int countLoadFail;
        protected Action onAdsClosed;
        protected AsdStatus status;
        protected DateTime lastLoadAds;

        protected AdsCallShow(AdManager adManager, string adUnit)
        {
            this.adManager = adManager;
            this.adUnit = adUnit;
        }

        public bool CanLoadAds()
        {
            if (countLoadFail >= 5) return false;
            if (status == AsdStatus.None)
            {
                if (DateTime.Now.Subtract(lastLoadAds).TotalSeconds >= Math.Pow(2, Math.Min(6, countLoadFail)))
                    return true;
            }

            return false;
        }

        public void LoadFailed()
        {
            countLoadFail++;
        }

        public void LoadSusscess()
        {
            countLoadFail = 0;
        }

        public string ToStringTimeLoad()
        {
            return $"{(int)DateTime.Now.Subtract(lastLoadAds).TotalSeconds}";
        }
    }
}