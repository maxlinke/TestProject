using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BSplineMaster : MonoBehaviour {

    [SerializeField, Range(QuadraticBezierSpline.MIN_GIZMO_SIZE, QuadraticBezierSpline.MAX_GIZMO_SIZE)] float gizmoSize;
    [SerializeField] BSplineObjectPool objectPool;

    IEnumerable<QuadraticBezierSpline> GetSplineChildren () {
        var children = GetComponentsInChildren<QuadraticBezierSpline>(true);
        foreach(var child in children){
            yield return child;
        }
    }

    IEnumerable<BSplineObjectPlacer> GetPlacerChildren () {
        var children = GetComponentsInChildren<BSplineObjectPlacer>(true);
        foreach(var child in children){
            yield return child;
        }
    }

    public void ToggleGizmos () {
        bool gotValueToUse = false;
        bool valueToUse = false;
        foreach(var child in GetSplineChildren()){
            if(!gotValueToUse){
                valueToUse = !child.drawGizmos;
                gotValueToUse = true;
            }
            child.drawGizmos = valueToUse;
        }
    }

    public void UpdateGizmoSizes () {
        foreach(var child in GetSplineChildren()){
            child.gizmoSize = gizmoSize;
        }
    }

    public void UpdateObjectPools () {
        foreach(var child in GetPlacerChildren()){
            child.UpdatePool(objectPool);
        }
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(BSplineMaster))]
public class BSplineMasterEditor : Editor {

    BSplineMaster bsm;

    void OnEnable () {
        bsm = target as BSplineMaster;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(GUILayout.Button("Toggle Gizmos")){
            bsm.ToggleGizmos();
        }
        if(GUILayout.Button("Update Gizmo Size")){
            bsm.UpdateGizmoSizes();
        }
        if(GUILayout.Button("Update Pools")){
            bsm.UpdateObjectPools();
        }
    }

}

#endif