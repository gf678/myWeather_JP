using UnityEngine;

public class CameraDragRotate : MonoBehaviour
{
    public Transform faceTarget;        // 캐릭터 머리(혹은 얼굴) 위치
    public float rotationSpeed = 0.2f;  // 드래그 민감도
    public float smoothReturnSpeed = 2f; // 복귀 속도
    private Vector3 lastMousePos;
    private bool isDragging = false;

    void Update()
    {
        // --- 모바일 터치 ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
                transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.Self);
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
        // --- PC 마우스 ---
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.Self);
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        lastMousePos = Input.mousePosition;

        // --- 드래그가 끝났다면, 카메라 위치 그대로 유지 + 얼굴 방향으로 회복 ---
        if (!isDragging && faceTarget != null)
        {
            // "카메라 위치"는 그대로, 바라보는 방향만 보정
            Vector3 lookDir = faceTarget.position - transform.position;
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothReturnSpeed);
        }
    }
}
