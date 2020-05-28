using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class QuaternionAngleAxisTester : MonoBehaviour {

    [SerializeField] public Vector3 lookAtTarget;
    [SerializeField] public float axisAngle;

    void Update () {
        var axis = lookAtTarget - transform.position;
        transform.rotation = Quaternion.AngleAxis(axisAngle, axis);
    }

    void OnDrawGizmosSelected () {
        var cc = Gizmos.color;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, lookAtTarget);
        Gizmos.color = cc;
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(QuaternionAngleAxisTester))]
public class QuaternionAngleAxisTesterEditor : Editor {

    QuaternionAngleAxisTester qaat;

    void OnEnable () {
        qaat = target as QuaternionAngleAxisTester;
    }

    void OnSceneGUI () {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        var newLookAtTarget = Handles.PositionHandle(qaat.lookAtTarget, Quaternion.identity);
        if(EditorGUI.EndChangeCheck()){
            serializedObject.FindProperty("lookAtTarget").vector3Value = newLookAtTarget;
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

}

#endif
