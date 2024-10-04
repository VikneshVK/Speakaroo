using UnityEngine;
using System.Collections;

namespace ResolutionMagic
{

    // this is a simple input script to show how to move the camera with the arrow keys
    
    public class InputManager : MonoBehaviour
    {

        float speed = 5f;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                ResolutionManager.Instance.MoveCameraInDirection(ResolutionManager.CameraDirection.Up, speed * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                ResolutionManager.Instance.MoveCameraInDirection(ResolutionManager.CameraDirection.Down,speed * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                ResolutionManager.Instance.MoveCameraInDirection(ResolutionManager.CameraDirection.Left,speed * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                ResolutionManager.Instance.MoveCameraInDirection(ResolutionManager.CameraDirection.Right,speed * Time.deltaTime);

            }
        }
    }
}