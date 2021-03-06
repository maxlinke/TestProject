﻿using UnityEditor;

namespace Boids {

    [CustomEditor(typeof(TargetPointBoids))]
    public class TargetPointBoidsEditor : PlainBoidsEditor { 

        protected override bool DrawPropertyCustom (SerializedProperty property) {
            if(base.DrawPropertyCustom(property)){
                return true;
            }
            if(property.name.Equals("boidTarget")){
                EditorTools.DrawObjectFieldWarnIfNull(property);
                return true;
            }
            return false;
        }

    }

}