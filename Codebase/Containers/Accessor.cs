using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
public class Accessor{
	public FieldInfo field;
	public PropertyInfo property;
	public object scope;
	public Type type;
	public int index;
	public Accessor(object scope,string name) : this(scope,name,-1){}
	public Accessor(object scope,string name,int index){
		Type type = scope is Type ? (Type)scope : scope.GetType();
		this.scope = scope;
		this.index = index;
		this.field = type.GetField(name);
		this.property = type.GetProperty(name);
		this.type = this.Get().GetType();
	}
	public object Get(){
		return this.Get(this.scope,this.index);
	}
	public object Get(int index){
		return this.Get(this.scope,index);
	}
	public object Get(object scope){
		return this.Get(scope,this.index);
	}
	public object Get(object scope,int index){
		object current = this.field == null ? this.property.GetValue(scope,null) : this.field.GetValue(scope);
		if(index == -1){
			return current;
		}
		if(current.GetType() == typeof(Vector3)){
			return ((Vector3)current)[index];
		}
		return ((IList)current)[index];
	}
	public void Set(object value){
		this.Set(this.scope,value,this.index);
	}
	public void Set(object value,int index){
		this.Set(this.scope,value,index);
	}
	public void Set(object scope,object value){
		this.Set(scope,value,this.index);
	}
	public void Set(object scope,object value,int index){
		int number = 0;
		if(this.type.Equals(typeof(bool))){
			if(Int32.TryParse(value.ToString(),out number)){
				value = number == 0 ? false : true;
			}
			else if(value.ToString().ToLower() != "true"){
				value = false;
			}
		}
		if(value.GetType() != typeof(Vector3) && value.GetType() != this.type){
			value = Convert.ChangeType(value,this.type);
		}
		if(index == -1){
			if(this.field != null){
				this.field.SetValue(scope,value);
			}
			else if(this.property != null){
				this.property.SetValue(scope,value,null);
			}
		}
		else{
			object container = this.Get(scope,-1);
			if(container.GetType() != typeof(Vector3)){
				((Array)container).SetValue(value,index);
			}
			else{
				Vector3 adjust = (Vector3)container;
				adjust[index] = Convert.ToSingle(value);
				this.Set(scope,adjust,-1);
			}
		}
	}
}