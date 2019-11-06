using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExposeGBuffers : MonoBehaviour {

    [SerializeField] Material gBufferExposeMat;
    [SerializeField] RawImage uiImageForGBuffer;
    [SerializeField] RawImage uiImageForProcessedTexture;

    RenderTexture blitTex;
    Texture2D processedTexture;

    void Start () {
        blitTex = new RenderTexture((int)(uiImageForGBuffer.rectTransform.rect.width), (int)(uiImageForGBuffer.rectTransform.rect.height), 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        uiImageForGBuffer.texture = blitTex;
        processedTexture = new Texture2D(blitTex.width, blitTex.height, TextureFormat.ARGB32, false);
        uiImageForProcessedTexture.texture = processedTexture;
    }

    void OnRenderImage (RenderTexture src, RenderTexture dst) {
        Graphics.Blit(src, blitTex, gBufferExposeMat); 
        Graphics.Blit(src, dst);

        var prevActive = RenderTexture.active;
        RenderTexture.active = blitTex;
        processedTexture.ReadPixels(new Rect(0, 0, blitTex.width, blitTex.height), 0, 0);

        var pixels = processedTexture.GetPixels();
        var newPixels = new Color[pixels.Length];

        Color prev = new Color();
        for(int y=0; y<processedTexture.height; y++){
            for(int x=0; x<processedTexture.width; x++){
                int i = (y * processedTexture.width) + x;
                Color current = pixels[i];
                float deltaR = current.r - prev.r;
                float outputValue = 0.5f + (deltaR / 2f);
                newPixels[i] = new Color(outputValue, outputValue, outputValue, 1);
                prev = current;
            }
        }

        processedTexture.SetPixels(newPixels);
        processedTexture.Apply(false);
        RenderTexture.active = prevActive;
    }
	
}
