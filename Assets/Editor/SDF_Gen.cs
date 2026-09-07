using UnityEngine;
using UnityEditor;
using System.IO;

public class SDFTextureGenerator : EditorWindow
{
    private Texture2D _source;
    private int _outputSize = 256;
    private float _maxDistance = 16f;
    private string _savePath = "Assets/SDF_Output.png";

    [MenuItem("Tools/SDF Generator")]
    static void Open()
    {
        GetWindow<SDFTextureGenerator>("SDF Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Source Texture (Alpha)", EditorStyles.boldLabel);
        _source = (Texture2D)EditorGUILayout.ObjectField(_source, typeof(Texture2D), false);

        _outputSize = EditorGUILayout.IntField("Output Size", _outputSize);
        _maxDistance = EditorGUILayout.FloatField("Max Distance (px)", _maxDistance);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);

        GUILayout.Space(10);

        GUI.enabled = _source != null;
        if (GUILayout.Button("Generate SDF"))
        {
            Generate();
        }
        GUI.enabled = true;
    }

    void Generate()
    {
        Texture2D src = GetReadableCopy(_source);
        Texture2D sdf = new Texture2D(_outputSize, _outputSize, TextureFormat.RGB24, false);

        Color[] srcPixels = src.GetPixels();
        Color[] outPixels = new Color[_outputSize * _outputSize];

        for (int y = 0; y < _outputSize; y++)
        {
            for (int x = 0; x < _outputSize; x++)
            {
                float u = x / (float)(_outputSize - 1);
                float v = y / (float)(_outputSize - 1);

                int sx = Mathf.RoundToInt(u * (src.width - 1));
                int sy = Mathf.RoundToInt(v * (src.height - 1));

                bool inside = srcPixels[sy * src.width + sx].a > 0.5f;

                float minDist = _maxDistance;

                for (int yy = 0; yy < src.height; yy++)
                {
                    for (int xx = 0; xx < src.width; xx++)
                    {
                        bool otherInside = srcPixels[yy * src.width + xx].a > 0.5f;
                        if (otherInside != inside) continue;

                        float dx = xx - sx;
                        float dy = yy - sy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);

                        if (dist < minDist)
                            minDist = dist;
                    }
                }

                float signedDist = inside ? minDist : -minDist;
                float normalized = Mathf.Clamp01(0.5f + signedDist / (_maxDistance * 2f));

                outPixels[y * _outputSize + x] = new Color(normalized, normalized, normalized, 1);
            }
        }

        sdf.SetPixels(outPixels);
        sdf.Apply();

        File.WriteAllBytes(_savePath, sdf.EncodeToPNG());
        AssetDatabase.Refresh();

        Debug.Log("SDF generated: " + _savePath);
    }

    Texture2D GetReadableCopy(Texture2D tex)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            tex.width, tex.height, 0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(tex, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D copy = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        copy.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return copy;
    }
}
