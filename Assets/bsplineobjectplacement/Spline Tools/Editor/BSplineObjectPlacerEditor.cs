using UnityEngine;
using UnityEditor;

namespace SplineTools {

    [CustomEditor(typeof(BSplineObjectPlacer))]
    public class BSplineObjectPlacerEditor : Editor {

        BSplineObjectPlacer bsop;

        void OnEnable () {
            bsop = target as BSplineObjectPlacer;
        }

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(10);
            if(GUILayout.Button("Delete placed objects")){
                bsop.DeletePlacedObjects();
            }
            if(GUILayout.Button("(Re)Place")){
                bsop.PlaceObjects();
            }
            if(GUILayout.Button("Randomize Seed")){
                bsop.RandomizeSeed();
            }
            GUILayout.Space(10);
            if(GUILayout.Button("Rotate placed objects 90°")){
                bsop.Rotate90Deg();
            }
            if(GUILayout.Button("Reverse direction")){
                bsop.ReverseDirection();
            }
        }

    }

}