using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public int length;
    public LineRenderer lineRend;
    public Vector3[] segmentPoses;
    public Transform targetDir;
    public float targetDistance;
    public float smoothSpeed;
    public GameObject mainGameobject;

    private Vector3[] segmentV;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        lineRend.positionCount = length;
        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];
        spriteRenderer = mainGameobject.GetComponent<SpriteRenderer>();  // Ensure this is correctly assigned
    }

    private void Update()
    {
        segmentPoses[0] = targetDir.position;

        for (int i = 1; i < segmentPoses.Length; i++)
        {
            // Adjust the target direction based on flipY state
            Vector3 adjustedTargetDir = spriteRenderer.flipY ? -targetDir.right : targetDir.right;

            segmentPoses[i] = Vector3.SmoothDamp(segmentPoses[i], segmentPoses[i - 1] +
                adjustedTargetDir * targetDistance, ref segmentV[i], smoothSpeed);
        }

        lineRend.SetPositions(segmentPoses);
    }
}