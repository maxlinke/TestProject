using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StructForJSONifying {

	public static StructForJSONifying defaultConfig = new StructForJSONifying(3.1415f);

	public float someFloat;

	public StructForJSONifying (float someFloat) {
		this.someFloat = someFloat;
	}

}
