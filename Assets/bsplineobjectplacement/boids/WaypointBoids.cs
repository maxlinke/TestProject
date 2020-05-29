using System.Collections.Generic;
using UnityEngine;

public class WaypointBoids : Boids {

    [System.Serializable]
    public class Waypoint {
        public const float MIN_SQ_RADIUS = 1f;
        public const float MAX_SQ_RADIUS = 10000f;

        [SerializeField] Transform point;
        [SerializeField] float sqRadius;

        public Vector3 Position => point.position;
        public float SqRadius => sqRadius;
        public float Radius => (sqRadius > 0 ? Mathf.Sqrt(sqRadius) : Mathf.Sqrt(-sqRadius));

        public bool IsValid => (point != null);
    }

    enum LastWaypointMode {
        DESTROY,
        LOOP
    }

    [SerializeField] LastWaypointMode lastWaypointMode;
    [SerializeField, Range(MIN_BEHAVIOR_WEIGHT, MAX_BEHAVIOR_WEIGHT)] float boidWaypointSeekWeight;
    [SerializeField] Waypoint[] waypoints;

    List<int> boidWaypoints;
    List<Boid> boidsToDelete;

    protected override bool AdditionalPreSpawnCheck (out string message) {
        var output = base.AdditionalPreSpawnCheck(out message);
        var waypointsValid = true;
        foreach(var waypoint in waypoints){
            waypointsValid &= waypoint.IsValid; 
        }
        if(!waypointsValid){
            message += "Not all waypoints are valid!\n";
        }
        if(waypoints.Length < 1){
            message += "No waypoints assigned!";
            waypointsValid = false;
        }
        return output && waypointsValid;
    }

    protected override void AdditionalPreSpawnSetup () {
        boidWaypoints = new List<int>();
        boidsToDelete = new List<Boid>();
    }

    protected override void OnAdditionalBoidAdded (Boid newBoid) {
        boidWaypoints.Add(0);
    }

    protected override void OnUpdateFinished () {
        if(boidsToDelete.Count > 0){
            for(int i=boidsToDelete.Count-1; i>=0; i--){
                var boid = boidsToDelete[i];
                int boidIndex = boids.IndexOf(boid);
                boids.RemoveAt(boidIndex);
                boidAccelerations.RemoveAt(boidIndex);
                boidWaypoints.RemoveAt(boidIndex);
                Destroy(boid.transform.gameObject);
            }
            boidsToDelete.Clear();
        }
    }

    protected override Vector3 GetAdditionalBehavior (int boidIndex) {
        var output = base.GetAdditionalBehavior(boidIndex);
        var boid = boids[boidIndex];
        int waypointIndex;
        Waypoint waypoint;
        Vector3 toWaypoint;
        float sqDist;
        UpdateWaypointData();
        if(sqDist < waypoint.SqRadius){
            var nextWaypointIndex = (waypointIndex + 1);
            if(nextWaypointIndex < waypoints.Length){
                boidWaypoints[boidIndex] = nextWaypointIndex;
                UpdateWaypointData();
            }else{
                switch(lastWaypointMode){
                    case LastWaypointMode.DESTROY:
                        boidsToDelete.Add(boid);
                        break;
                    case LastWaypointMode.LOOP:
                        boidWaypoints[boidIndex] = 0;
                        UpdateWaypointData();
                        break;
                    default:
                        Debug.LogError($"Unknown {nameof(LastWaypointMode)} \"{lastWaypointMode}\"!");
                        return output;
                }
            }            
        }
        output += toWaypoint.normalized * boidWaypointSeekWeight;
        return output;

        void UpdateWaypointData () {
            waypointIndex = boidWaypoints[boidIndex];
            waypoint = waypoints[waypointIndex];
            toWaypoint = waypoint.Position - boid.transform.position;
            sqDist = toWaypoint.sqrMagnitude;
        }
    }

    protected override void DrawAdditionalGizmos () {
        if(waypoints.Length < 1){
            return;
        }
        var cc = Gizmos.color;
        var errorCol = new Color(1f - cc.r, 1f - cc.g, 1f - cc.b, cc.a);
        Vector3 last = transform.position;
        bool valid = true;
        foreach(var waypoint in waypoints){
            if(waypoint.IsValid){
                var current = waypoint.Position;
                Gizmos.DrawWireSphere(current, waypoint.Radius);
                Gizmos.color = (valid ? cc : errorCol);
                Gizmos.DrawLine(last, current);
                Gizmos.color = cc;
                last = current;
                valid = true;
            }else{
                valid = false;
            }
        }
        if(lastWaypointMode == LastWaypointMode.LOOP && waypoints[0].IsValid){
            Gizmos.color = (valid ? cc : errorCol);
            Gizmos.DrawLine(last, waypoints[0].Position);
        }
        Gizmos.color = cc;
    }
	
}
