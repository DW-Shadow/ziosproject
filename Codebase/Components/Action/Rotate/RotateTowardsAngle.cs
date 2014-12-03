using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Angle")]
public class RotateTowardsAngle : ActionPart{
	public AttributeVector3 eulerAngle = Vector3.zero;
	public AttributeGameObject target = new AttributeGameObject();
	public LerpVector3 rotation = new LerpVector3();
	public override void Awake(){
		base.Awake();
		this.target.Setup("Target",this);
		this.rotation.Setup("Rotate Towards",this);
		this.rotation.isAngle.Set(true);
	}
	public override void Use(){
		Transform transform = this.target.Get().transform;
		Vector3 current = transform.localEulerAngles;
		transform.localEulerAngles = this.rotation.Step(current,this.eulerAngle);
		base.Use();
	}
}
