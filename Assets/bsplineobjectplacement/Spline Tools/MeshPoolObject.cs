using UnityEngine;

namespace SplineTools {

    [System.Serializable] 
    public class MeshPoolObject {

        [SerializeField] GameObject prefab = default;
        [SerializeField, Range(1, 10)] int occurences = default;
        [SerializeField] bool useBoxColliderForSpacing = default;
        [SerializeField] Material[] materials = default;
        [SerializeField] bool useCustomBounds = default;
        [SerializeField] Vector3 customBoundsCenter = Vector3.zero;
        [SerializeField] Vector3 customBoundsSize = Vector3.one;

        public GameObject Prefab => prefab;
        public int MaterialCount => materials.Length;
        public Material Material (int i) => materials[i];
        public int Occurences => occurences;
        public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;
        public bool UseCustomBounds => useCustomBounds;
        public Bounds CustomBounds => new Bounds(customBoundsCenter, customBoundsSize);

        public Material RandomMaterial (System.Random rng) {
            if(MaterialCount > 0){
                return materials[(rng != null) ? rng.Next(0, MaterialCount) : Random.Range(0, MaterialCount)];
            }
            rng?.Next();
            return null;
        }

        public MeshPoolObject () {
            occurences = 1;
        }

        public bool CanUseBoxCollForSpacing (out BoxCollider col, out string message) {
            return CanUseBoxCollForSpacing(prefab, out col, out message);
        }

        public static bool CanUseBoxCollForSpacing (GameObject prefab, out BoxCollider col, out string message) {
            if(prefab == null){
                col = null;
                message = "The GameObject is null!";
                return false;
            }
            var boxColliders = prefab.GetComponents<BoxCollider>();
            message = string.Empty;
            col = null;
            if(boxColliders == null || boxColliders.Length <= 0){
                message = "No BoxCollider on object!";
            }else if(boxColliders.Length > 1){
                message = "Multiple BoxColliders on object, this isn't supported!";
            }else{
                col = boxColliders[0];
            }
            return col != null;
        }

    }

}