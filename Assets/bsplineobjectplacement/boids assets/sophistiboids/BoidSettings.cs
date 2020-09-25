using UnityEngine;

namespace SophistiBoids {

    [CreateAssetMenu(menuName = "Boids/BoidSettings", fileName = "New Boid Settings")]
    public class BoidSettings : ScriptableObject {

        [Header("Movement")]
        [SerializeField] float maxSpeed = default;
        [SerializeField] float maxAccel = default;

        [Header("Ranges")]
        [SerializeField] BoidRange boidSeparationRange = default;
        [SerializeField] BoidRange boidAlignmentRange = default;
        [SerializeField] BoidRange boidCohesionRange = default;

        public float MaxSpeed => maxSpeed;
        public float MaxAccel => maxAccel;

        public BoidRange BoidSeparationRange => boidSeparationRange;
        public BoidRange BoidAlignmentRange => boidAlignmentRange;
        public BoidRange BoidCohesionRange => boidCohesionRange;
        
    }

}