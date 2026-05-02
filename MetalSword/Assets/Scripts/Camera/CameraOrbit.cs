using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("타겟(플레이어)")]
    [SerializeField] private Transform target;

    [Header("카메라 거리/높이 설정")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;

    [Header("회전 속도 (높을수록 빠르게)")]
    [SerializeField] private float rotationSpeed = 100f;

    //회전 각도
    private float currentAngle = 0f;

    private void LateUpdate()
    {
        if (target == null)
            return;
        float mouseX = Input.GetAxis("Mouse X");
        currentAngle += mouseX * rotationSpeed * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);

        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
        offset += Vector3.up * height;

        transform.position = target.position + offset;

        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
