using UnityEngine;

public class RotateOnDrag : MonoBehaviour
{
    public float speed = 5f;
    public Collider2D targetCollider1;  // First target collider
    public Collider2D targetCollider2;  // Second target collider
    private SpriteRenderer spriteRenderer;

    private Collider2D currentTargetCollider;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentTargetCollider = targetCollider1;  // Set an initial target
    }

    private void Update()
    {
        // Determine which collider to use based on Y position
        float objectY = transform.position.y;
        float targetY1 = targetCollider1.bounds.center.y;
        float targetY2 = targetCollider2.bounds.center.y;

        if (Mathf.Abs(objectY - targetY1) < Mathf.Abs(objectY - targetY2))
        {
            currentTargetCollider = targetCollider1;
        }
        else
        {
            currentTargetCollider = targetCollider2;
        }

        
        Vector2 targetPosition = currentTargetCollider.bounds.center;

       
        Vector2 direction = targetPosition - (Vector2)transform.position;

       
        if (spriteRenderer.flipX)
        {
            direction.x = -direction.x;
        }

        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);

        // Determine if the sprite should be flipped based on the Z rotation angle
        float zRotation = transform.rotation.eulerAngles.z;
        if (zRotation > 180)
        {
            zRotation -= 360;
        }

        spriteRenderer.flipY = zRotation < -90 || zRotation > 90;
    }
}
