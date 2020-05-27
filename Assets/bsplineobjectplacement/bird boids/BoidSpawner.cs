using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public class BoidSpawner : MonoBehaviour {

        public enum Shape {
            BOX,
            SPHERE
        }

        public enum PlacementType {
            VOLUME,
            SURFACE,
            RAYCASTED_DOWN
        }

        public enum SpawnState {
            IDLE,
            FLYING
        }

        [Header("Gizmos")]
        [SerializeField] bool alwaysDrawGizmos;
        [SerializeField] Color gizmoColor;

        [Header("Boids")]
        [SerializeField] Boid boidPrefab;
        // TODO settings externalized as a scriptable object
        // TODO also type and relations between types... (just use the scriptable object itself as a type?)
        // separation is general, alignment only for same "species" and cohesion can be negative for "predators"

        [Header("Spawner Generics")]
        [SerializeField] bool spawnOnStart;
        [SerializeField] int spawnNumber;
        [SerializeField] int randomSeed;

        [Header("Spawner Specifics")]
        [SerializeField] Vector3 extents;
        [SerializeField] Shape shape;

        void Start () {
            if(spawnOnStart){

            }
        }

        void OnDrawGizmos () {
            if(alwaysDrawGizmos){
                DrawGizmos(false);
            }
        }

        void OnDrawGizmosSelected () {
            DrawGizmos(true);
        }

        void DrawGizmos (bool detailed) {
            
            // if detailed also draw the spawn points
        }
        
        
    }

}