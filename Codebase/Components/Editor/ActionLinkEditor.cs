﻿using Zios;
using UnityEngine;
using UnityEditor;
namespace Zios{
    [CustomEditor(typeof(ActionLink),true)]
    public class ActionLinkEditor : StateLinkEditor{
		public override StateTable GetTable(){
		    ActionLink script = (ActionLink)this.target;
			return script.actionTable;
		}
    }
}