using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    namespace BrickEscape
    {
        public class AvatarProfile
        {
            private const string AVATAR_KEY = "P_AVA_";
            private const string FRAME_KEY = "P_FRA_";

            public List<AvatarInfor> avatarInforList = new();
            public List<FrameInfor> frameInforList = new();

            public AvatarProfile()
            {
                InitDefault();
                Load();     // ← Load trạng thái đã lưu
            }

            #region SAVE / LOAD

            public void Save()
            {
                // Save Avatar
                for (int i = 0; i < avatarInforList.Count; i++)
                {
                    AvatarInfor info = avatarInforList[i];
                    PlayerPrefs.SetInt(
                        AVATAR_KEY + (int)info.avatarID,
                        info.isUnlocked ? 1 : 0
                    );
                }

                // Save Frame
                for (int i = 0; i < frameInforList.Count; i++)
                {
                    FrameInfor info = frameInforList[i];
                    PlayerPrefs.SetInt(
                        FRAME_KEY + (int)info.frameID,
                        info.isUnlocked ? 1 : 0
                    );
                }

                PlayerPrefs.Save();
            }
        
            public void Load()
            {
                // Load Avatar
                for (int i = 0; i < avatarInforList.Count; i++)
                {
                    AvatarInfor info = avatarInforList[i];
                    info.isUnlocked = PlayerPrefs.GetInt(
                        AVATAR_KEY + (int)info.avatarID,
                        info.isUnlocked ? 1 : 0   // giữ default nếu chưa có key
                    ) == 1;
                }

                // Load Frame
                for (int i = 0; i < frameInforList.Count; i++)
                {
                    FrameInfor info = frameInforList[i];
                    info.isUnlocked = PlayerPrefs.GetInt(
                        FRAME_KEY + (int)info.frameID,
                        info.isUnlocked ? 1 : 0
                    ) == 1;
                }
            }

            #endregion

            #region UNLOCK API

            public bool UnlockAvatar(AvatarID id)
            {
                AvatarInfor info = avatarInforList.Find(a => a.avatarID == id);
                if (info == null || info.isUnlocked)
                    return false;

                info.isUnlocked = true;
                Save();
                return true;
            }

            public bool UnlockFrame(FrameID id)
            {
                FrameInfor info = frameInforList.Find(f => f.frameID == id);
                if (info == null || info.isUnlocked)
                    return false;

                info.isUnlocked = true;
                Save();
                return true;
            }

            #endregion

            #region DEFAULT INIT

            private void InitDefault()
            {
                avatarInforList.Clear();
                frameInforList.Clear();

                foreach (AvatarID id in Enum.GetValues(typeof(AvatarID)))
                {
                    avatarInforList.Add(
                        new AvatarInfor(id, id == AvatarID.Parrot) // default avatar
                    );
                }

                foreach (FrameID id in Enum.GetValues(typeof(FrameID)))
                {
                    frameInforList.Add(
                        new FrameInfor(id, id == FrameID.Frame_1) // default frame
                    );
                }
            }

            #endregion

            #region RESET

            public static void ResetAll()
            {
                foreach (AvatarID id in Enum.GetValues(typeof(AvatarID)))
                    PlayerPrefs.DeleteKey(AVATAR_KEY + (int)id);

                foreach (FrameID id in Enum.GetValues(typeof(FrameID)))
                    PlayerPrefs.DeleteKey(FRAME_KEY + (int)id);
            }

            #endregion



            public AvatarInfor GetAvatarInfor(AvatarID id)
            {
                for (int i = 0; i < avatarInforList.Count; i++)
                {
                    if (avatarInforList[i].avatarID == id)
                        return avatarInforList[i];
                }

                // nếu chưa tồn tại → tạo mới + load trạng thái
                bool unlocked = PlayerPrefs.GetInt(
                    "P_AVA_" + (int)id,
                    0
                ) == 1;

                AvatarInfor info = new AvatarInfor(id, unlocked);
                avatarInforList.Add(info);
                return info;
            }

            public FrameInfor GetFrameInfor(FrameID id)
            {
                for (int i = 0; i < frameInforList.Count; i++)
                {
                    if (frameInforList[i].frameID == id)
                        return frameInforList[i];
                }

                bool unlocked = PlayerPrefs.GetInt(
                    "P_FRA_" + (int)id,
                    0
                ) == 1;

                FrameInfor info = new FrameInfor(id, unlocked);
                frameInforList.Add(info);
                return info;
            }
        }
    }

}


    [Serializable]
public class AvatarInfor 
{
    public AvatarID avatarID;   
    public bool isUnlocked = false;
    public AvatarInfor(AvatarID avatarID, int unlock)
    {
        this.avatarID = avatarID;
        isUnlocked = unlock == 1 ? true : false;
    }
    public AvatarInfor(AvatarID avatarID, bool unlock)
    {
        this.avatarID = avatarID;
        isUnlocked = unlock ;
    }

}
[Serializable]
public class FrameInfor
{
    public FrameID frameID;
    public bool isUnlocked = false;
    public FrameInfor(FrameID frameID, int unlock)
    {
        this.frameID = frameID;
        isUnlocked = unlock == 1 ? true : false;
    }
    public FrameInfor(FrameID frameID, bool unlock)
    {
        this.frameID = frameID;
        isUnlocked = unlock;
    }
}