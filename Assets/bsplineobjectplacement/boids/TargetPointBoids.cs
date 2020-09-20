using UnityEngine;

public class TargetPointBoids : Boids {

    [SerializeField] Transform boidTarget = null;
    [SerializeField, Range(MIN_BEHAVIOR_WEIGHT, MAX_BEHAVIOR_WEIGHT)] float boidTargetSeekWeight = 2f;

    protected override Vector3 GetAdditionalBehavior (int boidIndex) {
        var output = base.GetAdditionalBehavior(boidIndex);
        if(boidTarget != null){
            var activePos = boids[boidIndex].transform.position;
            var toTarget = (boidTarget.position - activePos).normalized;
            output += boidTargetSeekWeight * toTarget;
        }
        return output;
    }

    protected override void DrawAdditionalGizmos () {
        base.DrawAdditionalGizmos();
        if(boidTarget != null){
            Gizmos.DrawLine(transform.position, boidTarget.position);
            Gizmos.DrawWireSphere(boidTarget.position, 1f);
        }
    }
	
}