using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public static class NavMeshExportUtility {

    [MenuItem("NavMeshExport/Save Current As Mesh Asset")]
    static void SaveCurrentAsMeshAsset () {
        var triangulation = NavMesh.CalculateTriangulation();
        if(triangulation.vertices == null || triangulation.vertices.Length < 1){
            EditorUtility.DisplayDialog("Error", "No NavMesh to triangulate!", "OK");
            return;
        }
        var mesh = new Mesh(){
            vertices = triangulation.vertices,
            triangles = triangulation.indices
        };
        var path = EditorUtility.SaveFilePanelInProject("Save Triangulation", "navMesh", "asset", string.Empty);
        if(!string.IsNullOrWhiteSpace(path)){
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = mesh;
            EditorGUIUtility.PingObject(mesh);
        }

    }

}
