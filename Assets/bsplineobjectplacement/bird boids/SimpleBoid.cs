using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boids {

    public class SimpleBoid : Boid {

        [Header("Self")]
        [SerializeField] float minSpeed;
        [SerializeField] float maxSpeed;
        [SerializeField] float maxAccel;

        [Header("Collisions")]
        [SerializeField] TerrainCollider groundCollider;
        [SerializeField] BoidRange groundAvoidance;
        [SerializeField] Collider[] colliders;
        [SerializeField] BoidRange colAvoidance;

        [Header("Grouping")]
        [SerializeField] float localGroupRadius;

        public string influenceDebugString { get; private set; }

        void Start () {
            velocity = transform.forward * minSpeed;
        }

        void Update () {
            if(!BoundingVolumeValid()){
                return;
            }
            influenceDebugString = string.Empty;
            GetBoundingVolumeAvoidance(out var bvInfluence, out var bvAvoidDir);
            GetColliderAvoidance(colliders, colAvoidance, out var colInfluence, out var colAvoidDir);
            GetGroundAvoidance(groundCollider, groundAvoidance, colAvoidance, out var groundInfluence, out var groundAvoidDir);
            var totalInfluence = 1f;
            var totalAccelDir = Vector3.zero;
            totalAccelDir += bvAvoidDir * (totalInfluence * bvInfluence);
            totalInfluence -= (totalInfluence * bvInfluence);
            totalAccelDir += groundAvoidDir * (totalInfluence * groundInfluence);
            totalInfluence -= (totalInfluence * groundInfluence);
            totalAccelDir += colAvoidDir * (totalInfluence * colInfluence);
            totalInfluence -= (totalInfluence * colInfluence);

            //TODO for the remaining influence, try to get to level flight... climbing is expensive after all

            var acceleration = totalAccelDir.normalized * Mathf.Min(totalAccelDir.magnitude * maxAccel, maxAccel); 
            
            influenceDebugString += $"\nbvAvoid: {bvInfluence:F3}";
            influenceDebugString += $"\ngroundAvoid: {groundInfluence:F3}";
            influenceDebugString += $"\ncolAvoid: {colInfluence:F3}";
            
            velocity += acceleration * Time.deltaTime;
            velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
        }

        void OnDrawGizmosSelected () {
            if(!BoundingVolumeValid()){
                return;
            }
            var localBVPos = transform.position - bv.transform.position;
            var worldBVExt = bv.bounds.extents;
            var xy0 = bv.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 1, 0)) - new Vector3(0, 0, worldBVExt.z);
            var xy1 = xy0 + new Vector3(0, 0, 2f * worldBVExt.z);
            var xz0 = bv.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 0, 1)) - new Vector3(0, worldBVExt.y, 0);
            var xz1 = xz0 + new Vector3(0, 2f * worldBVExt.y, 0);
            var yz0 = bv.transform.position + Vector3.Scale(localBVPos, new Vector3(0, 1, 1)) - new Vector3(worldBVExt.x, 0, 0);
            var yz1 = yz0 + new Vector3(2f * worldBVExt.x, 0, 0);
            var gizmoColorCache = Gizmos.color;
            Gizmos.color = Color.green;
            LineWithCubeAtEnd(xy0);
            LineWithCubeAtEnd(xy1);
            LineWithCubeAtEnd(xz0);
            LineWithCubeAtEnd(xz1);
            LineWithCubeAtEnd(yz0);
            LineWithCubeAtEnd(yz1);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, velocity);
            Gizmos.color = gizmoColorCache;

            void LineWithCubeAtEnd (Vector3 endPoint) {
                Gizmos.DrawLine(transform.position, endPoint);
                Gizmos.DrawCube(endPoint, Vector3.one * 0.1f);
            }
        }
        
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(SimpleBoid))]
    public class SimpleBoidEditor : Editor {

        SimpleBoid sb;

        void OnEnable () {
            sb = target as SimpleBoid;
        }

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            if(EditorApplication.isPlaying){
                GUILayout.Space(20f);
                GUILayout.Label(sb.influenceDebugString);
            }
        }

    }

    #endif

}