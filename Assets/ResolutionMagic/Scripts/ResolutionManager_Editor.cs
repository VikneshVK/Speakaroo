#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ResolutionMagic
{
    // this script creates the custom inspector for the Resolution Manager script
    [CustomEditor(typeof(ResolutionManager))]
    public class ResolutionManager_Editor : Editor
    {
        ResolutionManager myTarget;
        SerializedObject soTarget;
        SerializedProperty ZoomTo;
        SerializedProperty _zoomStyle;
        SerializedProperty ExtraCameras;
        SerializedProperty alwaysVisibleArea;
        SerializedProperty maxAreaBounds;
        SerializedProperty centreOnAlwaysVisibleArea;
        SerializedProperty objectsToKeepOnScreen;
        SerializedProperty _borderMargin;
        SerializedProperty _transformsToFocusOn;
        SerializedProperty autoRefresh;
        SerializedProperty _refreshEveryFrame;
        SerializedProperty _dontZoomIfVisible;
        SerializedProperty centreOn;
        SerializedProperty _transitionSpeed;

        // styling
        GUIStyle headingStyle;
        GUIStyle subheadingStyle;
        GUIStyle messageStyle;

        private void OnEnable()
        {
            myTarget = (ResolutionManager)target;
            soTarget = new SerializedObject(target);

            // fields
            ZoomTo = soTarget.FindProperty("ZoomTo");
            ExtraCameras = soTarget.FindProperty("ExtraCameras");
            centreOnAlwaysVisibleArea = soTarget.FindProperty("centreOnAlwaysVisibleArea");
            objectsToKeepOnScreen = soTarget.FindProperty("objectsToKeepOnScreen");
            centreOn = soTarget.FindProperty("centreOn");
            alwaysVisibleArea = soTarget.FindProperty("AlwaysVisibleBoundary");
            maxAreaBounds = soTarget.FindProperty("MaxAreaBoundary");
            _transformsToFocusOn = soTarget.FindProperty("transformsToFocusOn");
            autoRefresh = soTarget.FindProperty("AutoRefresh");
            _borderMargin = soTarget.FindProperty("borderMargin");
            _refreshEveryFrame = soTarget.FindProperty("refreshEveryFrame");
            _dontZoomIfVisible = soTarget.FindProperty("dontZoomIfVisible");
            _zoomStyle = soTarget.FindProperty("ZoomStyle");
            _transitionSpeed = soTarget.FindProperty("TransitionSpeed");

            // setup styles
            headingStyle = new GUIStyle();
			headingStyle.fontSize = 15;
			headingStyle.fontStyle = FontStyle.Bold;
			headingStyle.wordWrap = true;

			subheadingStyle = new GUIStyle();
			subheadingStyle.fontSize = 13;
			subheadingStyle.fontStyle = FontStyle.Italic;
			subheadingStyle.wordWrap = true;

			messageStyle = new GUIStyle();
			messageStyle.fontSize = 12;
			messageStyle.fontStyle = FontStyle.Italic;
			messageStyle.wordWrap = true;
        }
        public override void OnInspectorGUI()
        {
            soTarget.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("<color=green>Bounds Setup</color>"), headingStyle);
            EditorGUILayout.PropertyField(alwaysVisibleArea);
            EditorGUILayout.PropertyField(property: maxAreaBounds);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(new GUIContent("<color=green>Camera Sizing Method</color>"), headingStyle);
            EditorGUILayout.PropertyField(ZoomTo);
            EditorGUILayout.PropertyField(_zoomStyle);
            if(_zoomStyle.intValue == 1) // gradual zoom
            {
                EditorGUILayout.PropertyField(_transitionSpeed);
            }
            // background, 0 = displayareacanvas, 2 = tilemaps, 3 = transforms
            if(ZoomTo.intValue == 1)
            {
                EditorGUILayout.PropertyField(centreOnAlwaysVisibleArea);
            }      
  
            if(ZoomTo.intValue == 2 || ZoomTo.intValue == 3)
            {
                EditorGUILayout.PropertyField(_transformsToFocusOn);
                EditorGUILayout.PropertyField(_borderMargin);
            }
            if(ZoomTo.intValue == 3)
            {
                EditorGUILayout.PropertyField(_refreshEveryFrame);
                EditorGUILayout.PropertyField(_dontZoomIfVisible);
            }

            EditorGUILayout.LabelField(new GUIContent("<color=green>Additional Settings</color>"), headingStyle);
            EditorGUILayout.PropertyField(ExtraCameras);      
            EditorGUILayout.PropertyField(autoRefresh);     
            EditorGUILayout.Separator();
            if(GUILayout.Button("Force Refresh", new GUILayoutOption[] {  GUILayout.Height(40),  GUILayout.Width(200)}))
            {
                myTarget.RefreshResolution();
            }
            soTarget.ApplyModifiedProperties ();
        }
    }
}
#endif

