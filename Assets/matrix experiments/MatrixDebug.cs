using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class MatrixDebug : MonoBehaviour {

    Material mat;

    void Awake () {
        var mr = GetComponent<MeshRenderer>();
        if(mr == null){
            Debug.LogError("no meshrenderer!");
            return;
        }
        mat = mr.sharedMaterial;
        if(mat == null){
            Debug.LogError("no material on meshrenderer!");
            return;
        }
    }

    void OnWillRenderObject () {
        if(mat == null){
            Awake();
            return;
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

        var debugMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, true);
        //i'll be damned... wtf, so rendertotexture needs to be true i guess... (it doesn't change from forward to deferred)

        mat.SetMatrix("_DebugMatrix", debugMatrix);

        // var a = new Matrix4x4(
        //     new Vector4(2, 0, 0, 1),
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(4, 0, 0, -1));
        // mat.SetMatrix("_DebugMatrixA", a);
        // var b = new Matrix4x4(
        //     new Vector4(1, 0, 0, 0),
        //     new Vector4(0, 1, 0, 0),
        //     new Vector4(0, 0, 1, 0),
        //     new Vector4(0, 0, 0, 1));
        // mat.SetMatrix("_DebugMatrixB", b);
    }
	
}
