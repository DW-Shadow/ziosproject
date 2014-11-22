using Zios;
using UnityEngine;
public enum ForceType{Absolute,Relative}
[AddComponentMenu("Zios/Component/Action/Move/Add Force")]
public class AddForce : ActionPart{
	public ForceType type;
	public AttributeVector3 amount = Vector3.zero;
	public Target target = new Target();
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.amount.Setup("Amount",this);
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.amount;
		if(this.type == ForceType.Relative){
			amount = this.target.Get().transform.right * this.amount.x;
			amount += this.target.Get().transform.up * this.amount.y;
			amount += this.target.Get().transform.forward * this.amount.z;
		}
		this.target.Call("Add Force",amount);
	}
}