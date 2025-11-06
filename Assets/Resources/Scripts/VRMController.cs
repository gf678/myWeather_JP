using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMController : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        // 강제로 Idle 상태 유지
        animator.Play("fingerpoint");
    }

    void Update()
    {
        // 아무 입력 없이 항상 Idle 유지
        // animator.SetBool, animator.SetTrigger 사용 안함
    }
}
