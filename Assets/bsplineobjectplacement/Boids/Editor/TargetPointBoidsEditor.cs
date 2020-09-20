using UnityEditor;

[CustomEditor(typeof(TargetPointBoids))]
public class TargetPointBoidsEditor : BoidsEditor { 

    protected override bool IsSpecialProperty (SerializedProperty property) {
        return base.IsSpecialProperty(property) || property.name.Equals("boidTarget");
    }

    protected override void DrawSpecialProperty (SerializedProperty property) {
        base.DrawSpecialProperty(property);
        if(property.name.Equals("boidTarget")){
            ObjectFieldRedBackgroundIfNull(property);
        }
    }

}
