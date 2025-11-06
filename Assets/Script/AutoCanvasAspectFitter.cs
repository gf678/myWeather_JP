using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
[ExecuteAlways]
public class AutoCanvasAspectFitter : MonoBehaviour
{
    [Header("기준 비율 (세로형 앱 기준)")]
    public float referenceWidth = 1080f;
    public float referenceHeight = 1920f;

    [Header("비율 보정 강도 (0 = 가로 기준, 1 = 세로 기준, 0.5 = 중간)")]
    [Range(0f, 1f)] public float matchWidthOrHeight = 0.5f;

    private CanvasScaler scaler;
    private float lastWidth, lastHeight;

    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        ApplySettings();
    }

    void Update()
    {
        // 해상도 변경 시 자동 적용
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplySettings();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }

    void ApplySettings()
    {
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        // Aspect 비율 맞춤
        float targetAspect = referenceWidth / referenceHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            // 화면이 너무 넓은 경우 (가로가 길면) → 세로 기준으로 맞춤
            scaler.matchWidthOrHeight = 1f;
        }
        else if (currentAspect < targetAspect)
        {
            // 세로가 더 긴 경우 → 가로 기준으로 맞춤
            scaler.matchWidthOrHeight = 0f;
        }
        else
        {
            // 거의 동일하면 중간값 유지
            scaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }
}
