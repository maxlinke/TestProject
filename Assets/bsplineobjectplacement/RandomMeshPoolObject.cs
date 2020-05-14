using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    [CreateAssetMenu(menuName = "BSplineObjectPlacer/MeshPoolObject", fileName = "New RandomMeshPoolObject")]
    public class RandomMeshPoolObject : ScriptableObject {        // TODO also abstract. this is pretty specifically a MESH object

        [SerializeField] GameObject prefab;
        [SerializeField] Material[] materials;
        [SerializeField] bool useBoxColliderForSpacing;

        public GameObject Prefab => prefab;
        public Material this[int index] => materials[index];
        public int MaterialCount => materials.Length;
        public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;

        public Material RandomMaterial (System.Random rng = null) {
            if(materials == null || MaterialCount <= 0){
                rng?.Next();        // for consistency
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

    [CustomEditor(typeof(RandomMeshPoolObject))]
    public class RandomMeshPoolObjectEditor : Editor {

        RandomMeshPoolObject bso;

        void OnEnable () {
            bso = target as RandomMeshPoolObject;
        }

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            if(bso.Prefab == null){
                Prefix();
                EditorGUILayout.LabelField("No prefab assigned!");
                return;
            }
            var boxCols = bso.Prefab.GetComponents<BoxCollider>();
            bool useBoxCol = serializedObject.FindProperty("useBoxColliderForSpacing").boolValue;
            if(useBoxCol && boxCols.Length != 1){
                Prefix();
                if(boxCols.Length < 1){
                    EditorGUILayout.LabelField(MeshPool.NO_BOXCOLLS_WARNING);
                }else{
                    EditorGUILayout.LabelField(MeshPool.TOO_MANY_BOXCOLLS_WARNING);
                }
            }else if(!useBoxCol && boxCols.Length == 1){
                Prefix();
                EditorGUILayout.LabelField("Do you want to use the attached BoxCollider for spacing?");
            }

            void Prefix () {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("WARNING!!!");
            }
        }

    }

    #endif

}