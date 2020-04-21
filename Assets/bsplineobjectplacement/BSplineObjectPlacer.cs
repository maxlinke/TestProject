using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BSplineObjectPlacer : QuadraticBezierSpline {

    [Header("Settings")]
    [SerializeField] GroundMode groundMode;
    [SerializeField] Collider groundCollider;
    [SerializeField] PlacementMode placementMode;
    [SerializeField] bool flipSides;
    [SerializeField] Vector2 placementRandomness;
    [SerializeField] Vector3 rotationRandomness;

    enum GroundMode {
        DISABLED,
        SNAP,
        SNAP_AND_ALIGN
    }

    enum PlacementMode {
        CENTER,
        SIDE,
        CORNER  // which corner? front or back? how do i know the right corner?
    }

    void Start () {
        
    }

    void Update () {
        
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(BSplineObjectPlacer))]
public class BSplineObjectPlacerEditor : Editor {

    BSplineObjectPlacer bsop;

    void OnEnable () {
        bsop = target as BSplineObjectPlacer;
    }

    protected virtual void OnSceneGUI () {
        bsop.EditorHandles();
    }

}

#endif