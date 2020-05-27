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

        [Header("Collision Debug")]
        [SerializeField] TerrainCollider debugGroundCollider;
        [SerializeField] List<Collider> debugColliders;

        [Header("Grouping")]
        [SerializeField] float localGroupRadius;

        public string influenceDebugString { get; private set; }

        void Start () {
            velocity = transform.forward * minSpeed;
        }

        void Update () {
            influenceDebugString = string.Empty;
            GetBoundingVolumeAvoidance(out var bvInfluence, out var bvAvoidDir);
            GetColliderAvoidance(out var colInfluence, out var colAvoidDir);
            GetGroundAvoidance(out var groundInfluence, out var groundAvoidDir);
            // var totalInfluence = 1f;
            // var totalAccelDir = Vector3.zero;
            // totalAccelDir += bvAvoidDir * (totalInfluence * bvInfluence);
            // totalInfluence -= (totalInfluence * bvInfluence);
            // totalAccelDir += groundAvoidDir * (totalInfluence * groundInfluence);
            // totalInfluence -= (totalInfluence * groundInfluence);
            // totalAccelDir += colAvoidDir * (totalInfluence * colInfluence);
            // totalInfluence -= (totalInfluence * colInfluence);
            // var acceleration = totalAccelDir.normalized * Mathf.Min(totalAccelDir.magnitude * maxAccel, maxAccel); 

            var acceleration = bvAvoidDir + colAvoidDir + groundAvoidDir;
            acceleration = acceleration.normalized * Mathf.Min(acceleration.magnitude, 1f) * maxAccel;
            
            influenceDebugString += $"\nbvAvoid: {bvInfluence:F3}";
            influenceDebugString += $"\ngroundAvoid: {groundInfluence:F3}";
            influenceDebugString += $"\ncolAvoid: {colInfluence:F3}";
            
            velocity += acceleration * Time.deltaTime;
            velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
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