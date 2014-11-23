using Zios;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios{
	[CustomPropertyDrawer(typeof(Attribute),true)]
	public class AttributeDrawer : PropertyDrawer{
		public IAttributeAccess access;
		public float overallHeight;
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			this.OnGUI(new Rect(-10000,0,0,0),property,label);
			return this.overallHeight;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(!Attribute.ready){return;}
			this.overallHeight = base.GetPropertyHeight(property,label);
			if(this.access == null){
				object generic = property.GetObject<object>();
				if(generic is AttributeFloat){this.access = new AttributeAccess<float,AttributeFloat,AttributeFloatData,OperatorNumeral,SpecialNumeral>();}
				if(generic is AttributeInt){this.access = new AttributeAccess<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>();}
				if(generic is AttributeString){this.access = new AttributeAccess<string,AttributeString,AttributeStringData,OperatorString,SpecialString>();}
				if(generic is AttributeBool){this.access = new AttributeAccess<bool,AttributeBool,AttributeBoolData,OperatorBool,SpecialBool>();}
				if(generic is AttributeVector3){this.access = new AttributeAccess<Vector3,AttributeVector3,AttributeVector3Data,OperatorVector3,SpecialVector3>();}
				if(generic is AttributeGameObject){this.access = new AttributeAccess<GameObject,AttributeGameObject,AttributeGameObjectData,OperatorGameObject,SpecialGameObject>();}
			}
			this.access.Setup(this,area,property,label);
		}
	}
	public interface IAttributeAccess{
		void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label);
	}
	public class AttributeAccess<BaseType,Type,DataType,Operator,Special> : IAttributeAccess
		where Operator : struct
		where Special  : struct
		where Type     : Attribute<BaseType,Type,DataType,Operator,Special>,new()
		where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
		public AttributeDrawer drawer;
		public SerializedProperty property;
		public GUIContent label;
		public Rect fullRect;
		public Rect labelRect;
		public Rect valueRect;
		public Rect iconRect;
		public bool contextOpen;
		public Dictionary<AttributeData,bool> targetMode = new Dictionary<AttributeData,bool>();
		public bool formulaExpanded;
		public void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label){
			string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
			this.drawer = drawer;
			this.property = property;
			this.label = label;
			this.fullRect = area.SetHeight(EditorGUIUtility.singleLineHeight);
			this.iconRect = this.fullRect.SetSize(14,14);
			this.labelRect = this.fullRect.SetWidth(EditorGUIUtility.labelWidth);
			this.valueRect = this.fullRect.Add(labelRect.width,0,-labelRect.width,0);
			this.iconRect = this.fullRect.SetSize(14,14);
			GUI.changed = false;
			EditorGUI.BeginProperty(area,label,property);
			this.Draw();
			EditorGUI.EndProperty();
			if(GUI.changed){
				property.serializedObject.ApplyModifiedProperties();
				if(EditorWindow.mouseOverWindow != null){
					EditorWindow.mouseOverWindow.Repaint();
				}
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
		}
		public void Draw(){
			Type attribute = this.property.GetObject<Type>();
			DataType firstData = attribute.data[0];
			SerializedProperty firstProperty = property.FindPropertyRelative("data").GetArrayElementAtIndex(0);
			this.DrawContext(attribute,firstData);
			if(attribute.mode == AttributeMode.Normal){
				if(firstData.usage == AttributeUsage.Direct){
					this.DrawDirect(attribute,firstData,this.label);
				}
				if(firstData.usage == AttributeUsage.Shaped){
					GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconShaped"));
					this.labelRect = this.labelRect.AddX(16);
					this.DrawShaped(attribute,firstProperty,this.label);
				}
			}
			if(attribute.mode == AttributeMode.Linked){
				attribute.usage = AttributeUsage.Shaped;
				GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconLinked"));
				this.labelRect = this.labelRect.AddX(16);
				this.DrawShaped(attribute,firstProperty,this.label,false);
			}
			if(attribute.mode == AttributeMode.Formula){
				this.DrawFormula(attribute,this.label);
			}
		}
		public void DrawDirect(Type current,AttributeData data,GUIContent label,bool? drawOperator=null){
			EditorGUIUtility.labelWidth = this.labelRect.width;
			EditorGUIUtility.fieldWidth = this.valueRect.width;
			if(drawOperator != null){
				this.DrawOperator((DataType)data,(bool)!drawOperator);
				EditorGUIUtility.labelWidth += 101;
				EditorGUIUtility.fieldWidth += 81;
			}
			if(current is AttributeFloat){
				AttributeFloatData floatData = (AttributeFloatData)data;
				floatData.value = floatData.value.DrawLabeled(this.fullRect,label);
			}
			if(current is AttributeInt){
				AttributeIntData intData = (AttributeIntData)data;
				intData.value = intData.value.DrawLabeledInt(this.fullRect,label);
			}
			if(current is AttributeString){
				AttributeStringData stringData = (AttributeStringData)data;
				stringData.value = stringData.value.DrawLabeled(this.fullRect,label);
			}
			if(current is AttributeBool){
				AttributeBoolData boolData = (AttributeBoolData)data;
				boolData.value = boolData.value.DrawLabeled(this.fullRect,label);
			}
			if(current is AttributeVector3){
				AttributeVector3Data vector3Data = (AttributeVector3Data)data;
				vector3Data.value = vector3Data.value.DrawLabeled(this.fullRect,label);
			}
			if(current is AttributeGameObject){
				AttributeGameObjectData gameObjectData = (AttributeGameObjectData)data;
				gameObjectData.value = gameObjectData.value.DrawLabeledObject(this.fullRect,label);
			}
		}
		public void DrawShaped(Type attribute,SerializedProperty property,GUIContent label,bool drawSpecial=true,bool? drawOperator=null){
			label.DrawLabel(labelRect);
			DataType data = property.GetObject<DataType>();
			Target target = data.target;
			Rect toggleRect = this.valueRect.SetWidth(16);
			bool toggleActive = this.targetMode.ContainsKey(data) ? this.targetMode[data] : !data.referenceID.IsEmpty();
			this.targetMode[data] = toggleActive.Draw(toggleRect,GUI.skin.GetStyle("CheckmarkToggle"));
			if(toggleActive != this.targetMode[data]){
				if(attribute is AttributeGameObject){
					data.referenceID = toggleActive ? "" : data.referenceID;
				}
			}
			if(!this.targetMode[data]){
				Rect targetRect = this.valueRect.Add(18,0,-18,0);
				property.FindPropertyRelative("target").Draw(targetRect);
				return;
			}
			var lookup = attribute.GetLookupTable();
			List<string> attributeNames = new List<string>();
			List<string> attributeIDs = new List<string>();
			int attributeIndex = -1;
			if(target.direct != null){
				if(lookup.ContainsKey(target.direct)){
					foreach(var item in lookup[target.direct]){
						bool feedback = item.Value.id == attribute.id || item.Value.data[0].referenceID == attribute.id;
						if(!feedback){
							attributeNames.Add(item.Value.path);
						}
					}
					attributeNames = attributeNames.Order().OrderBy(item=>item.Contains("/")).ToList();
					foreach(string name in attributeNames){
						string id = lookup[target.direct].Values.ToList().Find(x=>x.path == name).id;
						attributeIDs.Add(id);
					}
				}
				if(!data.referenceID.IsEmpty()){
					attributeIndex = attributeIDs.IndexOf(data.referenceID);
				}
			}
			if(attributeNames.Count > 0){
				Rect attributeRect = this.valueRect.Add(18,0,-18,0);
				Rect specialRect = this.valueRect.Add(18,0,0,0).SetWidth(50);
				if(attributeIndex == -1){
					string message = data.referenceID.IsEmpty() ? "[Not Set]" : "[Missing]";
					attributeIndex = 0;
					attributeNames.Insert(0,message);
					attributeIDs.Insert(0,"0");
				}
				if(drawOperator != null){
					this.DrawOperator(data,(bool)!drawOperator);
					attributeRect = attributeRect.Add(81,0,-81,0);
					specialRect.x += 81;
				}
				if(drawSpecial){
					List<string> specialList = new List<string>();
					int specialIndex = 0;
					if(target.direct != null){
						string specialName = Enum.GetName(typeof(Special),data.special);
						specialList = Enum.GetNames(typeof(Special)).ToList();
						specialIndex = specialList.IndexOf(specialName);
					}
					if(specialIndex == -1){specialIndex = 0;}
					attributeRect = attributeRect.Add(51,0,-51,0);
					specialIndex = specialList.Draw(specialRect,specialIndex);
					data.special = ((Special[])Enum.GetValues(typeof(Special)))[specialIndex];
				}
				int previousIndex = attributeIndex;
				attributeIndex = attributeNames.Draw(attributeRect,attributeIndex);
				string name = attributeNames[attributeIndex];
				string id = attributeIDs[attributeIndex];
				if(attributeIndex != previousIndex){
					data.referencePath = name;
					data.referenceID = id;
				}
			}
			else{
				Rect warningRect = this.valueRect.Add(18,0,-18,0);
				string targetName = target.direct == null ? "Target" : target.direct.ToString().Strip("(UnityEngine.GameObject)").Trim();
				string typeName = attribute.GetType().ToString().ToLower().Strip("zios",".","attribute");
				string message = "<b>" + targetName.Truncate(16) + "</b> has no <b>"+typeName+"</b> attributes.";
				message.DrawLabel(warningRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawOperator(DataType data,bool disabled=false){
			Rect operatorRect = this.valueRect.Add(18,0,0,0).SetWidth(80);
			EditorGUIUtility.AddCursorRect(operatorRect,MouseCursor.Arrow);
			List<string> operatorList = new List<string>();
			GUIStyle style = new GUIStyle(EditorStyles.popup);
			style.alignment = TextAnchor.MiddleRight;
			style.contentOffset = new Vector2(-3,0);
			style.fontStyle = FontStyle.Bold;
			if(disabled){
				GUI.enabled = false;
				operatorList.Add("=");
				operatorList.Draw(operatorRect,0,style);
				GUI.enabled = true;
				return;
			}
			operatorList = Enum.GetNames(typeof(Operator)).ToList();
			string operatorName = Enum.GetName(typeof(Operator),data.sign);
			int operatorIndex = operatorList.IndexOf(operatorName);
			if(operatorIndex == -1){operatorIndex = 0;}
			for(int index=0;index<operatorList.Count;++index){
				string operatorAlias = operatorList[index];
				if(operatorAlias.Contains("Add")){operatorAlias="+";}
				if(operatorAlias.Contains("Sub")){operatorAlias="-";}
				if(operatorAlias.Contains("Mul")){operatorAlias="×";}
				if(operatorAlias.Contains("Div")){operatorAlias="÷";}
				operatorList[index] = operatorAlias;
			}
			operatorIndex = operatorList.Draw(operatorRect,operatorIndex,style);
			data.sign = ((Operator[])Enum.GetValues(typeof(Operator)))[operatorIndex];
		}
		public void DrawFormula(Type attribute,GUIContent label){
			Rect labelRect = this.labelRect.AddX(12);
			EditorGUIUtility.AddCursorRect(this.fullRect,MouseCursor.ArrowPlus);
			if(this.labelRect.AddX(16).Clicked() || this.valueRect.Clicked()){
				GUI.changed = true;
				this.formulaExpanded = !this.formulaExpanded;
			}
			this.formulaExpanded = EditorGUI.Foldout(labelRect,this.formulaExpanded,label,GUI.skin.GetStyle("IconFormula"));
			if(this.formulaExpanded){
				float lineHeight = EditorGUIUtility.singleLineHeight+2;
				this.fullRect = this.fullRect.SetX(45).AddWidth(-55);
				this.labelRect = this.labelRect.SetX(45).SetWidth(25);
				this.valueRect = this.valueRect.SetX(70).SetWidth(this.fullRect.width);
				SerializedProperty dataProperty = this.property.FindPropertyRelative("data");
				for(int index=0;index<attribute.data.Length;++index){
					SerializedProperty currentProperty = dataProperty.GetArrayElementAtIndex(index);
					DataType currentData = attribute.data[index];
					GUIContent formulaLabel = new GUIContent("#"+(index+1));
					this.fullRect = this.fullRect.AddY(lineHeight);
					this.labelRect = this.labelRect.AddY(lineHeight);
					this.valueRect.y += lineHeight;
					this.drawer.overallHeight += lineHeight;
					bool? operatorState = index == 0 ? (bool?)false : (bool?)true;
					if(currentData.usage == AttributeUsage.Direct){
						this.fullRect = this.fullRect.AddWidth(25);
						this.DrawDirect(attribute,currentData,formulaLabel,operatorState);
						this.fullRect = this.fullRect.AddWidth(-25);
					}
					else if(currentData.usage == AttributeUsage.Shaped){
						this.DrawShaped(attribute,currentProperty,formulaLabel,true,operatorState);
					}
					this.DrawContext(attribute,currentData,false,index!=0);
				}
				this.labelRect.y += lineHeight;
				this.drawer.overallHeight += lineHeight;
				if(GUI.Button(this.labelRect.SetWidth(100),"Add Attribute")){
					attribute.Add();
					GUI.changed = true;
				}
			}
			else{
				string message = "[expand for details]";
				message.DrawLabel(this.valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawContext(Attribute attribute,AttributeData data,bool showMode=true,bool showRemove=false){
			if(this.labelRect.Clicked(1)){
				this.contextOpen = true;
				GenericMenu menu = new GenericMenu();
				AttributeMode mode = attribute.mode;
				AttributeUsage usage = data.usage;
				MenuFunction removeAttribute = ()=>{attribute.Remove(data);};
				MenuFunction modeNormal  = ()=>{attribute.mode = AttributeMode.Normal;};
				MenuFunction modeLinked  = ()=>{attribute.mode = AttributeMode.Linked;};
				MenuFunction modeFormula = ()=>{attribute.mode = AttributeMode.Formula;};
				MenuFunction usageDirect = ()=>{data.usage = AttributeUsage.Direct;};
				MenuFunction usageShaped = ()=>{data.usage = AttributeUsage.Shaped;};
				bool normal = attribute.mode == AttributeMode.Normal;
				if(attribute.locked){
					menu.AddDisabledItem(new GUIContent("Attribute Mode Locked"));
					menu.ShowAsContext();
					return;
				}
				if(showMode){
					menu.AddItem(new GUIContent("Normal/Direct"),normal&&(usage==AttributeUsage.Direct),modeNormal+usageDirect);
					menu.AddItem(new GUIContent("Normal/Shaped"),normal&&(usage==AttributeUsage.Shaped),modeNormal+usageShaped);
					menu.AddItem(new GUIContent("Linked"),(mode==AttributeMode.Linked),modeLinked+usageShaped);
					if(attribute.canFormula){menu.AddItem(new GUIContent("Formula"),(mode==AttributeMode.Formula),modeFormula);}
				}
				else{
					menu.AddItem(new GUIContent("Direct"),normal&&(usage==AttributeUsage.Direct),usageDirect);
					menu.AddItem(new GUIContent("Shaped"),normal&&(usage==AttributeUsage.Shaped),usageShaped);
				}
				if(showRemove){
					menu.AddItem(new GUIContent("Remove"),false,removeAttribute);	
				}
				menu.ShowAsContext();
			}
			if(this.contextOpen && Event.current.button == 0){
				GUI.changed = true;
				this.ForceUpdate();
				this.contextOpen = false;
			}
		}
		public void ForceUpdate(){
			SerializedProperty forceUpdate = property.FindPropertyRelative("path");
			string path = forceUpdate.stringValue;
			forceUpdate.stringValue = "";
			forceUpdate.stringValue = path;
		}
	}
}