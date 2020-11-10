using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBoneCustom : MonoBehaviour {

    [Header("Debug")]
    [SerializeField] UnityEngine.UI.Text debugText = default;

    [Header("Translation")]
    [SerializeField] AnimationCurve springForce = AnimationCurve.Linear(0,0,1,1);
    [SerializeField] float maxDistance = 1f;
    [SerializeField] float maxForce = 1f;
    [SerializeField, Range(0, 1)] float gravityInfluence = 1f;

    Vector3 initLocalPosition;
    Vector3 lastWorldPosition;

    void Start () {
        initLocalPosition = transform.localPosition;
        lastWorldPosition = transform.position;
    }

    void LateUpdate () {
        transform.position = lastWorldPosition;
        var targetPosition = transform.parent.TransformPoint(initLocalPosition);
        lastWorldPosition = transform.position;
        Debug.DrawRay(targetPosition, Vector3.up, Color.magenta, Time.deltaTime, false);
        // if(debugText != null){
        //     debugText.text = $"dt: {Time.deltaTime:F3}\nfps:{(1f/Time.deltaTime):F1}\nlrp:{lerp:F3}";
        // }
    }
	
}
