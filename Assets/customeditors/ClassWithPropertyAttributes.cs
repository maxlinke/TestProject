using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClassWithPropertyAttributes : MonoBehaviour {

	public enum TestEnum{
		ASDF,
		GHJK,
		QWERTZ,
		UIOP
	}

		[Header("My Custom Hidden Property Thingy")]
	[SerializeField] bool aBool;
	[HideIf("aBool", true)] [SerializeField] float aFloat;
	[HideIf("aBool", true)] [SerializeField] int anInt;
	[HideIf("aBool", true)] [SerializeField] string aString;
	[HideIf("aBool", true)] [SerializeField] [Range(-1f, 1f)] float rangeFloat;
	[HideIf("aBool", true)] [SerializeField] string[] stringArray;
	[HideIf("aBool", true)] [SerializeField] UnityEvent unityEvent;		//i read online that unityevents don't serialize nicely... so that might have something to do with this
	[HideIf("aBool", true)] [SerializeField] TestEnum testEnum;

		[Header("Other Stuff")]
	[ContextMenuItem("Set this to \"boi\"", "SetSomeStringToBoi")]
	[SerializeField] string someString;
	void SetSomeStringToBoi () {
		someString = "boi";
	}

	[ContextMenu("Context Menu Thingy")]
	void ContextMenuTest () {
		Debug.Log("You clicked something in the context menu!");
	}

	void Start () {
		Debug.Log(someString);	//to make the compiler shut up...
	}

}
