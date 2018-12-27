using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGraphTester : MonoBehaviour {

	public enum CutoutMode {
		REMOVEINSIDE,
		REMOVEOUTSIDE
	}

	[SerializeField] Mesh mesh;
	[SerializeField] MeshFilter mf;
	[SerializeField] MeshCollider mc;

	[Header("Cutting stuff")]
	[SerializeField] KeyCode cutoutKey;
	[SerializeField] CutoutMode cutoutMode;
	[SerializeField] SphereCollider[] colliders;

	void Start () {
		MeshGraph mg = MeshGraph.FromMesh(mesh);
		Mesh reconstructed = mg.ToMesh();
		if(mf != null) mf.sharedMesh = reconstructed;
		if(mc != null) mc.sharedMesh = reconstructed;
	}

	void Update () {
		if(Input.GetKeyDown(cutoutKey)){
			Mesh modified;
			switch(cutoutMode){
				case CutoutMode.REMOVEINSIDE: modified = MeshCutter.RemoveAllVerticesInside(mesh, this.transform, colliders); break;
				case CutoutMode.REMOVEOUTSIDE: modified = MeshCutter.RemoveAllVerticesOutside(mesh, this.transform, colliders); break;
				default: throw new UnityException("Unknown CutoutMode \"" + cutoutMode.ToString() + "\"");
			}
			if(mf != null) mf.sharedMesh = modified;
			if(mc != null) mc.sharedMesh = modified;
		}
	}

	void Reset () {
		mesh = null;
		mf = GetComponent<MeshFilter>();
		mc = GetComponent<MeshCollider>();

		cutoutKey = KeyCode.C;
		cutoutMode = CutoutMode.REMOVEINSIDE;
		colliders = new SphereCollider[0];
	}
}
