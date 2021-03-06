using UnityEngine;
using UnityEngine.UI;
namespace Zios.Interface{
	[AddComponentMenu("Zios/Singleton/Debug/FPS (GUI)")]
	public class FpsGUI : MonoBehaviour{
		public Text fpsText;
		public Text frameTimeText;
		private int frames = 0;
		private float frameTime;
		private float lastUpdate;
		private float nextUpdate;
		public void Update(){
			this.frames += 1;
			if(Time.time >= this.nextUpdate){
				this.nextUpdate = Time.time + 1;
				this.fpsText.text = this.frames.ToString();
				this.frameTimeText.text = ((Time.time - this.frameTime) * 1000).ToString("0.0") + " ms";
				this.frames = 0;
			}
			this.frameTime = Time.time;
		}
	}
}