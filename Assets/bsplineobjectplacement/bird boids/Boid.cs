﻿using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public abstract class Boid : MonoBehaviour {

        [Header("ALL THIS SHIT IS TEMPORARY!!!")]
        [SerializeField] protected BoidRange boundingVolumeRange;
        [SerializeField] protected BoidRange colliderRange;
        [SerializeField] protected BoidRange groundHeightRange;

        public Vector3 position => transform.position;
        public Vector3 velocity { get; protected set; }
        // public float speed => velocity.magnitude;

        protected Bounds boundingVolume;
        protected List<Boid> boids;
        protected List<Collider> colliders;
        protected TerrainCollider groundCollider;

        public virtual void Initialize (List<Boid> boids, Bounds boundingVolume, List<Collider> colliders, TerrainCollider groundCollider) {
            this.boids = boids;
            this.boundingVolume = boundingVolume;
            this.colliders = colliders;
            this.groundCollider = groundCollider;
        }

        protected virtual void OnDrawGizmosSelected () {
            var localPos = transform.position - boundingVolume.center;
            var bvExt = boundingVolume.extents;
            var xy0 = boundingVolume.center + Vector3.Scale(localPos, new Vector3(1, 1, 0)) - new Vector3(0, 0, bvExt.z);
            var xy1 = xy0 + new Vector3(0, 0, 2f * bvExt.z);
            var xz0 = boundingVolume.center + Vector3.Scale(localPos, new Vector3(1, 0, 1)) - new Vector3(0, bvExt.y, 0);
            var xz1 = xz0 + new Vector3(0, 2f * bvExt.y, 0);
            var yz0 = boundingVolume.center + Vector3.Scale(localPos, new Vector3(0, 1, 1)) - new Vector3(bvExt.x, 0, 0);
            var yz1 = yz0 + new Vector3(2f * bvExt.x, 0, 0);
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

        protected void GetBoundingVolumeAvoidance (out float outputInfluence, out Vector3 outputDirection) {
            var localBVPos = transform.position - boundingVolume.center;
            var absLocalBVPos = localBVPos.Abs();
            var absExtents = boundingVolume.extents;
            var absDelta = absExtents - absLocalBVPos;
            var edgeDist = Mathf.Min(Mathf.Min(absDelta.x, absDelta.y), absDelta.z);
            outputInfluence = boundingVolumeRange.Evaluate(edgeDist);
            outputDirection = boundingVolumeRange.sign * outputInfluence * (transform.position - boundingVolume.center).normalized;
        }

        protected void GetColliderAvoidance (out float outputInfluence, out Vector3 outputDirection) {
            outputInfluence = 0f;
            outputDirection = Vector3.zero;
            var moveRay = new Ray(transform.position, velocity);
            foreach(var col in colliders){
                if(col.Raycast(moveRay, out var hit, colliderRange.max)){
                    var influence = colliderRange.Evaluate(hit.distance);
                    var colDir = colliderRange.sign * (col.bounds.center - transform.position).normalized;
                    outputInfluence = Mathf.Max(outputInfluence, influence);
                    outputDirection += influence * colDir;
                }
            }
        }

        protected void GetGroundAvoidance (out float outputInfluence, out Vector3 outputDirection) {
            outputInfluence = 0f;
            outputDirection = Vector3.zero;
            if(groundCollider == null){
                return;
            }
            var gc = groundCollider;
            var heightRayStartY = gc.bounds.center.y + gc.bounds.extents.y + 0.1f;
            var heightRayStart = new Vector3(transform.position.x, heightRayStartY, transform.position.z);
            var heightRayLength = 2f * gc.bounds.extents.y + 0.2f;
            var heightRay = new Ray(heightRayStart, Vector3.down);
            if(gc.Raycast(heightRay, out var heightHit, heightRayLength)){
                var height = transform.position.y - heightHit.point.y;
                var influence = groundHeightRange.Evaluate(height);
                outputInfluence = Mathf.Max(outputInfluence, influence);
                outputDirection += influence * Vector3.up;
                Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.green);
            }else{
                Debug.DrawRay(heightRayStart, Vector3.down * heightRayLength, Color.red);
            }
            var moveRay = new Ray(transform.position, velocity);
            if(gc.Raycast(moveRay, out var hit, colliderRange.max)){
                var influence = colliderRange.Evaluate(hit.distance);
                outputInfluence = Mathf.Max(outputInfluence, influence);
                outputDirection += influence * hit.normal;
            }
        }
        
    }

}