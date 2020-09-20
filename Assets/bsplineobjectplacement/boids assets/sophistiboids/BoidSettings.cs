using UnityEngine;

namespace SophistiBoids {

    [CreateAssetMenu(menuName = "Boids/BoidSettings", fileName = "New Boid Settings")]
    public class BoidSettings : ScriptableObject {

        [Header("Movement")]
        [SerializeField] float maxSpeed;
        [SerializeField] float maxAccel;

        [Header("Ranges")]
        [SerializeField] BoidRange boidSeparationRange;
        [SerializeField] BoidRange boidAlignmentRange;
        [SerializeField] BoidRange boidCohesionRange;

        public float MaxSpeed => maxSpeed;
        public float MaxAccel => maxAccel;

        public BoidRange BoidSeparationRange => boidSeparationRange;
        public BoidRange BoidAlignmentRange => boidAlignmentRange;
        public BoidRange BoidCohesionRange => boidCohesionRange;
        
    }

}