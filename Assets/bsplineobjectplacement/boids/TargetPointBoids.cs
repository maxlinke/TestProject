using UnityEngine;

namespace Boids {

    public class TargetPointBoids : PlainBoids {

        [SerializeField] TargetBehaviourParameters targetBehaviour = BehaviourParameters.TargetPointDefault;

        protected override Vector3 GetAdditionalBehavior (int boidIndex) {
            var output = base.GetAdditionalBehavior(boidIndex);
            var target = targetBehaviour.Target;
            if(target != null){
                var activePos = boids[boidIndex].transform.position;
                var toTarget = (target.position - activePos).normalized;
                output += targetBehaviour.Weight * toTarget;
            }
            return output;
        }

        protected override void DrawAdditionalGizmos () {
            base.DrawAdditionalGizmos();
            var target = targetBehaviour.Target;
            if(target != null){
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(target.position, 1f);
            }
        }
        
    }

}