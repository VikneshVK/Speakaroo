using UnityEngine;

public class GlassController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Blender_Jar"))
        {
            DraggingController draggingController = other.GetComponent<DraggingController>();
            if (draggingController != null)
            {
                draggingController.OnGlassCollision(this);
            }
        }
    }
}
