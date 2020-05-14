using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools {

    [CreateAssetMenu(menuName = "BSplineObjectPlacer/Structured Mesh Pool", fileName = "New StructuredMeshPool")]
    public class StructuredMeshPool : MeshPool {

        [SerializeField] StructuredPoolObject[] objects;

        public StructuredPoolObject this[int index] => objects[index];
        public override int ObjectCount => objects.Length;

        int nextIndex;

        protected override void Init () {
            base.Init();
            nextIndex = 0;
        }

        protected override void DeInit () {
            base.DeInit();
            nextIndex = 0;
        }

        protected override SplineObject GetNext (Vector3 measureAxis, System.Random rng) {
            if(objects == null || objects.Length <= 0){
                return null;
            }
            var spo = objects[nextIndex];
            var output = InstantiatePrefabAndMeasureSize(spo.Prefab, spo.Material, measureAxis, spo.UseBoxColliderForSpacing);
            nextIndex = (nextIndex + 1) % ObjectCount;
            return output;
        }

        [System.Serializable] 
        public class StructuredPoolObject {

            [SerializeField] GameObject prefab;
            [SerializeField] Material material;
            [SerializeField] bool useBoxColliderForSpacing;

            public GameObject Prefab => prefab;
            public Material Material => material;
            public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;

        }

    }

}