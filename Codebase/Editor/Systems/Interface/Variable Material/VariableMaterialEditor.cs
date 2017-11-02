using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Interface;
	using Event;
	[CanEditMultipleObjects]
	public class VariableMaterialEditor : ShaderGUI{
		public MaterialEditor editor;
		public Material material;
		public Shader shader;
		public string hash;
		public FileData parent;
		public static List<Material> allMaterials = new List<Material>();
		override public void OnGUI(MaterialEditor editor,MaterialProperty[] properties){
			EditorUI.Reset();
			this.editor = editor;
			this.material = (Material)editor.target;
			bool matching = this.shader == this.material.shader;
			if(!matching || VariableMaterial.dirty){this.Reload();}
			if(this.shader != null){
				EditorGUILayout.BeginHorizontal();
				string[] keywords = this.material.shaderKeywords;
				bool isHook = this.shader.name.EndsWith("#");
				bool isFlat = this.shader.name.Contains("#") && !isHook;
				bool isUpdated = !isFlat || this.shader.name.Split("#")[1].Split(".")[0] == this.hash;
				GUI.enabled = !this.parent.IsNull() && (isHook || this.parent.extension != "zshader");
				if(isFlat && "Unflatten".ToLabel().DrawButton()){VariableMaterial.Unflatten(editor.targets);}
				if(!isFlat && "Flatten".ToLabel().DrawButton()){VariableMaterial.Flatten(true,editor.targets);}
				GUI.enabled = UnityEvent.current.shift || !isUpdated;
				if("Update".ToLabel().DrawButton()){
					VariableMaterial.force = true;
					var materials = editor.targets.Cast<Material>().ToList();
					Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
				this.DrawProperties(editor,properties);
				//this.DrawSimpleProperties(editor,properties);
				//editor.PropertiesDefaultGUI(properties);
				if(GUI.changed){
					editor.serializedObject.ApplyModifiedProperties();
					Utility.SetDirty(editor.serializedObject.targetObject,false,true);
				}
				if(isFlat && !keywords.SequenceEqual(this.material.shaderKeywords)){
					VariableMaterial.Refresh(editor.target);
				}
			}
		}
		public void DrawSimpleProperties(MaterialEditor editor,MaterialProperty[] properties){
			var shader = editor.target.As<Material>().shader;
			for(var index = 0;index < properties.Length;++index){
				EditorGUIUtility.fieldWidth = 64f;
				EditorGUIUtility.labelWidth = Screen.width - EditorGUIUtility.fieldWidth - 64f;
				var current = properties[index];
				if((current.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None){
					editor.ShaderProperty(current,current.displayName);
				}
				EditorGUIUtility.fieldWidth = 0;
				EditorGUIUtility.labelWidth = 0;
			}
		}
		public void DrawProperties(MaterialEditor editor,MaterialProperty[] properties){
			var shader = editor.target.As<Material>().shader;
			for(var index = 0;index < properties.Length;++index){
				var current = properties[index];
				var label = current.displayName;
				if((current.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None){
					var handler = Utility.GetUnityType("MaterialPropertyHandler").CallMethod("GetHandler",shader,current.name);
					if(handler != null){
						editor.ShaderProperty(current,label);
						/*var position = EditorGUILayout.GetControlRect();
						var decorators = handler.GetVariable<List<MaterialPropertyDrawer>>("m_DecoratorDrawers");
						var drawer = handler.GetVariable<MaterialPropertyDrawer>("m_PropertyDrawer");
						if(decorators != null){
							foreach(MaterialPropertyDrawer decorator in decorators){
								decorator.CallExactMethod("OnGUI",position,current,label,editor);
							}
						}
						if(drawer != null){
							drawer.CallExactMethod("OnGUI",position,current,label,editor);
						}*/
					}
					else if(current.type == MaterialProperty.PropType.Color){current.colorValue = current.colorValue.Draw(label);}
					else if(current.type == MaterialProperty.PropType.Float){current.floatValue = current.floatValue.Draw(label);}
					else if(current.type == MaterialProperty.PropType.Range){current.floatValue = current.floatValue.DrawSlider(current.rangeLimits[0],current.rangeLimits[1],label);}
					else if(current.type == MaterialProperty.PropType.Texture){
						current.textureValue = current.textureValue.As<Texture2D>().Layout(-1,16).Draw<Texture2D>(label);
						EditorGUI.indentLevel += 1;
						var offset = current.textureScaleAndOffset;
						current.textureScaleAndOffset = new Vector2(offset.x,offset.y).DrawVector2("Tiling");
						current.textureScaleAndOffset = new Vector2(offset.z,offset.w).DrawVector2("Offset");
						EditorGUI.indentLevel -= 1;
						//current.textureValue = editor.TextureProperty(current,label);
					}
					else if(current.type == MaterialProperty.PropType.Vector){current.vectorValue.DrawVector4(label);}
				}
			}
			GUILayout.Space(10);
			"Other".ToLabel().DrawLabel(EditorStyles.boldLabel);
			editor.RenderQueueField();
			editor.EnableInstancingField();
			editor.DoubleSidedGIField();
		}
		public void Reload(){
			this.parent = VariableMaterial.GetParentShader(this.material);
			if(!this.parent.IsNull()){
				this.hash = this.parent.GetModifiedDate("MdyyHmmff") + "-" + this.material.shaderKeywords.Join(" ").ToMD5();
			}
			VariableMaterial.dirty = false;
			this.shader = this.material.shader;
			this.editor.Repaint();
		}
		[MenuItem("Zios/Material/Refresh Variable Materials (Scene)")]
		public static void RefreshScene(){
			List<Material> materials = new List<Material>();
			var renderers = Locate.GetSceneComponents<Renderer>();
			foreach(var renderer in renderers){materials.AddRange(renderer.sharedMaterials);}
			materials = materials.Distinct().ToList();
			Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
		}
		[MenuItem("Zios/Material/Refresh Variable Materials (All)")]
		public static void RefreshAll(){
			var materials = VariableMaterial.GetAll();
			Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
		}
		public static void RefreshStep(object collection,int index){
			var materials = (List<Material>)collection;
			EventStepper.title = "Updating " + materials.Count + " Materials";
			EventStepper.message = "Updating material : " + materials[index].name;
			VariableMaterial.Refresh(true,materials[index]);
		}
	}
}