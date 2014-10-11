﻿using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Clamp (Transform)")]
public class ClampTransform : ActionPart{
	public Target target = new Target();
	public ClerpVector3 position = new ClerpVector3();
	public ClerpVector3 rotation = new ClerpVector3();
	public ClerpVector3 scale = new ClerpVector3();
	public override void OnValidate(){
		this.DefaultRate("LateUpdate");
		this.DefaultPriority(15);
		base.OnValidate();
		this.target.AddSpecial("[Owner]",this.action.owner);
		this.target.AddSpecial("[Action]",this.action.gameObject);
		this.target.DefaultSearch("[Owner]");
		this.target.DefaultSearch("[Owner]");
		this.rotation.isAngle = true;
	}
	public override void Use(){
		Transform target = this.target.Get().transform;
		target.position = this.position.Step(target.position);
		target.localEulerAngles = this.rotation.Step(target.localEulerAngles);
		target.localScale = this.scale.Step(target.localScale);
		base.Use();
	}
	public override void End(){
		this.position.Reset();
		this.rotation.Reset();
		this.scale.Reset();
		base.End();
	}
}