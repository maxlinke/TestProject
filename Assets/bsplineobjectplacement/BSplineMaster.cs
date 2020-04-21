using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BSplineMaster : MonoBehaviour {

    [SerializeField, Range(BSplineObjectPlacer.MIN_GIZMO_SIZE, BSplineObjectPlacer.MAX_GIZMO_SIZE)] float gizmoSize;

    public IEnumerable<BSplineObjectPlacer> GetChildren () {
        var children = GetComponentsInChildren<BSplineObjectPlacer>(true);
        // if(children.Length > 0){
            foreach(var child in children){
                yield return child;
            }
        // }else{
        //     yield return null;
        // }
    }

    public void ToggleGizmos () {
        bool gotValueToUse = false;
        bool valueToUse = false;
        foreach(var child in GetChildren()){
            if(!gotValueToUse){
                valueToUse = !child.drawGizmos;
                gotValueToUse = true;
            }
            child.drawGizmos = valueToUse;
        }
    }

    public void UpdateGizmoSizes () {
        foreach(var child in GetChildren()){
            child.gizmoSize = gizmoSize;
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
    }

}

#endif