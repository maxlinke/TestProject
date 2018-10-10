using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UselessClass : MonoBehaviour {

		[Header("Regular Fields")]
	[UselessAttribute] [SerializeField] int anInt;
	[UselessAttribute] [SerializeField] bool aBool;
	[UselessAttribute] [SerializeField] float aFloat;
	[UselessAttribute] [SerializeField] string aString;

		[Header("More exotic fields")]
	[UselessAttribute] [SerializeField] int[] anArray;
	[UselessAttribute] [SerializeField] UnityEvent anEvent;

		[Header("Fields with attributes")]
	[UselessAttribute] [SerializeField] [Range(-1f, 1f)] float aRangeFloat;
	[UselessAttribute] [SerializeField] [Tooltip("This is a tooltip")] bool toolTipBool;

	//unity events still break :/

}
