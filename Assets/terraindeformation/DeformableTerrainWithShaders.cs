using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableTerrainWithShaders : MonoBehaviour {

	//assumptions: the object is a simple uv-unwrapped plane that's not rotated (y axis is okay)

	[SerializeField] MeshFilter mf;
	[SerializeField] MeshRenderer mr;
	[SerializeField] Collider col;
	[SerializeField] string texturePropertyName;
	[SerializeField] string deformationAmountPropertyName;
	[SerializeField] float deformationAmount;
	[SerializeField] float deformationRadiusScale;

	int controlTextureSize;
	MaterialPropertyBlock propBlock;
	Texture2D controlTexture;
	Color32[] pixels;

	void Start () {
		controlTextureSize = Mathf.NextPowerOfTwo(Mathf.FloorToInt(Mathf.Sqrt(mf.sharedMesh.vertexCount) + 0.5f));
		controlTexture = new Texture2D(controlTextureSize, controlTextureSize);
		pixels = GetFullColor32Array(controlTextureSize * controlTextureSize, Color.white);
		controlTexture.SetPixels32(pixels);
		controlTexture.Apply();
		propBlock = new MaterialPropertyBlock();
		propBlock.SetFloat(deformationAmountPropertyName, deformationAmount);
		propBlock.SetTexture(texturePropertyName, controlTexture);
	}

	void Reset () {
		mf = GetComponent<MeshFilter>();
		mr = GetComponent<MeshRenderer>();
		col = GetComponent<Collider>();
		controlTextureSize = 128;
		deformationAmount = 1;
		deformationRadiusScale = 1;
	}

	void Update () {
		mr.SetPropertyBlock(propBlock);
	}

	void OnCollisionEnter (Collision collision) {
		UpdateTexture(collision.collider);
	}

	void OnCollisionStay (Collision collision) {
		UpdateTexture(collision.collider);
	}

	void UpdateTexture (Collider otherCollider) {
		Vector3 colliderCenter = otherCollider.bounds.center;
		Vector3 colliderExtents = colliderCenter + (otherCollider.bounds.extents * deformationRadiusScale);

		bool allOkay = true;
		RaycastHit centerHit, extentsHit;
		allOkay &= Physics.Raycast(colliderCenter, Vector3.down, out centerHit, Mathf.Infinity, LayerMask.GetMask("DeformableTerrain"));
		allOkay &= Physics.Raycast(colliderExtents, Vector3.down, out extentsHit, Mathf.Infinity, LayerMask.GetMask("DeformableTerrain"));

		if(allOkay){
			Vector2 centerUV = centerHit.textureCoord;
			Vector2 extentsUV = extentsHit.textureCoord;
			int x, y, w, h;
			MakePixelRectangleData(centerUV, extentsUV, out x, out y, out w, out h);
			if(w < 1) w = 1;
			if(h < 1) h = 1;
			FillRectangle(ref pixels, controlTextureSize, controlTextureSize, x, y, w, h, Color.black);
			controlTexture.SetPixels32(pixels);
			controlTexture.Apply();
		}
	}

	void MakePixelRectangleData (Vector2 centerUV, Vector2 extentsUV, out int x, out int y, out int w, out int h) {
		int cx = Mathf.FloorToInt(centerUV.x * controlTextureSize);
		int cy = Mathf.FloorToInt(centerUV.y * controlTextureSize);
		int ex = Mathf.FloorToInt(extentsUV.x * controlTextureSize);
		int ey = Mathf.FloorToInt(extentsUV.y * controlTextureSize);
		int dx = ex - cx;
		int dy = ey - cy;
		int minX = Mathf.Min(cx - dx, cx + dx);
		int minY = Mathf.Min(cy - dy, cy + dy);
		int maxX = Mathf.Max(cx - dx, cx + dx);
		int maxY = Mathf.Max(cy - dy, cy + dy);
		x = minX;
		y = minY;
		w = maxX - minX;
		h = maxY - minY;
	}

	Color32 ToColor32 (Color color) {
		byte r = (byte)(255 * color.r);
		byte g = (byte)(255 * color.g);
		byte b = (byte)(255 * color.b);
		byte a = (byte)(255 * color.a);
		return new Color32(r, g, b, a);
	}

	Color32[] GetFullColor32Array (int size, Color color) {
		Color32 col32 = ToColor32(color);
		Color32[] output = new Color32[size];
		for(int i=0; i<size; i++){
			output[i] = col32;
		}
		return output;
	}

	void FillRectangle (ref Color32[] colors, int width, int height, int sx, int sy, int dx, int dy, Color newColor) {
		Color32 col32 = ToColor32(newColor);
		if((width - sx) < dx) dx = width - sx;
		if((height - sy) < dy) dy = height - sy;
		for(int i=0; i<dy; i++){
			for(int j=0; j<dx; j++){
				int y = i + sy;
				int x = j + sx;
				int pos = (y * width) + x;
				colors[pos] = col32;
			}
		}
	}
	
}
