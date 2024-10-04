#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ResolutionMagic
{
    [CustomEditor(typeof(CameraBoundary))]

    public class CameraBoundsEditor : Editor
    {
        CameraBoundary targetObject;
        SerializedObject soTarget;

        private SerializedProperty _name;
        private SerializedProperty lineColour;
        private SerializedProperty lineSprite;
        GUIStyle headingStyle;

        void OnEnable()
        {
            targetObject = target as CameraBoundary;
            soTarget = new SerializedObject(target);
            lineColour = soTarget.FindProperty("lineColour");
            lineSprite = soTarget.FindProperty("LineSprite");
            _name = soTarget.FindProperty("Name");

             headingStyle = new GUIStyle();
			headingStyle.fontSize = 15;
			headingStyle.fontStyle = FontStyle.Bold;
			headingStyle.wordWrap = true;
        }

        public override void OnInspectorGUI()
        {
            soTarget.Update();
            EditorGUILayout.PropertyField(_name);
            EditorGUILayout.LabelField(new GUIContent("<color=#1E90FF>Enable Gizmos in the Scene view to adjust this.</color>"), headingStyle);
            EditorGUILayout.PropertyField(lineColour);
            EditorGUILayout.PropertyField(lineSprite);

            soTarget.ApplyModifiedProperties ();
        }

        void OnSceneGUI()
        {
            
            if (targetObject == null || targetObject.topLeftPos == null || targetObject.bottomRightPos == null) return;

            var storedTopLeft = targetObject.topLeftPos;
            var storedBottomRight = targetObject.bottomRightPos;

            targetObject.topLeftPos = Handles.DoPositionHandle(targetObject.topLeftPos, Quaternion.identity);
            targetObject.bottomRightPos = Handles.DoPositionHandle(targetObject.bottomRightPos, Quaternion.identity);           

            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = lineColour.colorValue;

                if (targetObject.LineSprite == null)
                {
                    Handles.DrawLine(new Vector2(targetObject.topLeftPos.x, targetObject.topLeftPos.y), new Vector2(targetObject.bottomRightPos.x, targetObject.topLeftPos.y));

                    Handles.DrawLine(new Vector2(targetObject.bottomRightPos.x, targetObject.topLeftPos.y), new Vector2(targetObject.bottomRightPos.x, targetObject.bottomRightPos.y));

                    Handles.DrawLine(new Vector2(targetObject.bottomRightPos.x, targetObject.bottomRightPos.y), new Vector2(targetObject.topLeftPos.x, targetObject.bottomRightPos.y));

                    Handles.DrawLine(new Vector2(targetObject.topLeftPos.x, targetObject.bottomRightPos.y), new Vector2(targetObject.topLeftPos.x, targetObject.topLeftPos.y));
                }
                else
                {
                    Handles.DrawAAPolyLine(targetObject.LineSprite, new Vector2(targetObject.topLeftPos.x, targetObject.topLeftPos.y), new Vector2(targetObject.bottomRightPos.x, targetObject.topLeftPos.y));

                    Handles.DrawAAPolyLine(targetObject.LineSprite, new Vector2(targetObject.bottomRightPos.x, targetObject.topLeftPos.y), new Vector2(targetObject.bottomRightPos.x, targetObject.bottomRightPos.y));

                    Handles.DrawAAPolyLine(targetObject.LineSprite, new Vector2(targetObject.bottomRightPos.x, targetObject.bottomRightPos.y), new Vector2(targetObject.topLeftPos.x, targetObject.bottomRightPos.y));

                    Handles.DrawAAPolyLine(targetObject.LineSprite, new Vector2(targetObject.topLeftPos.x, targetObject.bottomRightPos.y), new Vector2(targetObject.topLeftPos.x, targetObject.topLeftPos.y));
                }
            }            

            if (storedTopLeft != targetObject.topLeftPos || storedBottomRight != targetObject.bottomRightPos)
            {
                EditorUtility.SetDirty(targetObject);
            }            
        }
    }
}
#endif
