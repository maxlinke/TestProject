using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SplineTools{

    [CreateAssetMenu(menuName = "Spline Tools/Mesh Pool", fileName = "New MeshPool")]
    public class MeshPool : ObjectPool {

        private const int MAX_DUPLICATE_AVOIDANCE_ITERATIONS = 32;

        [SerializeField] OutputType outputType;
        [SerializeField] RandomRepetitionType randomRepetitionType;
        [SerializeField] MeshPoolObject[] objects;

        public override int ObjectCount => (objects == null) ? 0 : objects.Length;
        public MeshPoolObject this[int index] => objects[index];

        List<MeshPoolObject> occurenceList;
        int nextIndex;
        MeshPoolObject lastPO;
        Material lastMat;

        public enum OutputType {
            ORDERED,
            RANDOM
        }

        public enum RandomRepetitionType {
            ALLOW,
            AVOID_DUPLICATE_OBJECT,
            AVOID_DUPLICATE_APPEARANCE
        }

        protected override void Init () {
            base.Init();
            nextIndex = 0;
            lastPO = null;
            lastMat = null;
            FillOccurenceList();
        }

        protected override void DeInit () {
            base.DeInit();
            occurenceList.Clear();
        }

        protected override SplineObject GetNext (Vector3 measureAxis, System.Random rng) {
            if(objects == null || objects.Length <= 0){
                return null;
            }
            MeshPoolObject po;
            Material mat;
            switch(outputType){
                case OutputType.ORDERED:
                    po = objects[nextIndex];
                    mat = po.RandomMaterial(rng);
                    break;
                case OutputType.RANDOM:
                    po = null;
                    mat = null;
                    for(int i=0; i<MAX_DUPLICATE_AVOIDANCE_ITERATIONS; i++){
                        var selectIndex = (rng != null) ? rng.Next(0, occurenceList.Count) : Random.Range(0, occurenceList.Count);
                        var validSelection = false;
                        po = occurenceList[selectIndex];
                        mat = po.RandomMaterial(rng);
                        var samePO = po == lastPO;
                        var sameMat = mat == lastMat;
                        switch(randomRepetitionType){
                            case RandomRepetitionType.ALLOW:
                                validSelection = true;
                                break;
                            case RandomRepetitionType.AVOID_DUPLICATE_APPEARANCE:
                                validSelection = !(samePO && sameMat);
                                break;
                            case RandomRepetitionType.AVOID_DUPLICATE_OBJECT:
                                validSelection = !samePO;
                                break;
                            default:
                                Debug.LogError($"Unknown {nameof(RandomRepetitionType)} \"{randomRepetitionType}\"!");
                                validSelection = true;
                                break;
                        }
                        if(validSelection){
                            break;
                        }
                    }
                    break;
                default:
                    Debug.LogError($"Unknown {typeof(OutputType)} \"{outputType}\"!");
                    return null;
            }
            var output = InstantiatePrefabAndMeasureSize(po.Prefab, mat, measureAxis, po.UseBoxColliderForSpacing);
            nextIndex = (nextIndex + 1) % ObjectCount;
            lastPO = po;
            lastMat = mat;
            return output;
        }

        void FillOccurenceList () {
            if(occurenceList != null && occurenceList.Count > 0){
                Debug.LogWarning("List was called to be filled, even though it wasn't empty. Code will run fine but this shouldn't happen!");
                return;
            }
            if(occurenceList == null){
                occurenceList = new List<MeshPoolObject>();
            }
            List<string> nullPrefabs = new List<string>();
            for(int i=0; i<objects.Length; i++){
                if(objects[i].Prefab == null){
                    nullPrefabs.Add($"Entry #{i}");
                }else{
                    for(int j=0; j<objects[i].Occurences; j++){
                        occurenceList.Add(objects[i]);
                    }
                }
            }
            if(nullPrefabs.Count > 0){
                string problemString = $"{nameof(ObjectPool)} \"{this.name}\" contains {nullPrefabs.Count} Prefab(s) that are null";
                for(int i=0; i<nullPrefabs.Count; i++){
                    problemString += $"\n   - {nullPrefabs[i]}";
                }
                Debug.LogError(problemString);
            }
        }

        public bool CanUseBoxCollForSpacing (GameObject inputGO, out BoxCollider col, out string message) {
            if(inputGO == null){
                col = null;
                message = "The GameObject is null!";
                return false;
            }
            var boxColliders = inputGO.GetComponents<BoxCollider>();
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

        protected SplineObject InstantiatePrefabAndMeasureSize (GameObject prefab, Material material, Vector3 measureAxis, bool useBoxColliderForSpacing) {
            var newGO = Instantiate(prefab, Vector3.zero, Quaternion.identity, null);   // do i really want the zero pos and identity rotation?
            float linearSize;
            if(material != null){
                var newGOMR = newGO.GetComponent<MeshRenderer>();
                newGOMR.sharedMaterial = material;
            }
            var canUseBoxColliderForSpacing = CanUseBoxCollForSpacing(newGO, out var col, out var msg);
            if(useBoxColliderForSpacing && !canUseBoxColliderForSpacing){
                Debug.LogError(msg);
            }
            if(useBoxColliderForSpacing && canUseBoxColliderForSpacing){
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

        public void DeleteObject (int deleteIndex) {
            var tempList = new List<MeshPoolObject>();
            for(int i=0; i<ObjectCount; i++){
                if(i == deleteIndex){
                    continue;
                }
                tempList.Add(objects[i]);
            }
            objects = tempList.ToArray();
        }

        public void AddObject () {
            var tempList = new List<MeshPoolObject>();
            for(int i=0; i<ObjectCount; i++){
                tempList.Add(objects[i]);
            }
            tempList.Add(new MeshPoolObject());
            objects = tempList.ToArray();
        }

        [System.Serializable] 
        public class MeshPoolObject {

            [SerializeField] GameObject prefab;
            [SerializeField, Range(1, 10)] int occurences;
            [SerializeField] bool useBoxColliderForSpacing;
            [SerializeField] Material[] materials;

            public GameObject Prefab => prefab;
            public int MaterialCount => materials.Length;
            public Material Material (int i) => materials[i];
            public int Occurences => occurences;
            public bool UseBoxColliderForSpacing => useBoxColliderForSpacing;

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

        }
        
    }

    // TODO in editor: when boxcolls not possible, set flag to false, gui.disable the field and add a tooltip

    #if UNITY_EDITOR

    [CustomEditor(typeof(MeshPool))]
    public class MeshPoolEditor : Editor {

        MeshPool mp;

        void OnEnable () {
            mp = target as MeshPool;
        }

        public override void OnInspectorGUI () {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(mp), typeof(MeshPool), false);
            GUI.enabled = true;

            var otProp = serializedObject.FindProperty("outputType");
            EditorGUILayout.PropertyField(otProp);
            if(((MeshPool.OutputType)(otProp.enumValueIndex)) == MeshPool.OutputType.RANDOM){
                var rrtProp = serializedObject.FindProperty("randomRepetitionType");
                EditorGUILayout.PropertyField(rrtProp);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Objects");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            var listProp = serializedObject.FindProperty("objects");
            int deleteIndex = -1;

            for(int i=0; i<mp.ObjectCount; i++){
                var poProp = listProp.GetArrayElementAtIndex(i);

                var prefabProp = poProp.FindPropertyRelative("prefab");
                InsetLine(1, i.ToString(), () => {
                    EditorGUILayout.PropertyField(prefabProp);
                    var bgCache = GUI.backgroundColor;
                    GUI.backgroundColor = (0.25f * Color.red) + (0.75f * bgCache);
                    if(GUILayout.Button("X", GUILayout.Width(20))){
                        deleteIndex = i;
                    }
                    GUI.backgroundColor = bgCache;
                });

                var occProp = poProp.FindPropertyRelative("occurences");
                InsetLine(1, string.Empty, () => {
                    var occVal = occProp.intValue;
                    if(occVal < 1){
                        occProp.intValue = 1;
                    }
                    EditorGUILayout.PropertyField(occProp);
                });

                var bcProp = poProp.FindPropertyRelative("useBoxColliderForSpacing");
                if(!mp.CanUseBoxCollForSpacing(mp[i].Prefab, out _, out var boxColMsg)){
                    GUI.enabled = false;
                    bcProp.boolValue = false;
                }
                InsetLine(1, string.Empty, () => {
                    EditorGUILayout.PropertyField(bcProp);
                    GUILayout.Label(boxColMsg);
                });
                GUI.enabled = true;

                var matProp = poProp.FindPropertyRelative("materials");
                InsetLine(1, string.Empty, () => {EditorGUILayout.PropertyField(matProp, true);});

                Separator();

                void InsetLine (int insetLevel, string label, System.Action drawLine) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label, GUILayout.Width(insetLevel * 20));
                    drawLine();
                    GUILayout.EndHorizontal();
                }
            }

            void Separator () {
                var guiCache = GUI.enabled;
                GUI.enabled = false;
                GUILayout.Box(string.Empty, GUILayout.Height(2), GUILayout.Width(EditorGUIUtility.currentViewWidth-30));
                GUI.enabled = guiCache;
            }

            if(deleteIndex != -1){
                Undo.RecordObject(mp, "Delete array element");
                mp.DeleteObject(deleteIndex);
            }

            if(GUILayout.Button("+")){
                Undo.RecordObject(mp, "Add array element");
                mp.AddObject();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

    #endif

}