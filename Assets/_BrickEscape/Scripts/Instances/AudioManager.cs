using BrickEscape;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.Core.Runtime.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum MusicID
{
    GamePlay = 0,
}

public enum SFXID
{
    ButtonClick = 0,
    Victory = 1,
    Lose = 2,
    Coin = 3,
    WrongMove = 4,
    Booster_Hint = 5,
    Booster_Hammer = 6,
    Booster_MagicWand = 7,
    IAP_Purchase = 8,
    Block_Escape = 9,
    Star_1 =10,
    Star_2 = 11,
    Star_3 = 12,
    Pop = 13,
    BlockMove_1 = 14,
    BlockMove_2 = 15,
    BlockMove_3 = 16,
    BlockMove_4 = 17,
    Block_Escape_New_1 = 18,
    Block_Escape_New_2 = 19,
}

public class AudioManager : SerializedSingleton<AudioManager>
{
    private readonly string musicMixer = "music";
    private readonly string sfxMixer = "sfx";
    
    //[SerializeField] private AudioSource _audioSFX;
    //[SerializeField] private AudioSource _audioMusic;

    [SerializeField] private GameObject _spammableSFXAudioSourceHolder;
    [SerializeField] private GameObject _nonSpammableSFXAudioSourceHolder;
    [SerializeField] private GameObject _musicAudioSourceHolder;
    
    [SerializeField] private AudioSource[] _spammableSFXSources;
    [SerializeField] private AudioSource[] _nonSpammableSFXSources;
    [SerializeField] private AudioSource[] _musicSources;
    
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _audioMixerGroup_sfx;
    [SerializeField] private AudioMixerGroup _audioMixerGroup_music;
    
    [SerializeField] private Dictionary<MusicID, AudioClip> musicClips;
    [SerializeField] private Dictionary<SFXID, AudioClip> sfxClips;

    private Tweener smoothStopMusic;
    private Sequence musicTransition;

    private int preloadSourceCount_spammable = 5;
    private int preloadSourceCount_nonSpammable = 2;
    
    private int spammableSFXSourceIndex = 0;
    private int nonSpammableSFXSourceIndex = 0;

    private AudioSource tempRef;
    private AudioSource tempSource_SFXSpammable;
    private AudioSource tempSource_SFXNonSpammable;
    private AudioSource tempSource_Music;
    
    private SFXID[] moveSounds = new SFXID[] { SFXID.BlockMove_1, SFXID.BlockMove_2, SFXID.BlockMove_3, SFXID.BlockMove_4 };
    private SFXID[] blockEscapeSounds = new SFXID[] { SFXID.Block_Escape_New_1, SFXID.Block_Escape_New_2 };

    private Coroutine spammableSFXChecker;
    
    private bool IsMusicOn => GameManager.Instance.PlayerProfile.MusicSetting;
    private bool IsSoundOn => GameManager.Instance.PlayerProfile.SoundSetting;
    
    #region Start, Update, Validate
    public override void Init()
    {
    }

    public void SetInfo()
    {
        PrespawnAudioSources();
        _audioMixer.SetFloat(musicMixer, GameManager.Instance.PlayerProfile.MusicSetting? 0f : -80f);
        _audioMixer.SetFloat(sfxMixer, GameManager.Instance.PlayerProfile.SoundSetting? 0f : -80f);
        
        EventManager.Instance.AddListener<MusicChange>(OnMusicVolumeChange);
        EventManager.Instance.AddListener<SoundChange>(OnSFXVolumeChange);
    }
    #endregion
    
    #region Public Functions
    public void PlayMusic(MusicID audioType)
    {
        if (musicClips.ContainsKey(audioType))
        {
            ChangeMusic(musicClips[audioType]);
        }
    }

    public void PlaySFX(SFXID sfxID)
    {
        if (sfxClips.ContainsKey(sfxID))
        {
            if (HandleSpammableSFX(sfxID)) return;

            tempSource_SFXNonSpammable = GetNextNonSpammableSFXSource();
            tempSource_SFXNonSpammable.PlayOneShot(sfxClips[sfxID]);
        }
    }

    public void PlayButtonSound()
    {
        tempSource_SFXSpammable = GetNextSpammableSFXSource();
        tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.ButtonClick]);
        if (GameManager.Instance.PlayerProfile.VibrationSetting)
            MMVibrationManager.Haptic(HapticTypes.LightImpact);
    }

    public void StopMusic(bool smoothStop = false)
    {
        //if (!IsMusicOn) return;
        
        tempSource_Music = GetFirstActiveMusicSource();
        if (tempSource_Music != null)
        {
            if (smoothStop)
            {
                if (smoothStopMusic != null)
                {
                    smoothStopMusic.Kill();
                    smoothStopMusic = null;
                }

                smoothStopMusic = tempSource_Music.DOFade(0, 1f)
                    .OnComplete(delegate
                    {
                        tempSource_Music.Stop();
                        smoothStopMusic = null;
                    });
            }
            else
            {
                tempSource_Music.Stop();
            }
        }
    }
    
    public void PlayRandomMoveSound()
    {
        PlaySFX(moveSounds[Random.Range(0, moveSounds.Length)]);
    }

    public void PlayRandomBlockEscapeSound()
    {
        PlaySFX(blockEscapeSounds[Random.Range(0, blockEscapeSounds.Length)]);
    }
    #endregion

    #region Private Functions
    void ChangeMusic(AudioClip musicClip)
    {
        //if (!IsMusicOn) return;
        
        if (musicTransition != null)
        {
            musicTransition.Kill();
            musicTransition = null;
        }

        musicTransition = DOTween.Sequence();
        
        var activeMusicSource = GetFirstActiveMusicSource();
        var inactiveMusicSource = GetFirstInactiveMusicSource();

        if (activeMusicSource == null)
        {
            foreach (AudioSource source in _musicSources)
            {
                source.volume = 0;
            }

            musicTransition
                .AppendCallback(delegate
                {
                    inactiveMusicSource.clip = musicClip;
                })
                .Append(
                    inactiveMusicSource.DOFade(1, 1f)
                        .OnStart(delegate
                        {
                            inactiveMusicSource.Play();
                        })
                )
                .OnComplete(delegate
                {
                    musicTransition = null;
                });
        }
        else
        {
            musicTransition
                .AppendCallback(delegate
                {
                    inactiveMusicSource.clip = musicClip;
                })
                .Append(
                    activeMusicSource.DOFade(0, 1f)
                    )
                .Join(
                    inactiveMusicSource.DOFade(1, 1f)
                        .OnStart(delegate
                        {
                            inactiveMusicSource.Play();
                        })
                )
                .AppendCallback(activeMusicSource.Stop)
                .OnComplete(delegate
                {
                    musicTransition = null;
                });
        }
    }

    void PrespawnAudioSources()
    {
        _spammableSFXSources = new AudioSource[preloadSourceCount_spammable];
        _nonSpammableSFXSources = new AudioSource[preloadSourceCount_nonSpammable];
        _musicSources = new AudioSource[preloadSourceCount_nonSpammable];
        
        for (int i = 0; i < preloadSourceCount_spammable; i++)
        {
            tempRef = _spammableSFXAudioSourceHolder.AddComponent<AudioSource>();
            tempRef.playOnAwake = false;
            tempRef.spatialBlend = 0;
            tempRef.outputAudioMixerGroup = _audioMixerGroup_sfx;
            _spammableSFXSources[i] = tempRef;
        }

        for (int i = 0; i < preloadSourceCount_nonSpammable; i++)
        {
            tempRef = _nonSpammableSFXAudioSourceHolder.AddComponent<AudioSource>();
            tempRef.playOnAwake = false;
            tempRef.spatialBlend = 0;
            tempRef.outputAudioMixerGroup = _audioMixerGroup_sfx;
            _nonSpammableSFXSources[i] = tempRef;
            
            tempRef = _musicAudioSourceHolder.AddComponent<AudioSource>();
            tempRef.playOnAwake = false;
            tempRef.loop = true;
            tempRef.spatialBlend = 0;
            tempRef.outputAudioMixerGroup = _audioMixerGroup_music;
            _musicSources[i] = tempRef;
        }
    }
    
    bool HandleSpammableSFX(SFXID sfxID)
    {
        switch (sfxID)
        {
            case SFXID.Coin:
                if (AddSpammableSFXForTracking(SFXID.Coin))
                {
                    tempSource_SFXSpammable = GetNextSpammableSFXSource();
                    tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.Coin]);
                }
                return true;
            case SFXID.Block_Escape_New_1:
                if (AddSpammableSFXForTracking(SFXID.Block_Escape_New_1))
                {
                    tempSource_SFXSpammable = GetNextSpammableSFXSource();
                    tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.Block_Escape_New_1]);
                }
                return true;
            case SFXID.Block_Escape_New_2:
                if (AddSpammableSFXForTracking(SFXID.Block_Escape_New_2))
                {
                    tempSource_SFXSpammable = GetNextSpammableSFXSource();
                    tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.Block_Escape_New_2]);
                }
                return true;
            case SFXID.Star_3:
                if (AddSpammableSFXForTracking(SFXID.Star_3))
                {
                    tempSource_SFXSpammable = GetNextSpammableSFXSource();
                    tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.Star_3]);
                }
                return true;
            case SFXID.Block_Escape:
                if (AddSpammableSFXForTracking(SFXID.Block_Escape))
                {
                    tempSource_SFXSpammable = GetNextSpammableSFXSource();
                    tempSource_SFXSpammable.PlayOneShot(sfxClips[SFXID.Block_Escape]);
                }
                return true;
        }

        return false;
    }
    #endregion

    #region Source Getters
    
    AudioSource GetNextSpammableSFXSource()
    {
        return _spammableSFXSources[spammableSFXSourceIndex++ % preloadSourceCount_spammable];
    }
    
    AudioSource GetNextNonSpammableSFXSource()
    {
        return _nonSpammableSFXSources[nonSpammableSFXSourceIndex++ % preloadSourceCount_nonSpammable];
    }
    
    AudioSource GetFirstInactiveMusicSource()
    {
        foreach (AudioSource source in _musicSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        return null;
    }
    
    AudioSource GetFirstActiveMusicSource()
    {
        foreach (AudioSource source in _musicSources)
        {
            if (source.isPlaying)
            {
                return source;
            }
        }
        
        return null;
    }
    #endregion
    
    #region Handle Spammable SFX

    [SerializeField] private List<SpammableSFXTrackingData> spammableSFXTrackingDatas = new List<SpammableSFXTrackingData>();
    [SerializeField] private SpammableSFXTrackingData tempTrackingData;
    private int maxAllowedSameSFXCount = 4;
    
    bool AddSpammableSFXForTracking(SFXID sfxID)
    {
        int currentSameSFXCount = 0;
        foreach (SpammableSFXTrackingData trackingData in spammableSFXTrackingDatas)
        {
            if (sfxID == trackingData.sfxID)
            {
                currentSameSFXCount++;
                if (currentSameSFXCount >= maxAllowedSameSFXCount)
                {
                    return false;
                }
            }
        }
        
        tempTrackingData = new SpammableSFXTrackingData(){sfxID = sfxID, timeEnd = Time.time + sfxClips[sfxID].length + 0.1f};
        spammableSFXTrackingDatas.Add(tempTrackingData);

        if (spammableSFXChecker == null)
        {
            spammableSFXChecker = StartCoroutine(SpammableSFXMainChecker());
        }
        return true;
    }

    IEnumerator SpammableSFXMainChecker()
    {
        float currentTime = Time.time;
        List<SpammableSFXTrackingData> spammableSFXTrackingDatasForDeletion = new List<SpammableSFXTrackingData>();
        
        while (spammableSFXTrackingDatas.Count > 0)
        {
            currentTime = Time.time;
            foreach (SpammableSFXTrackingData trackingData in spammableSFXTrackingDatas)
            {
                if (currentTime >= trackingData.timeEnd)
                {
                    spammableSFXTrackingDatasForDeletion.Add(trackingData);
                }
            }
            
            foreach (SpammableSFXTrackingData trackingData in spammableSFXTrackingDatasForDeletion)
            {
                spammableSFXTrackingDatas.Remove(trackingData);
            }
            
            spammableSFXTrackingDatasForDeletion.Clear();
            yield return null;
        }

        spammableSFXChecker = null;
    }
    
    #endregion

    #region Event Handlers
    void OnMusicVolumeChange(MusicChange e)
    {
        _audioMixer.SetFloat(musicMixer, e.currentMusicStatus? 0f : -80f);
    }
    
    void OnSFXVolumeChange(SoundChange e)
    {
        _audioMixer.SetFloat(sfxMixer, e.currentSoundStatus? 0f : -80f);
    }
    #endregion
}

public struct SpammableSFXTrackingData
{
    public SFXID sfxID;
    public float timeEnd;
}