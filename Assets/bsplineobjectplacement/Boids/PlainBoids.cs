using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public class PlainBoids : MonoBehaviour {

        const int SPAWN_ITERATION_COUNT_LIMIT = 9001;
        protected const float MIN_BEHAVIOR_WEIGHT = -2f;
        protected const float MAX_BEHAVIOR_WEIGHT = 2f;

        public enum Shape {
            BOX,
            SPHERE
        }

        [Header("Gizmos")]
        [SerializeField] protected bool alwaysDrawGizmos = true;
        [SerializeField] protected Color gizmoColor = Color.green;
        [SerializeField] protected float gizmoSize = 0.5f;

        [Header("Spawning Settings")]
        [SerializeField] protected bool spawnOnStart = true;
        [SerializeField] protected int spawnNumber = 100;
        [SerializeField] protected int randomSeed = 0x9FA2908;

        [Header("Spawner Settings")]
        [SerializeField] protected Shape shape = Shape.BOX;
        [SerializeField] protected float size = 20;
        
        [Header("Boid Settings")]
        [SerializeField] protected GameObject boidPrefab = null;
        [SerializeField] protected string initAnimName = string.Empty;
        [SerializeField] protected float boidMinSpeed = 5f;
        [SerializeField] protected float boidMaxSpeed = 10f;
        [SerializeField] protected float boidMaxAccel = 20f;
        [SerializeField] protected float boidMaxVisualRotationSpeed = 360f;

        [Header("Boid Behaviors")]
        [SerializeField] protected DistancedBehaviourParameters cohesion = BehaviourParameters.CohesionDefault;
        [SerializeField] protected DistancedBehaviourParameters alignment = BehaviourParameters.AlignmentDefault;
        [SerializeField] protected DistancedBehaviourParameters separation = BehaviourParameters.SeparationDefault;
        [SerializeField] protected TimedBehaviourParameters randomDirection = BehaviourParameters.RandomDirectionDefault;

        protected class Boid {
            public Transform transform;
            public Vector3 velocity;
            public Boid (Transform transform, Vector3 velocity) {
                this.transform = transform;
                this.velocity = velocity;
            }
        }

        protected GameObject boidParent;
        protected List<Boid> boids;
        protected List<Vector3> boidAccelerations;
        protected List<Vector3> boidRandomDirections;
        protected List<float> boidRandomTimers;

        void Start () {
            if(spawnOnStart){
                SpawnBoids();
            }
        }

        void Update () {
            if(boids == null){
                return;
            }
            CalculateBoidAccelerations();
            MoveBoids();
            OnUpdateFinished();
        }

        public void SpawnBoids () {
            if(boidPrefab == null){
                Debug.LogError("No boid prefab assigned!");
                return;
            }
            if(!AdditionalPreSpawnCheck(out var msg)){
                Debug.LogError(msg);
                return;
            }
            if(boids == null){
                boids = new List<Boid>();
                boidAccelerations = new List<Vector3>();
                boidRandomDirections = new List<Vector3>();
                boidRandomTimers = new List<float>();
                boidParent = new GameObject("Boid Parent");
                AdditionalPreSpawnSetup();
            }
            var rng = new System.Random(randomSeed);
            foreach(var point in GetSpawnPoints()){
                var newBoidGO = Instantiate(boidPrefab, point, Quaternion.LookRotation(this.transform.forward, Vector3.up), boidParent.transform);
                var newBoidAnim = newBoidGO.GetComponent<Animator>();
                if(newBoidAnim != null){
                    newBoidAnim.Play(initAnimName, 0, Random.value);
                }
                var newBoid = new Boid(newBoidGO.transform, newBoidGO.transform.forward * Mathf.Lerp(boidMinSpeed, boidMaxSpeed, (float)(rng.NextDouble())));
                boids.Add(newBoid);
                boidAccelerations.Add(Vector3.zero);
                boidRandomDirections.Add(Vector3.zero);
                boidRandomTimers.Add((float)(rng.NextDouble()) * randomDirection.Interval);
                OnAdditionalBoidAdded(newBoid);
            }
        }

        protected virtual bool AdditionalPreSpawnCheck (out string message) {
            message = string.Empty;
            return true;
        }

        protected virtual void AdditionalPreSpawnSetup () { }

        protected virtual void OnAdditionalBoidAdded (Boid newBoid) { }

        IEnumerable<Vector3> GetSpawnPoints () {
            var rng = new System.Random(randomSeed);
            int iteration = 0;
            int spawned = 0;
            while(iteration < SPAWN_ITERATION_COUNT_LIMIT && spawned < spawnNumber){
                iteration++;
                Vector3 tempPoint = 2f * new Vector3((float)(rng.NextDouble()), (float)(rng.NextDouble()), (float)(rng.NextDouble())) - Vector3.one;
                switch(shape){
                    case Shape.BOX:
                        break;
                    case Shape.SPHERE:
                        if(tempPoint.sqrMagnitude > 1f){
                            continue;
                        }
                        break;
                    default:
                        Debug.LogError($"Unknown {nameof(Shape)} \"{shape}\"!");
                        yield break;
                }
                tempPoint *= 0.5f * size;
                tempPoint = transform.TransformPoint(tempPoint);
                spawned++;
                yield return tempPoint;
            }
            if(iteration >= SPAWN_ITERATION_COUNT_LIMIT){
                Debug.LogWarning("Exceeded spawn iteration limit!");
            }
        }

        void CalculateBoidAccelerations () {
            float sqCohesionDist = cohesion.Distance * cohesion.Distance;
            float sqAlignmentDist = alignment.Distance * alignment.Distance;
            float sqSeparationDist = separation.Distance * separation.Distance;
            for(int i=0; i<boids.Count; i++){
                var activeBoid = boids[i];
                var activePos = activeBoid.transform.position;
                Vector3 flockPosSum = Vector3.zero;
                int flockPosCount = 0;
                Vector3 flockVelocitySum = Vector3.zero;
                int flockVelocityCount = 0;
                Vector3 separationSum = Vector3.zero;
                int separationCount = 0;
                for(int j=0; j<boids.Count; j++){
                    var otherBoid = boids[j];
                    var otherPos = otherBoid.transform.position;
                    var toOther = otherPos - activePos;
                    var otherSqDist = toOther.sqrMagnitude;
                    if(otherSqDist < sqCohesionDist){
                        flockPosSum += otherPos;
                        flockPosCount++;
                    }
                    if(otherSqDist < sqAlignmentDist){
                        flockVelocitySum += otherBoid.velocity;
                        flockVelocityCount++;
                    }
                    if(otherSqDist < sqSeparationDist){
                        separationSum -= toOther.normalized;
                        separationCount++;
                    }
                }
                boidRandomTimers[i] -= Time.deltaTime;
                if(boidRandomTimers[i] <= 0 && randomDirection.Interval > 0){
                    boidRandomDirections[i] = Random.insideUnitSphere;
                    boidRandomTimers[i] = Mathf.Repeat(boidRandomTimers[i], randomDirection.Interval);
                }
                var rawAccel = cohesion.Weight * ((flockPosSum / flockPosCount) - activePos).normalized;
                rawAccel += alignment.Weight * ((flockVelocitySum / flockVelocityCount) - activeBoid.velocity);
                rawAccel += separation.Weight * (separationSum / separationCount).normalized;
                rawAccel += randomDirection.Weight * boidRandomDirections[i];
                rawAccel += GetAdditionalBehavior(i);
                rawAccel *= boidMaxAccel;
                var accelMag = rawAccel.magnitude;
                if(accelMag > boidMaxAccel){
                    boidAccelerations[i] = boidMaxAccel * (rawAccel / accelMag);
                }else{
                    boidAccelerations[i] = rawAccel;
                }
            }
        }

        protected virtual Vector3 GetAdditionalBehavior (int boidIndex) {
            return Vector3.zero;
        }

        void MoveBoids () {
            for(int i=0; i<boids.Count; i++){
                var boid = boids[i];
                var accel = boidAccelerations[i];
                boid.velocity += accel * Time.deltaTime;
                boid.velocity = boid.velocity.normalized * Mathf.Clamp(boid.velocity.magnitude, boidMinSpeed, boidMaxSpeed);
                boid.transform.position += boid.velocity * Time.deltaTime;
                // boid.transform.rotation = Quaternion.LookRotation(boid.velocity, 10f * Vector3.up + accel);     // 10f is standing in for 9.81m/s^2 gravitational acceleration
                boid.transform.rotation = Quaternion.RotateTowards(boid.transform.rotation, Quaternion.LookRotation(boid.velocity, 10f * Vector3.up + accel), boidMaxVisualRotationSpeed * Time.deltaTime);
            }
        }

        protected virtual void OnUpdateFinished () { }

    #region Gizmos

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
            DrawShape();
            DrawStartDirection();
            DrawAdditionalGizmos();
            if(detailed){
                DrawSpawnPoints();
                DrawAdditionalDetailedGizmos();
            }
            Gizmos.color = colCache;

            void DrawShape () {
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
            }

            void DrawStartDirection () {
                var matCache = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                var tip = Vector3.forward * size;
                Gizmos.DrawLine(Vector3.zero, tip);
                Gizmos.matrix = Matrix4x4.Translate(transform.TransformPoint(tip)) * Matrix4x4.Rotate(transform.rotation) * Matrix4x4.Scale(Vector3.one * transform.lossyScale.Min());
                int steps = 16;
                for(int i=0; i<steps; i++){
                    float s = 2f * Mathf.PI * i / steps;
                    float t = 2f * Mathf.PI * (i+1) / steps;
                    var p1 = new Vector3(Mathf.Sin(s), Mathf.Cos(s), -2) * size * 0.1f;
                    var p2 = new Vector3(Mathf.Sin(t), Mathf.Cos(t), -2) * size * 0.1f;
                    Gizmos.DrawLine(p1, p2);
                    if(i % (steps / 4) == 0){
                        Gizmos.DrawLine(Vector3.zero, p1);
                    }
                }
                Gizmos.matrix = matCache;
            }

            void DrawSpawnPoints () {
                var pointGizmoSize = 0.5f * gizmoSize;
                foreach(var point in GetSpawnPoints()){
                    Gizmos.DrawSphere(point, pointGizmoSize);
                }
            }
        }

        protected virtual void DrawAdditionalGizmos () { }

        protected virtual void DrawAdditionalDetailedGizmos () { }

    #endregion
        
    }

}