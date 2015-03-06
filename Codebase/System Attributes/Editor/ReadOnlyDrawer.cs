﻿using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer{
	    public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
		    return EditorGUI.GetPropertyHeight(property,label,true);
	    }
	    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label){
			if(!Event.current.IsUseful()){return;}
		    GUI.enabled = false;
		    EditorGUI.PropertyField(position,property,label,true);
		    GUI.enabled = true;
	    }
    }
}