using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NabaGame.Core.Runtime.Singleton;
using UnityEngine;

namespace BrickEscape
{
    public class GameController : Singleton<GameController>
    {
        public LevelGenerator levelGenerator;
        public PlayerControl playerControl;
        public PlayerControl_EffectOnly playerControl_EffectOnly;
        public BoosterManager boosterManager;
        public TutorialManager tutorialManager;
        public AudioManager audioManager;
        public HeartManager heartManager;
        public TimeManager timeManager;
        public BattlepassManager battlepassManager;
     
        #region Start, Update, Validate

        public override void Init()
        {
            
        }

        public void Start()
        {
            playerControl.Init();
            boosterManager.Init();
            tutorialManager.Init();
            audioManager.SetInfo();
            heartManager.GetDataOnStartup();
            heartManager.SetInfo();
            battlepassManager.Init();
            timeManager.Init();
            
            audioManager.PlayMusic(MusicID.GamePlay);
        }
        
        #endregion
        
        #region Player Control

        public void ChangePlayerControlState(bool state)
        {
            if (playerControl != null)
                playerControl.gameObject.SetActive(state);
        }

        public void ChangePlayerControlState(bool state, bool changeTheEffectOnlyVer)
        {
            if (playerControl_EffectOnly != null)
                playerControl_EffectOnly.gameObject.SetActive(state);
        }
        
        #endregion
    }
}
