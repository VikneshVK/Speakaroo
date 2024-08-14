using UnityEngine;
using System.Collections;

public class GlassController : MonoBehaviour
{
    public float pourAngle = 45f; // Maximum angle for pouring
    public float rotationSpeed = 5f; // Speed of rotation

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Blender_Jar"))
        {
            RotateJarTowardsGlass(other, true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Blender_Jar"))
        {
            RotateJarTowardsGlass(other, false);
        }
    }

    private void RotateJarTowardsGlass(Collider2D jarCollider, bool isEntering)
    {
        DraggingController draggingController = jarCollider.GetComponent<DraggingController>();
        if (draggingController != null)
        {
            if (isEntering)
            {
                // Determine the direction of rotation based on the glass tag
                float targetAngle = gameObject.CompareTag("Glass1") ? pourAngle : -pourAngle;

                // Smoothly rotate towards the target angle
                draggingController.StartCoroutine(RotateJar(draggingController, targetAngle));
            }
            else
            {
                // Reset rotation when exiting
                draggingController.transform.rotation = Quaternion.identity;
            }
        }
    }

    private IEnumerator RotateJar(DraggingController draggingController, float targetAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        while (Quaternion.Angle(draggingController.transform.rotation, targetRotation) > 0.1f)
        {
            draggingController.transform.rotation = Quaternion.Slerp(draggingController.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        draggingController.transform.rotation = targetRotation; // Ensure it reaches the exact angle
    }
}
