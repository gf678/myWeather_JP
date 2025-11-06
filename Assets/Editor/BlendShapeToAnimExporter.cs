using UnityEngine;
using UnityEditor;
using System.IO;

public class BlendShapeToAnimExporter_Advanced : EditorWindow
{
    private SkinnedMeshRenderer targetRenderer;
    private string animName = "Expression";

    [MenuItem("Tools/BlendShape Exporter (All/Eye/Mouth)")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapeToAnimExporter_Advanced>("BlendShape Exporter");
    }

    void OnGUI()
    {
        GUILayout.Label("🎭 BlendShape → Animation Exporter", EditorStyles.boldLabel);

        targetRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Target Renderer", targetRenderer, typeof(SkinnedMeshRenderer), true);
        animName = EditorGUILayout.TextField("Animation Name", animName);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎬 Export ALL"))
            ExportBlendShapesToAnim(animName + "_All", ExportType.All);

        if (GUILayout.Button("👀 Export EYE"))
            ExportBlendShapesToAnim(animName + "_Eye", ExportType.Eye);

        if (GUILayout.Button("👄 Export MOUTH"))
            ExportBlendShapesToAnim(animName + "_Mouth", ExportType.Mouth);
        EditorGUILayout.EndHorizontal();
    }

    enum ExportType { All, Eye, Mouth }

    void ExportBlendShapesToAnim(string fileName, ExportType type)
    {
        if (targetRenderer == null)
        {
            Debug.LogError("❌ SkinnedMeshRenderer를 지정하세요!");
            return;
        }

        var clip = new AnimationClip();
        var mesh = targetRenderer.sharedMesh;

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string shapeName = mesh.GetBlendShapeName(i);
            float weight = targetRenderer.GetBlendShapeWeight(i);

            if (Mathf.Approximately(weight, 0f)) continue; // 0은 무시

            // Export 타입 필터링
            if (type == ExportType.Eye && !shapeName.ToLower().Contains("eye")) continue;
            if (type == ExportType.Mouth && !shapeName.ToLower().Contains("mouth")) continue;

            string path = AnimationUtility.CalculateTransformPath(targetRenderer.transform, null);
            string propertyName = $"blendShape.{shapeName}";

            var curve = new AnimationCurve();
            curve.AddKey(0f, weight);
            curve.AddKey(1f / clip.frameRate, weight);

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer), propertyName), curve);
        }

        // 저장 경로 생성
        if (!Directory.Exists("Assets/Animations"))
            Directory.CreateDirectory("Assets/Animations");

        string savePath = $"Assets/Animations/{fileName}.anim";
        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"✅ {fileName}.anim 파일 생성 완료! 경로: {savePath}");
    }
}
