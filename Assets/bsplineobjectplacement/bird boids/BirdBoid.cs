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

        // terrain for ground, height above for lower flight ceiling

        [Header("Visuals")]
        [SerializeField] Animator animator;

        [Header("Traditional Boid Stuff")]
        [SerializeField] BoidRange boidSeparationRange;
        [SerializeField] BoidRange boidAlignmentRange;
        [SerializeField] BoidRange boidCohesionRange;

        // void Ini () {
        //     velocity = transform.forward * 
        // }

        void Update () {
            
        }
        
    }

}