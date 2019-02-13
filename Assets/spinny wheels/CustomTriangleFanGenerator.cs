using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTriangleFanGenerator : MonoBehaviour {

	[Header("Components")]
	[SerializeField] MeshFilter mf;

	[Header("Settings")]
	[SerializeField, Range(3, 24)] int numberOfSlices;
	[SerializeField] float radius;
	[SerializeField] float inlineRadius;

	[SerializeField] bool executeOnStart;
	[SerializeField] bool drawGizmos;

	Mesh generatedMesh;

	void Start () {
		if(executeOnStart) GenerateAndApply();
	}

	void OnDrawGizmos () {
		if(drawGizmos && (generatedMesh != null)){
			Gizmos.color = new Color(0f, 0f, 0f, 0.5f);
			for(int i=0; i<generatedMesh.vertexCount; i++){
				Gizmos.DrawSphere(transform.TransformPoint(generatedMesh.vertices[i]), 0.05f);
			}
		}
	}

	[ContextMenu("Generate with current settings")]
	void GenerateAndApply () {
		generatedMesh = GetTriangleFanMesh(numberOfSlices, radius);
		mf.sharedMesh = generatedMesh;
	}

	Mesh GetTriangleFanMesh (int numberOfSlices, float radius) {
		Vector3[] vertices = new Vector3[numberOfSlices + 1];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] texcoords = new Vector2[vertices.Length];
		for(int i=0; i<numberOfSlices; i++){
			float iFrac = ((float)i) / numberOfSlices;
			float x = iFrac * 2f * Mathf.PI;
			vertices[i] = new Vector3(radius * Mathf.Sin(x), radius * Mathf.Cos(x), 0f);

		}
		vertices[numberOfSlices] = Vector3.zero;
		for(int i=0; i<vertices.Length; i++){
			normals[i] = Vector3.back;
			Vector2 uv = new Vector2(vertices[i].x, vertices[i].y);
			uv += new Vector2(radius, radius);
			uv /= 2f * radius;
			texcoords[i] = uv;
		}
		int[] indices = new int[3 * numberOfSlices];
		for(int i=0; i<numberOfSlices; i++){
			int j = i * 3;
			indices[j+0] = i;
			indices[j+1] = (i + 1) % numberOfSlices;
			indices[j+2] = numberOfSlices;
		}
		Mesh output = new Mesh();
		output.vertices = vertices;
		output.normals = normals;
		output.uv = texcoords;
		output.triangles = indices;
		output.name = "Custom Triangle Fan";
		output.RecalculateBounds();
		output.RecalculateTangents();
		return output;
	}
}
