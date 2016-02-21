using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Inputs{
	[Serializable]
	public class InputGroup{
		public string name;
		public List<InputAction> actions = new List<InputAction>();
		public static void Setup(){
			foreach(var group in InputManager.instance.groups){
				foreach(var action in group.actions){
					InputGroup.SetupTransition(action);
				}
			}
		}
		public static void SetupTransition(InputAction action){
			if(action.transition.IsNull() || action.transition.acceleration.keys.Length < 1){
				action.transition = new Actions.TransitionComponents.Transition();
			}
			action.transition.Setup("InputManager/"+action.name+"/",InputManager.instance);
		}
		public static void Save(){
			if(InputManager.instance.groups.Count < 1){return;}
			var contents = "";
			var file = FileManager.Find("InputControls.cfg",true,false) ?? FileManager.CreateFile("InputControls.cfg");
			foreach(var group in InputManager.instance.groups){
				foreach(var action in group.actions){
					var helpPath = FileManager.GetPath(action.helpImage);
					contents = contents.AddLine("["+group.name.ToPascalCase()+"-"+action.name.ToPascalCase()+"]");
					if(!helpPath.IsEmpty()){contents = contents.AddLine("HelpImage " + helpPath);}
					var transition = action.transition;
					if(transition.time.Get() != 0.5f){contents = contents.AddLine("Transition-Time " + transition.time.Get());}
					if(transition.speed.Get() != 3){contents = contents.AddLine("Transition-Speed " + transition.speed.Get());}
					if(transition.acceleration.Serialize() != "0-0-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Acceleration " + transition.acceleration.Serialize());}
					if(transition.acceleration.Serialize() != "0-0-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Acceleration " + transition.acceleration.Serialize());}
					if(transition.deceleration.Serialize() != "1-1-0-0|1-1-0-0"){contents = contents.AddLine("Transition-Deceleration " + transition.deceleration.Serialize());}
				}
			}
			file.WriteText(contents);
		}
		public static void Load(){
			if(!Application.isEditor){return;}
			var file = FileManager.Find("InputControls.cfg",true,false) ?? FileManager.CreateFile("InputControls.cfg");
			if(file.IsNull()){return;}
			string group = "";
			string action = "";
			var text = file.GetText().GetLines();
			foreach(var line in text){
				if(line.IsEmpty()){continue;}
				if(line.ContainsAll("[","-")){
					var parts = line.Remove("[","]").Split("-");
					group = parts[0].ToTitleCase().Trim();
					action = parts[1].ToTitleCase().Trim();
					continue;
				}
				var name = line.Parse(""," ").Trim();
				var value = line.Parse(" ").Trim();
				var inputGroup = InputManager.instance.groups.Find(x=>x.name.Trim()==group) ?? InputManager.instance.groups.AddNew();
				var inputAction = inputGroup.actions.Find(x=>x.name.Trim()==action) ?? inputGroup.actions.AddNew();
				inputGroup.name = group;
				inputAction.name = action;
				InputGroup.SetupTransition(inputAction);
				if(name.Contains("HelpImage")){inputAction.helpImage = FileManager.GetAsset<Sprite>(value);}
				if(name.Contains("Time")){inputAction.transition.time.Set(value.ToFloat());}
				if(name.Contains("Speed")){inputAction.transition.speed.Set(value.ToFloat());}
				if(name.Contains("Acceleration")){inputAction.transition.acceleration.Deserialize(value);}
				if(name.Contains("Deceleration")){inputAction.transition.deceleration.Deserialize(value);}
			}
		}
	}
}