using UnityEngine;
namespace Zios.Actions.NetworkComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/General/Distance Range")]
	public class DistanceRange : StateMonoBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeGameObject target = new AttributeGameObject();
		public AttributeFloat minimumDistance = Mathf.Infinity;
		public AttributeFloat maximumDistance = 0;
		public override void Awake(){
			base.Awake();
			this.DefaultRate("LateUpdate");
			this.source.Setup("Source",this);
			this.target.Setup("Target",this);
			this.minimumDistance.Setup("Minimum Distance",this);
			this.maximumDistance.Setup("Maximum Distance",this);
		}
		public override void Use(){
			Transform source = this.source.Get().transform;
			Transform target = this.target.Get().transform;
			float distance = Vector3.Distance(source.position,target.position);
			if(distance.Between(this.minimumDistance,this.maximumDistance)){
				base.Use();
			}
			else{
				base.End();
			}
		}
	}
}