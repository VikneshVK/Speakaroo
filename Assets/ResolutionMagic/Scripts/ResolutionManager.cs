using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace ResolutionMagic
{
    public class ResolutionManager : MonoBehaviour
    {
        #region SETTINGS      
        Vector2 screenSize; // stores the screen size so a change can be detected
        /// USER DEFINED SETTINGS
        [Tooltip("The type of camera automation you want. See the documentation for full details.")]
        public ZoomType ZoomTo;
        [Tooltip("The camera can be set instantly to the correct zoom/position, or it can be set to smoothly move to the new zoom/position.")]
        public ZoomMethod ZoomStyle;
        [Tooltip("How quickly the camera transitions to its new zoom and position when set to do so gradually.")]
        public float TransitionSpeed = 5f;
        [Tooltip("Optionally enable multiple cameras to mimic the main camera's zoom/size")]
        [SerializeField] Camera[] ExtraCameras;
        [Tooltip("Focus on the renderers or tilemaps in these transforms (and children)")]
        [SerializeField] Transform[] transformsToFocusOn;
        [Tooltip("Add an optional margin around the focused items (i.e. make the camera view slightly larger than necessary). The X value is the margin to the sides and the Y value is the margin top/bottom.")]
        [SerializeField] Vector2 borderMargin;

        [Tooltip("When selected, the camera will refresh automatically every frame. When not selected, the camera will only change if the screen resolution/ratio changes.")]
        [SerializeField] bool refreshEveryFrame = true;
        [Tooltip("When set to zoom to objects/transforms, selecting this option will prevent the camera from zooming if the objects are already visible on screen")]
        [SerializeField] bool dontZoomIfVisible = false;

        struct CameraState
        {
            public float OrthagraphicSize;
            public Vector3 Position;

            public ZoomType ZoomToType;

            public Transform[] FocusTransforms;

            public bool RefreshEveryFrame;
            public Vector2 Margin;

        }

        CameraState cachedCamState;

        float cameraSpeed = 0.25f;
        float previousCameraSize;
        float _calculatedCameraSize;

        public float CalculatedCameraSize
        {
            get
            {
                return _calculatedCameraSize;
            }
            private set
            {
                _calculatedCameraSize = value;
            }
        }

        #endregion

        [Tooltip("The rectangle that represents the area that should always be kept visible.")]
        public CameraBoundary AlwaysVisibleBoundary;
        [Tooltip("The rectangle that represents all your game content that can be displayed when the screen shape doesn't match the main game area shape.")]
        public CameraBoundary MaxAreaBoundary;

        float resHeight = 0f; // and resWidth - used to store the most recent resolution so we can check if it has changed
        float resWidth = 0f;
        static ResolutionManager myInstance;

        public static ResolutionManager Instance
        {
            get { return myInstance; }
        }
        float alwaysVisibleWidth;
        float alwaysVisibleHeight;

        [Tooltip("Override the camera to centre on the Always Displayed area instead of the Max Bounds. Useful to keep the Always Visible area centred when the Max Bounds is a different shape.")]
        [SerializeField] bool centreOnAlwaysVisibleArea = true;
        public bool AutoRefresh = true;

        public event CameraZoomChangedEvent CameraZoomChanged;
        public delegate void CameraZoomChangedEvent(float previousSize, float newSize);

        void Awake()
        {
            myInstance = this;
            previousCameraSize = Camera.main.orthographicSize;
        }

        void Start()
        {
            RefreshResolution();
        }
        void Update()
        {
            if (AutoRefresh)
            {
                var currentScreenSize = new Vector2(Screen.width, Screen.height);

                if (currentScreenSize != screenSize || (ZoomTo == ZoomType.FocusOnTransforms && (refreshEveryFrame)))
                {
                    RefreshResolution();
                }

            }
        }

        #region PUBLIC METHODS
        // these methods are accessible from anywhere in your game as long as there is an instance of this script available
        // you access them like the following example:
        public void MoveCameraInDirection(CameraDirection direction, float moveDistance, bool moveSafely = true)
        {
            // move the camera in the specified direction and the specified distance
            // if RestrictScreenToBackground or moveSafely is true the camera will only move if the movement won't cause content outside the background region to show on the screen

            Vector3 newCameraPos = Camera.main.transform.position;

            switch (direction) // to move it two directions (e.g. up and left), call this function  for each direction separately
            {
                case CameraDirection.Up:
                    newCameraPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y + moveDistance, -10f);
                    break;
                case CameraDirection.Down:
                    newCameraPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - moveDistance, -10f);
                    break;
                case CameraDirection.Left:
                    newCameraPos = new Vector3(Camera.main.transform.position.x - moveDistance, Camera.main.transform.position.y, -10f);
                    break;
                case CameraDirection.Right:
                    newCameraPos = new Vector3(Camera.main.transform.position.x + moveDistance, Camera.main.transform.position.y, -10f);
                    break;
            }

            if (moveSafely)
                MoveCameraSafely(newCameraPos, direction); // only move the camera if the movement will not reveal space outside the background area
            else
                MoveCameraUnsafely(newCameraPos); // move the camera regardless
        }

        public void MoveCameraPosition(Vector2 newPosition, bool moveSafely = true)
        {
            Vector2 currentPos = (Vector2)Camera.main.transform.position;

            if (moveSafely)
            {
                Vector3 newCameraYPos = new Vector3(Camera.main.transform.position.x, newPosition.y, Camera.main.transform.position.z);
                var dirY = newCameraYPos.y >= currentPos.y ? CameraDirection.Up : CameraDirection.Down;
                MoveCameraSafely(newCameraYPos, dirY);

                Vector3 newCameraXPos = new Vector3(newPosition.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
                var dirX = newCameraXPos.x >= currentPos.x ? CameraDirection.Right : CameraDirection.Left;
                MoveCameraSafely(newCameraXPos, dirX); // only move the camera if the movement will not reveal space outside the background area
            }
            else
            {
                var newCameraPos = new Vector3(newPosition.x, newPosition.y, Camera.main.transform.position.z);

                MoveCameraUnsafely(newCameraPos); // move the camera regardless
            }
        }

        public void RefreshResolution()
        {
            previousCameraSize = Camera.main.orthographicSize;
            // choose the correct zoom method based on the ZoomTo property
            switch (ZoomTo)
            {
                case ZoomType.AlwaysDisplayedArea:
                    ZoomToAlwaysVisibleArea();
                    break;
                case ZoomType.MaximumBounds:
                    ZoomCameraToMaxGameArea();
                    break;
                case ZoomType.FocusOnTilemaps:
                    if (transformsToFocusOn != null && transformsToFocusOn.Length > 0)
                    {
                        ZoomCameraToTileMap(transformsToFocusOn, borderMargin);
                    }
                    else
                    {
                        Debug.LogWarning("No transforms set in the Resolution Manager to focus on. Falling back to default zoom to Always Visible area.");
                        ZoomToAlwaysVisibleArea();
                    }
                    break;
                case ZoomType.FocusOnTransforms:
                    if (transformsToFocusOn != null && transformsToFocusOn.Length > 0)
                    {
                        ZoomCameraToTransform(transformsToFocusOn, borderMargin);
                    }
                    else
                    {
                        Debug.LogWarning("No transforms set in the Resolution Manager to focus on. Falling back to default zoom to Always Visible area.");
                        ZoomToAlwaysVisibleArea();
                    }
                    break;
            }
        }

        void CompleteResolutionRefresh()
        {
            // store the current screen resolution for use elsewhere
            resHeight = Screen.height;
            resWidth = Screen.width;

            // store the always visible area dimensions for later use
            alwaysVisibleHeight = AlwaysVisibleHeight;
            alwaysVisibleWidth = AlwaysVisbleWidth;
            _scaleFactor = 0;
            // remember the size in case it needs to be retrieved later
            CalculatedCameraSize = Camera.main.orthographicSize;

            // raise the camerazoomchanged event if it has been subscribed to anywhere
            if (CameraZoomChanged != null) CameraZoomChanged(previousCameraSize, CalculatedCameraSize);
        }

        void SetCamera(ObjectsBounds bounds, Camera cam, Vector2 centre, bool staySafe = true)
        {
            // get the desired width and height
            float desiredWidth = bounds.BottomRight.x - bounds.TopLeft.x;
            float desiredHeight = bounds.TopLeft.y - bounds.BottomRight.y;
            // get the camera size required to match the height and width
            float idealSizeOrthHeight = desiredHeight / 2f;
            float idealSizeOrthWidth = (desiredWidth / cam.aspect) / 2f;
            // choose the size that is biggest to ensure all corners are visible
            var targetSize = idealSizeOrthHeight > idealSizeOrthWidth ? idealSizeOrthHeight : idealSizeOrthWidth;
            var centrePos = new Vector3(centre.x, centre.y, -10);
            if (ZoomStyle == ZoomMethod.Instant)
            {
                MoveCameraPosition(centrePos, staySafe);
                cam.orthographicSize = targetSize;
                screenSize = new Vector2(Screen.width, Screen.height);
                foreach (Camera c in ExtraCameras)
                {
                    c.orthographicSize = cam.orthographicSize;
                }
                CompleteResolutionRefresh();
            }
            else
            {
                StopCoroutine("GradualCameraZoom");
                StartCoroutine(SmoothCameraChange(cam, centrePos, targetSize));
            }
        }

        public ObjectsBounds GetTransformBounds(Transform[] transforms)
        {
            float left = Mathf.Infinity;
            float right = -Mathf.Infinity;
            float top = -Mathf.Infinity;
            float bottom = Mathf.Infinity;
            List<Transform> allTransforms = new List<Transform>();
            foreach (var t in transforms)
            {
                if (!allTransforms.Contains(t)) allTransforms.Add(t);
                var objs = t.GetComponentsInChildren<Transform>();
                foreach (var tr in objs)
                {
                    if (!allTransforms.Contains(tr)) allTransforms.Add(tr);
                }
            }
            bool foundRenderer = false;
            // find the extremes in each direction
            foreach (var obj in allTransforms)
            {
                var rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (obj.position.x - (rend.bounds.size.x / 2) < left) left = obj.position.x - (rend.bounds.size.x / 2);
                    if (obj.position.x + (rend.bounds.size.x / 2) > right) right = obj.position.x + (rend.bounds.size.x / 2);
                    if (obj.position.y - (rend.bounds.size.y / 2) < bottom) bottom = obj.position.y - (rend.bounds.size.y / 2);
                    if (obj.position.y + (rend.bounds.size.y / 2) > top) top = obj.position.y + (rend.bounds.size.y / 2);
                    foundRenderer = true;
                }
            }

            if (!foundRenderer)
            {
                Debug.LogError("No renderers found to base camera zoom on.");
                var ob = new ObjectsBounds()
                {
                    Invalid = true
                };
                return ob;
            }

            // get the furthest ponts on each axis     
            var topLeft = new Vector2(left, top);
            var bottomRight = new Vector2(right, bottom);

            var b = new ObjectsBounds()
            {
                TopLeft = topLeft,
                BottomRight = bottomRight
            };
            return b;
        }

        public ObjectsBounds GetTilemapBounds(Transform[] parentTransforms)
        {
            var left = Mathf.Infinity;
            var top = -Mathf.Infinity;
            var right = -Mathf.Infinity;
            var bottom = Mathf.Infinity;

            List<Tilemap> tilemaps = new List<Tilemap>();
            foreach (var t in parentTransforms)
            {
                var tms = t.GetComponentsInChildren<Tilemap>();
                foreach (var tm in tms)
                {
                    if (!tilemaps.Contains(tm)) tilemaps.Add(tm);
                }
            }
            bool foundCells = false;
            // find the extremes in each direction
            if (tilemaps != null && tilemaps.Count > 0)
            {
                foreach (var obj in tilemaps)
                {
                    obj.CompressBounds();
                    if (obj.cellBounds.xMin < left) left = obj.cellBounds.xMin;
                    if (obj.cellBounds.xMax > right) right = obj.cellBounds.xMax;
                    if (obj.cellBounds.yMin < bottom) bottom = obj.cellBounds.yMin;
                    if (obj.cellBounds.yMax > top) top = obj.cellBounds.yMax;
                    foundCells = true;
                }
            }

            if (!foundCells)
            {
                Debug.LogError("No tilemap content found to base camera zoom on.");
                var ob = new ObjectsBounds()
                {
                    Invalid = true
                };
                return ob;
            }
            else
            {

                // zoom camera to show level as large as possible        
                var topLeft = new Vector2(left, top);
                var bottomRight = new Vector2(right, bottom);

                return new ObjectsBounds() { TopLeft = topLeft, BottomRight = bottomRight };
            }
        }

        private float MaxHeight
        {
            get
            {
                return Mathf.Abs(MaxAreaBoundary.topLeftPos.y - MaxAreaBoundary.bottomRightPos.y);
            }
        }

        private float MaxWidth
        {
            get
            {
                return Mathf.Abs(MaxAreaBoundary.bottomRightPos.x - MaxAreaBoundary.topLeftPos.y);
            }
        }

        public void TurnOnBlackBars()
        {
            SetBlackBarsState(true);
        }

        public void TurnOffBlackBars()
        {
            SetBlackBarsState(false);
        }

        public void FocusOnObject(Transform focusTransform, float duration = 0f)
        {
            var bounds = GetTransformBounds(new Transform[] { focusTransform });
            if (!bounds.Invalid)
            {
                CacheCameraDetails(Camera.main);
                ZoomTo = ZoomType.FocusOnTransforms;
                refreshEveryFrame = true;
                transformsToFocusOn = new Transform[] { focusTransform };
                borderMargin = Vector2.zero;
                RefreshResolution();
                StartCoroutine(WaitAndRestoreCachedCamera(duration));
            }
        }

        IEnumerator WaitAndRestoreCachedCamera(float duration)
        {
            //pauseUpdating = true;
            yield return new WaitForSeconds(duration);
            RestoreCachedCameraDetails(Camera.main);
            RefreshResolution();
        }

        IEnumerator SmoothCameraChange(Camera cam, Vector3 targetCentre, float targetSize, bool staySafe = true)
        {
            bool centred = false;
            bool zoomed = false;
            while (!centred && !zoomed)
            {
                if (!centred)
                {
                    cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetCentre, Time.deltaTime * TransitionSpeed);
                    if (cam.transform.position == targetCentre) centred = true;
                }
                if (!zoomed)
                {
                    if (cam.orthographicSize < targetSize)
                    {
                        cam.orthographicSize += Time.deltaTime * TransitionSpeed;
                        foreach (Camera c in ExtraCameras)
                        {
                            c.orthographicSize = cam.orthographicSize;
                        }
                        if (cam.orthographicSize > targetSize)
                        {
                            cam.orthographicSize = targetSize;
                            foreach (Camera c in ExtraCameras)
                            {
                                c.orthographicSize = targetSize;
                            }
                            zoomed = true;
                        }
                    }
                    if (cam.orthographicSize > targetSize)
                    {
                        cam.orthographicSize -= Time.deltaTime * TransitionSpeed;
                        foreach (Camera c in ExtraCameras)
                        {
                            c.orthographicSize = cam.orthographicSize;
                        }
                        if (cam.orthographicSize < targetSize)
                        {
                            cam.orthographicSize = targetSize;
                            foreach (Camera c in ExtraCameras)
                            {
                                c.orthographicSize = cam.orthographicSize;
                            }
                            zoomed = true;
                        }
                    }
                }
                yield return null;
            }
        }

        #endregion

        #region PRIVATE METHODS

        void CacheCameraDetails(Camera cam)
        {
            cachedCamState = new CameraState()
            {
                OrthagraphicSize = cam.orthographicSize,
                Position = cam.transform.position,
                ZoomToType = ZoomTo,
                FocusTransforms = transformsToFocusOn,
                RefreshEveryFrame = refreshEveryFrame,
                Margin = borderMargin
            };
        }

        void RestoreCachedCameraDetails(Camera cam)
        {
            cam.orthographicSize = cachedCamState.OrthagraphicSize;
            cam.transform.position = cachedCamState.Position;
            ZoomTo = cachedCamState.ZoomToType;
            transformsToFocusOn = cachedCamState.FocusTransforms;
            refreshEveryFrame = cachedCamState.RefreshEveryFrame;
            borderMargin = cachedCamState.Margin;
        }
        void MoveCameraUnsafely(Vector3 newCameraPosition)
        {
            // move the camera to the specified position
            // this method will always move the camera regardless of what content will be shown to the player
            // it may reveal 'black bars' or objects outside the game area
            Camera.main.transform.position = newCameraPosition;
        }

        void MoveCameraSafely(Vector3 newCameraPosition, CameraDirection direction)
        {
            // an example of moving the camera only if it doesn't make space outside the background visible
            bool safe = false;

            switch (direction)
            {
                case CameraDirection.Up:
                    safe = !PointIsVisibleToCamera(new Vector2(Camera.main.transform.position.x, MaxTopEdge - cameraSpeed));
                    break;
                case CameraDirection.Down:
                    safe = !PointIsVisibleToCamera(new Vector2(Camera.main.transform.position.x, MaxBottomEdge + cameraSpeed));
                    break;
                case CameraDirection.Left:
                    safe = !PointIsVisibleToCamera(new Vector2(MaxLeftEdge + cameraSpeed, Camera.main.transform.position.y));
                    break;
                case CameraDirection.Right:
                    safe = !PointIsVisibleToCamera(new Vector2(MaxRightEdge - cameraSpeed, Camera.main.transform.position.y));
                    break;
            }

            if (safe) Camera.main.transform.position = newCameraPosition;
        }

        void ZoomToAlwaysVisibleArea()
        {
            // get the position of the object we're zooming in on in viewport co-ordinates
            var bounds = new ObjectsBounds()
            {
                TopLeft = AlwaysVisibleBoundary.topLeftPos,
                BottomRight = AlwaysVisibleBoundary.bottomRightPos
            };
            SetCamera(bounds, Camera.main, AlwaysVisibleBoundary.Centre);
        }

        void ZoomCameraToMaxGameArea()
        {
            // centre on the always visible area first, as this is where all zooming originates
            var centre = centreOnAlwaysVisibleArea ? AlwaysVisibleBoundary.Centre : MaxAreaBoundary.Centre;
            var bounds = new ObjectsBounds()
            {
                TopLeft = MaxAreaBoundary.topLeftPos,
                BottomRight = MaxAreaBoundary.bottomRightPos
            };
            SetCamera(bounds, Camera.main, centre);
        }

        void ZoomCameraToTileMap(Transform[] tilemapsParents, Vector2 margin)
        {
            var bounds = GetTilemapBounds(tilemapsParents);
            if (bounds.Invalid)
            {
                ZoomTo = ZoomType.AlwaysDisplayedArea;
                ZoomToAlwaysVisibleArea();
                return;
            }
            if (margin != Vector2.zero) bounds.AddMargin(margin);
            var centre = bounds.Centre;
            SetCamera(bounds, Camera.main, centre);
        }

        void ZoomCameraToTransform(Transform[] transforms, Vector2 margin)
        {
            var bounds = GetTransformBounds(transforms);
            if (bounds.Invalid)
            {
                ZoomTo = ZoomType.AlwaysDisplayedArea;
                ZoomToAlwaysVisibleArea();
                return;
            }
            if (margin != Vector2.zero) bounds.AddMargin(margin);
            if (dontZoomIfVisible)
            {
                if (PointIsVisibleToCamera(bounds.TopLeft) && PointIsVisibleToCamera(bounds.BottomRight)) return;
            }
            var centre = bounds.Centre;
            SetCamera(bounds, Camera.main, centre);
        }

        static bool PointIsVisibleToCamera(Vector2 point)
        {
            if (Camera.main.WorldToViewportPoint(point).x < 0 || Camera.main.WorldToViewportPoint(point).x > 1 || Camera.main.WorldToViewportPoint(point).y > 1 || Camera.main.WorldToViewportPoint(point).y < 0)
                return false;

            return true;
        }

        bool PointIsWithinBackground(Vector2 point)
        {
            if (point.x > MaxTopEdge || point.x < MaxBottomEdge || point.y > MaxRightEdge || point.y < MaxLeftEdge)
                return false;

            return true;
        }

        public void ToggleBlackBars()
        {
            var barsPrefab = GameObject.FindGameObjectWithTag("RM_Black_Bars");
            if (barsPrefab != null)
            {
                barsPrefab.GetComponent<BlackBars>().Enabled = !barsPrefab.GetComponent<BlackBars>().Enabled;
            }
            else
            {
                Debug.Log("Resolution Magic warning: trying to toggle black bars, but black bars prefab not found.");
            }
        }
        public void SetBlackBarsState(bool isActive)
        {
            var barsPrefab = GameObject.FindGameObjectWithTag("RM_Black_Bars");
            if (barsPrefab != null)
            {
                if (isActive)
                    barsPrefab.GetComponent<BlackBars>().Enabled = true;
                else
                    barsPrefab.GetComponent<BlackBars>().Enabled = false;
            }
            else
            {
                Debug.Log("Resolution Magic warning: trying to toggle black bars, but black bars prefab not found.");
            }
        }

        #endregion

        #region PROPERTIES


        // the EDGE properties return the furthest point on the relevant edge, e.g. the CameraLeftEdge is the leftmost position the camera can see
        // and AlwaysVisibleLeftEdge is the topmost point on the always visible area (which will not necessarily be the same as the top of the screen)
        // these values are in regular vector space with (0,0) representing the centre of the screen
        public float ScreenLeftEdge
        {
            get
            {
                Vector2 topLeft = new Vector2(0, 1);
                Vector2 topLeftInScreen = Camera.main.ViewportToWorldPoint(topLeft);
                return topLeftInScreen.x;
            }
        }

        public float ScreenRightEdge
        {
            get
            {
                Vector2 edge = new Vector2(1, 0);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(edge);
                return edgeVector.x;
            }
        }

        public float ScreenTopEdge
        {
            get
            {
                Vector2 edge = new Vector2(1, 1);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(edge);
                return edgeVector.y;
            }
        }

        public float ScreenBottomEdge
        {
            get
            {
                Vector2 edge = new Vector2(1, 0);
                Vector2 edgeVector = Camera.main.ViewportToWorldPoint(edge);
                return edgeVector.y;
            }
        }

        public Vector2 ScreenTopLeft
        {
            get
            {
                return new Vector2(ScreenLeftEdge, ScreenTopEdge);
            }
        }

        public Vector2 ScreenTopRight
        {
            get
            {
                return new Vector2(ScreenRightEdge, ScreenTopEdge);
            }
        }

        public Vector2 ScreenBottomLeft
        {
            get
            {
                return new Vector2(ScreenLeftEdge, ScreenBottomEdge);
            }
        }

        public Vector2 ScreenBottomRight
        {
            get
            {
                return new Vector2(ScreenRightEdge, ScreenBottomEdge);
            }
        }

        public float AlwaysVisibleLeftEdge
        {
            get
            {
                return AlwaysVisibleBoundary.topLeftPos.x;
            }
        }

        public float AlwaysVisibleRightEdge
        {
            get
            {
                return AlwaysVisibleBoundary.bottomRightPos.x;
            }
        }

        public float AlwaysVisibleTopEdge
        {
            get
            {
                return AlwaysVisibleBoundary.topLeftPos.y;
            }
        }

        public float AlwaysVisibleBottomEdge
        {
            get
            {
                return AlwaysVisibleBoundary.bottomRightPos.y;
            }
        }


        public float AlwaysVisbleWidth
        {
            get
            {
                return Mathf.Abs(AlwaysVisibleBoundary.bottomRightPos.x - AlwaysVisibleBoundary.topLeftPos.x);
            }
        }

        public float AlwaysVisibleHeight
        {
            get
            {
                return Mathf.Abs(AlwaysVisibleBoundary.topLeftPos.y - AlwaysVisibleBoundary.bottomRightPos.y);
            }
        }

        public float ScreenWidth
        {
            get
            {
                Vector2 topRightCorner = new Vector2(ScreenRightEdge, 0);
                float width = topRightCorner.x * 2;
                return width;
            }
        }

        public float ScreenHeight
        {
            get
            {

                Vector2 topRightCorner = new Vector2(0, ScreenTopEdge);
                float height = topRightCorner.y * 2;
                return height;
            }
        }

        public float MaxLeftEdge
        {
            get
            {
                return MaxAreaBoundary.topLeftPos.x;
            }
        }

        public float MaxRightEdge
        {
            get
            {
                return MaxAreaBoundary.bottomRightPos.x;
            }
        }

        public float MaxTopEdge
        {
            get
            {
                return MaxAreaBoundary.topLeftPos.y;
            }
        }

        public float MaxBottomEdge
        {
            get
            {
                return MaxAreaBoundary.bottomRightPos.y;
            }
        }

        private float _scaleFactor = 0;
        public float ScaleFactor
        {
            get
            {
                if (_scaleFactor == 0)
                {
                    float ratio;
                    float alwaysVisibleArea;
                    float screenArea;
                    alwaysVisibleArea = alwaysVisibleHeight * alwaysVisibleWidth;
                    float screenX = ScreenRightEdge * 2; // double the distance from the centre to the screen edge
                    float screenY = ScreenTopEdge * 2; // double the distance from the centre to the screen edge
                    screenArea = screenX * screenY;
                    ratio = screenArea / alwaysVisibleArea;
                    _scaleFactor = Mathf.Sqrt(ratio);
                    return _scaleFactor;
                }
                return _scaleFactor;
            }
        }

        public Vector2 ScreenEdgeAsVector(AlignPoint AlignedEdge)
        {
            switch (AlignedEdge)
            {

                case ResolutionManager.AlignPoint.Centre:
                    return Vector2.zero;

                case ResolutionManager.AlignPoint.Left:
                    return new Vector2(ScreenLeftEdge, ScreenCentre.y);


                case ResolutionManager.AlignPoint.Bottom:
                    return new Vector2(ScreenCentre.x, ScreenBottomEdge);


                case ResolutionManager.AlignPoint.Right:
                    return new Vector2(ScreenRightEdge, ScreenCentre.y);

                case ResolutionManager.AlignPoint.Top:
                    return new Vector2(ScreenCentre.x, ScreenTopEdge);

                case ResolutionManager.AlignPoint.TopLeftCorner:
                    return new Vector2(ScreenLeftEdge, ScreenTopEdge);


                case ResolutionManager.AlignPoint.TopRightCorner:
                    return new Vector2(ScreenRightEdge, ScreenTopEdge);


                case ResolutionManager.AlignPoint.BottomLeftCorner:
                    return new Vector2(ScreenLeftEdge, ScreenBottomEdge);


                case ResolutionManager.AlignPoint.BottomRightCorner:
                    return new Vector2(ScreenRightEdge, ScreenBottomEdge);
            }
            return new Vector2(0, 0);
        }

        public Vector2 ScreenEdgeAsVector(Edge AlignedEdge)
        {
            switch (AlignedEdge)
            {
                case ResolutionManager.Edge.Left:
                    return new Vector2(ScreenLeftEdge, 0);


                case ResolutionManager.Edge.Bottom:
                    return new Vector2(0, ScreenBottomEdge);


                case ResolutionManager.Edge.Right:
                    return new Vector2(ScreenRightEdge, 0);

                case ResolutionManager.Edge.Top:
                    return new Vector2(0, ScreenTopEdge);


            }
            return new Vector2(0, 0);
        }

        public Vector2 ScreenCentre
        {
            // returns the x-coordinate that is the centre of the screen on the x axis regardless of where the camera is
            get
            {
                Vector2 zeroZero = new Vector2(0.5f, 0.5f);
                Vector2 zeroZeroToWorld = Camera.main.ViewportToWorldPoint(zeroZero);
                return zeroZeroToWorld;
            }
        }
        public Vector2 AlwaysVisibleEdgeAsVector(AlignPoint AlignedEdge)
        {
            switch (AlignedEdge)
            {
                case ResolutionManager.AlignPoint.Left:
                    return new Vector2(AlwaysVisibleLeftEdge, 0);

                case ResolutionManager.AlignPoint.Bottom:
                    return new Vector2(0, AlwaysVisibleBottomEdge);

                case ResolutionManager.AlignPoint.Right:
                    return new Vector2(AlwaysVisibleRightEdge, 0);

                case ResolutionManager.AlignPoint.Top:
                    return new Vector2(0, AlwaysVisibleTopEdge);

                case ResolutionManager.AlignPoint.TopLeftCorner:
                    return new Vector2(AlwaysVisibleLeftEdge, AlwaysVisibleTopEdge);


                case ResolutionManager.AlignPoint.TopRightCorner:
                    return new Vector2(AlwaysVisibleRightEdge, AlwaysVisibleTopEdge);


                case ResolutionManager.AlignPoint.BottomLeftCorner:
                    return new Vector2(AlwaysVisibleLeftEdge, AlwaysVisibleBottomEdge);


                case ResolutionManager.AlignPoint.BottomRightCorner:
                    return new Vector2(AlwaysVisibleRightEdge, AlwaysVisibleBottomEdge);
            }
            return new Vector2(0, 0);
        }

        public Vector2 AlwaysVisibleEdgeAsVector(Edge AlignedEdge)
        {
            // overload to only allow edges (i.e. not corners) for placing the border around the screen/always visible area
            switch (AlignedEdge)
            {
                case ResolutionManager.Edge.Left:
                    return new Vector2(AlwaysVisibleLeftEdge, 0);


                case ResolutionManager.Edge.Bottom:
                    return new Vector2(0, AlwaysVisibleBottomEdge);


                case ResolutionManager.Edge.Right:
                    return new Vector2(AlwaysVisibleRightEdge, 0);

                case ResolutionManager.Edge.Top:
                    return new Vector2(0, AlwaysVisibleTopEdge);
            }
            return new Vector2(0, 0);
        }
        public string ScreenAspectRatio()
        {
            // Resolution Magic doesn't use this, but you can use it to get the current screen ratio
            // you can add/remove resolutions to suit your project

            // needs to be fleshed out
            float ratio;
            string orientation;
            if (Screen.width > Screen.height)
            {
                ratio = (float)Screen.width / Screen.height;
                orientation = "landscape";
            }
            else
            {
                ratio = (float)Screen.height / Screen.width;
                orientation = "portrait";
            }

            // NOTE: because screen sizes can vary slightly
            // we need to use fuzzy logic to get the closest match rather than checking for the exact ratio
            // e.g. a screen might report a ratio of 1.59999 instead of 1.6

            if (ratio < 1.38f)
                return "4x3" + orientation;

            if (ratio < 1.59f)
                return "3x2" + orientation;

            if (ratio < 1.63f)
                return "16x10" + orientation;

            if (ratio < 1.7f)
                return "5x3" + orientation;

            if (ratio < 1.82f)
                return "16x9" + orientation;

            return "not_detected"; //fallback for very narrow or weird screens

        }
        #endregion

        #region ENUMS

        [System.Serializable]
        public enum ZoomType
        {
            AlwaysDisplayedArea,
            MaximumBounds,
            FocusOnTilemaps,
            FocusOnTransforms
        }

        public enum ZoomMethod
        {
            Instant,
            Smooth
        }

        public enum AlignPoint
        {
            Centre,
            Top,
            Bottom,
            Left,
            Right,
            TopLeftCorner,
            TopRightCorner,
            BottomLeftCorner,
            BottomRightCorner
        }

        public enum CameraDirection
        {
            Up,
            Down,
            Left,
            Right
        }
        public enum Edge
        {
            Left,
            Top,
            Right,
            Bottom
        }

        public enum AlignObject
        {
            Screen,
            AlwaysDisplayedArea
        }

        #endregion
    }
}