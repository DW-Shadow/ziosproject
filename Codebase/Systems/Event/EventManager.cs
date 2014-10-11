using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
public static class Events{
	private static Dictionary<GameObject,Dictionary<string,List<object>>> objectEvents = new Dictionary<GameObject,Dictionary<string,List<object>>>();
	private static Dictionary<string,List<object>> events = new Dictionary<string,List<object>>();
	static Events(){
		Events.objectEvents.Clear();
		Events.events.Clear();
	}
	public static void AddGet(string name,MethodStringReturn method){Events.Add(name,(object)method);}
	public static void AddGet(string name,MethodReturn method){Events.Add(name,(object)method);}
	public static void Add(string name,Method method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodObject method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFull method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodString method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodInt method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodFloat method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodBool method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector2 method){Events.Add(name,(object)method);}
	public static void Add(string name,MethodVector3 method){Events.Add(name,(object)method);}
	public static void AddGetTarget(string name,MethodReturn method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,Method method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodObject method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodFull method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodString method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodInt method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodFloat method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodBool method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodVector2 method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,MethodVector3 method,GameObject target){Events.AddTarget(name,(object)method,target);}
	public static void AddTarget(string name,object method,GameObject target){
		if(!Events.objectEvents.ContainsKey(target)){
			Events.objectEvents[target] = new Dictionary<string,List<object>>();
		}
		if(!Events.objectEvents[target].ContainsKey(name)){
			Events.objectEvents[target][name] = new List<object>();
		}
		if(!Events.objectEvents[target][name].Contains(method)){
			Events.objectEvents[target][name].Add(method);
		}
	}	
	public static void Add(string name,object method){
		object methodTarget = ((Delegate)method).Target;
		if(!Events.events.ContainsKey(name)){
			Events.events[name] = new List<object>();
		}
		if(!Events.events[name].Contains(method)){
			Events.events[name].Add(method);
		}
		if(methodTarget != null){
			Type type = methodTarget.GetType();
			if(type.IsSubclassOf((typeof(MonoBehaviour)))){
				Events.AddTarget(name,method,((MonoBehaviour)methodTarget).gameObject);
			}
		}
	}
	public static void Handle(object callback,object[] values){
		object value = values.Length > 0 ? values[0] : null;
		if(callback is MethodFull){
			((MethodFull)callback)(values);
		}
		else if(value == null || callback is Method){
			((Method)callback)();
		}
		else if(value is object && callback is MethodObject){
			((MethodObject)callback)((object)value);
		}
		else if(value is int && callback is MethodInt){
			((MethodInt)callback)((int)value);
		}
		else if(value is float && callback is MethodFloat){
			((MethodFloat)callback)((float)value);
		}
		else if(value is string && callback is MethodString){
			((MethodString)callback)((string)value);
		}
		else if(value is bool && callback is MethodBool){
			((MethodBool)callback)((bool)value);
		}
		else if(value is Vector2 && callback is MethodVector2){
			((MethodVector2)callback)((Vector2)value);
		}
		else if(value is Vector3 && callback is MethodVector3){
			((MethodVector3)callback)((Vector3)value);
		}
	}
	public static object HandleGet(object callback,object[] values){
		object value = values.Length > 0 ? values[0] : null;
		if(callback is MethodReturn){
			return ((MethodReturn)callback)();
		}
		else if(value is object && callback is MethodObjectReturn){
			return ((MethodObjectReturn)callback)((object)value);
		}
		else if(value is int && callback is MethodIntReturn){
			return ((MethodIntReturn)callback)((int)value);
		}
		else if(value is float && callback is MethodFloatReturn){
			return ((MethodFloatReturn)callback)((float)value);
		}
		else if(value is string && callback is MethodStringReturn){
			return ((MethodStringReturn)callback)((string)value);
		}
		else if(value is bool && callback is MethodBoolReturn){
			return ((MethodBoolReturn)callback)((bool)value);
		}
		else if(value is Vector2 && callback is MethodVector2Return){
			return ((MethodVector2Return)callback)((Vector2)value);
		}
		else if(value is Vector3 && callback is MethodVector3Return){
			return ((MethodVector3Return)callback)((Vector3)value);
		}
		return null;
	}
	public static object Query(string name,object result=null,params object[] values){
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				return Events.HandleGet(callback,values);
			}
		}
		return result;
	}
	public static object Query(GameObject target,string name,object[] values,object result=null){
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				foreach(object callback in Events.objectEvents[target][name]){
					return Events.HandleGet(callback,values);
				}
			}
		}
		return result;
	}
	public static object QueryChildren(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(result != null){return result;}
		if(self){Events.Query(target,name,values,result);}
		Transform[] children = target.GetComponentsInChildren<Transform>();
		foreach(Transform transform in children){
			if(transform.gameObject == target){continue;}
			result = Events.QueryChildren(transform.gameObject,name,values,result,true);
			if(result != null){return result;}
		}
		return result;
	}
	public static object QueryParents(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(result != null){return result;}
		if(self){Events.Query(target,name,values,result);}
		Transform parent = target.transform.parent;
		while(parent != null){
			result = Events.QueryParents(parent.gameObject,name,values,result,true);
			parent = parent.parent;
			if(result != null){return result;}
		}
		return result;
	}
	public static object QueryFamily(GameObject target,string name,object[] values,object result=null,bool self=false){
		if(self){result = Events.Query(target,name,values,result);}
		result = Events.QueryChildren(target,name,values,result,false);
		result = Events.QueryParents(target,name,values,result,false);
		return result;
	}
	public static void Call(string name,params object[] values){
		if(Events.events.ContainsKey(name)){
			foreach(object callback in Events.events[name]){
				Events.Handle(callback,values);
			}
		}
	}
	public static void Call(GameObject target,string name,object[] values){
		if(Events.objectEvents.ContainsKey(target)){
			if(Events.objectEvents[target].ContainsKey(name)){
				foreach(object callback in Events.objectEvents[target][name]){
					Events.Handle(callback,values);
				}
			}
		}
	}
	public static void CallChildren(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Transform[] children = target.GetComponentsInChildren<Transform>();
		foreach(Transform transform in children){
			if(transform.gameObject == target){continue;}
			Events.CallChildren(transform.gameObject,name,values,true);
		}
	}
	public static void CallParents(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Transform parent = target.transform.parent;
		while(parent != null){
			Events.CallParents(parent.gameObject,name,values,true);
			parent = parent.parent;
		}
	}
	public static void CallFamily(GameObject target,string name,object[] values,bool self=false){
		if(self){Events.Call(target,name,values);}
		Events.CallChildren(target,name,values);
		Events.CallParents(target,name,values);
	}
}
public static class GameObjectEvents{
	public static object Query(this GameObject current,string name,params object[] values){
		return Events.Query(current,name,values);
	}
	public static object QueryChildren(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryChildren(current,name,values,null,self);
	}
	public static object QueryParents(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryParents(current,name,values,null,self);
	}
	public static object QueryFamily(this GameObject current,string name,bool self=true,params object[] values){
		return Events.QueryFamily(current,name,values,null,self);
	}
	public static void Call(this GameObject current,string name,params object[] values){
		Events.Call(current,name,values);
	}
	public static void CallChildren(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallChildren(current,name,values,self);
	}
	public static void CallParents(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallParents(current,name,values,self);
	}
	public static void CallFamily(this GameObject current,string name,bool self=true,params object[] values){
		Events.CallFamily(current,name,values,self);
	}
}