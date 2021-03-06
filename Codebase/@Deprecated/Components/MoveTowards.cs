using UnityEngine;
namespace Zios.Actions.MoveComponents{
	using Containers.Math;
	using Attributes;
	using Event;
	using Motion;
	[AddComponentMenu("Zios/Component/Action/Move/Move Towards")]
	public class MoveTowards : StateMonoBehaviour{
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeVector3 goal = Vector3.zero;
		public LerpVector3 travel = new LerpVector3();
		public override void Awake(){
			base.Awake();
			this.target.Setup("Target",this);
			this.goal.Setup("Goal",this);
			this.travel.Setup("Travel",this);
			this.AddDependent<ColliderController>(this.target);
			this.warnings.AddNew("Deprecated. Consider using formula-based AttributeTransition with ExposeTransform components.");
		}
		public override void End(){
			this.travel.Reset();
			base.End();
		}
		public override void Use(){
			base.Use();
			foreach(GameObject target in this.target){
				Vector3 current = this.travel.Step(target.transform.position,this.goal);
				Vector3 amount = current-target.transform.position;
				target.CallEvent("Add Move Raw",new Vector3(amount.x,0,0));
				target.CallEvent("Add Move Raw",new Vector3(0,amount.y,0));
				target.CallEvent("Add Move Raw",new Vector3(0,0,amount.z));
			}
		}
	}
}