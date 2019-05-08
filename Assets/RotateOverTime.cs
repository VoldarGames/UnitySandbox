using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public bool RotateXAxis;
    public bool RotateYAxis;
    public bool RotateZAxis;
    public float AngularSpeed = 1;


    private float currentXDegree;
    private float currentYDegree;
    private float currentZDegree;
    // Update is called once per frame
    void Update()
    {
        if(RotateXAxis)
        {
            currentXDegree = (AngularSpeed * Time.deltaTime) % 360;
        }

        if(RotateYAxis)
        {
            currentYDegree = (AngularSpeed * Time.deltaTime) % 360;
        }

        if(RotateZAxis)
        {
            currentZDegree = (AngularSpeed * Time.deltaTime) % 360;
        }

        gameObject.transform.Rotate(currentXDegree, currentYDegree, currentZDegree);
    }
}
