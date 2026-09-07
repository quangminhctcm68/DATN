using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    [CreateAssetMenu(fileName = "Data Collection", menuName = "GameData/ProfileData")]
    public class ProfileData : SerializedScriptableObject
    {
        public RawFrameData rawFrameData;
        public RawAvatarData rawAvatarData;
        public Dictionary<int,FrameID> frameDic = new Dictionary<int, FrameID>();
        public Dictionary<int,AvatarID> AvaDic = new Dictionary<int,AvatarID>();

        [Button("Build Dictionaries")]
        public void BuildDictionaries()
        {
            frameDic.Clear();
            AvaDic.Clear();

            // ===== Frame =====
            if (rawFrameData != null && rawFrameData.rawFrames != null)
            {
                for (int i = 0; i < rawFrameData.rawFrames.Count; i++)
                {
                    RawFrame raw = rawFrameData.rawFrames[i];

                    if (frameDic.ContainsKey(raw.LevelComplete))
                    {
                        Debug.LogWarning(
                            $"[ProfileData] Duplicate Frame LevelComplete: {raw.LevelComplete}"
                        );
                        continue;
                    }

                    frameDic.Add(raw.LevelComplete, raw.FrameID);
                }
            }

            // ===== Avatar =====
            if (rawAvatarData != null && rawAvatarData.rawAvatars != null)
            {
                for (int i = 0; i < rawAvatarData.rawAvatars.Count; i++)
                {
                    RawAvatar raw = rawAvatarData.rawAvatars[i];

                    if (AvaDic.ContainsKey(raw.LevelComplete))
                    {
                        Debug.LogWarning(
                            $"[ProfileData] Duplicate Avatar LevelComplete: {raw.LevelComplete}"
                        );
                        continue;
                    }

                    AvaDic.Add(raw.LevelComplete, raw.AvatarID);
                }
            }
        }


        public void CheckUnlock(int level)
        {
            // ===== Unlock Frame =====
            if (frameDic.TryGetValue(level, out FrameID frameID))
            {
               GameManager.Instance.PlayerProfile.avatarProfile.UnlockFrame(frameID);
            }

            // ===== Unlock Avatar =====
            if (AvaDic.TryGetValue(level, out AvatarID avatarID))
            {
                GameManager.Instance.PlayerProfile.avatarProfile.UnlockAvatar(avatarID);
                Debug.Log($"[Unlock] Avatar unlocked: {avatarID} at level {level}");
            }
        }

        public void ForceUnlock_Avatar(AvatarID avatarID)
        {
            GameManager.Instance.PlayerProfile.avatarProfile.UnlockAvatar(avatarID);
        }

        public void ForceUnlock_Frame(FrameID frameID)
        {
            GameManager.Instance.PlayerProfile.avatarProfile.UnlockFrame(frameID);
        }
    }
}
