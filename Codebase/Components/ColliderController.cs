﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class CollisionData{
	public bool isSource;
	public ColliderController sourceController;
	public GameObject gameObject;
	public Vector3 direction;
	public float force;
	public CollisionData(ColliderController controller,GameObject gameObject,Vector3 direction,float force,bool isSource){
		this.sourceController = controller;
		this.gameObject = gameObject;
		this.direction = direction;
		this.force = force;
		this.isSource = isSource;
	}
}
[AddComponentMenu("Zios/Component/General/Collider Controller")]
public class ColliderController : MonoBehaviour{
	static public Collider[] triggers;
	static public bool triggerSetup;
	static public bool HasTrigger(Collider collider){
		return Array.IndexOf(ColliderController.triggers,collider) != -1;
	}
	public List<Vector3> move = new List<Vector3>();
	public List<Vector3> lastMove = new List<Vector3>();
	public Dictionary<string,bool> blocked = new Dictionary<string,bool>();
	public Dictionary<string,float> lastBlockedTime = new Dictionary<string,float>();
	public bool[] freezePosition = new bool[3]{false,false,false};
	public float maxStepHeight = 0.15f;
	public float maxSlopeHeight = 0.15f;
	public float hoverWidth = 0.0001f;
	public float skinWidth = 0.0001f;
	public bool alt = false;
	public bool persistentBlockChecks;
	public void Awake(){
		Events.Add("OnMove",this.Move);
		this.ResetBlocked(true);
	}
	public void Start(){
		if(!ColliderController.triggerSetup){
			Collider[] colliders = (Collider[])Resources.FindObjectsOfTypeAll(typeof(Collider));
			List<Collider> triggers = new List<Collider>();
			foreach(Collider collider in colliders){
				if(collider.isTrigger){
					collider.isTrigger = false;
					triggers.Add(collider);
				}
			}
			ColliderController.triggers = triggers.ToArray();
			ColliderController.triggerSetup = true;
		}
	}
	public void OnEnable(){
		if(this.rigidbody == null){
			this.gameObject.AddComponent("Rigidbody");
		}	
		this.rigidbody.isKinematic = true;
		this.rigidbody.Sleep();
	}
	public void LateUpdate(){
		this.UpdatePosition();
	}
	public void ResetBlocked(bool clearTime=false){
		string[] names = new string[]{"forward","back","up","down","right","left"};
		foreach(string name in names){
			this.blocked[name] = false;
			if(clearTime){this.lastBlockedTime[name] = 0;}
		}
	}
	public float GetUnblockedDuration(string name){
		return Time.time - this.lastBlockedTime[name];
	}
	public void CheckBlocked(){
		if(this.persistentBlockChecks){
			RaycastHit hit;
			this.rigidbody.WakeUp();
			float distance = this.hoverWidth + 0.01f;
			this.blocked["forward"] = this.rigidbody.SweepTest(this.transform.forward,out hit,distance);
			this.blocked["back"] = this.rigidbody.SweepTest(-this.transform.forward,out hit,distance);
			this.blocked["up"] = this.rigidbody.SweepTest(this.transform.up,out hit,distance);
			this.blocked["down"] = this.rigidbody.SweepTest(-this.transform.up,out hit,distance);
			this.blocked["right"] = this.rigidbody.SweepTest(this.transform.right,out hit,distance);
			this.blocked["left"] = this.rigidbody.SweepTest(-this.transform.right,out hit,distance);
			this.rigidbody.Sleep();
		}
		foreach(var item in this.blocked){
			if(this.blocked[item.Key]){
				this.lastBlockedTime[item.Key] = Time.time;
			}
		}
	}
	public void Move(Vector3 move){
		if(this.freezePosition[0]){move.x = 0;}
		if(this.freezePosition[1]){move.y = 0;}
		if(this.freezePosition[2]){move.z = 0;}
		if(move != Vector3.zero){
			this.move.Add(move);
		}
	}
	public Vector3 NullBlocked(Vector3 move){
		if(this.blocked["left"] && move.x < 0){move.x = 0;}
		if(this.blocked["right"] && move.x > 0){move.x = 0;}
		if(this.blocked["up"] && move.y > 0){move.y = 0;}
		if(this.blocked["down"] && move.y < 0){move.y = 0;}
		if(this.blocked["forward"] && move.z > 0){move.z = 0;}
		if(this.blocked["back"] && move.z < 0){move.z = 0;}
		return move;
	}
	public void UpdatePosition(){
		if(this.move.Count > 0){
			Func<float,float> GetDistance = x => Mathf.Clamp(x-this.hoverWidth,this.hoverWidth,Mathf.Infinity);
			this.ResetBlocked();
			this.rigidbody.WakeUp();
			foreach(Vector3 current in this.move){
				Vector3 move = this.NullBlocked(current) * Time.deltaTime;
				if(move == Vector3.zero){continue;}
				RaycastHit hit,stepHit;
				Vector3 offset = ((CapsuleCollider)this.collider).center;
				Vector3 startPosition = this.rigidbody.position;
				Vector3 direction = move.normalized;
				float distance = Vector3.Distance(startPosition,startPosition+move);
				bool contact = this.rigidbody.SweepTest(direction,out hit,distance+this.hoverWidth);
				bool isTrigger = ColliderController.HasTrigger(hit.collider);
				if(contact && this.maxStepHeight != 0 && !isTrigger && move.y == 0){
					bool onGround = this.rigidbody.SweepTest(-this.transform.up,out stepHit,this.hoverWidth+0.01f);
					if(onGround){
						this.rigidbody.position = startPosition + (this.transform.up * this.maxStepHeight);
						this.Freeze();
						if(!this.rigidbody.SweepTest(direction,out stepHit,distance+this.hoverWidth)){
							this.rigidbody.position += direction * GetDistance(stepHit.distance);
							this.Freeze();
							this.rigidbody.SweepTest(-this.transform.up,out stepHit);
							this.rigidbody.position += (-this.transform.up * GetDistance(stepHit.distance));
							this.Freeze();
							continue;
						}
						this.rigidbody.position = startPosition;
					}
				}
				if(contact){
					if(isTrigger){
						hit.transform.gameObject.Call("OnTrigger",this.collider);
						continue;
					}
					this.rigidbody.position += direction * (hit.distance-this.hoverWidth);
					this.Freeze();
					CollisionData otherCollision = new CollisionData(this,this.gameObject,-direction,distance,false);
					CollisionData selfCollision = new CollisionData(this,hit.transform.gameObject,direction,distance,true);
					if(direction.z > 0){this.blocked["forward"] = true;}
					if(direction.z < 0){this.blocked["back"] = true;}
					if(direction.y > 0){this.blocked["up"] = true;}
					if(direction.y < 0){this.blocked["down"] = true;}
					if(direction.x > 0){this.blocked["right"] = true;}
					if(direction.x < 0){this.blocked["left"] = true;}
					hit.transform.gameObject.Call("OnCollide",otherCollision);
					this.gameObject.Call("OnCollide",selfCollision);
				}
				else{
					this.rigidbody.position = startPosition + move;
					this.Freeze();
				}
				/*Vector3 end = startPosition + move;
				Color color = contact ? Color.red : Color.green;
				this.alt = !this.alt;
				if(this.alt){color *= 0.5f;}
				Debug.DrawLine(startPosition,end,color,0.5f);*/
			}
			this.Freeze();
			this.transform.position = this.rigidbody.position;
			this.lastMove = this.move.Copy();
			this.move.Clear();
			this.rigidbody.Sleep();
		}
		this.CheckBlocked();
	}
	public void Freeze(){
		Vector3 position = this.rigidbody.position;
		if(this.freezePosition[0]){position.x = this.transform.position.x;}
		if(this.freezePosition[1]){position.y = this.transform.position.y;}
		if(this.freezePosition[2]){position.z = this.transform.position.z;}
		this.rigidbody.position = position;
	}
	public void OnDrawGizmosSelected(){
		Gizmos.color = Color.white;
		Vector3 raise = this.transform.up * this.maxStepHeight;
		Vector3 start = this.transform.position;
		Vector3 end = this.transform.position+(this.transform.forward*2);
		Gizmos.DrawLine(start+raise,end+raise);
	}
}
