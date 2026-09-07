using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace BrickEscape
{
    public class Race_Avatar : MonoBehaviour
    {
        public bool isPlayer = false;

        public Image Frame;
        public Image Avatar;
        public TextMeshProUGUI name_text;
        public RectTransform rect;
        public float moveDown = -3000f;
        public float rotateZ = 60f;

        [SerializeField] private RectTransform _destination;

        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _jumpPower = 100f;
        [SerializeField] private int _numJumps = 1;

        private void OnValidate() 
        {
            rect = GetComponent<RectTransform>();
            if (!isPlayer) 
            {
                name_text.gameObject.SetActive(false);
            }
            else 
            {
                name_text.gameObject.SetActive(true);
            }
        }
        public void SetInfor() 
        {
            if (isPlayer) 
            {
                SetInforPlayer();
            }
            else 
            {
                RandomProfile();
            }
        }


        [Button]
        public void SetInforPlayer()
        {
            FrameID rdFrame = GameManager.Instance.PlayerProfile.currentFrame;
            AvatarID rdAva = GameManager.Instance.PlayerProfile.currentAvatar;
            Frame.sprite = GameManager.Instance.spriteCollection.GetFrameSprite(rdFrame);
            Avatar.sprite = GameManager.Instance.spriteCollection.GetAvatarSprite(rdAva);
        }

        [Button]
        public void RandomProfile() 
        {
            FrameID rdFrame = EnumHelper.GetRandomEnum<FrameID>();
            AvatarID rdAva = EnumHelper.GetRandomEnum<AvatarID>();
            Frame.sprite = GameManager.Instance.spriteCollection.GetFrameSprite(rdFrame);
            Avatar.sprite = GameManager.Instance.spriteCollection.GetAvatarSprite(rdAva);
        }


        public Tween JumpToPos(RectTransform des)
        {
            if (rect == null || des == null)
                return null;

            DOTween.Kill(rect);

            // Convert sang local space
            Vector2 targetPos;
            RectTransform parent = rect.parent as RectTransform;

            Vector3 worldPos = des.position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out targetPos
            );

            float halfTime = _duration * 0.5f;

            Sequence seq = DG.Tweening.DOTween.Sequence();

            // =========================
            // 1. MOVE (jump)
            // =========================
            seq.Join(
                rect.DOJumpAnchorPos(
                    targetPos,
                    _jumpPower,
                    _numJumps,
                    _duration
                ).SetEase(Ease.OutQuad)
            );

            // =========================
            // 2. SCALE - stretch khi bay lên
            // =========================
            seq.Insert(0f,
                rect.DOScale(new Vector3(0.85f, 1.2f, 1f), halfTime)
                    .SetEase(Ease.OutBack)
            );

            // =========================
            // 3. SCALE - squash khi rơi xuống
            // =========================
            seq.Insert(halfTime,
                rect.DOScale(new Vector3(1.15f, 0.85f, 1f), halfTime)
                    .SetEase(Ease.InBack)
            );

            // =========================
            // 4. RETURN scale về normal
            // =========================
            seq.Append(
                rect.DOScale(Vector3.one, 0.1f)
                    .SetEase(Ease.OutBack)
            );

            return seq;
        }



        [Button]
        public void FallAnim()
        {
            DOTween.Kill(rect);
            //DOTween.Kill(img);

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
      
            Sequence seq = DOTween.Sequence();

            // rơi xuống
            seq.Join(
                    DOTweenModuleUI.DOAnchorPosY(
                    rect,
                    moveDown,
                    1f
                ).SetEase(Ease.InBack)
            );
            seq.OnComplete(() => 
            {
                GameManager.Instance.pooling.DespawnAvatar(gameObject);
            });
            ////// 🔥 rotation tạo cảm giác sao rơi
            //seq.Join(
            //    ShortcutExtensions.DORotate(
            //        rect,
            //        new Vector3(0f, 0f, rotateZ),
            //        1f,
            //        RotateMode.FastBeyond360
            //    ).SetEase(Ease.Linear)
            //);

        }

        public Tween PlayCelebrate(int loopCount = 8)
        {
            if (rect == null) return null;

            DOTween.Kill(rect);

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < loopCount; i++)
            {
                float randomJump = Random.Range(_jumpPower * 0.6f, _jumpPower * 1.2f);
                float randomDuration = Random.Range(_duration * 0.6f, _duration * 1.0f);

                float halfTime = randomDuration * 0.5f;

                // =========================
                // 1. Jump tại chỗ
                // =========================
                seq.Append(
                    rect.DOJumpAnchorPos(
                        rect.anchoredPosition, // giữ nguyên vị trí
                        randomJump,
                        1,
                        randomDuration
                    ).SetEase(Ease.OutQuad)
                );

                // =========================
                // 2. Stretch (bay lên)
                // =========================
                seq.Insert(seq.Duration() - randomDuration,
                    rect.DOScale(new Vector3(0.85f, 1.2f, 1f), halfTime)
                        .SetEase(Ease.OutQuad)
                );

                // =========================
                // 3. Squash (tiếp đất)
                // =========================
                seq.Insert(seq.Duration() - halfTime,
                    rect.DOScale(new Vector3(1.15f, 0.85f, 1f), halfTime)
                        .SetEase(Ease.InQuad)
                );

                // =========================
                // 4. Reset scale
                // =========================
                seq.Append(
                    rect.DOScale(Vector3.one, 0.08f)
                        .SetEase(Ease.OutBack)
                );

                // =========================
                // 5. Delay nhẹ giữa các lần nhảy (tạo nhịp)
                // =========================
                seq.AppendInterval(Random.Range(0.05f, 0.15f));
            }
        
            return seq;
        }

    }
}
