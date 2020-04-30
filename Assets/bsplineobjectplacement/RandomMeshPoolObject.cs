using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools {

    [CreateAssetMenu(menuName = "BSplineObjectPlacer/MeshPoolObject", fileName = "New RandomMeshPoolObject")]
    public class RandomMeshPoolObject : ScriptableObject {        // TODO also abstract. this is pretty specifically a MESH object

        public const string NO_BOXCOLLS_WARNING = "No BoxCollider on object, even though it is marked to be used!";
        public const string TOO_MANY_BOXCOLLS_WARNING = "Multiple BoxColliders on object, this isn't supported!";

        [SerializeField] GameObject prefab;
        [SerializeField] Material[] materials;
        [SerializeField] bool useBoxColliderForSpacing;

        public GameObject Prefab => prefab;
        public Material this[int index] => materials[index];
        public int MaterialCount => materials.Length;

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
        
        public GameObject CreateSelfAndMeasureSize (Material newMaterial, Vector3 measureAxis, out float linearSize) {
            var newGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, null);   // do i really want the zero pos and identity rotation?
            if(newMaterial != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.sharedMaterial = newMaterial;
            }
            BoxCollider col = null; 
            if(useBoxColliderForSpacing){
                var boxColliders = newGO.GetComponents<BoxCollider>();
                if(boxColliders == null || boxColliders.Length <= 0){
                    Debug.LogError(NO_BOXCOLLS_WARNING);
                }else if(boxColliders.Length > 1){
                    Debug.LogError(TOO_MANY_BOXCOLLS_WARNING);
                }else{
                    col = boxColliders[0];
                }
            }
            if(col != null){
                var worldColCenter = newGO.transform.TransformPoint(col.center);
                var worldColSize = newGO.transform.TransformVector(col.size);
                var worldRD = -measureAxis;
                var worldRO = worldColCenter - worldRD * worldColSize.magnitude * 2f;
                if(col.Raycast(new Ray(worldRO, worldRD), out var colRayHit, 2f * worldColSize.magnitude)){
                    linearSize = 2f * (worldColCenter - colRayHit.point).magnitude;
                }else{
                    Debug.LogError("No Hit! WHAT?!!?!?");
                    linearSize = float.NaN;
                }
            }else{
                var newGOMF = newGO.GetComponent<MeshFilter>();
                var bounds = newGOMF.sharedMesh.bounds;
                var localDir = newGO.transform.InverseTransformVector(measureAxis).normalized;
                var boundsRO = bounds.center + (localDir * bounds.extents.magnitude * 2f);
                if(bounds.IntersectRay(new Ray(boundsRO, -localDir), out float boundsHitDist)){
                    var boundsHit = boundsRO - localDir * boundsHitDist;
                    var worldBoundsCenter = newGO.transform.TransformPoint(bounds.center);
                    var worldBoundsHit = newGO.transform.TransformPoint(boundsHit);
                    linearSize = 2f * (worldBoundsCenter - worldBoundsHit).magnitude;
                }else{
                    Debug.LogError("No Hit! WHAT?!!?!?");
                    linearSize = float.NaN;
                }
            }
            return newGO;
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
                    EditorGUILayout.LabelField(RandomMeshPoolObject.NO_BOXCOLLS_WARNING);
                }else{
                    EditorGUILayout.LabelField(RandomMeshPoolObject.TOO_MANY_BOXCOLLS_WARNING);
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