using System.Collections;
using System.Collections.Generic;
using NabaGame.UI;
using UnityEngine;

namespace BrickEscape
{
    public class NoNetworkPanel : BaseUI
    {
        public bool IsActive;

        private void OnEnable()
        {
            IsActive = true;
        }

        private void OnDisable()
        {
            IsActive = false;
        }
        
        public void OnTap()
        {
            gameObject.SetActive(false);
        }
        
        public void SetShow()
        {
            if (!IsActive)
            {
                gameObject.SetActive(true);
            }
        }
        
        public void SetHide()
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
