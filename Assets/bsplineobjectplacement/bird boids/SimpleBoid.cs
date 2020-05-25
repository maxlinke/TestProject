using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBoid : MonoBehaviour {

    [Header("Self")]
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxAccel;

    [Header("Bounding Volume")]
    [SerializeField] BoxCollider boundingVolume;
    [SerializeField] float bvAvoidDistStart;
    [SerializeField] float bvAvoidDistEnd;

    [Header("Grouping")]
    [SerializeField] float localGroupRadius;

    public Vector3 velocity { get; private set; }
    public float speed => velocity.magnitude;
    public Vector3 position => transform.position;

    static List<SimpleBoid> boids;

    void Start () {
        velocity = transform.forward * minSpeed;
    }

    void Update () {
        if(!BoundingVolumeValid()){
            return;
        }
        var acceleration = Vector3.zero;
        GetBoundingVolumeAvoidance(out var bvInfluence, out var bvAvoidDir);
        acceleration += bvAvoidDir * bvInfluence * maxAccel;
        
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
        var localBVPos = transform.position - boundingVolume.transform.position;
        var absLocalBVPos = AbsVector3(localBVPos);
        var absExtents = AbsVector3(boundingVolume.transform.TransformVector(boundingVolume.size * 0.5f));
        var absDelta = absExtents - absLocalBVPos;
        var edgeDist = Mathf.Min(Mathf.Min(absDelta.x, absDelta.y), absDelta.z);
        outputInfluence = Mathf.Clamp01(1f - ((edgeDist - bvAvoidDistEnd) / (bvAvoidDistStart - bvAvoidDistEnd)));
        outputDirection = (boundingVolume.transform.TransformPoint(boundingVolume.center) - transform.position).normalized;
    }

    Vector3 AbsVector3 (Vector3 input) {
        return new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
    }

    void OnDrawGizmosSelected () {
        if(!BoundingVolumeValid()){
            return;
        }
        var localBVPos = transform.position - boundingVolume.transform.position;
        var worldBVSize = boundingVolume.transform.TransformVector(boundingVolume.size);
        var xy0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 1, 0)) - Vector3.Scale(worldBVSize, new Vector3(0, 0, 0.5f));
        var xy1 = xy0 + new Vector3(0, 0, worldBVSize.z);
        var xz0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(1, 0, 1)) - Vector3.Scale(worldBVSize, new Vector3(0, 0.5f, 0));
        var xz1 = xz0 + new Vector3(0, worldBVSize.y, 0);
        var yz0 = boundingVolume.transform.position + Vector3.Scale(localBVPos, new Vector3(0, 1, 1)) - Vector3.Scale(worldBVSize, new Vector3(0.5f, 0, 0));
        var yz1 = yz0 + new Vector3(worldBVSize.x, 0, 0);
        var gizmoColorCache = Gizmos.color;
        Gizmos.color = Color.green;
        LineWithCubeAtEnd(xy0);
        LineWithCubeAtEnd(xy1);
        LineWithCubeAtEnd(xz0);
        LineWithCubeAtEnd(xz1);
        LineWithCubeAtEnd(yz0);
        LineWithCubeAtEnd(yz1);
        Gizmos.color = gizmoColorCache;

        void LineWithCubeAtEnd (Vector3 endPoint) {
            Gizmos.DrawLine(transform.position, endPoint);
            Gizmos.DrawCube(endPoint, Vector3.one * 0.1f);
        }
    }
	
}
