using System;
using UnityEngine;
namespace Zios.Actions{
	using Event;
	using Attributes;
	[AddComponentMenu("")]
	public class StateMonoBehaviour : ManagedMonoBehaviour{
		[Advanced] public StateOccurrence occurrence = StateOccurrence.Default;
		[Internal] public StateTable controller;
		[Internal] public string id;
		[Internal] public AttributeBool usable = false;
		[Internal] public AttributeBool active = false;
		[Internal] public AttributeBool used = false;
		[NonSerialized] public bool? nextState;
		public override void Awake(){
			base.Awake();
			Events.Add("On Disable",this.End,this);
			Events.Register("On Start",this);
			Events.Register("On End",this);
			this.usable.Setup("Usable",this);
			this.active.Setup("Active",this);
			this.used.Setup("Used",this);
			this.usable.Set(this.controller.IsNull());
		}
		#if UNITY_EDITOR
		[ContextMenu("Toggle Breakdown")]
		public virtual void ToggleLinkBreakdown(){
			Utility.ToggleEditorPref("StateMonoBehaviourEditor-ToggleBreakdown");
		}
		#endif
		public void DefaultAlias(string name){
			if(this.alias.IsEmpty()){
				this.alias = name;
			}
		}
		public override void Step(){
			bool usedOnce = this.used && this.occurrence == StateOccurrence.Once;
			if(!usedOnce){
				if(this.usable){this.Use();}
				else if(this.active){this.End();}
			}
			else if(!this.usable){this.End();}
			else if(this.active){
				this.active.Set(false);
				if(!this.controller.IsNull()){this.controller.dirty = true;}
			}
		}
		public virtual void Use(){this.Toggle(true);}
		public virtual void End(){this.Toggle(false);}
		public virtual void Toggle(bool state){
			if(!Application.isPlaying){return;}
			bool resetUsed = this.used && this.occurrence == StateOccurrence.Once && !state;
			if(resetUsed || (state != this.active)){
				if(this.controller.IsEnabled()){
					this.nextState = state;
					return;
				}
				this.Apply(state);
			}
		}
		public virtual void Apply(bool state){
			this.nextState = null;
			this.active.Set(state);
			this.used.Set(state);
			this.CallEvent(state ? "On Start" : "On End");
		}
	}
	public enum StateOccurrence{Default,Constant,Once};
}