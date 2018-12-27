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

	private struct IndexTriangle {
		public int index1, index2, index3;
	}

	private string name;
	private Bounds bounds;
	List<GraphVertex> vertices;

	public MeshGraph () {
		name = "";
		vertices = new List<GraphVertex>();
	}

	public void RemoveVertex (GraphVertex vertex) {
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
		List<IndexTriangle> triangles = new List<IndexTriangle>();
		for(int i=0; i<vertices.Count; i++){
			outputVertices[i] = vertices[i].position;
			outputNormals[i] = vertices[i].normal;
			outputUV[i] = vertices[i].texcoord;
			outputTangents[i] = vertices[i].tangent;
			triangles.AddRange(GetTriangles(vertices[i], false));
		}
		int[] outputTriangles = new int[triangles.Count * 3];
		for(int i=0; i<triangles.Count; i++){
			int i3 = i * 3;
			outputTriangles[i3+0] = triangles[i].index1;
			outputTriangles[i3+1] = triangles[i].index2;
			outputTriangles[i3+2] = triangles[i].index3;
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
		if(mesh.vertices.LongLength != (long)mesh.vertices.Length) throw new UnityException("Mesh \"" + mesh.name + "\" has too many vertices to become a MeshGraph!");
		if((mesh.triangles.Length % 3) != 0) throw new UnityException("Mesh \"" + mesh.name + "\" has a weird interpretation of triangles...");
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
		for(int i=0; i<mesh.triangles.Length; i+=3){
			int index1 = mesh.triangles[i+0];
			int index2 = mesh.triangles[i+1];
			int index3 = mesh.triangles[i+2];
			verts[index1].ConnectTo(verts[index2]);
			verts[index1].ConnectTo(verts[index3]);
			verts[index2].ConnectTo(verts[index3]);
		}
		MeshGraph output = new MeshGraph();
		output.name = mesh.name;
		output.bounds = mesh.bounds;
		output.vertices.AddRange(verts);
		return output;
	}

	private IndexTriangle[] GetTriangles (GraphVertex vertex, bool includeLowerIndexVertices = true) {
		List<IndexTriangle> output = new List<IndexTriangle>();
		GraphVertex[] connected = vertex.GetConnectedVertices();
		int mainIndex = vertices.IndexOf(vertex);
		//this is an easy way of making sure you only get each triangle once...
		if(!includeLowerIndexVertices){
			List<GraphVertex> newConnected = new List<GraphVertex>();
			for(int i=0; i<connected.Length; i++){
				if(vertices.IndexOf(connected[i]) > mainIndex){
					newConnected.Add(connected[i]);
				}
			}
			connected = newConnected.ToArray();
		}
		//caching indices because it may improve performance...
		int[] connectedIndices = new int[connected.Length];
		for(int i=0; i<connected.Length; i++){
			connectedIndices[i] = vertices.IndexOf(connected[i]);
		}
		//if two connected vertices are themselves connected, we've got a triangle.
		for(int i=0; i<connected.Length-1; i++){
			for(int j=i+1; j<connected.Length; j++){
				if(connected[i].IsConnectedTo(connected[j])){
					Vector3 averageNormal = vertex.normal + connected[i].normal + connected[j].normal;
					Vector3 crossNormal = Vector3.Cross(connected[i].position - vertex.position, connected[j].position - connected[i].position);	//TODO replace all this yo.
					IndexTriangle newTriangle = new IndexTriangle();
					//keeping the winding order
					if(Vector3.Dot(averageNormal, crossNormal) > 0){
						newTriangle.index1 = mainIndex;
						newTriangle.index2 = connectedIndices[i];
						newTriangle.index3 = connectedIndices[j];
					}else{
						newTriangle.index1 = connectedIndices[j];
						newTriangle.index2 = connectedIndices[i];
						newTriangle.index3 = mainIndex;
					}
					output.Add(newTriangle);
				}
			}
		}
		return output.ToArray();
	}

}
