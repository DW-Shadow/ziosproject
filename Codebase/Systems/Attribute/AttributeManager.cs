using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
using Attribute = Zios.Attribute;
using Action = Zios.Action;
[AddComponentMenu("Zios/Singleton/Attribute Manager")][ExecuteInEditMode]
public class AttributeManager : MonoBehaviour{
	private float nextStep;
	private bool initialSearch;
	private bool refresh;
	public void OnApplicationQuit(){
		Utility.EditorUpdate(this.Start,true);
	}
	public void OnDestroy(){
		Utility.RemoveAssetUpdate(this.ReSearch);
		Utility.RemoveHierarchyUpdate(this.ReSearch);
		Utility.RemoveEditorUpdate(this.Start);
	}
	public void Update(){
		Utility.AssetUpdate(this.ReSearch);
		Utility.HierarchyUpdate(this.ReSearch);
		Utility.EditorUpdate(this.Start,true);
	}
	public void ReSearch(){this.refresh=true;}
	public void SceneRefresh(bool full=true){
		if(full){
			Attribute.all.Clear();
			AttributeFloat.lookup.Clear();
			AttributeVector3.lookup.Clear();
			AttributeBool.lookup.Clear();
			AttributeInt.lookup.Clear();
			AttributeString.lookup.Clear();
		}
		AttributeBox[] boxes = Locate.GetSceneObjects<AttributeBox>();
		StateMonoBehaviour[] states = Locate.GetSceneObjects<StateMonoBehaviour>();
		AttributeExposer[] exposers = Locate.GetSceneObjects<AttributeExposer>();
		foreach(AttributeBox box in boxes){
			if(full || !box.gameObject.activeInHierarchy || !box.enabled){box.Awake();}
		}
		foreach(StateMonoBehaviour state in states){
			if(full || !state.gameObject.activeInHierarchy || !state.enabled){state.Awake();}
		}
		foreach(AttributeExposer exposer in exposers){
			if(full || !exposer.gameObject.activeInHierarchy || !exposer.enabled){exposer.Awake();}
		}
		this.Setup();
	}
	public void Start(){
		if(Application.isPlaying || Time.realtimeSinceStartup > this.nextStep){
			this.nextStep = Time.realtimeSinceStartup + 1;
			if(!this.initialSearch || this.refresh){
				this.SceneRefresh(!Application.isPlaying);
				this.initialSearch = true;
				this.refresh = false;
				return;
			}
			this.Setup();
		}
	}
	public void Setup(){
		foreach(var attribute in Attribute.all.Copy()){
			if(attribute.parent.IsNull()){
				Attribute.all.Remove(attribute);
				continue;
			}
		}
		foreach(var attribute in Attribute.all){attribute.SetupTable();}
		foreach(var attribute in Attribute.all){attribute.SetupData();}
		Attribute.ready = true;
	}
}