using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public class BoidSpawner : MonoBehaviour {

        const int SPAWN_ITERATION_COUNT_LIMIT = 9001;

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
        [SerializeField] float size;
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
            var colCache = Gizmos.color;
            Gizmos.color = gizmoColor;
            var matCache = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            switch(shape){
                case Shape.BOX:
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * size);
                    break;
                case Shape.SPHERE:
                    Gizmos.DrawWireSphere(Vector3.zero, 0.5f * size);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(Shape)} \"{shape}\"!");
                    return;
            }
            Gizmos.matrix = matCache;
            if(detailed){

                foreach(var point in GetSpawnPoints()){
                    Gizmos.DrawCube(point, Vector3.one * 0.1f);
                }
            }
            Gizmos.color = colCache;
        }

        IEnumerable<Vector3> GetSpawnPoints () {
            var rng = new System.Random(randomSeed);
            int iteration = 0;
            int spawned = 0;
            while(iteration < SPAWN_ITERATION_COUNT_LIMIT && spawned < spawnNumber){
                iteration++;
                Vector3 tempPoint;
                // switch(shape){
                //     case Shape.BOX:
                        tempPoint = new Vector3((float)(rng.NextDouble()), (float)(rng.NextDouble()), (float)(rng.NextDouble()));
                        tempPoint *= 2f;
                        tempPoint -= Vector3.one;
                        // break;
                //     case Shape.SPHERE:
                //         tempPoint = 
                //         break;
                //     default:
                //         Debug.LogError($"Unknown {nameof(Shape)} \"{shape}\"!");
                //         yield break;
                // }
                tempPoint *= 0.5f * size;
                tempPoint = transform.TransformPoint(tempPoint);
                spawned++;
                yield return tempPoint;
            }
            if(iteration >= SPAWN_ITERATION_COUNT_LIMIT){
                Debug.LogWarning("Exceeded spawn iteration limit!");
            }
        }
        
        
    }

}