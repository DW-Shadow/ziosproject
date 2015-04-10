﻿using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Action State (Freeze)")]
public class ActionStateFreeze : ActionLink{
	public AttributeBool freezeOnUse = false;
	public AttributeBool freezeOnEnd = false;
	public override void Awake(){
		base.Awake();
		string warning = "This component has been deprecated and likely should not be used.";
		if(!this.dependents.Exists(x=>x.message==warning)){
			this.dependents.AddNew().message = warning;
		}
		this.freezeOnUse.Setup("Freeze On Use",this);
		this.freezeOnEnd.Setup("Freeze On End",this);
	}
	public override void Use(){
		if(this.stateLink.inUse && this.freezeOnEnd){return;}
		base.Use();
	}
	public override void End(){
		if(this.stateLink.inUse && this.freezeOnUse){return;}
		base.End();
	}
}
