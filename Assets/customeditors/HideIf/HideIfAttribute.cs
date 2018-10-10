using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class HideIfAttribute : PropertyAttribute {

	public readonly string comparedPropertyName;
	public readonly object comparedValue;
	public readonly ComparisonType comparisonType;
	public readonly DisablingType disablingType;

	public HideIfAttribute (string comparedPropertyName, object comparedValue, ComparisonType comparisonType = ComparisonType.EQUALS, DisablingType disablingType = DisablingType.HIDE) {
		this.comparedPropertyName = comparedPropertyName;
		this.comparedValue = comparedValue;
		this.comparisonType = comparisonType;
		this.disablingType = disablingType;
//		this.order = -999999;
	}

}
