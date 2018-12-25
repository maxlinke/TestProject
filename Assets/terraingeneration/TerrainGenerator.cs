using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

	//TODO optional texture as noise source 
	//TODO other shapes than a rectangle (free form would be nice but i have no clue how to do it nicely) (maybe confine to a bunch of colliders OR cut out certain colliders?)

	private const int MAX_RNG_OFFSET = 1024;

	[Header("Components")]
	[SerializeField] MeshFilter mf;
	[SerializeField] MeshCollider mc;

	[Header("General Settings")]
	[SerializeField] string seed;
	[SerializeField] [Range(1, 1024)] int xTiles;
	[SerializeField] [Range(1, 1024)] int zTiles;
	[SerializeField] float tileSize;
	[SerializeField] float uvScale;			//TODO make optional (also editor) either just 0-1 uv coords for the whole thing or what i currently do

	[Header("Deformation Settings")]
	[SerializeField] [Range(-1f, 1f)] float deformationDirection;
	[SerializeField] float deformationStrength;
	[SerializeField] bool DEBUG_USE_TEXTURES_AS_SOURCE;				//TODO enum? and use the editor to only draw one of the arrays
	[SerializeField] PerlinNoiseSource[] perlinNoiseSources;
	[SerializeField] TextureNoiseSource[] textureNoiseSources;

	void Reset () {
		mf = GetComponent<MeshFilter>();
		mc = GetComponent<MeshCollider>();

		seed = "";
		xTiles = 32;
		zTiles = 32;
		tileSize = 1f;
		uvScale = 1f;

		deformationDirection = 0f;
		deformationStrength = 1f;
		DEBUG_USE_TEXTURES_AS_SOURCE = false;
		perlinNoiseSources = new PerlinNoiseSource[0];
		textureNoiseSources = new TextureNoiseSource[0];
	}

	public void Generate () {
		NoiseSource[] noiseSources;
		if(DEBUG_USE_TEXTURES_AS_SOURCE){
			noiseSources = textureNoiseSources;
		}else{
			noiseSources = perlinNoiseSources;
		}
		InitNoiseSources(ref noiseSources, GetRandomNumberGenerator(seed)); 

		int xVerts = xTiles + 1;
		int zVerts = zTiles + 1;
		int numberOfVerts = xVerts * zVerts;
		Vector3[] vertices = new Vector3[numberOfVerts];
		Vector3[] normals = new Vector3[numberOfVerts];
		Vector2[] texcoords = new Vector2[numberOfVerts];
		float iOffset = (float)xTiles / 2f;
		float jOffset = (float)zTiles / 2f;

		for(int j=0; j<zVerts; j++){
			for(int i=0; i<xVerts; i++){
				int index = (j * xVerts) + i;
				float x = tileSize * (i - iOffset);
				float z = tileSize * (j - jOffset);
				float deformNoise = 0f;
				for(int n=0; n<noiseSources.Length; n++){
					deformNoise += noiseSources[n].Evaluate(x, z);
				}
				float y = (deformNoise + deformationDirection) * deformationStrength;
				vertices[index] = new Vector3(x, y, z);
				texcoords[index] = new Vector2(x, z) / uvScale;
//				normals[index] = irrelevant because they get calculated at the end
			}
		}

		int numberOfTris = xTiles * zTiles * 2;
		int[] triangles = new int[numberOfTris * 3];

		for(int j=0; j<zTiles; j++){
			for(int i=0; i<xTiles; i++){
				int quad = (j * xTiles) + i;
				int triStart = quad * 6;
				int vertStart = (j * xVerts) + i;
				triangles[triStart + 0] = vertStart;
				triangles[triStart + 1] = vertStart + xVerts;
				triangles[triStart + 2] = vertStart + xVerts + 1;
				triangles[triStart + 3] = vertStart;
				triangles[triStart + 4] = vertStart + 1 + xVerts;
				triangles[triStart + 5] = vertStart + 1;
			}
		}

		Mesh terrain = new Mesh();
		terrain.name = "Custom Terrain";
		terrain.vertices = vertices;
		terrain.normals = normals;
		terrain.triangles = triangles;
		terrain.uv = texcoords;
		terrain.RecalculateBounds();
		terrain.RecalculateNormals();
		terrain.RecalculateTangents();
		mf.sharedMesh = terrain;
		mc.sharedMesh = terrain;
	}

	System.Random GetRandomNumberGenerator (string seed) {
		seed = seed.Replace(" ", "");
		if(seed.Length < 1) return new System.Random();
		return new System.Random(seed.GetHashCode());
	}

	void InitNoiseSources (ref NoiseSource[] noiseSources, System.Random rng) {
		for(int i=0; i<noiseSources.Length; i++){
			noiseSources[i].offset = new Vector2(
				rng.Next(-MAX_RNG_OFFSET, MAX_RNG_OFFSET),
				rng.Next(-MAX_RNG_OFFSET, MAX_RNG_OFFSET)
			);
		}
	}

}
