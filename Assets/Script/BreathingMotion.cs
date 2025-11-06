using UnityEngine;

public class BreathingMotion : MonoBehaviour
{
    [Header("호흡 설정")]
    public float amplitude = 0.005f; // 움직임 크기 (0.005 = 0.5cm 정도)
    public float frequency = 0.8f;   // 초당 호흡 주기
    public float chestTilt = 1.5f;   // 살짝 고개나 가슴이 움직이게

    private Vector3 initialPos;
    private Quaternion initialRot;
    private float offset;

    void Start()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
        offset = Random.Range(0f, Mathf.PI * 2); // 개체별 약간 어긋남 (자연스럽게)
    }

    void Update()
    {
        float t = Time.time * frequency + offset;
        float breathing = Mathf.Sin(t) * amplitude;

        // 위아래 살짝 움직임
        transform.localPosition = initialPos + new Vector3(0, breathing, 0);

        // 살짝 회전 (가슴이 오르내리듯)
        transform.localRotation = initialRot * Quaternion.Euler(breathing * chestTilt, 0, 0);
    }
}
