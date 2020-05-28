using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour {

    const int SPAWN_ITERATION_COUNT_LIMIT = 9001;

    public enum Shape {
        BOX,
        SPHERE
    }

    [Header("Gizmos")]
    [SerializeField] bool alwaysDrawGizmos = true;
    [SerializeField] Color gizmoColor = Color.green;
    [SerializeField] float gizmoSize = 0.1f;

    [Header("Spawning Settings")]
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] int spawnNumber = 100;
    [SerializeField] int randomSeed = 0x9FA2908;

    [Header("Spawner Settings")]
    [SerializeField] float size = 20;
    [SerializeField] Shape shape = Shape.BOX;
    
    [Header("Boid Settings")]
    [SerializeField] GameObject boidPrefab = null;
    [SerializeField] string initAnimName = string.Empty;
    [SerializeField] float boidMinSpeed = 5f;
    [SerializeField] float boidMaxSpeed = 10f;
    [SerializeField] float boidMaxAccel = 100f;
    [SerializeField] float boidCohesionDistance = 7f;
    [SerializeField, Range(0f, 10f)] float boidCohesionWeight = 1.5f;
    [SerializeField] float boidAlignmentDistance = 5f;
    [SerializeField, Range(0f, 10f)] float boidAlignmentWeight = 0.5f;
    [SerializeField] float boidSeparationDistance = 1f;
    [SerializeField, Range(0f, 10f)] float boidSeparationWeight = 6.67f;
    [SerializeField] Transform boidTarget = null;
    [SerializeField, Range(0f, 10f)] float boidTargetSeekWeight = 10f;

    class Boid {
        public Transform transform;
        public Vector3 velocity;
        public Boid (Transform transform, Vector3 velocity) {
            this.transform = transform;
            this.velocity = velocity;
        }
    }

    GameObject boidParent;
    List<Boid> boids;
    List<Vector3> boidAccelerations;

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
    }

    public void SpawnBoids () {
        if(boids != null){
            Debug.LogError("Already spawned boids! Multiple spawning isn't supported currently!");
            return;
        }
        if(boidPrefab == null){
            Debug.LogError("No boid prefab assigned!");
            return;
        }
        boids = new List<Boid>();
        boidAccelerations = new List<Vector3>();
        boidParent = new GameObject("Boid Parent");
        var rng = new System.Random(randomSeed);
        foreach(var point in GetSpawnPoints()){
            var randomVec = new Vector3((float)(rng.NextDouble()), (float)(rng.NextDouble()), (float)(rng.NextDouble())) * 2f - Vector3.one;
            var boidForward = (this.transform.forward + 0.1f * randomVec).normalized;
            var newBoidGO = Instantiate(boidPrefab, point, Quaternion.LookRotation(boidForward, Vector3.up), boidParent.transform);
            var newBoidAnim = newBoidGO.GetComponent<Animator>();
            if(newBoidAnim != null){
                newBoidAnim.Play(initAnimName, 0, Random.value);
            }
            boids.Add(new Boid(newBoidGO.transform, boidForward * Mathf.Lerp(boidMinSpeed, boidMaxSpeed, (float)(rng.NextDouble()))));
            boidAccelerations.Add(Vector3.zero);
        }
    }

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
        float sqCohesionDist = boidCohesionDistance * boidCohesionDistance;
        float sqAlignmentDist = boidAlignmentDistance * boidAlignmentDistance;
        float sqSeparationDist = boidSeparationDistance * boidSeparationDistance;
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
            var cohesion = ((flockPosSum / flockPosCount) - activePos).normalized;
            var alignment = (flockVelocitySum / flockVelocityCount) - activeBoid.velocity;
            var separation = (separationSum / separationCount).normalized;
            var rawAccel = boidCohesionWeight * cohesion + boidAlignmentWeight * alignment + boidSeparationWeight * separation;
            if(boidTarget != null){
                var toTarget = (boidTarget.position - activePos).normalized;
                rawAccel += boidTargetSeekWeight * toTarget;
            }
            var accelMag = rawAccel.magnitude;
            if(accelMag > boidMaxAccel){
                boidAccelerations[i] = boidMaxAccel * (rawAccel / accelMag);
            }else{
                boidAccelerations[i] = rawAccel;
            }
        }
    }

    void MoveBoids () {
        for(int i=0; i<boids.Count; i++){
            var boid = boids[i];
            var accel = boidAccelerations[i];
            boid.velocity += accel * Time.deltaTime;
            boid.velocity = boid.velocity.normalized * Mathf.Clamp(boid.velocity.magnitude, boidMinSpeed, boidMaxSpeed);
            boid.transform.position += boid.velocity * Time.deltaTime;
            boid.transform.rotation = Quaternion.LookRotation(boid.velocity, 10f * Vector3.up + accel);     // 10f is standing in for 9.81m/s^2 gravitational acceleration
        }
    }

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
        if(boidTarget != null){
            Gizmos.DrawLine(transform.position, boidTarget.position);
            Gizmos.DrawWireSphere(boidTarget.position, 1f);
        }
        if(detailed){
            var pointGizmoSize = Vector3.one * gizmoSize;
            foreach(var point in GetSpawnPoints()){
                Gizmos.DrawCube(point, pointGizmoSize);
            }
        }
        Gizmos.color = colCache;
    }

#endregion
	
}
