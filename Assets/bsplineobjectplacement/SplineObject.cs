using UnityEngine;

namespace SplineTools {

    public class SplineObject {

        public GameObject Prefab { get; private set; }
        public Material Material { get; private set; }
        public bool UseBoxColliderForSpacing { get; private set; }

        public SplineObject (GameObject prefab, Material material, bool useBoxColliderForSpacing) {
            this.Prefab = prefab;
            this.Material = material;
            this.UseBoxColliderForSpacing = useBoxColliderForSpacing;
        }
    
    }

}