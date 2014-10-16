using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Zios.Action;
using ActionPart = Zios.ActionPart;
public static class EventUtility{
	public static void Add(MonoBehaviour script,string name,object callback,bool useOwner=true,bool useAction=true){
		if(script is ActionPart || script is Action){
			Action parent = script is ActionPart ? ((ActionPart)script).action : (Action)script;
			GameObject action = parent.gameObject;
			GameObject owner = parent.owner;
			GameObject[] actionGroup = new GameObject[]{action};
			GameObject[] ownerGroup = new GameObject[]{owner};
			bool multiscope = name.Contains("*");
			string fullName = parent.alias.Strip(" ");
			string general = name.Replace("*","");
			string specific = name.Replace("*",fullName);
			if(useAction){
				Events.AddScope(general,callback,actionGroup);
				if(multiscope){Events.AddScope(specific,callback,actionGroup);}
			}
			if(useOwner){
				Events.AddScope(general,callback,ownerGroup);
				if(multiscope){Events.AddScope(specific,callback,ownerGroup);}
			}
		}
	}
	public static void AddGet(MonoBehaviour script,string name,MethodStringReturn method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddGet(MonoBehaviour script,string name,MethodReturn method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,Method method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodObject method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodFull method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodString method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodInt method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodFloat method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodBool method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodVector2 method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void Add(MonoBehaviour script,string name,MethodVector3 method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,Method method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodObject method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodFull method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodString method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodInt method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodFloat method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodBool method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodVector2 method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
	public static void AddSet(MonoBehaviour script,string name,MethodVector3 method,bool useOwner=true,bool useAction=true){EventUtility.Add(script,name,(object)method,useOwner,useAction);}
}