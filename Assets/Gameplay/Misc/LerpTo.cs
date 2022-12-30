using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LerpTo : MonoBehaviour
{

    public Transform target;
    [SerializeField] float lerpTime;
    [SerializeField] Vector3 offset;

    private void FixedUpdate()
    {
        if(target) transform.position = Vector3.Lerp(transform.position, target.position + offset, lerpTime);
    }
}
