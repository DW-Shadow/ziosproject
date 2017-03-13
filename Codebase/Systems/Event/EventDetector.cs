#pragma warning disable 0618
using UnityEngine;
namespace Zios.Event{
	[AddComponentMenu("")][ExecuteInEditMode]
	public class EventDetector : MonoBehaviour{
		private static bool showTime = false;
		private float loadStart;
		public static bool loading = true;
		public void Loading(){
			this.loadStart = Time.realtimeSinceStartup;
			EventDetector.loading = true;
		}
		public virtual void OnValidate(){
			this.Loading();
			Events.Call("On Validate");
		}
		public virtual void Awake(){
			Events.Add("On Asset Modifying",this.Loading);
			Events.Add("On Application Quit",this.Loading);
			Events.Add("On Enter Play",this.Loading);
			Events.Add("On Exit Play",this.Loading);
			Events.Register("On Awake");
			Events.Register("On Start");
			Events.Register("On Update");
			Events.Register("On Fixed Update");
			Events.Register("On Late Update");
			Events.Register("On Enable");
			Events.Register("On Disable");
			Events.Register("On GUI");
			Events.Register("On Destroy");
			Events.Register("On Validate");
			Events.Register("On Reset");
			Events.Register("On Player Connected");
			Events.Register("On Player Disconnected");
			Events.Register("On Level Was Loaded");
			Events.Register("On Master Server Event");
			Events.Register("On Application Quit");
			Events.Register("On Application Focus");
			Events.Register("On Application Pause");
			Events.Register("On Disconnected From Server");
			Events.Call("On Awake");
		}
		public virtual void Start(){Events.Call("On Start");}
		public virtual void Update(){
			Utility.CheckLoaded(false);
			if(!Application.isLoadingLevel && EventDetector.loading){
				Events.Call("On Level Was Loaded");
				float totalTime = Mathf.Max(Time.realtimeSinceStartup-this.loadStart,0);
				if(EventDetector.showTime){
					Debug.Log("[Scene] : Load complete -- " + (totalTime) + " seconds.");
				}
				this.loadStart = 0;
				EventDetector.loading = false;
			}
			Events.Call("On Update");
		}
		public virtual void FixedUpdate(){Events.Call("On Fixed Update");}
		public virtual void LateUpdate(){Events.Call("On Late Update");}
		public virtual void OnPlayerConnected(){Events.Call("On Player Connected");}
		public virtual void OnPlayerDisconnected(){Events.Call("On Player Disconnected");}
		//public virtual void OnLevelWasLoaded(int level){Event.Call("On Level Was Loaded",level);}
		public virtual void OnMasterServerEvent(){Events.Call("On Master Server Event");}
		public virtual void OnApplicationQuit(){Events.Call("On Application Quit");}
		public virtual void OnApplicationFocus(){Events.Call("On Application Focus");}
		public virtual void OnApplicationPause(){Events.Call("On Application Pause");}
		public virtual void OnDisconnectedFromServer(){Events.Call("On Disconnected From Server");}
		public virtual void OnGUI(){Events.Call("On GUI");}
		public virtual void OnEnable(){Events.Call("On Enable");}
		public virtual void OnDisable(){Events.Call("On Disable");}
		public virtual void OnDestroy(){
			Events.Call("On Destroy");
			Events.RemoveAll(this);
		}
		public virtual void Reset(){Events.Call("On Reset");}
	}
}