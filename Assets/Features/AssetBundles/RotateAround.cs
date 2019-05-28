using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Vector3 Target;
    public float Speed;
    public bool LimitAngle;
    bool directionRight = true;
    float currentAngle;
    public float AngleLimit;

    // Update is called once per frame
    void Update()
    {
        if (LimitAngle)
        {            
            if (directionRight)
            {
                currentAngle += Speed * Time.deltaTime;
            }
            else
            {
                currentAngle -= Speed * Time.deltaTime;
            }

            if(Mathf.Abs(currentAngle) > AngleLimit)
            {
                directionRight = !directionRight;
            }

            if (directionRight)
            {
                transform.RotateAround(Target, Vector3.up, Speed * Time.deltaTime);
            }
            else
            {
                transform.RotateAround(Target, Vector3.up, -Speed * Time.deltaTime);
            }
            
        }
        else
        {
            transform.RotateAround(Target, Vector3.up, Speed * Time.deltaTime);
        }
    }
}
