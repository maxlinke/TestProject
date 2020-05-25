using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBoid : MonoBehaviour {

    [Header("Movement Space")]
    [SerializeField] BoxCollider boundingVolume;
    // [SerializeField] 

    [Header("Grouping")]
    [SerializeField] float localGroupRadius = 10f;


    public Vector3 velocity { get; private set; }
    public Vector3 position => transform.position;

    static List<SimpleBoid> boids;

    void Start () {

    }

    void Update () {
        if(boundingVolume == null){
            return;
        }
        if(boundingVolume.transform.rotation != Quaternion.identity){
            boundingVolume.transform.rotation = Quaternion.identity;
            Debug.LogWarning("bounding volume must be axis-aligned! fixed it for ya...");
        }
    }
	
}
