using UnityEngine;

namespace ResolutionMagic
{
    public class CameraBoundary : MonoBehaviour
    {
        [SerializeField] string Name;
        [Tooltip("You can draw the border lines with a sprite to make them thicker and easier to see.")]
        public Texture2D LineSprite;

        [Tooltip("Select the colour for the box that is drawn to show this boundary")]
        public Color lineColour;

        // the two corner positions
        // together these can be used to form opposite corners of a box
        [SerializeField] public Vector2 topLeftPos = new Vector2(-10f, 10f);
        [SerializeField] public Vector2 bottomRightPos = new Vector2(10f, 0f);


        // returns the centre of the box created by the two corners
        public Vector2 Centre
        {
            get
            {
                return new Vector2((bottomRightPos.x - topLeftPos.x)/2f + topLeftPos.x, (topLeftPos.y - bottomRightPos.y)/2f + bottomRightPos.y);
            }
        }
    }
}