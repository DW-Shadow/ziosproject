﻿using Zios;
using UnityEngine;
    namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Rotate/Towards/Rotate Towards Target")]
    public class RotateTowardsTarget : RotateTowardsPoint{
	    public AttributeGameObject target = new AttributeGameObject();
	    public override void Awake(){
		    base.Awake();
		    this.target.Setup("Target",this);
			this.goal.showInEditor = false;;
	    }
	    public override void Use(){
			Vector3 goalPosition = this.target.Get().transform.position;
			this.goal.Set(goalPosition);
		    base.Use();
	    }
    }
}
