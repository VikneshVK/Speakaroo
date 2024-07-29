using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spin : MonoBehaviour
{
    public Vector3 rotateAmount;
    public float rotateTime = 1f;  // Default to 1 to prevent division by zero

    void Update()
    {
        if (rotateTime > 0f)  // Ensure rotateTime is positive
        {
            transform.Rotate(rotateAmount * Time.deltaTime / rotateTime);
        }
    }
}
