using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public class BirdBoid : Boid {

        // settings for this boird
        // placer
        // init with list of ALL others (including this one)
        // static list of all boirds

        // collision avoidance? select colliders only... and only occasionally

        // singleton boird util thingy

        [Header("Visuals")]
        [SerializeField] Animator animator;

        [Header("Traditional Boid Stuff")]
        [SerializeField] BoidRange boidSeparationRange;
        [SerializeField] BoidRange boidAlignmentRange;
        [SerializeField] BoidRange boidCohesionRange;

        bool initialized => boids != null;

        public override void Initialize (List<Boid> boids, Bounds boundingVolume, List<Collider> colliders, TerrainCollider groundCollider) {
            base.Initialize(boids, boundingVolume, colliders, groundCollider);
            animator.Play("bird_boid_flap_medium", 0, Random.value);
            // velocity = transform.forward * 
        }

        void Update () {
            if(!initialized){
                return;
            }

            animator.SetInteger("flapSpeed", 1);
        }
        
    }

}