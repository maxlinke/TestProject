using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGraph {

	public class GraphVertex {
		
		public Vector3 position;
		public Vector3 normal;
		public Vector2 texcoord;
		public Vector4 tangent;
		private List<GraphVertex> connected;

		public GraphVertex () {
			position = Vector3.zero;
			normal = Vector3.zero;
			texcoord = Vector2.zero;
			tangent = Vector4.zero;
			connected = new List<GraphVertex>();
		}

		public override string ToString () {
			return "Position : " + position.ToString() + "\n" + 
				"Normal : " + normal.ToString() + "\n" + 
				"UV : " + texcoord.ToString() + "\n" + 
				"Tangent : " + tangent.ToString() + "\n" + 
				"Connections : " + connected.Count;
		}

		public GraphVertex[] GetConnectedVertices () {
			return connected.ToArray();
		}

		public bool IsConnectedTo (GraphVertex other) {
			for(int i=0; i<connected.Count; i++){
				if(connected[i].Equals(other)) return true;
			}
			return false;
		}

		public void ConnectTo (GraphVertex other) {
			if(!IsConnectedTo(other)){
				connected.Add(other);
				other.ConnectTo(this);	//the if statement prevents duplicates AND infinite loops. yey.
			}
		}

		public void DisconnectFrom (GraphVertex other) {
			connected.Remove(other);	//nothin fancy needed here...
		}

	}

	private class VertexTriangle {

		public GraphVertex this[int index] {
			get { return vertices[index]; }
			set { vertices[index] = value; }
		}

		private GraphVertex[] vertices;

		public VertexTriangle () {
			vertices = new GraphVertex[3];
		}

		public VertexTriangle (GraphVertex v1, GraphVertex v2, GraphVertex v3) {
			vertices = new GraphVertex[]{v1, v2, v3};
		}

		public bool Contains (GraphVertex vertex) {
			for(int i=0; i<vertices.Length; i++){
				if(vertices[i].Equals(vertex)) return true;
			}
			return false;
		}

	}

	private string name;
	private Bounds bounds;
	List<GraphVertex> vertices;
	List<VertexTriangle> triangles;

	public MeshGraph () {
		name = "";
		vertices = new List<GraphVertex>();
		triangles = new List<VertexTriangle>();
	}

	public void RemoveVertex (GraphVertex vertex) {
		List<VertexTriangle> connectedTris = new List<VertexTriangle>();
		for(int i=0; i<triangles.Count; i++){
			if(triangles[i].Contains(vertex)) connectedTris.Add(triangles[i]);
		}
		for(int i=0; i<connectedTris.Count; i++){
			triangles.Remove(connectedTris[i]);
		}
		for(int i=0; i<vertices.Count; i++){
			vertices[i].DisconnectFrom(vertex);
		}
		vertices.Remove(vertex);
	}

	public GraphVertex[] GetVertices () {
		return vertices.ToArray();
	}

	public Mesh ToMesh () {
		Vector3[] outputVertices = new Vector3[vertices.Count];
		Vector3[] outputNormals = new Vector3[vertices.Count];
		Vector2[] outputUV = new Vector2[vertices.Count];
		Vector4[] outputTangents = new Vector4[vertices.Count];
		for(int i=0; i<vertices.Count; i++){
			outputVertices[i] = vertices[i].position;
			outputNormals[i] = vertices[i].normal;
			outputUV[i] = vertices[i].texcoord;
			outputTangents[i] = vertices[i].tangent;
		}
		int[] outputTriangles = new int[triangles.Count * 3];
		for(int i=0; i<triangles.Count; i++){
			VertexTriangle tri = triangles[i];
			int i3 = i * 3;
			outputTriangles[i3+0] = vertices.IndexOf(tri[0]);
			outputTriangles[i3+1] = vertices.IndexOf(tri[1]);
			outputTriangles[i3+2] = vertices.IndexOf(tri[2]);
		}
		Mesh output = new Mesh();
		output.vertices = outputVertices;
		output.normals = outputNormals;
		output.uv = outputUV;
		output.tangents = outputTangents;
		output.triangles = outputTriangles;
		output.name = this.name;
		output.bounds = this.bounds;
		return output;
	}

	public static MeshGraph FromMesh (Mesh mesh) {
		bool copyNormals = (mesh.normals.Length == mesh.vertices.Length);
		bool copyUVs = (mesh.uv.Length == mesh.vertices.Length);
		bool copyTangents = (mesh.tangents.Length == mesh.vertices.Length);
		GraphVertex[] verts = new GraphVertex[mesh.vertices.Length];
		for(int i=0; i<mesh.vertices.Length; i++){
			GraphVertex newVert = new GraphVertex();
			newVert.position = mesh.vertices[i];
			if(copyNormals) newVert.normal = mesh.normals[i];
			if(copyUVs) newVert.texcoord = mesh.uv[i];
			if(copyTangents) newVert.tangent = mesh.tangents[i];
			verts[i] = newVert;
		}
		VertexTriangle[] tris = new VertexTriangle[mesh.triangles.Length / 3];
		for(int i=0; i<mesh.triangles.Length; i+=3){
			int index1 = mesh.triangles[i+0];
			int index2 = mesh.triangles[i+1];
			int index3 = mesh.triangles[i+2];
			verts[index1].ConnectTo(verts[index2]);
			verts[index1].ConnectTo(verts[index3]);
			verts[index2].ConnectTo(verts[index3]);
			tris[i/3] = new VertexTriangle(verts[index1], verts[index2], verts[index3]);
		}
		MeshGraph output = new MeshGraph();
		output.name = mesh.name;
		output.bounds = mesh.bounds;
		output.vertices.AddRange(verts);
		output.triangles.AddRange(tris);
		return output;
	}

}
