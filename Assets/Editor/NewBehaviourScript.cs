using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ExportBlendTreePositions
{
    [MenuItem("Tools/Export BlendTree Positions")]
    public static void ExportPositions()
    {
        // ✅ 선택한 에셋이 Blend Tree 인지 확인
        Object selected = Selection.activeObject;
        if (!(selected is BlendTree blendTree))
        {
            Debug.LogError("⚠️ Blend Tree를 선택하고 실행하세요!");
            return;
        }

        // ✅ 데이터 저장 리스트
        List<PositionData> positions = new List<PositionData>();

        foreach (var motion in blendTree.children)
        {
            positions.Add(new PositionData
            {
                motionName = motion.motion != null ? motion.motion.name : "null",
                posX = motion.position.x,
                posY = motion.position.y
            });
        }

        // ✅ JSON 변환
        string json = JsonHelper.ToJson(positions.ToArray(), true);

        // ✅ 저장 경로 선택
        string path = EditorUtility.SaveFilePanel("Export BlendTree Positions", "", blendTree.name + "_positions.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        File.WriteAllText(path, json, Encoding.UTF8);
        Debug.Log($"✅ BlendTree 좌표를 JSON으로 내보냈습니다! → {path}");
    }

    [System.Serializable]
    public class PositionData
    {
        public string motionName;
        public float posX;
        public float posY;
    }

    // ✅ Unity에서 배열을 JSON으로 내보내기 위한 헬퍼
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array, bool prettyPrint = false)
        {
            Wrapper<T> wrapper = new Wrapper<T> { Items = array };
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
