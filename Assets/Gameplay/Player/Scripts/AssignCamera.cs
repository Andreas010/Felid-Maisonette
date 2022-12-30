using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignCamera : MonoBehaviour
{

    [SerializeField] Transform cameraTarget;

    private void Start()
    {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<LerpTo>().target = cameraTarget;
    }

}
