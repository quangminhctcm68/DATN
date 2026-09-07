using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ví dụ item script — gắn vào itemPrefab.
/// Chỉ cần implement SetInfo(int index).
/// </summary>

/// <summary>
/// Gắn script này vào ScrollView GameObject (cùng chỗ với InfiniteVerticalLoopScroll).
/// </summary>
public class ExampleScrollController : MonoBehaviour
{
    [SerializeField] private InfiniteVerticalLoopScroll loopScroll;

    // Dữ liệu giả — thay bằng list thực của bạn
    [SerializeField] string[] _data;

    void Start()
    {
        // 1. Chuẩn bị data
        _data = new string[100];
        for (int i = 0; i < _data.Length; i++)
            _data[i] = $"Row {i} — some content here";

        // 2. Init: truyền tổng số item và callback SetInfo
        loopScroll.Init(OnSetItem);

        // ── Nếu muốn loop vô hạn (không giới hạn item):
        // loopScroll.Init(-1, OnSetItem);
    }

    /// <summary>
    /// Đây là hàm duy nhất bạn cần viết.
    /// itemRT = RectTransform của item đang được hiển thị.
    /// index  = data index (đã được normalize về [0, totalCount)).
    /// </summary>
    private void OnSetItem(ScrollItem itemRT, int index)
    {
        // Lấy component item và gọi SetInfo
        itemRT.SetInfo(index);

        // Hoặc nếu không dùng component riêng:
        // itemRT.Find("Label").GetComponent<Text>().text = _data[index];
    }

    // ── Các thao tác runtime ──────────────────────────────────────────────

    public void OnClickScrollToMiddle()
    {
        loopScroll.ScrollToIndex(_data.Length / 2);
    }

    public void OnClickRefreshAll()
    {
        loopScroll.RefreshAll();
    }
}
