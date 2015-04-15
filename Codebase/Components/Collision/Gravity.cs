﻿using Zios;
using UnityEngine;
using System.Collections;
namespace Zios{
    [RequireComponent(typeof(ColliderController))]
    [AddComponentMenu("Zios/Component/Physics/Gravity")]
    public class Gravity : ManagedMonoBehaviour{
	    public AttributeVector3 intensity = new Vector3(0,-9.8f,0);
	    public AttributeFloat scale = 1.0f;
	    public AttributeBool disabled = false;
	    public override void Awake(){
		    base.Awake();
		    this.intensity.Setup("Intensity",this);
		    this.disabled.Setup("Disabled",this);
		    this.scale.Setup("Scale",this);
	    }
	    public override void Step(){
		    if(!this.disabled){
			    Vector3 amount = (this.intensity*this.scale);
			    this.gameObject.CallEvent("Add Force",amount);
		    }
	    }
    }
}
