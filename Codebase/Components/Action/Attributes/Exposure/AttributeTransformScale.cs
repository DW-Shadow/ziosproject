﻿#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Scale)")]
public class AttributeTransformScale : DataMonoBehaviour{
	[HideInInspector] public AttributeVector3 scale = Vector3.zero;
	public override void Awake(){
		base.Awake();
		this.scale.Setup("Scale",this);
		this.scale.getMethod = ()=>this.transform.localScale;
		this.scale.setMethod = value=>this.transform.localScale = value;
	}
}
