using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH.Ads
{
    public interface IAdsView
    {
        // void Init(AdManager adManager, string ID);
        void Load();
        void CheckLoad();
        bool IsAdsLoaded();
        void Show(Action callback,string placement);
        void ShowReplace(Action callback, string placement);
        void Hide();
    }
}