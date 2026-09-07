using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class UISound : MonoBehaviour
    {
        public ParticleSystem Start_1;
        public ParticleSystem Start_2;
        public ParticleSystem Start_3;

        public Transform StartL;
        public Transform StartM;
        public Transform StartR;

        [SerializeField] TextMeshProUGUI coinText;
        public void PlaySoundStart1() 
        {
            AudioManager.Instance.PlaySFX(SFXID.Star_1);
        }
        public void PlayStart1()
        {
            Start_1.Play();
        }
        public void PlayStart2()
        {
            Start_2.Play();

        }
        public void PlayStart3()
        {
            Start_3.Play();
        }


        public void ShakeStart1()
        {
            //Shake(StartL);
        }
        public void ShakeStart2()
        {
            //Shake(StartM);

        }
        public void ShakeStart3()
        {
            //Shake(StartR);
        }







        public void PlaySoundStart2()
        {
            AudioManager.Instance.PlaySFX(SFXID.Star_2);
            Start_2.Play();
        }
        public void PlaySoundStart3()
        {
            AudioManager.Instance.PlaySFX(SFXID.Star_3);
            Start_3.Play();
        }
       public void PlaySoundPop()
        {
            AudioManager.Instance.PlaySFX(SFXID.Pop);
        }
        public void PlaySoundCoin()
        {
            AudioManager.Instance.PlaySFX(SFXID.Coin);

            UIMainManager.Instance.winPanel.OnPlayCoinEffect();


        }






       
        public void PlaySoundVictory()
        {
            AudioManager.Instance.PlaySFX(SFXID.Victory);
        }

        public void Shake(Transform trans)
        {
            trans.DOKill();

            trans.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(trans.DORotate(new Vector3(0, 0, 20), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, -20), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, 15), 0.08f))
               .Append(trans.DORotate(new Vector3(0, 0, -15), 0.08f))
               .Append(trans.DORotate(Vector3.zero, 0.05f));

        }

    }
}
