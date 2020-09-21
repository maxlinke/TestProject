using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    public class BSplineMaster : MonoBehaviour {

        [Header("Gizmo Settings")]
        [SerializeField, Range(BezierSpline.MIN_GIZMO_SIZE, BezierSpline.MAX_GIZMO_SIZE)] public float gizmoSize = BezierSpline.DEFAULT_GIZMO_SIZE;

        [Header("Randomness")]
        [SerializeField] public Vector3 placementRandomness = Vector3.zero;
        [SerializeField] public float rotationRandomness = 0f;

        [Header("Object Settings")]
        [SerializeField] public ObjectPool objectPool = default;
        [SerializeField] public float spaceBetweenObjects = 0f;

        [Header("Placement Settings")]
        [SerializeField] public BSplineObjectPlacer.DistanceMode distanceMode = default;
        [SerializeField] public BSplineObjectPlacer.PlacementMode placementMode = default;
        [SerializeField] public Collider groundCollider = default;
        [SerializeField] public bool noOvershoot = true;

        public IEnumerable<BezierSpline> GetSplineChildren () {
            var children = GetComponentsInChildren<BezierSpline>(true);
            foreach(var child in children){
                yield return child;
            }
        }

        public IEnumerable<BSplineObjectPlacer> GetPlacerChildren () {
            var children = GetComponentsInChildren<BSplineObjectPlacer>(true);
            foreach(var child in children){
                yield return child;
            }
        }

        public void ToggleGizmos () {
            bool gotValueToUse = false;
            bool valueToUse = false;
            foreach(var child in GetSplineChildren()){
                #if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(child, "Change whether to draw gizmos");
                #endif
                if(!gotValueToUse){
                    valueToUse = !child.alwaysDrawGizmos;
                    gotValueToUse = true;
                }
                child.alwaysDrawGizmos = valueToUse;
            }
        }

        public void UpdateGizmoSettings () {
            foreach(var child in GetSplineChildren()){
                #if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(child, "Change gizmo size");
                #endif
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
                child.UpdatePlacementSettings(distanceMode, placementMode, groundCollider, noOvershoot);
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
    
}

