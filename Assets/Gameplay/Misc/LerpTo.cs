using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTo : MonoBehaviour
{

    [SerializeField] Transform target;
    [SerializeField] float lerpTime;
    [SerializeField] Vector3 offset;

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, lerpTime);
    }
}
