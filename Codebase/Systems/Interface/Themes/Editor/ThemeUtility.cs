using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Interface{
	using UnityEditor;
	public partial class Theme{
		[MenuItem("Edit/Themes/Development/Sync/Names [GUISkin]")]
		public static void SyncSkinNames(){Theme.SyncSkinNames("");}
		public static void SyncSkinNames(string path=""){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Sync Names [GUISkin]",Theme.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiskin");
			foreach(var file in files){
				var stylesSkin = file.GetAsset<GUISkin>().customStyles;
				var stylesReflected = file.name.Contains(".") ? Theme.ReflectStyles(file.name) : null;
				var stylesInternal = file.name.Contains(".") ? stylesReflected.Values.ToArray() : EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).customStyles;
				if(stylesSkin.Length == stylesInternal.Length){
					for(int index=0;index<stylesSkin.Length;++index){
						var name = stylesSkin[index].name;
						var goal = stylesInternal[index].name;
						if(file.name.Contains(".")){
							if(!goal.IsEmpty()){goal = " ["+goal.Split("[")[0].Trim()+"]";}
							goal = stylesReflected.Keys.ToArray()[index] + goal;
						}
						if(name != goal){
							Debug.Log("[Themes] Fixed style name in " + file.name + ". Was " + name + ". Now " + goal);
							stylesSkin[index].name = goal;
						}
					}
					Utility.SetAssetDirty(file.GetAsset<GUISkin>());
					continue;
				}
				Debug.LogWarning("[Themes] Mismatched number of styles -- " + file.name + ". Found " + stylesSkin.Length + ", but expected " + stylesInternal.Length + ". Possible version conflict.");
			}
		}
		public static Dictionary<string,GUIStyle> ReflectStyles(string path,bool showWarnings=true){
			var empty = new Dictionary<string,GUIStyle>();
			var fieldName = path.Split(".").Last();
			var fieldFlags = fieldName.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
			var typeStatic = Utility.GetUnityType(path);
			var typeInstance = Utility.GetUnityType(path.Replace("."+fieldName,""));
			if(!typeStatic.IsNull()){
				return typeStatic.GetVariables<GUIStyle>(null,fieldFlags);
			}
			if(!typeInstance.IsNull()){
				var target = typeInstance.GetVariable(fieldName);
				if(target.IsNull()){
					try{
						target = Activator.CreateInstance(typeInstance.GetVariableType(fieldName));
						typeInstance.SetVariable(fieldName,target);
					}
					catch{return empty;}
				}
				return target.GetVariables<GUIStyle>();
			}
			if(showWarnings){Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + path + ". Possible version conflict.");}
			return empty;
		}
		[MenuItem("Edit/Themes/Development/Localize [Assets]")]
		public static void LocalizeAssets(){Theme.LocalizeAssets("");}
		public static void LocalizeAssets(string path="",bool includeBuiltin=false){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Localize Theme [Assets]",Theme.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiskin");
			foreach(var file in files){
				string assetPath = "";
				var skin = file.GetAsset<GUISkin>();
				foreach(var style in skin.GetStyles()){
					if(!style.font.IsNull()){
						assetPath = path+"/Font/"+style.font.name;
						if(!includeBuiltin && FileManager.GetPath(style.font).Contains("unity editor resources")){continue;}
						var font = FileManager.GetAsset<Font>(assetPath+".ttf",false);
						font = font ?? FileManager.GetAsset<Font>(assetPath+".otf",false);
						style.font = font ?? style.font;
					}
					foreach(var state in style.GetStates()){
						if(state.background.IsNull()){continue;}
						if(!includeBuiltin && FileManager.GetPath(state.background).Contains("unity editor resources")){continue;}
						assetPath = path+"/Background/"+state.background.name+".png";
						state.background = FileManager.GetAsset<Texture2D>(assetPath) ?? state.background;
					}
				}
				Utility.SetDirty(skin,false,true);
			}
			AssetDatabase.SaveAssets();
		}
		[MenuItem("Edit/Themes/Development/Sync/To Base Style [GUISkin]")]
		public static void SyncToBase(){Theme.SyncStyle();}
		[MenuItem("Edit/Themes/Development/Sync/From Base Style [GUISkin]")]
		public static void SyncFromBase(){Theme.SyncStyle(true);}
		public static void SyncStyle(bool flipPattern=false){
			var source = FileManager.GetAsset<GUISkin>(EditorUtility.OpenFilePanel("Apply From [GUISkin]",Theme.storagePath,"guiskin"));
			var destination = FileManager.GetAsset<GUISkin>(EditorUtility.OpenFilePanel("Apply To [GUISkin]",Theme.storagePath,"guiskin"));
			var skinStyles = destination.GetStyles();
			foreach(var style in source.GetStyles()){
				var name = flipPattern ? style.name : style.name.Parse("[","]");
				var styleMatch = flipPattern ? skinStyles.Where(x=>x.name.Contains(name)) : skinStyles.Where(x=>x.name==name);
				foreach(var match in styleMatch){
					Debug.Log("[Themes] Applied " + source.name + "." + style.name + " to " + destination.name + "." + match.name);
					match.Use(style);
				}
			}
			Utility.SetAssetDirty(destination);
		}
		[MenuItem("Edit/Themes/Development/Sync/Dynamic Textures")]
		public static void SyncTextures(){Theme.Apply("",true);}
	}
}