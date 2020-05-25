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
    [SerializeField] float desiredDistanceToBoundingVolume;

    [Header("Grouping")]
    [SerializeField] float localGroupRadius = 10f;


    public Vector3 velocity { get; private set; }
    public float speed => velocity.magnitude;
    public Vector3 position => transform.position;

    static List<SimpleBoid> boids;

    void Start () {
        velocity = transform.forward * minSpeed;
    }

    void Update () {
        if(boundingVolume == null){
            return;
        }
        if(boundingVolume.transform.rotation != Quaternion.identity){
            boundingVolume.transform.rotation = Quaternion.identity;
            Debug.LogWarning("bounding volume must be axis-aligned! fixed it for ya...");
        }
        var acceleration = Vector3.zero;
        GetBoundingVolumeAvoidance(out var bvInfluence, out var bvAvoidDir);
        acceleration += bvAvoidDir * bvInfluence * maxAccel;
        Debug.DrawRay(transform.position, bvAvoidDir * bvInfluence * 10f, Color.red);
        
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
    }

    void GetBoundingVolumeAvoidance (out float outputInfluence, out Vector3 outputDirection) {
        var localBVPos = transform.position - boundingVolume.transform.position;
        var absLocalBVPos = AbsVector3(localBVPos);
        var absExtents = AbsVector3(boundingVolume.transform.TransformVector(boundingVolume.size * 0.5f));
        Debug.DrawRay(boundingVolume.transform.position, absLocalBVPos, Color.green);
        Debug.DrawRay(boundingVolume.transform.position, absExtents, Color.blue);
        var absDelta = absExtents - absLocalBVPos;
        Debug.Log(absDelta);
        var edgeDist = Mathf.Min(Mathf.Min(absDelta.x, absDelta.y), absDelta.z);
        outputInfluence = Mathf.Clamp01(1f - (edgeDist / desiredDistanceToBoundingVolume));
        outputDirection = (boundingVolume.transform.TransformPoint(boundingVolume.center) - transform.position).normalized;
    }

    Vector3 AbsVector3 (Vector3 input) {
        return new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
    }
	
}
