using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AffectionDisplayer : MonoBehaviour {

	public const int MIN_PERSON_LIKE = -1;
	public const int MAX_PERSON_LIKE = 1;
	public const int MIN_GROUP_LIKE = -4;
	public const int MAX_GROUP_LIKE = 4;

	[Header("Components")]
	[SerializeField] Text nameTextField;
	[SerializeField] Text groupTextField;



	int personLike;
	int groupLike;

	void Awake () {
		GameObject asdf = new GameObject("asdf", typeof(RectTransform));
		asdf.transform.parent = this.transform.parent;
		GameObject textObject = new GameObject("text",  typeof(RectTransform), typeof(Text));
		textObject.particleSystem = asdf.transform;
	}

	void Start () {
		
	}
	
	void Update () {
		
	}

	public void Initialize (string personName, string personGroup, int personLike, int groupLike) {
		string firstName, lastName;
		SplitName(personName, out firstName, out lastName);
		nameTextField.text = firstName + "\n" + lastName;
		groupTextField.text = personGroup;
		this.personLike = Mathf.Clamp(personLike, MIN_PERSON_LIKE, MAX_PERSON_LIKE);
		this.groupLike = Mathf.Clamp(groupLike, MIN_GROUP_LIKE, MAX_GROUP_LIKE);
	}

	void SplitName (string name, out string partOne, out string partTwo) {
		string[] parts = name.Split(null);	//null means whitespace... weird, i know...
		if(parts.Length == 1){
			partOne = parts[0];
			partTwo = null;
		}else if(parts.Length == 2){
			partOne = parts[0];
			partTwo = parts[1];
		}else{
			bool doneWithPartOne = false;
			partOne = string.Empty;
			partTwo = string.Empty;
			for(int i=0; i<parts.Length-1; i++){
				if(!doneWithPartOne){
					if(!IsPreposition(parts[i])){
						partOne += parts[i] + " ";
					}else{
						partTwo = parts[i] + " ";
						doneWithPartOne = true;
					}
				}else{
					partTwo += parts[i] + " ";
				}
			}
			partTwo += parts[parts.Length-1];
			if(partOne.EndsWith(" ")) partOne = partOne.Remove(partOne.Length-1, 1);
		}
	}

	static string[] prefixes = new string[]{
		"mr", "ms", "mrs", "dr", "dipl", "ing", "med"
	};

	bool IsPrefix (string possiblePrefix) {
		Regex rgx = new Regex("[^a-zA-z0-9 -]");
		possiblePrefix = rgx.Replace(possiblePrefix.ToLower(), "");
		for(int i=0; i<prefixes.Length; i++){
			if(prefixes[i].Equals(possiblePrefix)) return true;
		}
		return false;
	}

	static string[] prepositions = new string[]{
		"von", "van", "zu", "of", "de", "di"
	};

	bool IsPreposition (string possiblePreposition) {
		possiblePreposition = possiblePreposition.ToLower();
		for(int i=0; i<prepositions.Length; i++){
			if(prepositions[i].Equals(possiblePreposition)) return true;
		}
		return false;
	}

}
