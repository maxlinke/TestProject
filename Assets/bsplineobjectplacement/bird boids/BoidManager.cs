using System.Collections.Generic;
using UnityEngine;

namespace Boids {
    
    public class BoidManager : MonoBehaviour {

        [Header("Gizmos")]
        [SerializeField] bool alwaysDrawGizmos;
        [SerializeField] Color gizmoColor;

        [Header("Boid Environment")]
        [SerializeField] Vector3 boundingVolumeExtents;
        [SerializeField] TerrainCollider ground;
        [SerializeField] List<Collider> colliders;

        Bounds boundingVolume;
        List<Boid> boids;
        bool initialized = false;

        void Awake () {
            if(!initialized){
                Initialize();
            }
        }

        void Initialize () {
            if(initialized){
                Debug.LogError("Duplicate init call, aborting!");
                return;
            }
            boids = new List<Boid>();
            boundingVolume = new Bounds(transform.position, boundingVolumeExtents);
            this.initialized = true;
        }

        void OnDrawGizmos () {
            if(alwaysDrawGizmos){
                DrawGizmos();
            }
        }

        void OnDrawGizmosSelected () {
            DrawGizmos();
        }

        void DrawGizmos () {
            #if UNITY_EDITOR
            var cc = Gizmos.color;
            Gizmos.color = gizmoColor;
            if(!UnityEditor.EditorApplication.isPlaying){
                Gizmos.DrawWireCube(transform.position, boundingVolumeExtents * 2);
            }else{
                Gizmos.DrawWireCube(boundingVolume.center, boundingVolume.extents * 2);
            }
            Gizmos.color = cc;
            #endif
        }

        public void RegisterAndInitializeBoid (Boid newBoid, BoidSettings settings) {
            if(!initialized){
                Initialize();
            }
            if(boids.Contains(newBoid)){
                Debug.LogError("Attempt to register an already registered boid!");
                return;
            }
            boids.Add(newBoid);
            newBoid.Initialize(boids, settings, boundingVolume, colliders, ground);
        }
        
    }

}