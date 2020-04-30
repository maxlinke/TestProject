using UnityEngine;

namespace SplineTools {

    public class SplineObject {

        public GameObject SpawnedObject { get; private set; }
        public float LinearSize { get; private set; }

        public SplineObject (GameObject spawnedObject, float linearSize) {
            this.SpawnedObject = spawnedObject;
            this.LinearSize = linearSize;
        }
    
    }

}