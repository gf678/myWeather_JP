using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Target")]
    public Transform faceTarget;       // 카메라가 바라볼 대상 (캐릭터 머리 등)

    [Header("Zoom Settings")]
    public float zoomSpeed = 3f;       // 확대/축소 속도
    public float minDistance = 1.5f;   // 최소 거리
    public float maxDistance = 5f;     // 최대 거리

    private float currentDistance;

    void Start()
    {
        if (faceTarget != null)
            currentDistance = Vector3.Distance(transform.position, faceTarget.position);
    }

    void Update()
    {
        if (faceTarget == null) return;

        // --- 🖱 PC 마우스 휠 줌 ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            ZoomCamera(-scroll * zoomSpeed);
        }

        // --- 🤲 모바일 핀치 줌 ---
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (prevPos1 - prevPos2).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;
            float diff = currentMagnitude - prevMagnitude;

            ZoomCamera(-diff * 0.01f);
        }
    }

    private void ZoomCamera(float increment)
    {
        // 현재 거리 계산 및 보정
        currentDistance = Mathf.Clamp(currentDistance + increment, minDistance, maxDistance);

        // 카메라와 대상 간 방향 벡터 계산
        Vector3 dir = (transform.position - faceTarget.position).normalized;
        transform.position = faceTarget.position + dir * currentDistance;
    }
}
