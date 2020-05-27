using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public abstract class Boid : MonoBehaviour {

        [Header("Bounding Volume / Movement Space")]
        [SerializeField] protected BoxCollider bv;
        [SerializeField] protected BoidRange bvRange;

        public Vector3 position => transform.position;
        public Vector3 velocity { get; protected set; }
        // public float speed => velocity.magnitude;

        protected List<Boid> boids;

        public virtual void Initialize (List<Boid> boids) {
            this.boids = boids;
        }

        protected bool BoundingVolumeValid () {
            if(bv == null){
                return false;
            }
            if(bv.transform.rotation != Quaternion.identity){
                bv.transform.rotation = Quaternion.identity;
                Debug.LogWarning("bounding volume must be axis-aligned! fixed it for ya...");
            }
            return true;
        }

        protected void GetBoundingVolumeAvoidance (out float outputInfluence, out Vector3 outputDirection) {
            var localBVPos = transform.position - bv.bounds.center;
            var absLocalBVPos = localBVPos.Abs();
            var absExtents = bv.bounds.extents;
            var absDelta = absExtents - absLocalBVPos;
            var edgeDist = Mathf.Min(Mathf.Min(absDelta.x, absDelta.y), absDelta.z);
            outputInfluence = bvRange.Evaluate(edgeDist);
            outputDirection = (bv.bounds.center - transform.position).normalized;
        }

        protected void GetColliderAvoidance (IEnumerable<Collider> cols, BoidRange colAvoid, out float outputInfluence, out Vector3 outputDirection) {
            outputInfluence = 0f;
            outputDirection = Vector3.zero;
            var moveRay = new Ray(transform.position, velocity);
            foreach(var col in cols){
                if(col.Raycast(moveRay, out var hit, colAvoid.max)){
                    var influence = colAvoid.Evaluate(hit.distance);
                    var colDir = (transform.position - col.bounds.center).normalized;
                    outputInfluence = Mathf.Max(outputInfluence, influence);
                    outputDirection += influence * colDir;
                }
            }
        }

        protected void GetGroundAvoidance (Collider gc, BoidRange gAvoid, BoidRange colAvoid, out float outputInfluence, out Vector3 outputDirection) {
            outputInfluence = 0f;
            outputDirection = Vector3.zero;
            if(gc == null){
                return;
            }
            var heightRayStartY = gc.bounds.center.y + gc.bounds.extents.y + 0.1f;
            var heightRayStart = new Vector3(transform.position.x, heightRayStartY, transform.position.z);
            var heightRayLength = 2f * gc.bounds.extents.y + 0.2f;
            var heightRay = new Ray(heightRayStart, Vector3.down);
            if(gc.Raycast(heightRay, out var heightHit, heightRayLength)){
                var height = transform.position.y - heightHit.point.y;
                var influence = gAvoid.Evaluate(height);
                outputInfluence = Mathf.Max(outputInfluence, influence);
                outputDirection += influence * Vector3.up;
                Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.green);
            }else{
                Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.red);
            }
            var moveRay = new Ray(transform.position, velocity);
            if(gc.Raycast(moveRay, out var hit, colAvoid.max)){
                var influence = colAvoid.Evaluate(hit.distance);
                outputInfluence = Mathf.Max(outputInfluence, influence);
                outputDirection += influence * hit.normal;
            }
        }
        
    }

}