using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class InfiniteVerticalLoopScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ── Inspector ──────────────────────────────────────────────────────────
    [Header("Item Setup")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float itemHeight = 100f;
    [SerializeField] private float itemSpacing = 0f;
    [SerializeField] private int bufferCount = 2;
    [SerializeField] private float paddingBottom = 0f; // ← THÊM
    // ── Runtime state ──────────────────────────────────────────────────────
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;
    [SerializeField] private RectTransform _viewport;

    [SerializeField] private int _totalCount;
    private Action<ScrollItem, int> _onSetItem; 

    [SerializeField] private  List<RectTransform> _pool = new List<RectTransform>(); // tất cả RT đã tạo
    [SerializeField] private  List<int> _poolDataIndex = new List<int>();    // data index của từng slot
    [SerializeField] private  List<ScrollItem>  _scrollItems = new List<ScrollItem>();

    [SerializeField] private int _firstVisibleSlot = 0; // index trong _pool của item ở đầu danh sách
    [SerializeField] private int _topDataIndex = 0;  // data index của item đầu tiên đang hiển thị

    private float ItemStep => itemHeight + itemSpacing;

    // ── Drag forward ──────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData e) => _scrollRect.OnBeginDrag(e);
    public void OnDrag(PointerEventData e) => _scrollRect.OnDrag(e);
    public void OnEndDrag(PointerEventData e) => _scrollRect.OnEndDrag(e);

    public void Init(Action<ScrollItem, int> onSetItem)
    {
        _onSetItem = onSetItem;

        _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.vertical = true;
        _scrollRect.horizontal = false;
        //_scrollRect.onValueChanged.RemoveAllListeners();
        _scrollRect.onValueChanged.AddListener(_ => CheckAndFill());
        Debug.Log("Init");

        PlaceAllSlots();
        RefreshAllSlots();

    }

    [Button]
    public void InitCache()
    {

        _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.vertical = true;
        _scrollRect.horizontal = false;


        _content = _scrollRect.content;
        _viewport = _scrollRect.viewport != null ? _scrollRect.viewport : _scrollRect.GetComponent<RectTransform>();

        // ── Anchor content bottom-left, pivot bottom ──
        _content.anchorMin = new Vector2(0, 0);
        _content.anchorMax = new Vector2(1, 0);
        _content.pivot = new Vector2(0.5f, 0);
        _content.anchoredPosition = Vector2.zero;

        float viewportH = _viewport.rect.height;
        if (viewportH <= 0) viewportH = Screen.height;
        int slotCount = Mathf.CeilToInt(viewportH / ItemStep) + bufferCount * 2 + 1;
        if (_totalCount > 0 && slotCount > _totalCount)
            slotCount = _totalCount;

        DestroyPool();
        for (int i = 0; i < slotCount; i++)
        {
            var go = Instantiate(itemPrefab, _content);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);           // pivot bottom
            rt.sizeDelta = new Vector2(0, itemHeight);
            _pool.Add(rt);
            _poolDataIndex.Add(i);
            _scrollItems.Add(rt.GetComponent<ScrollItem>());
        }
        UpdateContentSize();

        _topDataIndex = 0;
        _firstVisibleSlot = 0;

        PlaceAllSlots();
        RefreshAllSlots();

    }
    [Button]
    public void ClearCache() 
    {
        for(int i= 0; i< _pool.Count; i++) 
        {
            DestroyImmediate(_pool[i].gameObject);
      
        }
        _pool.Clear();
        _poolDataIndex.Clear();
        _scrollItems.Clear();
    }

    /// <summary>
    /// Refresh toàn bộ item đang hiển thị (dùng khi data thay đổi).
    /// </summary>
    public void RefreshAll()
    {
        RefreshAllSlots();
    }

    public void RefreshItemAt(int index)
    {
        for (int s = 0; s < _pool.Count; s++)
        {
            if (_poolDataIndex[s] == NormalizeIndex(index))
            {
                _onSetItem?.Invoke(_scrollItems[s], _poolDataIndex[s]);
                return;
            }
        }
    }

    /// <summary>
    /// Cuộn ngay lập tức đến item có dataIndex = index.
    /// </summary>
    /// 


    [SerializeField] int testIndex = 0;
    [Button]
    public void TestScroll() 
    {
        ScrollToIndex(testIndex,false);
    }



    public void JumpToIndex(int index, bool immediate = true)
    {
        _scrollRect.StopMovement();
        index = NormalizeIndex(index-1);

        float targetY = paddingBottom + index * ItemStep;
        float maxY = Mathf.Max(0, GetTotalContentHeight() - _viewport.rect.height);
        targetY = Mathf.Clamp(targetY, 0, maxY);

        _content.anchoredPosition = new Vector2(0, -targetY);
        Debug.Log(targetY);
        // Reset pool data về đúng index, giữ nguyên Y vật lý của item
        _firstVisibleSlot = 0;
        _topDataIndex = index;
        PlaceAllSlots();
        RefreshAllSlots();
    }


    public void ScrollToIndex(int index, bool immediate = true, float duration = 0.5f)
    {
        if (_scrollRect == null || _content == null || _viewport == null)
            return;

        _scrollRect.StopMovement();

        index = NormalizeIndex(index - 1);

        float totalHeight = GetTotalContentHeight();
        float viewportHeight = _viewport.rect.height;

        if (float.IsNaN(totalHeight) || float.IsInfinity(totalHeight))
            totalHeight = 0;

        if (float.IsNaN(viewportHeight) || float.IsInfinity(viewportHeight))
            viewportHeight = 0;

        float targetY = paddingBottom + index * ItemStep;

        if (float.IsNaN(targetY) || float.IsInfinity(targetY))
            targetY = 0;

        float maxY = Mathf.Max(0, totalHeight - viewportHeight);

        if (float.IsNaN(maxY) || float.IsInfinity(maxY))
            maxY = 0;

        targetY = Mathf.Clamp(targetY, 0, maxY);

        _firstVisibleSlot = 0;
        _topDataIndex = index;

        PlaceAllSlots();
        RefreshAllSlots();

        float finalY = -targetY + 300;

        if (float.IsNaN(finalY) || float.IsInfinity(finalY))
            finalY = 0;

        if (immediate)
        {
            _content.anchoredPosition = new Vector2(0, finalY);
        }
        else
        {
            _content
                .DOAnchorPosY(finalY, duration)
                .SetEase(Ease.OutCubic)
                .OnUpdate(CheckAndFill);
        }
    }
    // ══════════════════════════════════════════════════════════════════════
    // PRIVATE LOGIC
    // ══════════════════════════════════════════════════════════════════════

    //private void LateUpdate()
    //{
    //    CheckAndFill();
    //}

    /// <summary>
    /// Kiểm tra top/bottom, recycle & spawn item khi cần.
    /// </summary>
    // Thêm field cache ở đầu class
    private Vector2 _anchorCache = Vector2.zero;
    private void CheckAndFill()
    {
        if (_pool.Count == 0 || _onSetItem == null) return;
        Profiler.BeginSample("float");
        float scrollY = _content.anchoredPosition.y;
        float viewportH = _viewport.rect.height;
        float viewBottom = -scrollY;
        float viewTop = viewBottom + viewportH;
        float bufH = bufferCount * ItemStep;
        float fillBottom = viewBottom - bufH;
        float fillTop = viewTop + bufH;
        Profiler.EndSample();
        Profiler.BeginSample("Up");
        // ── Vuốt LÊN ──────────────────────────────────────────────────────────
        while (true)
        {
            int lastSlot = (_firstVisibleSlot + _pool.Count - 1) % _pool.Count;
            RectTransform lastRT = _pool[lastSlot];

            if (lastRT.anchoredPosition.y > fillTop)
            {
                RectTransform firstRT = _pool[_firstVisibleSlot];
                int newDataIndex = _poolDataIndex[_firstVisibleSlot] - 1;
                if (newDataIndex < 0) break;
                if (_totalCount > 0) newDataIndex = NormalizeIndex(newDataIndex);

                _anchorCache.x = 0f;
                _anchorCache.y = firstRT.anchoredPosition.y - ItemStep;
                lastRT.anchoredPosition = _anchorCache;

                _poolDataIndex[lastSlot] = newDataIndex;
                _onSetItem(_scrollItems[lastSlot], newDataIndex);
                _firstVisibleSlot = lastSlot;
                _topDataIndex = newDataIndex;
            }
            else break;
        }
        Profiler.EndSample();
        Profiler.BeginSample("Down");
        // ── Vuốt XUỐNG ────────────────────────────────────────────────────────
        while (true)
        {
            RectTransform firstRT = _pool[_firstVisibleSlot];

            if (firstRT.anchoredPosition.y + itemHeight < fillBottom)
            {
                int lastSlot = (_firstVisibleSlot + _pool.Count - 1) % _pool.Count;
                RectTransform lastRT = _pool[lastSlot];
                int newDataIndex = _poolDataIndex[lastSlot] + 1;
                if (_totalCount > 0) newDataIndex = NormalizeIndex(newDataIndex);

                _anchorCache.x = 0f;
                _anchorCache.y = lastRT.anchoredPosition.y + ItemStep;
                firstRT.anchoredPosition = _anchorCache;

                _poolDataIndex[_firstVisibleSlot] = newDataIndex;
                _onSetItem(_scrollItems[_firstVisibleSlot], newDataIndex);
                _firstVisibleSlot = (_firstVisibleSlot + 1) % _pool.Count;
                _topDataIndex = newDataIndex;
            }
            else break;
        }
        Profiler.EndSample();
        Profiler.BeginSample("Loop");
        // ── Ẩn item dưới paddingBottom ─────────────────────────────────────────
        float hideThreshold = paddingBottom - itemHeight;
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].gameObject.SetActive(_pool[i].anchoredPosition.y >= hideThreshold);
        }
        Profiler.EndSample();
    }
    // ── Helpers ──────────────────────────────────────────────────────────

    private void PlaceAllSlots()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            int dataIndex = _topDataIndex + i;
            _pool[i].anchoredPosition = new Vector2(0, paddingBottom + dataIndex * ItemStep); // ← + paddingBottom
        }
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            int dataIndex = (_topDataIndex + i);
            if (_totalCount > 0) dataIndex = NormalizeIndex(dataIndex);
            _poolDataIndex[i] = dataIndex;
            _onSetItem?.Invoke(_scrollItems[i], dataIndex);
        }
    }

    private void UpdateContentSize()
    {
        float h = GetTotalContentHeight();
        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private float GetTotalContentHeight()
    {
        if (_totalCount <= 0) return 1_000_000f;
        return paddingBottom + _totalCount * itemHeight + Mathf.Max(0, _totalCount - 1) * itemSpacing; // ← + paddingBottom
    }

    /// <summary>Map index về [0, totalCount) khi finite.</summary>
    private int NormalizeIndex(int index)
    {
        if (_totalCount <= 0) return index;
        return ((index % _totalCount) + _totalCount) % _totalCount;
    }

    private void DestroyPool()
    {
        foreach (var rt in _pool)
            if (rt != null) Destroy(rt.gameObject);
        _pool.Clear();
        _poolDataIndex.Clear();
    }

    private void OnDestroy() => DestroyPool();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (itemHeight <= 0) itemHeight = 100f;
        if (bufferCount < 1) bufferCount = 1;
    }
#endif
}
