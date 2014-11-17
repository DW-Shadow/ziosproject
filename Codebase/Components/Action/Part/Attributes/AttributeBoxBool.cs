using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Attribute/Attribute Box (Bool)")]
public class AttributeBoxBool : AttributeBox<AttributeBool>{
	public override void Reset(){
		this.value = true;
		base.Reset();
	}
	public override void Awake(){
		base.Awake();
	}
}