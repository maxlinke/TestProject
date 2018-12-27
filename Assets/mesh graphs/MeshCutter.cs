using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutter {

	public static Mesh RemoveAllVerticesInside (Mesh mesh, Transform meshTransform, SphereCollider[] colliders) {
		return RemoveVerts(mesh, meshTransform, colliders, true);
	}

	public static Mesh RemoveAllVerticesOutside (Mesh mesh, Transform meshTransform, SphereCollider[] colliders) {
		return RemoveVerts(mesh, meshTransform, colliders, false);
	}

	private static Mesh RemoveVerts (Mesh mesh, Transform meshTransform, SphereCollider[] colliders, bool removeInside) {
		MeshGraph mg = MeshGraph.FromMesh(mesh);
		MeshGraph.GraphVertex[] vertsInMesh = mg.GetVertices();
		List<MeshGraph.GraphVertex> vertsToRemove = new List<MeshGraph.GraphVertex>();
		for(int i=0; i<vertsInMesh.Length; i++){
			Vector3 worldPos = meshTransform.TransformPoint(vertsInMesh[i].position);
			for(int j=0; j<colliders.Length; j++){
				Vector3 lossyScale = colliders[j].transform.lossyScale;
				float transformScale = Mathf.Max(Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)), Mathf.Abs(lossyScale.z));
				float actualRadius = colliders[j].radius * transformScale;
				bool vertexIsInside = ((worldPos - colliders[j].transform.position).sqrMagnitude < (actualRadius * actualRadius));
				if(vertexIsInside && removeInside){
					vertsToRemove.Add(vertsInMesh[i]);
					break;
				}else if(!vertexIsInside && !removeInside){
					vertsToRemove.Add(vertsInMesh[i]);
					break;
				}
			}
		}
		for(int i=0; i<vertsToRemove.Count; i++){
			mg.RemoveVertex(vertsToRemove[i]);
		}
		return mg.ToMesh();
	}

}
