using Zios;
using System;
using System.Linq;
using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/Action/Spherecast (Target)")]
	public class SphereCastTarget : StateMonoBehaviour{
		public AttributeGameObject source = new AttributeGameObject();
		public AttributeFloat radius = 1;
		public LayerMask layers = -1;
		[Advanced] public Color debugColor = new Color(1,1,1,0.4f);
		[HideInInspector] public AttributeGameObject hits = new AttributeGameObject();
		[Internal] public GameObject[] hitList = new GameObject[0];
		public override void Awake(){
			base.Awake();
			this.source.Setup("Source",this);
			this.radius.Setup("Radius",this);
			this.hits.Setup("Hits",this);
			this.hits.info.mode = AttributeMode.Linked;
			this.hits.getMethod = ()=>{return this.hitList.FirstOrDefault();};
			this.hits.enumerateMethod = ()=>{return this.hitList.Select(x=>x).GetEnumerator();};
		}
		public override void Use(){
			Vector3 sourcePosition = this.source.Get().transform.position;
			this.hitList = Physics.OverlapSphere(sourcePosition,this.radius,this.layers.value).Select(x=>x.gameObject).ToArray();
			bool state = this.hitList.Length > 0;
			this.Toggle(state);
		}
		public void OnDrawGizmosSelected(){
			Gizmos.color = this.debugColor;
			if(!this.source.Get().IsNull()){
				Vector3 sourcePosition = this.source.Get().transform.position;
				Gizmos.DrawSphere(sourcePosition,this.radius);
			}
		}
	}
}