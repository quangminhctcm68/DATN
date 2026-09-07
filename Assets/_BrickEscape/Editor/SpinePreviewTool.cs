using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Spine.Unity;
using Spine;
using System.Linq;

public class SpinePreviewWindow : OdinEditorWindow
{
    [MenuItem("Tools/Spine/SkeletonGraphic Preview")]
    static void Open()
    {
        GetWindow<SpinePreviewWindow>().Show();
    }

    // ─── Inspector fields ───────────────────────────────────────────────────

    [Title("Target")]
    [Required]
    public SkeletonGraphic target;

    [ValueDropdown(nameof(GetAnimations))]
    [OnValueChanged(nameof(OnAnimationChanged))]
    public string animationName;

    [Title("Playback")]
    [OnValueChanged(nameof(OnLoopChanged))]
    public bool loop = true;

    [Range(0, 3)]
    public float speed = 1f;

    // ─── Private state ──────────────────────────────────────────────────────

    float time;
    float duration;
    bool playing;
    double lastEditorTime;

    // Timeline drag state
    bool isDraggingTimeline;
    int timelineControlID;

    // Cached styles
    GUIStyle timeLabelStyle;

    // Layout constants
    const float TIMELINE_HEIGHT = 28f;
    const float HEADER_HEIGHT = 18f;
    const float TICK_SPACING_MIN = 40f;
    const int FPS = 30;

    // ─── Lifecycle ──────────────────────────────────────────────────────────

    protected override void OnEnable()
    {
        base.OnEnable();
        EditorApplication.update += EditorUpdate;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EditorApplication.update -= EditorUpdate;
    }

    // ─── Editor update loop ─────────────────────────────────────────────────

    void EditorUpdate()
    {
        if (!playing || target == null || string.IsNullOrEmpty(animationName)) return;

        double now = EditorApplication.timeSinceStartup;
        float delta = (float)(now - lastEditorTime) * speed;
        lastEditorTime = now;

        time += delta;

        if (time >= duration)
        {
            if (loop) time = time % Mathf.Max(duration, 0.0001f);
            else { time = duration; playing = false; }
        }

        ApplyToScene(delta);
        Repaint();
    }

    // ─── Custom GUI ─────────────────────────────────────────────────────────

    protected override void OnImGUI()
    {
        base.OnImGUI(); // Odin draws: target, animationName, loop, speed

        InitStyles();

        EditorGUILayout.Space(6);

        DrawTransportButtons();

        EditorGUILayout.Space(4);

        // Time / Frame label
        float safeD = Mathf.Max(duration, 0.0001f);
        int curFrame = Mathf.RoundToInt(time * FPS);
        int totalFrame = Mathf.RoundToInt(safeD * FPS);
        EditorGUILayout.LabelField(
            $"Frame  {curFrame} / {totalFrame}    |    {time:F3}s / {safeD:F3}s",
            timeLabelStyle);

        EditorGUILayout.Space(2);

        // Timeline widget
        Rect timelineRect = GUILayoutUtility.GetRect(
            GUIContent.none, GUIStyle.none,
            GUILayout.Height(HEADER_HEIGHT + TIMELINE_HEIGHT),
            GUILayout.ExpandWidth(true));

        // Small horizontal padding so it doesn't touch the window edge
        timelineRect = new Rect(timelineRect.x + 4, timelineRect.y,
                                timelineRect.width - 8, timelineRect.height);

        DrawTimeline(timelineRect, safeD);

        EditorGUILayout.Space(4);
    }

    // ─── Transport buttons ───────────────────────────────────────────────────

    void DrawTransportButtons()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // ◼ Stop
        GUI.backgroundColor = new Color(0.85f, 0.35f, 0.35f);
        if (GUILayout.Button("◼  Stop", GUILayout.Width(90), GUILayout.Height(28)))
            DoStop();

        GUILayout.Space(4);

        // ▶ Play  /  ⏸ Pause  (toggle)
        if (playing)
        {
            GUI.backgroundColor = new Color(0.95f, 0.85f, 0.25f);
            if (GUILayout.Button("⏸  Pause", GUILayout.Width(100), GUILayout.Height(28)))
                DoPause();
        }
        else
        {
            GUI.backgroundColor = new Color(0.35f, 0.85f, 0.45f);
            if (GUILayout.Button("▶  Play", GUILayout.Width(100), GUILayout.Height(28)))
                DoPlay();
        }

        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    // ─── Timeline renderer ───────────────────────────────────────────────────

    void DrawTimeline(Rect fullRect, float safeD)
    {
        Rect headerRect = new Rect(fullRect.x, fullRect.y,
                                   fullRect.width, HEADER_HEIGHT);
        Rect barRect = new Rect(fullRect.x, fullRect.y + HEADER_HEIGHT,
                                   fullRect.width, TIMELINE_HEIGHT);

        // Backgrounds
        EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f));
        EditorGUI.DrawRect(barRect, new Color(0.22f, 0.22f, 0.22f));

        // Tick marks
        DrawTicks(headerRect, safeD);

        // Progress fill
        float progress = Mathf.Clamp01(time / safeD);
        Rect fillRect = new Rect(barRect.x, barRect.y,
                                  barRect.width * progress, barRect.height);
        EditorGUI.DrawRect(fillRect, new Color(0.25f, 0.6f, 1f, 0.30f));

        // Playhead needle (spans full height)
        float needleX = fullRect.x + fullRect.width * progress;
        EditorGUI.DrawRect(new Rect(needleX - 1, fullRect.y, 2, fullRect.height),
                           new Color(0.3f, 0.78f, 1f, 0.95f));

        // Diamond handle on the bar
        DrawDiamond(needleX, barRect.center.y, 6, new Color(0.3f, 0.78f, 1f));

        // Frame number badge next to needle
        DrawNeedleLabel(needleX, headerRect, Mathf.RoundToInt(time * FPS));

        // Border
        DrawRectOutline(fullRect, new Color(0.12f, 0.12f, 0.12f));

        // Input
        HandleTimelineInput(fullRect, barRect, safeD);
    }

    void DrawTicks(Rect headerRect, float safeD)
    {
        if (safeD <= 0 || headerRect.width <= 0) return;

        // Pick the coarsest interval where ticks are at least TICK_SPACING_MIN px apart
        float[] candidates = {
            1f/FPS, 2f/FPS, 5f/FPS, 10f/FPS,
            15f/FPS, 20f/FPS, 0.5f, 1f, 2f, 5f, 10f, 30f, 60f
        };
        float tickInterval = candidates[candidates.Length - 1];
        foreach (float c in candidates)
        {
            if ((c / safeD) * headerRect.width >= TICK_SPACING_MIN)
            { tickInterval = c; break; }
        }

        float labelInterval = tickInterval;
        while ((labelInterval / safeD) * headerRect.width < TICK_SPACING_MIN * 2.2f)
            labelInterval *= 2f;

        GUIStyle tickLabel = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9,
            alignment = TextAnchor.UpperCenter,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };

        for (float t = 0; t <= safeD + tickInterval * 0.5f; t += tickInterval)
        {
            float x = headerRect.x + (t / safeD) * headerRect.width;
            float normalised = t / labelInterval;
            bool isMajor = Mathf.Abs(normalised - Mathf.Round(normalised)) < 0.01f;

            float tickH = isMajor ? HEADER_HEIGHT * 0.55f : HEADER_HEIGHT * 0.28f;
            Color col = isMajor ? new Color(0.55f, 0.55f, 0.55f)
                                  : new Color(0.36f, 0.36f, 0.36f);
            EditorGUI.DrawRect(new Rect(x, headerRect.yMax - tickH, 1, tickH), col);

            if (isMajor)
            {
                int frame = Mathf.RoundToInt(t * FPS);
                GUI.Label(new Rect(x - 20, headerRect.y, 40, HEADER_HEIGHT),
                          frame.ToString(), tickLabel);
            }
        }
    }

    // Small diamond shape for the playhead handle
    void DrawDiamond(float cx, float cy, float r, Color col)
    {
        Handles.BeginGUI();
        Handles.color = col;
        Vector3[] verts = {
            new Vector3(cx,     cy - r, 0),
            new Vector3(cx + r, cy,     0),
            new Vector3(cx,     cy + r, 0),
            new Vector3(cx - r, cy,     0),
        };
        Handles.DrawAAConvexPolygon(verts);
        Handles.EndGUI();
    }

    void DrawNeedleLabel(float needleX, Rect headerRect, int frame)
    {
        const float badgeW = 34f;
        float badgeX = Mathf.Clamp(needleX - badgeW * 0.5f,
                                   headerRect.x,
                                   headerRect.xMax - badgeW);
        Rect badgeRect = new Rect(badgeX, headerRect.y + 1, badgeW, HEADER_HEIGHT - 2);
        EditorGUI.DrawRect(badgeRect, new Color(0.15f, 0.5f, 0.85f, 0.85f));
        GUIStyle s = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 9,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        GUI.Label(badgeRect, frame.ToString(), s);
    }

    void DrawRectOutline(Rect r, Color col)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), col);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), col);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), col);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), col);
    }

    // ─── Input handling ──────────────────────────────────────────────────────

    void HandleTimelineInput(Rect fullRect, Rect barRect, float safeD)
    {
        UnityEngine.Event e = UnityEngine.Event.current;
        timelineControlID = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.MouseDown && fullRect.Contains(e.mousePosition))
        {
            isDraggingTimeline = true;
            GUIUtility.hotControl = timelineControlID;
            playing = false;
            ScrubToX(e.mousePosition.x, fullRect, safeD);
            e.Use();
        }

        if (e.type == EventType.MouseDrag && isDraggingTimeline)
        {
            ScrubToX(e.mousePosition.x, fullRect, safeD);
            e.Use();
        }

        if (e.type == EventType.MouseUp && isDraggingTimeline)
        {
            isDraggingTimeline = false;
            GUIUtility.hotControl = 0;
            e.Use();
        }

        // Keyboard: Left / Right arrow = step one frame
        if (e.type == EventType.KeyDown && fullRect.Contains(e.mousePosition))
        {
            if (e.keyCode == KeyCode.RightArrow)
            { time = Mathf.Min(time + 1f / FPS, safeD); SampleAtTime(time); Repaint(); e.Use(); }
            else if (e.keyCode == KeyCode.LeftArrow)
            { time = Mathf.Max(time - 1f / FPS, 0); SampleAtTime(time); Repaint(); e.Use(); }
        }

        if (fullRect.Contains(e.mousePosition))
            EditorGUIUtility.AddCursorRect(fullRect, MouseCursor.SlideArrow);
    }

    void ScrubToX(float mouseX, Rect barRect, float safeD)
    {
        float t = Mathf.InverseLerp(barRect.x, barRect.xMax, mouseX);
        time = Mathf.Clamp(t * safeD, 0, safeD);
        SampleAtTime(time);
        Repaint();
    }

    // ─── Playback helpers ─────────────────────────────────────────────────────

    void DoPlay()
    {
        if (target == null || string.IsNullOrEmpty(animationName)) return;
        target.Initialize(false);
        if (!loop && time >= duration) time = 0;

        var entry = target.AnimationState.SetAnimation(0, animationName, loop);
        entry.TrackTime = time;
        target.AnimationState.TimeScale = speed;

        playing = true;
        lastEditorTime = EditorApplication.timeSinceStartup;
    }

    void DoPause()
    {
        playing = false;
        if (target != null) target.AnimationState.TimeScale = 0;
    }

    void DoStop()
    {
        playing = false;
        time = 0;
        if (target != null)
        {
            target.AnimationState.TimeScale = 0;
            SampleAtTime(0);
        }
    }

    // ─── Animation change ────────────────────────────────────────────────────

    void OnAnimationChanged()
    {
        if (target == null || string.IsNullOrEmpty(animationName)) return;
        target.Initialize(false);
        var animData = target.Skeleton.Data.FindAnimation(animationName);
        if (animData == null) return;

        duration = animData.Duration;
        time = 0;
        playing = false;

        target.AnimationState.SetAnimation(0, animationName, loop);
        target.AnimationState.TimeScale = 0;
        SampleAtTime(0);
    }

    void OnLoopChanged()
    {
        if (target == null || string.IsNullOrEmpty(animationName)) return;
        var entry = target.AnimationState.GetCurrent(0);
        if (entry != null) entry.Loop = loop;
    }

    // ─── Core sampling ────────────────────────────────────────────────────────

    void ApplyToScene(float deltaTime)
    {
        if (target == null || target.AnimationState == null) return;
        target.AnimationState.TimeScale = speed;
        target.Update(deltaTime);
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    void SampleAtTime(float sampleTime)
    {
        if (target == null || string.IsNullOrEmpty(animationName)) return;
        if (target.Skeleton == null) target.Initialize(false);
        var skeleton = target.Skeleton;
        var animData = skeleton.Data.FindAnimation(animationName);
        if (animData == null) return;

        skeleton.SetToSetupPose();
        animData.Apply(skeleton, 0f, sampleTime, false, null, 1f,
            MixBlend.Setup, MixDirection.In);
        skeleton.UpdateWorldTransform();
        target.UpdateMesh();

        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    // ─── Dropdown helper ──────────────────────────────────────────────────────

    string[] GetAnimations()
    {
        if (target == null) return new string[0];
        target.Initialize(false);
        return target.Skeleton.Data.Animations.Select(a => a.Name).ToArray();
    }

    // ─── Style init ───────────────────────────────────────────────────────────

    void InitStyles()
    {
        if (timeLabelStyle != null) return;
        timeLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
        };
    }
}