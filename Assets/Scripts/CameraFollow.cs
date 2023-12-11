using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed;

    [SerializeField]
    private Vector3 offset;

    private Vector3 velocity = Vector3.zero;

    private void Update()
    {
        FollowWithSmoothDamp();
        //FollowWithLerp();
    }
    

    void FollowWithSmoothDamp()
    {
        Vector2 targetPos = target.TransformPoint(offset);

        Vector3 finalPos =  Vector3.SmoothDamp(transform.position,targetPos,ref velocity,followSpeed);
        finalPos.z = -10;
        transform.position = finalPos;
    }

    void FollowWithLerp()
    {
        Vector2 targetPos = target.position + offset;

        Vector3 finalPos = Vector2.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        finalPos.z = -10;

        transform.position = finalPos;
    }

}
