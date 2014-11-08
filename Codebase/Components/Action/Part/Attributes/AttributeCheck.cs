using Zios;
using System;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Check")]
public class AttributeCheck : ActionPart{
	public AttributeBool value = false;
	public override void Start(){
		base.Start();
		this.value.Setup("",this);
		this.value.usage = AttributeUsage.Shaped;
	}
	public override void Use(){
		bool active = this.value.Get();
		if(active){base.Use();}
		else{base.End();}
	}
}
