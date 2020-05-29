using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaypointBoids : Boids {

    [System.Serializable]
    public class Waypoint {
        public const float MIN_RADIUS = 1f;
        public const float MAX_RADIUS = 100f;
        public const float MIN_SQ_RADIUS = MIN_RADIUS * MIN_RADIUS;
        public const float MAX_SQ_RADIUS = MAX_RADIUS * MAX_RADIUS;

        [SerializeField] Transform point = null;
        [SerializeField] float sqRadius = MIN_SQ_RADIUS;

        public Vector3 Position => point.position;
        public float SqRadius => sqRadius;
        public float Radius => (sqRadius > 0 ? Mathf.Sqrt(sqRadius) : Mathf.Sqrt(-sqRadius));

        public bool IsValid => (point != null);

        public Waypoint (Transform point, float sqRadius) {
            this.point = point;
            this.sqRadius = sqRadius;
        }

        public Waypoint () {
            this.point = null;
            this.sqRadius = MIN_SQ_RADIUS;
        }
    }

    enum LastWaypointMode {
        DESTROY,
        LOOP
    }

    [SerializeField] LastWaypointMode lastWaypointMode = LastWaypointMode.DESTROY;
    [SerializeField, Range(MIN_BEHAVIOR_WEIGHT, MAX_BEHAVIOR_WEIGHT)] float boidWaypointSeekWeight = 1f;
    [SerializeField] List<Waypoint> waypoints;

    List<int> boidWaypoints;
    List<Boid> boidsToDelete;

    public Waypoint this[int index] => waypoints[index];
    public int WaypointCount => (waypoints == null ? 0 : waypoints.Count);

    protected override bool AdditionalPreSpawnCheck (out string message) {
        var output = base.AdditionalPreSpawnCheck(out message);
        if(waypoints == null){
            message += "Waypoints list is null?!?!";
            return false;
        }
        var waypointsValid = true;
        foreach(var waypoint in waypoints){
            waypointsValid &= waypoint.IsValid; 
        }
        if(!waypointsValid){
            message += "Not all waypoints are valid!\n";
        }
        if(WaypointCount < 1){
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
            if(nextWaypointIndex < WaypointCount){
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
        if(WaypointCount < 1){
            return;
        }
        var cc = Gizmos.color;
        var errorCol = new Color(1f - cc.r, 1f - cc.g, 1f - cc.b, cc.a);
        Vector3 last = transform.position;
        bool valid = true;
        foreach(var waypoint in waypoints){
            if(waypoint.IsValid){
                var current = waypoint.Position;
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

    protected override void DrawAdditionalDetailedGizmos () {
        foreach(var waypoint in waypoints){
            if(waypoint.IsValid){
                Gizmos.DrawWireSphere(waypoint.Position, waypoint.Radius);
            }
        }
    }

    private bool IndexCheckAndComplain (int inputIndex) {
        if(inputIndex < 0){
            Debug.LogError("Negative indices are not allowed!");
            return false;
        }
        if(inputIndex >= WaypointCount){
            Debug.LogError("Index out of bounds!");
            return false;
        }
        return true;
    }

    public void RemoveWaypoint (int deleteIndex) {
        if(!IndexCheckAndComplain(deleteIndex)){
            return;
        }
        BuildSafeUndo.RecordObject(this, "Delete point");
        waypoints.RemoveAt(deleteIndex);
    }

    public void AddWaypoint () {
        BuildSafeUndo.RecordObject(this, "Add point");
        if(waypoints == null){
            waypoints = new List<Waypoint>();
        }
        waypoints.Add(new Waypoint(null, Waypoint.MIN_SQ_RADIUS));
    }

    public void MovePointIndex (int startIndex, int delta) {
        if(!IndexCheckAndComplain(startIndex)){
            return;
        }
        BuildSafeUndo.RecordObject(this, "Move point index");
        int endIndex = Mathf.Min(WaypointCount - 1, Mathf.Max(0, startIndex + delta));
        var movePoint = waypoints[startIndex];
        waypoints.RemoveAt(startIndex);
        waypoints.Insert(endIndex, movePoint);
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(WaypointBoids))]
public class WaypointBoidsEditor : BoidsEditor {

    const int DEFAULT_INDEX = -1;
    float INLINE_BUTTON_WIDTH => 20f;
    float INLINE_BUTTON_HEIGHT => EditorGUIUtility.singleLineHeight - 2;

    protected override MonoScript GetCallingScript () => MonoScript.FromMonoBehaviour((WaypointBoids)target);
    protected override System.Type GetCallingType () => typeof(WaypointBoids);

    protected override void DrawAdditionalProperties () {
        base.DrawAdditionalProperties();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lastWaypointMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boidWaypointSeekWeight"));

        var wpArrayProp = serializedObject.FindProperty("waypoints");
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(wpArrayProp.displayName);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        var bgCache = GUI.backgroundColor;
        var errorBGCol = (0.25f * Color.red) + (0.75f * bgCache);
        var boidsScript = target as WaypointBoids;

        if(wpArrayProp.arraySize < 1){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("No waypoints assigned!");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        int removeIndex = DEFAULT_INDEX;
        for(int i=0; i<wpArrayProp.arraySize; i++){
            var wpProp = wpArrayProp.GetArrayElementAtIndex(i);
            var wpTransformProp = wpProp.FindPropertyRelative("point");
            var wpSQRadProp = wpProp.FindPropertyRelative("sqRadius");
            var wp = boidsScript[i];
            Line(i.ToString(), () => {
                GUI.backgroundColor = wp.IsValid ? bgCache : errorBGCol;
                EditorGUILayout.PropertyField(wpTransformProp);
                GUI.backgroundColor = errorBGCol;
                if(GUILayout.Button("X", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
                    removeIndex = i;
                }
                GUI.backgroundColor = bgCache;
            });
            Line(string.Empty, () => {
                var origRadius = wp.Radius;
                var newRadius = EditorGUILayout.Slider("Radius", origRadius, WaypointBoids.Waypoint.MIN_RADIUS, WaypointBoids.Waypoint.MAX_RADIUS);
                wpSQRadProp.floatValue = newRadius * newRadius;
                if(GUILayout.Button("Λ", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
                    boidsScript.MovePointIndex(i, -1);
                }
                if(GUILayout.Button("V", GUILayout.Width(INLINE_BUTTON_WIDTH), GUILayout.Height(INLINE_BUTTON_HEIGHT))){
                    boidsScript.MovePointIndex(i, +1);
                }
            });

            void Line (string labelText, System.Action drawLine) {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelText, GUILayout.Width(24));
                drawLine();
                GUILayout.EndHorizontal();
            }
        }

        if(removeIndex != DEFAULT_INDEX){
            boidsScript.RemoveWaypoint(removeIndex);
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("+", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2))){
            boidsScript.AddWaypoint();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.backgroundColor = bgCache;
    }

}

#endif