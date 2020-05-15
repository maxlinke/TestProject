using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bird Boid Settings", fileName = "New Bird Boid Settings")]
public class BirdBoidSettings : ScriptableObject {

    [SerializeField] TerrainCollider ground;
    [SerializeField] float upperFlightCeiling;

	
}
