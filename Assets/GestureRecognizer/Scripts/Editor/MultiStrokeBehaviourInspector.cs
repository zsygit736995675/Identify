using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiStrokeBehaviour))]
public class MultiStrokeBehaviourInspector : Editor {

    public override void OnInspectorGUI() {
        MultiStrokeBehaviour msb = (MultiStrokeBehaviour)target;

        msb.isEnabled = EditorGUILayout.Toggle(new GUIContent("Is enabled", "Disable or enable gesture recognition"), msb.isEnabled);
        msb.forceCopy = EditorGUILayout.Toggle(new GUIContent("Force copy", "Overwrite the XML file in persistent data path"), msb.forceCopy);
        msb.libraryToLoad = EditorGUILayout.TextField(new GUIContent("Library to load", "The name of the gesture library to load. Do NOT include '.xml'"), msb.libraryToLoad);
        msb.distanceBetweenPoints = EditorGUILayout.FloatField(new GUIContent("Distance between points", "A new point will be placed if it is this further than the last point."), msb.distanceBetweenPoints);
        msb.minimumPointsToRecognize = EditorGUILayout.IntField(new GUIContent("Minimum points to recognize", "Minimum amount of points required to recognize a gesture."), msb.minimumPointsToRecognize);
        msb.lineMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Line material", "Material for the line renderer."), msb.lineMaterial, typeof(Material), false);
        msb.startThickness = EditorGUILayout.FloatField(new GUIContent("Start thickness", "Start thickness of the gesture."), msb.startThickness);
        msb.endThickness = EditorGUILayout.FloatField(new GUIContent("End thickness", "End thickness of the gesture."), msb.endThickness);
        msb.startColor = EditorGUILayout.ColorField(new GUIContent("Start color", "Start color of the gesture."), msb.startColor);
        msb.endColor = EditorGUILayout.ColorField(new GUIContent("End color", "End color of the gesture."), msb.endColor);
        msb.gestureLimitType = (GestureRecognizer.GestureLimitType)EditorGUILayout.EnumPopup(new GUIContent("Gesture limit type", "Limits gesture drawing to a specific area"), msb.gestureLimitType);

        if (msb.gestureLimitType == GestureRecognizer.GestureLimitType.RectBoundsClamp || msb.gestureLimitType == GestureRecognizer.GestureLimitType.RectBoundsIgnore) {
            msb.gestureLimitRectBounds = (RectTransform)EditorGUILayout.ObjectField(new GUIContent("Gesture limit rect bounds", "RectTransform to limit gesture"), msb.gestureLimitRectBounds, typeof(RectTransform), true);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(msb);
    }
}