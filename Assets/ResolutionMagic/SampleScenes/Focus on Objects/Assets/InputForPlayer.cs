using UnityEngine;
using System.Collections;

namespace ResolutionMagic
{    

    // this is a simple input script to show how to move the camera with the arrow keys
    
    public class InputForPlayer : MonoBehaviour
    {
        Transform player;

        void Awake()
        {
            player = transform;
        }

        float speed = 5f;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                player.Translate(Vector2.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                player.Translate(Vector2.down * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                player.Translate(Vector2.left * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                player.Translate(Vector2.right * speed * Time.deltaTime);
            }
        }
    }
}