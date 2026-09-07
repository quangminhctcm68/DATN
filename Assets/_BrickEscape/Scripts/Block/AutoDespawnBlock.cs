using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class AutoDespawnBlock : MonoBehaviour
    {
        [SerializeField] private MoveableBlock moveableBlock;

        private void OnBecameInvisible()
        {
            moveableBlock.DespawnBlock();
        }
    }
}
