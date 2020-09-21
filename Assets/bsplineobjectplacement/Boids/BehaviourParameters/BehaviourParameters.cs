using UnityEngine;
using WaypointMode = Boids.WaypointBoids.LastWaypointMode;

namespace Boids {

    [System.Serializable]
    public abstract class BehaviourParameters {

        public const float MIN_WEIGHT = -2f;
        public const float MAX_WEIGHT = 2f;

        [SerializeField, Range(MIN_WEIGHT, MAX_WEIGHT)] protected float m_weight = 0f;

        public float Weight => m_weight;

        public static DistancedBehaviourParameters CohesionDefault   => new DistancedBehaviourParameters(7f, 0.5f);
        public static DistancedBehaviourParameters AlignmentDefault  => new DistancedBehaviourParameters(5f, 0.2f);
        public static DistancedBehaviourParameters SeparationDefault => new DistancedBehaviourParameters(1f, 1.5f);
        
        public static TimedBehaviourParameters RandomDirectionDefault => new TimedBehaviourParameters(5f, 0.667f);
        public static TargetBehaviourParameters TargetPointDefault => new TargetBehaviourParameters(null, 2f);
        public static WaypointBehaviourParameters WaypointDefault => new WaypointBehaviourParameters(WaypointMode.DestroyAtEnd, 1f);
    }

    [System.Serializable]
    public class DistancedBehaviourParameters : BehaviourParameters {
        
        [SerializeField, Unit("m", 20f)] float m_distance = 0f;

        public float Distance  => m_distance;

        public DistancedBehaviourParameters (float distance, float weight) {
            this.m_distance = distance;
            this.m_weight = weight;
        }

    }

    [System.Serializable]
    public class TimedBehaviourParameters : BehaviourParameters {
        
        [SerializeField, Unit("s", 20f)] float m_interval = 0f;

        public float Interval => m_interval;

        public TimedBehaviourParameters (float interval, float weight) {
            this.m_interval = interval;
            this.m_weight = weight;
        }
        
    }

    [System.Serializable]
    public class TargetBehaviourParameters : BehaviourParameters {

        [SerializeField] Transform m_target = default;

        public Transform Target => m_target;

        public TargetBehaviourParameters (Transform target, float weight) {
            this.m_target = target;
            this.m_weight = weight;
        }

    }

    [System.Serializable]
    public class WaypointBehaviourParameters : BehaviourParameters {

        [SerializeField] WaypointMode m_mode = default;

        public WaypointMode Mode => m_mode;

        public WaypointBehaviourParameters (WaypointMode mode, float weight) {
            this.m_mode = mode;
            this.m_weight = weight;
        }

    }

}