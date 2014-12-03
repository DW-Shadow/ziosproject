using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public static class Locate{
	public static bool cleanGameObjects = false;
	public static List<Type> cleanComponents = new List<Type>();
	public static List<GameObject> cleanSiblings = new List<GameObject>();
	public static Dictionary<Type,GameObject[]> siblings = new Dictionary<Type,GameObject[]>();
	public static Dictionary<Type,GameObject[]> enabledSiblings = new Dictionary<Type,GameObject[]>();
	public static Dictionary<Type,GameObject[]> disabledSiblings = new Dictionary<Type,GameObject[]>();
	public static GameObject[] sceneObjects = new GameObject[0];
	public static GameObject[] enabledObjects = new GameObject[0];
	public static GameObject[] disabledObjects = new GameObject[0];
	public static Dictionary<Type,Component[]> sceneComponents = new Dictionary<Type,Component[]>();
	public static Dictionary<Type,Component[]> enabledComponents = new Dictionary<Type,Component[]>();
	public static Dictionary<Type,Component[]> disabledComponents = new Dictionary<Type,Component[]>();
	static Locate(){
		#if UNITY_EDITOR
		EditorApplication.hierarchyWindowChanged += Locate.SetDirty;
		#endif
	}
	public static void SetDirty(){
		Locate.cleanGameObjects = false;
		Locate.cleanComponents.Clear();
	}
	public static GameObject GetScenePath(string name,bool autocreate=true){
		string[] parts = name.Split('/');
		string path = "";
		GameObject current = null;
		Transform parent = null;
		foreach(string part in parts){
			path = path + "/" + part;
			current = GameObject.Find(path);
			if(current == null){
				if(!autocreate){
					return null;
				}
				current = new GameObject();
				current.name = part;
				current.transform.parent = parent; 
			}
			parent = current.transform;
		}
		return current;
	}
	public static void Build<Type>() where Type : Component{
		List<Type> enabled = new List<Type>();
		List<Type> disabled = new List<Type>();
		Type[] all = (Type[])Resources.FindObjectsOfTypeAll(typeof(Type));
		foreach(Type current in all){
			if(current.IsPrefab()){continue;}
			if(current.gameObject.activeInHierarchy){enabled.Add(current);}
			else{disabled.Add(current);}
		}
		Locate.sceneComponents[typeof(Type)] = enabled.Extend(disabled).ToArray();
		Locate.enabledComponents[typeof(Type)] = enabled.ToArray();
		Locate.disabledComponents[typeof(Type)] = disabled.ToArray();
		Locate.cleanComponents.Add(typeof(Type));
		if(typeof(Type) == typeof(Transform)){
			List<GameObject> enabledObjects = enabled.Select(x=>x.gameObject).ToList();
			List<GameObject> disabledObjects = disabled.Select(x=>x.gameObject).ToList();
			Locate.sceneObjects = enabledObjects.Extend(disabledObjects).ToArray();
			Locate.enabledObjects = enabledObjects.ToArray();
			Locate.disabledObjects = disabledObjects.ToArray();
			Locate.cleanGameObjects = true;
		}
	}
	public static GameObject[] FindAll(string name){
		if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
		List<GameObject> matches = new List<GameObject>();
		foreach(GameObject current in Locate.enabledObjects){
			if(current.name == name){
				matches.Add(current);
			}
		}
		return matches.ToArray();
	}
	public static bool HasDuplicate(string name){
		if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
		List<GameObject> amount = new List<GameObject>();
		GameObject root = new GameObject("root");
		foreach(var current in Locate.sceneObjects){
			GameObject parent = current.GetParent();
			if(parent.IsNull()){parent = root;}
			if(current.name == name){
				if(amount.Contains(parent)){
					Utility.Destroy(root);
					return true;
				}
				amount.Add(parent);
			}
		}
		Utility.Destroy(root);
		return false;
	}
	public static GameObject[] GetSceneObjects(bool includeEnabled=true,bool includeDisabled=true){
		if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
		if(includeEnabled && includeDisabled){return Locate.sceneObjects;}
		if(!includeEnabled){return Locate.disabledObjects;}
		return Locate.enabledObjects;
	}
	public static Type[] GetSceneObjects<Type>(bool includeEnabled=true,bool includeDisabled=true) where Type : Component{
		if(!Locate.cleanComponents.Contains(typeof(Type))){Locate.Build<Type>();}
		if(includeEnabled && includeDisabled){return (Type[])Locate.sceneComponents[typeof(Type)];}
		if(!includeEnabled){return (Type[])Locate.disabledComponents[typeof(Type)];}
		return (Type[])Locate.enabledComponents[typeof(Type)];
	}
	public static GameObject Find(string name,bool includeHidden=true){
		if(!Locate.cleanGameObjects){Locate.Build<Transform>();}
		if(!name.Contains("/")){return GameObject.Find(name);}
		GameObject[] all;
		all = includeHidden ? Locate.sceneObjects : Locate.enabledObjects;
		foreach(GameObject current in all){
			string path = current.GetPath();
			if(path == name || path.Trim("/") == name || path.TrimLeft("/") == name || path.TrimRight("/") == name){
				return current;
			}
		}
		return null;
	}
}