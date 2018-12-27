using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGraphTester : MonoBehaviour {

	[SerializeField] Mesh mesh;
	[SerializeField] MeshFilter mf;
	[SerializeField] MeshCollider mc;

	void Start () {
		MeshGraph mg = MeshGraph.FromMesh(mesh);
		Mesh reconstructed = mg.ToMesh();
		if(mf != null) mf.sharedMesh = reconstructed;
		if(mc != null) mc.sharedMesh = reconstructed;
	}

	void Reset () {
		mesh = null;
		mf = GetComponent<MeshFilter>();
		mc = GetComponent<MeshCollider>();
	}
}
