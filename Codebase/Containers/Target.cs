﻿using Zios;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
    public enum TargetMode{Search,Direct};
    [Serializable]
    public class Target{
		public static string defaultSearch = "[Self]";
	    public List<GameObject> special = new List<GameObject>();
	    public List<string> specialNames = new List<string>();
	    public string search = "";
	    public GameObject directObject;
	    public GameObject searchObject;
	    public Component parent;
	    public TargetMode mode = TargetMode.Search;
		public string path;
	    private string fallbackSearch = "";
	    public static implicit operator Transform(Target value){return value.Get().transform;}
	    public static implicit operator GameObject(Target value){return value.Get();}
	    public static implicit operator UnityObject(Target value){return value.Get();}
	    public GameObject Get(){
		    GameObject result = this.mode == TargetMode.Search ? this.searchObject : this.directObject;
		    if(result == null && Application.isPlaying){
			    Debug.LogWarning("[Target] No target found for : " + this.path,this.parent);
		    }
		    return result;
	    }
	    public void Setup(string path,Component parent){
			this.path = parent.GetPath() + "/" + path;
		    this.parent = parent;
		    if(!Application.isPlaying){
			    this.AddSpecial("[This]",parent.gameObject);
			    this.AddSpecial("[Self]",parent.gameObject);
			    this.AddSpecial("[Next]",parent.gameObject.GetNextSibling(true));
			    this.AddSpecial("[Previous]",parent.gameObject.GetPreviousSibling(true));
			    this.AddSpecial("[NextEnabled]",parent.gameObject.GetNextSibling());
			    this.AddSpecial("[PreviousEnabled]",parent.gameObject.GetPreviousSibling());
			    this.AddSpecial("[Root]",parent.gameObject.GetPrefabRoot());
				Events.Add("On Validate",(Method)this.Search,parent);
		    }
		    if(parent is ActionLink || parent is StateLink){
			    ActionLink actionLink = parent is ActionLink ? (ActionLink)parent : null;
			    StateLink stateLink = parent is StateLink ? (StateLink)parent : actionLink.stateLink;
			    GameObject linkObject = stateLink != null ? stateLink.gameObject : actionLink.gameObject;
			    GameObject ownerObject = stateLink != null ? stateLink.owner : actionLink.gameObject;
			    this.AddSpecial("[Owner]",ownerObject);
			    this.AddSpecial("[Action]",linkObject);
			    this.AddSpecial("[ActionLink]",linkObject);
			    this.AddSpecial("[StateLink]",linkObject);
		    }
			if(!Application.isPlaying){
				string defaultSearch = Target.defaultSearch = PlayerPrefs.GetString("Target-DefaultSearch","[Self]");
				this.SetFallback(defaultSearch);
				if(this.mode == TargetMode.Search && this.search.IsEmpty()){
					this.Search();
				}
			}
	    }
		public void SetFallback(string name){this.fallbackSearch = name;}
	    public void AddSpecial(string name,GameObject target){
		    if(target.IsNull()){target = this.parent.gameObject;}
		    if(!this.specialNames.Any(x=>x.Contains(name,true))){
			    this.specialNames.Add(name);
			    this.special.Add(target);
		    }
		    else{
			    int index = this.specialNames.FindIndex(x=>x.Contains(name,true));
			    this.special[index] = target;
		    }
	    }
	    public void Search(){
			if(this.search.IsEmpty()){this.search = this.fallbackSearch;}
		    string search = this.search.Replace("\\","/");
		    if(!search.IsEmpty()){
				for(int index=0;index<this.special.Count;++index){
					string specialName = this.specialNames[index];
					GameObject special = this.special[index];
					if(!special.IsNull() && search.Contains(specialName,true)){
						string specialPath = special.GetPath();
						search = search.Replace(specialName,specialPath,true);
					}
				}
				if(search.ContainsAny("/",".")){
					string[] parts = search.Split("/");
					string total = "";
					GameObject current = null;
					for(int index=0;index<parts.Length;++index){
						string part = parts[index];
						current = GameObject.Find(total);
						if(part == ".." || part == "." || part.IsEmpty()){
							if(part.IsEmpty()){continue;}
							if(total.IsEmpty()){
								int specialIndex = this.specialNames.FindIndex(x=>x.Contains("[this]",true));
								current = specialIndex != -1 ? this.special[index] : null;
								if(!current.IsNull()){
									if(part == ".."){
										total = current.GetParent().IsNull() ? "" : current.GetParent().GetPath();
									}
									else{total = current.GetPath();}
								}
								continue;
							}
							current = GameObject.Find(total);
							if(!current.IsNull()){
								if(part == ".."){
									total = current.GetParent().IsNull() ? "" : current.GetParent().GetPath();
								}
								continue;
							}
						}
						GameObject next = GameObject.Find(total+part+"/");
						if(next.IsNull() && !current.IsNull() && Attribute.lookup.ContainsKey(current)){
							var match = Attribute.lookup[current].Where(x=>x.Value.info.name.Matches(part)).FirstOrDefault().Value;
							if(match is AttributeGameObject){
								next = match.As<AttributeGameObject>().Get();
								if(!next.IsNull()){
									total = next.GetPath();
								}
								continue;
							}
						}
						total += part + "/";
					}
					search = total;
				}
				this.searchObject = Locate.Find(search);
		    }
	    }
    }
}