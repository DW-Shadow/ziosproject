using UnityEditor;
using UnityEngine;
namespace Zios.UI{
	[CustomPropertyDrawer(typeof(Target),true)]
	public class TargetDrawer : PropertyDrawer{
		public bool setup;
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			property.serializedObject.Update();
			string skin = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skin + ".guiskin");
			Target target = property.GetObject<Target>();
			TargetDrawer.Draw(area,target,label);
		}
		public static void Draw(Rect area,Target target,GUIContent label){
			Rect toggleRect = new Rect(area);
			Rect propertyRect = new Rect(area);
			float labelWidth = label.text.IsEmpty() ? 0 : EditorGUIUtility.labelWidth;
			propertyRect.x += labelWidth + 18;
			propertyRect.width -= labelWidth + 18;
			toggleRect.x += labelWidth;
			toggleRect.width = 18;
			bool previousMode = target.mode == TargetMode.Direct;
			bool currentMode = previousMode.Draw(toggleRect,"",GUI.skin.GetStyle("TargetToggle"));
			if(previousMode != currentMode){
				target.mode = target.mode == TargetMode.Direct ? TargetMode.Search : TargetMode.Direct;
			}
			label.DrawLabel(area,null,true);
			if(target.mode == TargetMode.Direct){
				target.directObject = target.directObject.Draw<GameObject>(propertyRect,"",true);
			}
			else{
				Rect textRect = propertyRect;
				string result = target.searchObject != null ? target.searchObject.GetPath().Trim("/") : "Not Found.";
				Vector2 textSize = GUI.skin.textField.CalcSize(new GUIContent(target.search));
				Vector2 subtleSize = GUI.skin.GetStyle("SubtleInfo").CalcSize(new GUIContent(result));
				float subtleX = propertyRect.x+propertyRect.width-subtleSize.x;
				float subtleWidth = subtleSize.x;
				float minimumX = propertyRect.x+textSize.x+3;
				if(subtleX < minimumX){
					subtleWidth -= (minimumX-subtleX);
					subtleX = minimumX;
				}
				propertyRect = propertyRect.SetX(subtleX).SetWidth(subtleWidth);
				EditorGUIUtility.AddCursorRect(propertyRect,MouseCursor.Zoom);
				if(!target.searchObject.IsNull() && propertyRect.Clicked(0)){
					Selection.activeGameObject = target.searchObject;
					Event.current.Use();
				}
				target.search = target.search.Draw(textRect);
				result.DrawLabel(propertyRect,GUI.skin.GetStyle("SubtleInfo"));
			}
			if(GUI.changed && !target.IsNull()){
				target.Search();
				if(target.parent is DataMonoBehaviour){
					var parent = target.parent.As<DataMonoBehaviour>();
					parent.DelayEvent(parent.path,"On Validate");
					Utility.SetDirty(parent);
				}
			}
		}
	}
}