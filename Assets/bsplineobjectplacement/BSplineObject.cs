using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "BSplineObjectPlacer/BSplineObject", fileName = "New Object")]
public class BSplineObject : ScriptableObject {

    [SerializeField] GameObject prefab;
    [SerializeField] Material[] materials;
    [SerializeField] bool useBoxColliderForSpacing;

    public GameObject Prefab => prefab;
    public Material this[int index] => materials[index];
    public int MaterialCount => materials.Length;
    public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;

    public Material RandomMaterial (System.Random rng = null) {
        if(materials == null || materials.Length <= 0){
            return null;
        }
        if(rng == null){
            return materials[Random.Range(0, MaterialCount)];
        }else{
            return materials[rng.Next(0, MaterialCount)];
        }
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(BSplineObject))]
public class BSplineObjectEditor : Editor {

    BSplineObject bso;

    void OnEnable () {
        bso = target as BSplineObject;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        var boxCols = bso.Prefab.GetComponents<BoxCollider>();
        bool useBoxCol = serializedObject.FindProperty("useBoxColliderForSpacing").boolValue;
        if(useBoxCol && boxCols.Length != 1){
            EditorGUILayout.Space();
            if(boxCols.Length < 1){
                EditorGUILayout.LabelField("No BoxCollider on object, even though it is marked to be used!");
            }else{
                EditorGUILayout.LabelField("Multiple BoxColliders on object, this isn't supported!");
            }
        }else if(!useBoxCol && boxCols.Length == 1){
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Do you want to use the attached BoxCollider for spacing?");
        }
    }

}

#endif