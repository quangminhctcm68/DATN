using System.Collections;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using BrickEscape;
using System;

/// <summary>
/// Animation 3 ngôi sao bay vào vị trí cố định (kiểu màn hình kết quả game).
/// Mỗi sao bay từ ngoài màn hình theo đường cong vào vị trí target, có hiệu ứng bounce + scale punch khi đến nơi.
/// </summary>
public class StarResultAnimation : MonoBehaviour
{
    [Header("Star References")]
    [Tooltip("3 GameObject ngôi sao, theo thứ tự: trái, giữa, phải")]
    public RectTransform[] stars = new RectTransform[3];

    [Header("Target Positions (vị trí cố định cuối)")]
    [Tooltip("Vị trí anchored của 3 sao khi đã bay vào xong")]
    public Vector2[] targetPositions = new Vector2[3]
    {
        new Vector2(-220f, 0f),   // Sao 1 - trái
        new Vector2(0f,   40f),   // Sao 2 - giữa (cao hơn 1 chút)
        new Vector2(220f,  0f),   // Sao 3 - phải
    };

    [Header("Spawn Positions (vị trí xuất phát ngoài màn hình)")]
    public Vector2[] spawnPositions = new Vector2[3]
    {
        new Vector2(-600f, -400f),  // Từ dưới-trái
        new Vector2(0f,   -600f),   // Từ dưới-giữa
        new Vector2(600f, -400f),   // Từ dưới-phải
    };

    [Header("Timing")]
    public float delayBetweenStars = 0.25f;   // Độ trễ giữa các sao
    public float flyDuration = 0.55f;   // Thời gian bay vào
    public float punchDuration = 0.35f;   // Thời gian hiệu ứng punch khi đến nơi
    public float initialDelay = 0.3f;    // Delay trước khi bắt đầu toàn bộ animation

    [Header("Easing")]
    public Ease flyEase = Ease.OutBack;      // Ease khi bay vào
    public Ease punchEase = Ease.OutElastic;   // Ease khi punch scale

    [Header("Arc / Overshoot")]
    [Tooltip("Độ cong của đường bay (dùng waypoint trung gian). 0 = đường thẳng")]
    [Range(0f, 300f)]
    public float arcHeight = 120f;

    [Header("Scale Settings")]
    public Vector3 startScale = Vector3.zero;          // Scale ban đầu (ẩn)
    public Vector3 normalScale = Vector3.one;           // Scale bình thường
    public Vector3 punchScaleAdd = new Vector3(0.4f, 0.4f, 0f); // Thêm vào khi punch


    //private void OnValidate()
    //{
    //    LevelButton lv = GetComponent<LevelButton>();
    //    stars[0] = lv.star[0].rectTransform;
    //    stars[1] = lv.star[1].rectTransform;
    //    stars[2] = lv.star[2].rectTransform;
    //}
    // ---------------------------------------------------------------
    public Action action;
    [Button]
    public void Play()
    {
        ResetStars();
        StartCoroutine(PlayAnimation());
    }
    public void SetAction(Action e)
    {
        action = null;
        action = e;
    }


    /// <summary>
    /// Đặt lại tất cả sao về trạng thái ban đầu (ẩn, nằm ở spawn position).
    /// Gọi hàm này trước khi play animation.
    /// </summary>
    public void ResetStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;
            stars[i].anchoredPosition = spawnPositions[i];
            stars[i].localScale = startScale;
            stars[i].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Chạy toàn bộ animation. Có thể gọi lại để replay.
    /// </summary>
    public IEnumerator PlayAnimation()
    {
        yield return new WaitForSeconds(initialDelay);

        // Thứ tự: Sao 1 → Sao 2 → Sao 3 (lần lượt, delay nhau)
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            int index = i; // capture for lambda
            float starDelay = i * delayBetweenStars;

            // Chạy từng sao không blocking (dùng DOTween sequence riêng)
            PlayStarSequence(index, starDelay);
        }

        // Chờ đủ thời gian cho sao cuối hoàn thành
        float totalTime = (stars.Length - 1) * delayBetweenStars + flyDuration + punchDuration + 0.1f;
        yield return new WaitForSeconds(totalTime);

        // Callback sau khi tất cả sao đã vào xong (thêm logic tại đây)
        OnAllStarsLanded(action);
    }

    // ---------------------------------------------------------------

    private void PlayStarSequence(int index, float delay)
    {
        RectTransform star = stars[index];
        Sequence seq = DOTween.Sequence();

        // ── Bước 1: Delay trước khi sao này bay ──────────────────────
        seq.AppendInterval(delay);

        // ── Bước 2: Scale từ 0 lên (xuất hiện nhỏ) khi bắt đầu bay ──
        seq.Append(
            star.DOScale(normalScale * 0.8f, flyDuration * 0.2f)
                .SetEase(Ease.OutQuad)
        );

        // ── Bước 3: Bay theo đường cong đến target ───────────────────
        // Tạo waypoint trung gian để tạo arc
        Vector2 midPoint = Vector2.Lerp(spawnPositions[index], targetPositions[index], 0.5f)
                           + new Vector2(0f, arcHeight);

        // Di chuyển qua midpoint rồi đến target (2 đoạn)
        seq.Join(
            star.DOAnchorPos(midPoint, flyDuration * 0.5f)
                .SetEase(Ease.OutQuad)
        );
        seq.Append(
            star.DOAnchorPos(targetPositions[index], flyDuration * 0.5f)
                .SetEase(flyEase)
        );

        // Scale về bình thường trong lúc đang bay
        seq.Join(
            star.DOScale(normalScale, flyDuration * 0.5f)
                .SetEase(Ease.OutBack)
        );

        // ── Bước 4: Punch scale khi đến vị trí (hiệu ứng "đóng dấu") ─
        seq.Append(
            star.DOPunchScale(punchScaleAdd, punchDuration, vibrato: 1, elasticity: 0.5f)
                .SetEase(punchEase)
        );

        // ── Bước 5: Đảm bảo về đúng scale + position sau cùng ────────
        seq.AppendCallback(() =>
        {
            star.localScale = normalScale;
            star.anchoredPosition = targetPositions[index];
            OnStarLanded(index);
        });

        seq.SetLink(star.gameObject); // tự kill khi object bị destroy
        seq.Play();
    }

    // ---------------------------------------------------------------
    // Callbacks — override hoặc kết nối event tại đây

    protected virtual void OnStarLanded(int starIndex)
    {

    }

    protected void OnAllStarsLanded(Action action)
    {
        action.Invoke();
    }

    // ---------------------------------------------------------------
    // Editor helper: preview trong Inspector

#if UNITY_EDITOR
    [ContextMenu("Preview Animation (Play Mode)")]
    private void PreviewInEditor()
    {
        if (!Application.isPlaying) return;
        DOTween.KillAll();
        ResetStars();
        StartCoroutine(PlayAnimation());
    }
#endif
}