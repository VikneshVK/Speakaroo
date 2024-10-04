using UnityEngine;
namespace ResolutionMagic
{

// simple movement using the basic Unity input system
// Attach to any object, adjust the speed if necessary, then move it around with keyboard/joystick, etc.
// this is intended only for prototyping and demonstrations
public class SimpleMovement2D : MonoBehaviour
{
    [SerializeField] float speed = 4;
    [SerializeField] bool flipSprite = true;

    [SerializeField] bool useVerticalMovement = true;
    SpriteRenderer sprite;
    
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        var movement = new Vector3(Input.GetAxis("Horizontal"), useVerticalMovement ? Input.GetAxis("Vertical") : 0, 0);
        if(sprite != null && flipSprite)
        {
            sprite.flipX = Input.GetAxis("Horizontal") < 0;
        }
        
        transform.position += movement * Time.deltaTime * speed;
    }
}
}
