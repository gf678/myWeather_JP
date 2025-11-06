/*using UnityEngine;
using System.Collections;

public class EyeBlinkController : MonoBehaviour
{
    public SkinnedMeshRenderer bodyRenderer; // 얼굴(=Body) Mesh

    [Header("BlendShape 이름 설정")]
    public string blinkLeftName = "eyeBlinkLeft";
    public string blinkRightName = "eyeBlinkRight";
    public string winkName = "ウィンク"; // 👈 일본어 이름 블렌드쉐이프 (Wink)
    public string blink1Name = "eyeBlink";   // 추가: 수동 제어용 1
    public string blink2Name = "eyeBlink2";  // 추가: 수동 제어용 2

    [Header("깜빡임 속도 및 간격")]
    public float blinkSpeed = 1f;
    public float minInterval = 2.5f;
    public float maxInterval = 10.0f;
    public float desync = 0.1f;

    private int leftIndex;
    private int rightIndex;
    private int winkIndex;
    private int blink1Index;
    private int blink2Index;

    private float blinkWeightLeft = 0f;
    private float blinkWeightRight = 0f;
    private float nextBlinkTime;
    private bool blinking = false;

    void Start()
    {
        if (bodyRenderer == null)
            bodyRenderer = GetComponent<SkinnedMeshRenderer>();

        leftIndex = bodyRenderer.sharedMesh.GetBlendShapeIndex(blinkLeftName);
        rightIndex = bodyRenderer.sharedMesh.GetBlendShapeIndex(blinkRightName);
        winkIndex = bodyRenderer.sharedMesh.GetBlendShapeIndex(winkName);
        blink1Index = bodyRenderer.sharedMesh.GetBlendShapeIndex(blink1Name);
        blink2Index = bodyRenderer.sharedMesh.GetBlendShapeIndex(blink2Name);

        if (leftIndex < 0 || rightIndex < 0)
        {
            Debug.LogError("❌ eyeBlinkLeft / eyeBlinkRight 블렌드쉐이프를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        SetNextBlinkTime();
    }

    void LateUpdate()
    {
        // 🔹 윙크 중 체크
        bool isWinking = (winkIndex >= 0 && bodyRenderer.GetBlendShapeWeight(winkIndex) > 0.1f);

        // 🔹 eyeBlink / eyeBlink2가 80 이상이면 깜빡임 비활성화
        bool isManualBlinking = false;
        if (blink1Index >= 0 && bodyRenderer.GetBlendShapeWeight(blink1Index) >= 80f)
            isManualBlinking = true;
        if (blink2Index >= 0 && bodyRenderer.GetBlendShapeWeight(blink2Index) >= 80f)
            isManualBlinking = true;

        // 👁️ 자동 깜빡임 실행 조건: 수동 블링크 & 윙크가 모두 비활성일 때만
        if (Time.time >= nextBlinkTime && !blinking && !isManualBlinking)
        {
            StartCoroutine(BlinkRoutine(isWinking));
        }

        // 값 보정
        blinkWeightLeft = Mathf.Clamp(blinkWeightLeft, 0f, 100f);
        blinkWeightRight = Mathf.Clamp(blinkWeightRight, 0f, 100f);

        // 반영
        bodyRenderer.SetBlendShapeWeight(leftIndex, blinkWeightLeft);
        bodyRenderer.SetBlendShapeWeight(rightIndex, blinkWeightRight);
    }

    IEnumerator BlinkRoutine(bool isWinking)
    {
        blinking = true;

        // 눈 감기
        while ((blinkWeightLeft < 100f && !isWinking) || blinkWeightRight < 100f)
        {
            if (!isWinking)
                blinkWeightLeft = Mathf.MoveTowards(blinkWeightLeft, 100f, Time.deltaTime * blinkSpeed * 100);

            blinkWeightRight = Mathf.MoveTowards(blinkWeightRight, 100f, Time.deltaTime * blinkSpeed * 100);
            yield return null;
        }

        // 잠깐 유지
        yield return new WaitForSeconds(0.05f + Random.Range(0f, desync));

        // 눈 뜨기
        while ((blinkWeightLeft > 0f && !isWinking) || blinkWeightRight > 0f)
        {
            if (!isWinking)
                blinkWeightLeft = Mathf.MoveTowards(blinkWeightLeft, 0f, Time.deltaTime * blinkSpeed * 100);

            blinkWeightRight = Mathf.MoveTowards(blinkWeightRight, 0f, Time.deltaTime * blinkSpeed * 100);
            yield return null;
        }

        blinking = false;
        SetNextBlinkTime();
    }

    void SetNextBlinkTime()
    {
        nextBlinkTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
*/