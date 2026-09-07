using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class HardLevelAction : MonoBehaviour
    {
        public void OnDoneAnim() 
        {
            gameObject.SetActive(false);
        }
    }
}
