using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "SpinnyWheels/SpinnyWheelEffect")]
public class SpinnyWheelEffect : ScriptableObject {

	public enum EffectType {
		VALUE,				//1, 2, 3, 4
		LIKEDELTA,			//+1 / -1
		SUSPICIONDELTA,		//+1
		KNOCKOUT			//dont draw the value field
	}

	[SerializeField] EffectType type = EffectType.VALUE;
	[SerializeField] Color elementColor = Color.white;
	[SerializeField, Range(0f, 1f)] float backgroundSaturation = 0f;
	[SerializeField, Range(0f, 1f)] float backgroundBrightness = 1f;
	[SerializeField] bool overrideBackgroundMaterial = false;
	[SerializeField] Material backgroundMaterial = null;
	[SerializeField] bool useSprite = false;
	[SerializeField] Sprite sprite = null;
	[SerializeField] string text = "";
	[SerializeField] int value = 0;

	public EffectType Type { get { return type; } }
	public Color ElementColor { get { return elementColor; } }
	public float BackgroundSaturation { get { return backgroundSaturation; } }
	public float BackgroundBrightness { get { return backgroundBrightness; } }
	public bool OverrideBackgroundMaterial { get { return overrideBackgroundMaterial; } }
	public Material BackgroundMaterial { get { return backgroundMaterial; } }
	public bool UseSprite { get { return useSprite; } }
	public Sprite Sprite { get { return sprite; } }
	public string Text { get { return text; } }
	public int Value { get { return value; } }

}
