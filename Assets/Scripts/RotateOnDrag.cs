using UnityEngine;

public class RotateOnDrag : MonoBehaviour
{
    public float speed = 5f;
    public Transform target;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 direction = target.position - transform.position;

        // If the sprite is flipped, invert the direction's x-component to maintain line direction
        if (spriteRenderer.flipX)
        {
            direction.x = -direction.x;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);

        // Check the Z rotation angle and flip the sprite accordingly
        float zRotation = transform.rotation.eulerAngles.z;
        if (zRotation > 180)
        {
            zRotation -= 360;
        }

        if (zRotation >= -90 && zRotation <= 90)
        {
            spriteRenderer.flipY = false;
        }
        else
        {
            spriteRenderer.flipY = true;
        }
    }
}
