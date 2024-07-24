using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tube : MonoBehaviour
{
    public int lenth;
    public LineRenderer lineRend;
    public Vector3[] segmentPoses;
    public Transform targetDir;
    public float targetDistance;
    public float smoothSpeed;

    private int segments = 15;
    private Vector3[] segmentV;

    private void Start()
    {

        lineRend.positionCount = lenth;
        segmentPoses = new Vector3[lenth];
        segmentV = new Vector3[lenth];
    }

    private void Update()
    {
        segmentPoses[0] = targetDir.position;

        for (int i = 1; i < segmentPoses.Length; i++)
        {
            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i - 1] +
                targetDir.right * (targetDistance/2), ref segmentV[i], smoothSpeed);
        }

        lineRend.SetPositions(segmentPoses);
    }
}