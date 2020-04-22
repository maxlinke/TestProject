using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BSplineMaster : MonoBehaviour {

    [Header("Gizmo Settings")]
    [SerializeField, Range(QuadraticBezierSpline.MIN_GIZMO_SIZE, QuadraticBezierSpline.MAX_GIZMO_SIZE)] float gizmoSize;

    [Header("Randomness")]
    [SerializeField] Vector3 placementRandomness;
    [SerializeField] float rotationRandomness;
    
    [Header("Object Settings")]
    [SerializeField] BSplineObjectPool objectPool;
    [SerializeField] float spaceBetweenObjects;

    [Header("Placement Settings")]
    [SerializeField] BSplineObjectPlacer.DistanceMode distanceMode;
    [SerializeField] BSplineObjectPlacer.GroundMode groundMode;
    [SerializeField] Collider groundCollider;
    [SerializeField] bool noOvershoot;

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
                valueToUse = !child.alwaysDrawGizmos;
                gotValueToUse = true;
            }
            child.alwaysDrawGizmos = valueToUse;
        }
    }

    public void UpdateGizmoSettings () {
        foreach(var child in GetSplineChildren()){
            child.gizmoSize = gizmoSize;
        }
    }

    public void UpdateRandomizationSettings () {
        foreach(var child in GetPlacerChildren()){
            child.UpdateRandomizationSettings(placementRandomness, rotationRandomness);
        }
    }    

    public void UpdateObjectSettings () {
        foreach(var child in GetPlacerChildren()){
            child.UpdateObjectSettings(objectPool, spaceBetweenObjects);
        }
    }

    public void UpdatePlacementSettings () {
        foreach(var child in GetPlacerChildren()){
            child.UpdatePlacementSettings(distanceMode, groundMode, groundCollider, noOvershoot);
        }
    }

    public void RandomizeSeeds () {
        foreach(var child in GetPlacerChildren()){
            child.RandomizeSeed();
        }
    }

    public void ReplaceAll () {
        foreach(var child in GetPlacerChildren()){
            child.PlaceObjects();
        }
    }

    public void ClearAll () {
        foreach(var child in GetPlacerChildren()){
            child.DeletePlacedObjects();
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
        GUILayout.Space(10);
        if(GUILayout.Button("Toggle Gizmos")){
            bsm.ToggleGizmos();
        }
        if(GUILayout.Button("Update Gizmo Settings")){
            bsm.UpdateGizmoSettings();
        }
        if(GUILayout.Button("Update Randomization Settings")){
            bsm.UpdateRandomizationSettings();
        }
        if(GUILayout.Button("Update Object Settings")){
            bsm.UpdateObjectSettings();
        }
        if(GUILayout.Button("Update Placement Settings")){
            bsm.UpdatePlacementSettings();
        }
        GUILayout.Space(10);
        if(GUILayout.Button("(Re)Place All")){
            bsm.ReplaceAll();
        }
        if(GUILayout.Button("Randomize All")){
            bsm.RandomizeSeeds();
        }
        if(GUILayout.Button("Clear All")){
            bsm.ClearAll();
        }
    }

}

#endif