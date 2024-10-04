using UnityEngine;

namespace ResolutionMagic
{

    public class BlackBars : MonoBehaviour
    {


        [SerializeField] private GameObject leftBar;
        [SerializeField]
        private GameObject rightBar;
        [SerializeField]
        private GameObject topBar;
        [SerializeField]
        private GameObject bottomBar;
        [SerializeField] bool DebugMode;

        Transform myTransform; // reference to the transform
        [SerializeField]
        bool _enabled = true;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (value)
                    EnableBlackBars();
                else
                {
                    DisableBlackBars();
                }
            }
        }

        void Start()
        {
            myTransform = transform;
            EnableBlackBars();
        }

        void EnableBlackBars()
        {
            ToggleBarSprites(true);
            ScaleBars();
            PlaceBars();
        }

        void DisableBlackBars()
        {
            ToggleBarSprites(false);
        }

        void ToggleBarSprites(bool isOn)
        {
            leftBar.GetComponent<SpriteRenderer>().enabled = isOn;
            rightBar.GetComponent<SpriteRenderer>().enabled = isOn;
            topBar.GetComponent<SpriteRenderer>().enabled = isOn;
            bottomBar.GetComponent<SpriteRenderer>().enabled = isOn;
        }

        void SetBarSize(Transform bar, bool isVertical)
        {
            float mySize = 0;
            bar.localScale = new Vector3(1, 1, 0);
            mySize = bar.GetComponent<Renderer>().bounds.size.x;

            float scalex = 30f;
            float scaley = 30f;
            if (isVertical)
            {
                scalex = (ResolutionManager.Instance.AlwaysVisbleWidth / mySize) * 1.5f;
            }
            else
            {
                scaley = (ResolutionManager.Instance.AlwaysVisibleHeight / mySize) * 1.5f;
            }

            bar.localScale = new Vector3(scalex, scaley, 0);
        }

        void SetBarPosition(Transform bar, ResolutionManager.AlignPoint alignment)
        {
            Vector2 adjustment = (Vector2)Camera.main.transform.position - ResolutionManager.Instance.AlwaysVisibleBoundary.Centre;
            switch (alignment)
            {
                case ResolutionManager.AlignPoint.Bottom:
                    adjustment.y -= (bar.GetComponent<Renderer>().bounds.size.y / 1.95f);
                    break;
                case ResolutionManager.AlignPoint.Top:
                    adjustment.y += (bar.GetComponent<Renderer>().bounds.size.y / 1.95f);
                    break;
                case ResolutionManager.AlignPoint.Left:
                    adjustment.x -= (bar.GetComponent<Renderer>().bounds.size.x / 1.95f);
                    break;
                case ResolutionManager.AlignPoint.Right:
                    adjustment.x += (bar.GetComponent<Renderer>().bounds.size.x / 1.95f);
                    break;
            }
            bar.position = ResolutionManager.Instance.AlwaysVisibleEdgeAsVector(alignment) + adjustment;
        }

        void ScaleBars()
        {
            SetBarSize(leftBar.transform, false);
            SetBarSize(rightBar.transform, false);
            SetBarSize(topBar.transform, true);
            SetBarSize(bottomBar.transform, true);
        }

        void PlaceBars()
        {
            SetBarPosition(leftBar.transform, ResolutionManager.AlignPoint.Left);
            SetBarPosition(rightBar.transform, ResolutionManager.AlignPoint.Right);
            SetBarPosition(topBar.transform, ResolutionManager.AlignPoint.Top);
            SetBarPosition(bottomBar.transform, ResolutionManager.AlignPoint.Bottom);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (DebugMode)
            {
                Gizmos.color = Color.red;

            }
        }
#endif
    }
}
