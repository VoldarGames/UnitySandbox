using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Vector3 Target;
    public float Speed;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Target, Vector3.up, Speed * Time.deltaTime);
    }
}
