using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineTools{

    public abstract class MeshPool : ObjectPool {

        public const string NO_BOXCOLLS_WARNING = "No BoxCollider on object, even though it is marked to be used!";
        public const string TOO_MANY_BOXCOLLS_WARNING = "Multiple BoxColliders on object, this isn't supported!";

        protected SplineObject InstantiatePrefabAndMeasureSize (GameObject prefab, Material material, Vector3 measureAxis, bool useBoxColliderForSpacing) {
            var newGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, null);   // do i really want the zero pos and identity rotation?
            float linearSize;
            if(material != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.sharedMaterial = material;
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
                if(newGOMF == null){
                    newGOMF = newGO.GetComponentInChildren<MeshFilter>();
                }
                if(newGOMF == null){
                    Debug.LogError("No MeshFilter to measure the size on!");
                    linearSize = float.NaN;
                }else{
                    var bounds = newGOMF.sharedMesh.bounds;
                    var localDir = newGOMF.transform.InverseTransformVector(measureAxis).normalized;
                    var boundsRO = bounds.center + (localDir * bounds.extents.magnitude * 2f);
                    if(bounds.IntersectRay(new Ray(boundsRO, -localDir), out float boundsHitDist)){
                        var boundsHit = boundsRO - localDir * boundsHitDist;
                        var worldBoundsCenter = newGOMF.transform.TransformPoint(bounds.center);
                        var worldBoundsHit = newGOMF.transform.TransformPoint(boundsHit);
                        linearSize = 2f * (worldBoundsCenter - worldBoundsHit).magnitude;
                    }else{
                        Debug.LogError("No Hit! WHAT?!!?!?");
                        linearSize = float.NaN;
                    }
                }
            }
            return new SplineObject(newGO, linearSize);
        }
        
    }

}