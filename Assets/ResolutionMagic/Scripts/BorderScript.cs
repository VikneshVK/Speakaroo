using UnityEngine;
using System.Collections;

namespace ResolutionMagic
{
    public class BorderScript : MonoBehaviour
    {

        // this script calculates the correct size and position of border edges it's attached to
        // you should not need to modify the properties unless you want something unusual
        // to have walls on some edges but not others, simply disable (or remove) the edges you don't want to use


        // each edge of the border must be aligned to a screen edge, such as the top or left
        [SerializeField]
        ResolutionManager.Edge alignTo;

         BorderScript[] BorderItems; // a list of border items this script moves and sizes

        // alignType determines if the border is placed at the edge of the screen or at the edge of the canvas
        // you would align it to the canvas if your border is around a set play area, and align to the screen if you want a border at the edge of the player's screen regardless of resolution/ratio
        [SerializeField]
        ResolutionManager.AlignObject alignType;

        // each collider can be set to be as wide as the AlwaysDisplayedArea or the whole screen
        [SerializeField]
        ResolutionManager.AlignObject sizeTo;

        [Tooltip("Do you want the border to resize in response to the camera being zoomed/changed?")]
        [SerializeField] bool autoResize = true; // defaults to true to retain the behaviour in previous versions of the asset
        Transform myTransform; // reference to the transform

        void Start()
        {
            myTransform = transform; // cache the transform
            BorderItems = GetComponentsInChildren<BorderScript>();
            ResetBorders();
            if(autoResize)
            {
                ResolutionManager.Instance.CameraZoomChanged += CameraZoomUpdated;
            }
        }

        void ResetBorders()
        {
            foreach (BorderScript bScr in BorderItems)
            {
                bScr.UpdateEdge();
            }
        }

        void CameraZoomUpdated(float oldSize, float newSize)
        {
            ResetBorders();
        }


        public void UpdateEdge()
        {
            // place this at its correct location on the screen edge
            if (alignType == ResolutionManager.AlignObject.AlwaysDisplayedArea)
                myTransform.position = ResolutionManager.Instance.AlwaysVisibleEdgeAsVector(alignTo);
            else
                myTransform.position = ResolutionManager.Instance.ScreenEdgeAsVector(alignTo);

            // get the height or width of the object by setting its scale to 1 and then measuring
            float mySize = 0;
            myTransform.localScale = new Vector3(1, 1, 0);
            switch (alignTo)
            {
                case ResolutionManager.Edge.Bottom:
                case ResolutionManager.Edge.Top:
                    mySize = myTransform.GetComponent<Renderer>().bounds.size.x;
                    break;
                case ResolutionManager.Edge.Left:
                case ResolutionManager.Edge.Right:
                    mySize = myTransform.GetComponent<Renderer>().bounds.size.y;
                    break;
            }

            float scale = 1;

            // scale the transform to match the aligned edge, e.g. canvas left
            // note: since the screen and canvas are always rectangular the left/right and top/bottom always match
            switch (alignTo)
            {
                case ResolutionMagic.ResolutionManager.Edge.Bottom:
                case ResolutionMagic.ResolutionManager.Edge.Top:
                    if (sizeTo == ResolutionManager.AlignObject.AlwaysDisplayedArea)
                        scale = ResolutionManager.Instance.AlwaysVisbleWidth / mySize;
                    else
                        scale = ResolutionManager.Instance.ScreenWidth / mySize;

                    myTransform.localScale = new Vector3(scale, 1, 0);
                    break;

                case ResolutionMagic.ResolutionManager.Edge.Left:
                case ResolutionMagic.ResolutionManager.Edge.Right:
                    if (sizeTo == ResolutionManager.AlignObject.AlwaysDisplayedArea)
                        scale = ResolutionManager.Instance.AlwaysVisibleHeight / mySize;
                    else
                        scale = ResolutionManager.Instance.ScreenHeight / mySize;

                    myTransform.localScale = new Vector3(1, scale, 0);
                    break;
            }

        }
    }
}