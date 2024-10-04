using UnityEngine;
namespace ResolutionMagic
{

    // simple movement using the basic Unity input system
    // Attach to any object, adjust the speed if necessary, then move it around with keyboard/joystick, etc.
    // this is intended only for prototyping and demonstrations
    public class SimpleCameraFollow2D : MonoBehaviour
    {
        [SerializeField] Transform objectToFollow;

        void Awake()
        {
            if (objectToFollow == null)
            {
                Debug.LogError("There is nothing for the camera follow to follow! Drag the player or something into the Inspector field.");
            }
        }
        void LateUpdate()
        {
            ResolutionManager.Instance.MoveCameraPosition(objectToFollow.position, true);
        }
    }
}
