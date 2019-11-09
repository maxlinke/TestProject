using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MatrixDebug : MonoBehaviour {

    MeshRenderer mr;
    MaterialPropertyBlock mpb;

    void Awake () {
        mr = GetComponent<MeshRenderer>();
        if(mr == null){
            Debug.LogError("no meshrenderer!");
            return;
        }
        mpb = new MaterialPropertyBlock();
    }

    void OnWillRenderObject () {
        if(mr == null){
            mr = GetComponent<MeshRenderer>();
            return;
        }
        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }

        var translationMatrix = Matrix4x4.Translate(transform.position);
        var rotationMatrix = Matrix4x4.Rotate(transform.rotation);
        var scaleMatrix = Matrix4x4.Scale(transform.lossyScale);

        Matrix4x4 modelMatrix = translationMatrix * rotationMatrix * scaleMatrix; 
        // model matrix is fine

        // var debugMatrix = Camera.current.worldToCameraMatrix;
        // view matrix is also fine

        // var debugMatrix = Camera.current.projectionMatrix;
        // projection matrix is NOT fine

        // var debugMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, false);
        // almost done (the y-field [1][1] is still inverted...)

        // var debugMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, true);
        //i'll be damned... wtf, so rendertotexture needs to be true i guess... (it doesn't change from forward to deferred)

        var debugMatrix = (Camera.current.worldToCameraMatrix * modelMatrix).inverse.transpose;

        mpb.SetMatrix("_DebugMatrix", debugMatrix);
        mr.SetPropertyBlock(mpb);
    }
	
}
