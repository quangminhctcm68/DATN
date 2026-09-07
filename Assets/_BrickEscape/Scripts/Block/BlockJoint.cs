using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tiny;
using UnityEngine;

namespace BrickEscape
{
    public class BlockJoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer jointRenderer;
        
        [SerializeField] private GameObject arrowHolder;
        [SerializeField] private SpriteRenderer arrowBody;

        [SerializeField] private Color arrowOgColor;

        [SerializeField] private Vector3 newPos_Up;
        [SerializeField] private Vector3 newPos_Right;
        [SerializeField] private Vector3 newPos_Down;
        [SerializeField] private Vector3 newPos_Left;
        
        private MaterialPropertyBlock materialPropertyBlock;
        
        #region Start, Update, Validate

        public void OnValidate()
        {
            jointRenderer = GetComponent<SpriteRenderer>();
            arrowOgColor = arrowBody.color;
        }

        #endregion
        
        #region Show/Hide Arrow

        public void ShowArrow(BlockMovement direction, int arrowSize)
        {
            arrowHolder.SetActive(true);
            
            switch (direction)
            {
                case BlockMovement.Up:
                    arrowHolder.transform.localPosition = newPos_Up;
                    arrowHolder.transform.localEulerAngles = Vector3.zero;
                    break;
                case BlockMovement.Down:
                    arrowHolder.transform.localPosition = newPos_Down;
                    arrowHolder.transform.localEulerAngles = new Vector3(0, 0, 180);
                    break;
                case BlockMovement.Left:
                    arrowHolder.transform.localPosition = newPos_Left;
                    arrowHolder.transform.localEulerAngles = new Vector3(0, 0, 90);
                    break;
                case BlockMovement.Right:
                    arrowHolder.transform.localPosition = newPos_Right;
                    arrowHolder.transform.localEulerAngles = new Vector3(0, 0, 270);
                    break;
            }

            ShowArrowBody(arrowSize, direction);
        }

        public void ShowArrowBody(int arrowSize)
        {
            arrowBody.gameObject.SetActive(true);
            arrowBody.size = new Vector2(arrowSize < 1? 0.75f : arrowSize + 0.5f, 0.65f);
        }
        
        public void ShowArrowBody(int arrowSize, BlockMovement direction)
        {
            arrowBody.gameObject.SetActive(true);
            arrowBody.size = new Vector2(
                (arrowSize < 1? 0.75f : arrowSize + 0.5f) + 0.1f +
                //(direction is BlockMovement.Up or BlockMovement.Down? 0.1f : 0f)
                (direction is BlockMovement.Left or BlockMovement.Right && arrowSize > 1 ? 0.4f : 0f)
                , 0.65f);
        }

        public void RestoreArrowAndArrowBodyColor()
        {
            
            arrowBody.color = arrowOgColor;
        }

        public void FadeArrowAndArrowBody()
        {
            Color tempColor = arrowOgColor;
            tempColor.a = 0;
            
            arrowBody.color = tempColor;
        }

        public void HideArrowAndArrowBody()
        {
            arrowHolder.SetActive(false);
            //arrowBody.gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Joint Color

        public void SetColor(Color color)
        {
            jointRenderer.color = color;
        }
        
        #endregion
        
        #region Getters, Setters

        public GameObject ArrowHolder => arrowHolder;
        public SpriteRenderer ArrowBodyRenderer => arrowBody;
        public Color ArrowOgColor => arrowOgColor;

        #endregion
    }
}
