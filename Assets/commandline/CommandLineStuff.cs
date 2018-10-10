using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandLineStuff : MonoBehaviour {

	[SerializeField] Text textField;

	void Start () {
		string[] args = System.Environment.GetCommandLineArgs();
		if(args.Length == 0){
			textField.text = "no command line args available";
		}else{
			textField.text = args[0];
			for(int i=1; i<args.Length; i++){
				textField.text += "\n" + args[i];
			}
		}
	}

}
