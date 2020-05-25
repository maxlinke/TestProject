using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleBoid : MonoBehaviour {

    [Header("Self")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxAccel;

    [Header("Bounding Volume")]
    [SerializeField] BoxCollider boundingVolume;
    [SerializeField] AvoidanceSettings bvAvoidance;

    [Header("Collisions")]
    [SerializeField] TerrainCollider groundCollider;
    [SerializeField] AvoidanceSettings groundAvoidance;
    [SerializeField] Collider[] colliders;
    [SerializeField] AvoidanceSettings colAvoidance;

    [Header("Grouping")]
    [SerializeField] float localGroupRadius;

    public Vector3 velocity { get; private set; }
    public float speed => velocity.magnitude;
    public Vector3 position => transform.position;

    public string influenceDebugString { get; private set; }

    static List<SimpleBoid> boids;

    [System.Serializable]
    struct AvoidanceSettings {
        public float minDist;
        public float maxDist;
        public float power;
    }

    void Start () {
        velocity = transform.forward * minSpeed;
    }

    void Update () {
        if(!BoundingVolumeValid()){
            return;
        }
        influenceDebugString = string.Empty;
        GetBoundingVolumeAvoidance(out var bvInfluence, out var bvAvoidDir);
        GetColliderAvoidance(out var colInfluence, out var colAvoidDir);
        GetGroundAvoidance(out var groundInfluence, out var groundAvoidDir);
        var totalInfluence = 1f;
        var totalAccelDir = Vector3.zero;
        totalAccelDir += bvAvoidDir * (totalInfluence * bvInfluence);
        totalInfluence -= (totalInfluence * bvInfluence);
        totalAccelDir += groundAvoidDir * (totalInfluence * groundInfluence);
        totalInfluence -= (totalInfluence * groundInfluence);
        totalAccelDir += colAvoidDir * (totalInfluence * colInfluence);
        totalInfluence -= (totalInfluence * colInfluence);

        var acceleration = totalAccelDir.normalized * Mathf.Min(totalAccelDir.magnitude * maxAccel, maxAccel); 
        
        influenceDebugString += $"\nbvAvoid: {bvInfluence:F3}";
        influenceDebugString += $"\ngroundAvoid: {groundInfluence:F3}";
        influenceDebugString += $"\ncolAvoid: {colInfluence:F3}";
        
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
    }

    bool BoundingVolumeValid () {
        if(boundingVolume == null){
            return false;
        }
        if(boundingVolume.transform.rotation != Quaternion.identity){
            boundingVolume.transform.rotation = Quaternion.identity;
            Debug.LogWarning("bounding volume must be axis-aligned! fixed it for ya...");
        }
        return true;
    }

    void GetBoundingVolumeAvoidance (out float outputInfluence, out Vector3 outputDirection) {
        var localBVPos = transform.position - boundingVolume.bounds.center;
        var absLocalBVPos = localBVPos.Abs();
        var absExtents = boundingVolume.bounds.extents;
        var absDelta = absExtents - absLocalBVPos;
        var edgeDist = Mathf.Min(Mathf.Min(absDelta.x, absDelta.y), absDelta.z);
        var edgeDistInterval = (edgeDist - bvAvoidance.minDist) / (bvAvoidance.maxDist - bvAvoidance.minDist);
        outputInfluence = Mathf.Pow(Mathf.Clamp01(1f - edgeDistInterval), bvAvoidance.power);
        outputDirection = (boundingVolume.bounds.center - transform.position).normalized;
    }

    void GetColliderAvoidance (out float outputInfluence, out Vector3 outputDirection) {
        outputInfluence = 0f;
        outputDirection = Vector3.zero;
        var moveRay = new Ray(transform.position, velocity);
        foreach(var col in colliders){
            if(col.bounds.IntersectRay(moveRay)){
                if(col.Raycast(moveRay, out var hit, colAvoidance.maxDist)){
                    var influenceInterval = (hit.distance - colAvoidance.minDist) / (colAvoidance.maxDist - colAvoidance.minDist);
                    var influence = Mathf.Pow(Mathf.Clamp01(1f - influenceInterval), colAvoidance.power);
                    var colDir = (transform.position - col.bounds.center).normalized;
                    outputInfluence = Mathf.Max(outputInfluence, influence);
                    outputDirection += influence * colDir;
                }
            }
        }
        // if(groundCollider != null){
        //     if(groundCollider.Raycast(moveRay, out var hit, colAvoidance.maxDist)){
        //         var influenceInterval = (hit.distance - colAvoidance.minDist) / (colAvoidance.maxDist - colAvoidance.minDist);
        //         var influence = Mathf.Pow(Mathf.Clamp01(1f - influenceInterval), colAvoidance.power);
        //         outputInfluence = Mathf.Max(outputInfluence, influence);
        //         outputDirection += influence * hit.normal;
        //     }
        // }
    }

    void GetGroundAvoidance (out float outputInfluence, out Vector3 outputDirection) {
        outputInfluence = 0f;
        outputDirection = Vector3.zero;
        if(groundCollider == null){
            return;
        }
        var heightRayStartY = groundCollider.bounds.center.y + groundCollider.bounds.extents.y + 0.1f;
        var heightRayStart = new Vector3(transform.position.x, heightRayStartY, transform.position.z);
        var heightRayLength = 2f * groundCollider.bounds.extents.y + 0.2f;
        var heightRay = new Ray(heightRayStart, Vector3.down);
        if(groundCollider.Raycast(heightRay, out var heightHit, heightRayLength)){
            var height = transform.position.y - heightHit.point.y;
            var influenceInterval = (height - groundAvoidance.minDist) / (groundAvoidance.maxDist - groundAvoidance.minDist);
            var influence = Mathf.Pow(Mathf.Clamp01(1f - influenceInterval), groundAvoidance.power);
            outputInfluence = Mathf.Max(outputInfluence, influence);
            outputDirection += influence * Vector3.up;
            Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.green);
        }else{
            Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.red);
        }
        var moveRay = new Ray(transform.position, velocity);
        if(groundCollider.Raycast(moveRay, out var hit, colAvoidance.maxDist)){
            var influenceInterval = (hit.distance - colAvoidance.minDist) / (colAvoidance.maxDist - colAvoidance.minDist);
            var influence = Mathf.Pow(Mathf.Clamp01(1f - influenceInterval), colAvoidance.power);
            outputInfluence = Mathf.Max(outputInfluence, influence);
            outputDirection += influence * hit.normal;
        }
    }

    void OnDrawGizmosSelected () {
        if(!BoundingVolumeValid()){
            return;
        }
        var localBVPos = transform.position - boundingVolume.transform.position;
        var worldBVExt = boundingVolume.bounds.extents;
        var xy0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 1, 0)) - new Vector3(0, 0, worldBVExt.z);
        var xy1 = xy0 + new Vector3(0, 0, 2f * worldBVExt.z);
        var xz0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 0, 1)) - new Vector3(0, worldBVExt.y, 0);
        var xz1 = xz0 + new Vector3(0, 2f * worldBVExt.y, 0);
        var yz0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(0, 1, 1)) - new Vector3(worldBVExt.x, 0, 0);
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