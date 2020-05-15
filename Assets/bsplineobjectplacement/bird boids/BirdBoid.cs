using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBoid : MonoBehaviour {

    static List<BirdBoid> allBirdBoids;

    void OnEnable () {
        if(allBirdBoids == null){
            allBirdBoids = new List<BirdBoid>();
        }
    }

    // settings for this boird
    // placer
    // init with list of ALL others (including this one)
    // static list of all boirds

    // collision avoidance? select colliders only... and only occasionally

    // singleton boird util thingy

    // terrain for ground, height above for lower flight ceiling

    void Start () {
        
    }

    void Update () {
        
    }
	
}
