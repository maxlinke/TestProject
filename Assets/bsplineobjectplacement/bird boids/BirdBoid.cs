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

        bool initialized => settings != null;

        public override void Initialize (List<Boid> boids, BoidSettings settings, Bounds boundingVolume, List<Collider> colliders, TerrainCollider groundCollider) {
            base.Initialize(boids, settings, boundingVolume, colliders, groundCollider);
            animator.Play("bird_boid_flap_medium", 0, Random.value);
            animator.SetBool("isFlying", true);
            velocity = transform.forward * settings.MaxSpeed;
            // initialized = true;
        }

        void Update () {
            if(!initialized){
                return;
            }
            GetTraditionalBoidsBehavior(out var boidsInfluence, out var boidsDirection);
            GetBoundingVolumeAvoidance(out var boundsInfluence, out var boundsDirection);

            velocity += boundsInfluence * settings.MaxAccel * Time.deltaTime * boundsDirection;
            velocity += (1f - boundsInfluence) * settings.MaxAccel * Time.deltaTime * boidsDirection;
            velocity = velocity.normalized * settings.MaxSpeed;

            transform.position += velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
            animator.SetInteger("flapSpeed", 1);
        }
        
    }

}