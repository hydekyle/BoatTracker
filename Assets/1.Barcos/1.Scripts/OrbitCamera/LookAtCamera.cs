using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform target;

    void Start()
    {
        target = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.LookAt(target);
    }
}
