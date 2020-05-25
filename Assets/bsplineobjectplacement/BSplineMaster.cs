using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    public class BSplineMaster : MonoBehaviour {

        [Header("Gizmo Settings")]
        [SerializeField, Range(BezierSpline.MIN_GIZMO_SIZE, BezierSpline.MAX_GIZMO_SIZE)] float gizmoSize = BezierSpline.DEFAULT_GIZMO_SIZE;

        [Header("Randomness")]
        [SerializeField] Vector3 placementRandomness = Vector3.zero;
        [SerializeField] float rotationRandomness = 0f;

        [Header("Object Settings")]
        [SerializeField] ObjectPool objectPool = default;
        [SerializeField] float spaceBetweenObjects = 0f;

        [Header("Placement Settings")]
        [SerializeField] BSplineObjectPlacer.DistanceMode distanceMode = default;
        [SerializeField] BSplineObjectPlacer.GroundMode groundMode = default;
        [SerializeField] Collider groundCollider = default;
        [SerializeField] bool noOvershoot = true;

        IEnumerable<BezierSpline> GetSplineChildren () {
            var children = GetComponentsInChildren<BezierSpline>(true);
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
                Undo.RecordObject(child, "Change whether to draw gizmos");
                if(!gotValueToUse){
                    valueToUse = !child.alwaysDrawGizmos;
                    gotValueToUse = true;
                }
                child.alwaysDrawGizmos = valueToUse;
            }
        }

        public void UpdateGizmoSettings () {
            foreach(var child in GetSplineChildren()){
                Undo.RecordObject(child, "Change gizmo size");
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
    
}

