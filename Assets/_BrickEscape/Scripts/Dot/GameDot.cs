using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class GameDot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer dotRenderer;
        [SerializeField] private Color dotDefaultColor;

        #region Start, Update, Validate

        private void OnValidate()
        {
            dotRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        #endregion

        #region Getters, Setters

        public SpriteRenderer DotRenderer => dotRenderer;
        public Color DotDefaultColor => dotDefaultColor;

        #endregion

        #region Public Functions

        public void ResetColor()
        {
            dotRenderer.color = dotDefaultColor;
        }

        #endregion
    }
}
