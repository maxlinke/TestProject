using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TargetPointBoids : Boids {

    [SerializeField] Transform boidTarget = null;
    [SerializeField, Range(MIN_BEHAVIOR_WEIGHT, MAX_BEHAVIOR_WEIGHT)] float boidTargetSeekWeight = 2f;

    protected override Vector3 GetAdditionalBehavior (int boidIndex) {
        var output = base.GetAdditionalBehavior(boidIndex);
        if(boidTarget != null){
            var activePos = boids[boidIndex].transform.position;
            var toTarget = (boidTarget.position - activePos).normalized;
            output += boidTargetSeekWeight * toTarget;
        }
        return output;
    }

    protected override void DrawAdditionalGizmos () {
        base.DrawAdditionalGizmos();
        if(boidTarget != null){
            Gizmos.DrawLine(transform.position, boidTarget.position);
            Gizmos.DrawWireSphere(boidTarget.position, 1f);
        }
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(TargetPointBoids))]
public class TargetPointBoidsEditor : BoidsEditor {

    protected override MonoScript GetCallingScript () => MonoScript.FromMonoBehaviour((TargetPointBoids)target);
    protected override System.Type GetCallingType () => typeof(TargetPointBoids);

    protected override void DrawAdditionalProperties () {
        base.DrawAdditionalProperties();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boidTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boidTargetSeekWeight"));
    }

}

#endif