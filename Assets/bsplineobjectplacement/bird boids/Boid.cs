using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids {

    public abstract class Boid : MonoBehaviour {

        public Vector3 position => transform.position;
        public Vector3 velocity { get; protected set; }

        
        
    }

}