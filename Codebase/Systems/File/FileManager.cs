using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	using Containers;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	[InitializeOnLoad]
	public static class FileManager{
		private static string path;
		private static string dataPath;
		public static bool monitor = true;
		private static bool debug = false;
		private static bool clock = false;
		private static bool fullScan = true;
		private static float lastMonitor;
		private static Dictionary<string,FileMonitor> monitors = new Dictionary<string,FileMonitor>();
		public static Dictionary<string,List<FileData>> filesByPath = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,List<FileData>> filesByType = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData> folders = new Dictionary<string,FileData>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData[]> cache = new Dictionary<string,FileData[]>();
		public static Dictionary<UnityObject,object> assets = new Dictionary<UnityObject,object>();
		public static Hierarchy<Type,string,string,UnityObject> namedAssets = new Hierarchy<Type,string,string,UnityObject>();
		static FileManager(){
			FileManager.dataPath = Application.dataPath;
			FileManager.Refresh();
		}
		public static void Monitor(){
			if(!FileManager.monitor){return;}
			var time = FileManager.GetTime();
			if(time>FileManager.lastMonitor){
				foreach(var item in FileManager.monitors){
					if(item.Value.WasChanged()){
						FileManager.Refresh();
						break;
					}
				}
				FileManager.lastMonitor = time + 0.3f;
			}
		}
		//===============
		// Storage
		//===============
		public static float GetTime(){
			//if(FileManager.inThread){return 0;}
			return Time.realtimeSinceStartup;
		}
		public static void Load(){
			var time = FileManager.GetTime();
			if(FileManager.Exists("Temp/FileManager.data")){
				int mode = 0;
				string extension = "";
				string lastPath = "";
				var lines = File.ReadAllLines("Temp/FileManager.data");
				for(int index=0;index<lines.Length;++index){
					var line = lines[index];
					if(line.Contains("[Files]")){mode = 1;}
					else if(line.Contains("[Folders]")){mode = 2;}
					else if(line.StartsWith("(")){extension = line.Parse("(",")");}
					else if(line.StartsWith("=")){lastPath = line.Remove("=").Replace("$",FileManager.path);}
					else if(line.StartsWith("+")){lastPath += line.Remove("+");}
					else{
						var fileData = new FileData();
						fileData.directory = lastPath;
						fileData.name = line;
						if(mode == 1){
							fileData.fullName = fileData.name+"."+extension;
							fileData.path = fileData.directory+"/"+fileData.fullName;
							fileData.extension = extension;
						}
						else if(mode == 2){
							fileData.path = fileData.directory+"/"+fileData.name;
							fileData.isFolder = true;
						}
						FileManager.BuildCache(fileData);
					}
				}
			}
			if(FileManager.clock){Debug.Log("[FileManager] : Load cache complete -- " + (FileManager.GetTime()-time) + " seconds.");}
		}
		public static void Save(){
			string lastPath = ")@#(*$";
			var time = FileManager.GetTime();
			FileManager.Create("Temp");
			using(var output = new StreamWriter("Temp/FileManager.data",false)){
				output.WriteLine("[Files]");
				foreach(var item in FileManager.filesByType){
					var extension = item.Key;
					var files = item.Value;
					output.WriteLine("("+extension+")");
					foreach(var file in files){
						FileManager.SaveData(file,output,ref lastPath);
					}
				}
				output.WriteLine("[Folders]");
				foreach(var item in FileManager.folders){
					FileManager.SaveData(item.Value,output,ref lastPath);
				}
			}
			if(FileManager.clock){Debug.Log("[FileManager] : Save cache complete -- " + (FileManager.GetTime()-time) + " seconds.");}
		}
		public static void SaveData(FileData data,StreamWriter output,ref string lastPath){
			var directory = data.directory.Replace(FileManager.path,"$");
			if(directory == lastPath){}
			else if(directory.Contains(lastPath)){
				var addition = directory.Replace(lastPath,"");
				output.WriteLine("+"+addition);
				lastPath += addition;
			}
			else{
				output.WriteLine("="+directory);
				lastPath = directory;
			}
			output.WriteLine(data.name);
		}
		//===============
		// Setup
		//===============
		public static void Refresh(){
			var time = FileManager.GetTime();
			Event.Add("On Editor Update",FileManager.Monitor).SetPermanent();
			Event.Add("On Asset Changed",FileManager.Refresh).SetPermanent();
			FileManager.assets.Clear();
			FileManager.filesByPath.Clear();
			FileManager.filesByType.Clear();
			FileManager.folders.Clear();
			FileManager.cache.Clear();
			FileManager.path = FileManager.dataPath.GetDirectory();
			FileManager.Scan(FileManager.path);
			FileManager.Scan(FileManager.path+"/Temp",true);
			if(FileManager.fullScan){FileManager.Scan(FileManager.dataPath,true);}
			FileManager.Save();
			if(FileManager.clock){Debug.Log("[FileManager] : Refresh complete -- " + (FileManager.GetTime()-time) + " seconds.");}
		}
		public static void Scan(string directory,bool deep=false){
			if(!Directory.Exists(directory)){return;}
			string[] fileEntries = Directory.GetFiles(directory);
			string[] folderEntries = Directory.GetDirectories(directory);
			if(!FileManager.monitors.ContainsKey(directory)){
				FileManager.monitors[directory] = new FileMonitor(directory);
			}
			FileManager.filesByPath.AddNew(directory);
			foreach(string filePath in fileEntries){
				if(filePath.ContainsAny(".meta","unitytemp","unitylock")){continue;}
				var path = filePath.Replace("\\","/");
				FileManager.BuildCache(new FileData(path));
			}
			foreach(string folderPath in folderEntries){
				if(folderPath.ContainsAny(".svn","~")){continue;}
				var path = folderPath.Replace("\\","/");
				FileManager.BuildCache(new FileData(path,true));
				if(deep){FileManager.Scan(path,true);}
			}
		}
		public static void BuildCache(FileData file){
			FileManager.cache["!"+file.path.ToLower()] = file.AsArray();
			if(!file.isFolder){
				FileManager.cache["!"+file.fullName.ToLower()] = file.AsArray();
				FileManager.filesByType.AddNew(file.extension).Add(file);
				FileManager.filesByPath.AddNew(file.directory).Add(file);
				return;
			}
			FileManager.folders[file.path] = file;
		}
		//===============
		// Primary
		//===============
		public static FileData[] FindAll(string name,bool showWarnings=true,bool firstOnly=false){
			name = name.Replace("\\","/");
			var time = FileManager.GetTime();
			if(name == "" && showWarnings){
				Debug.LogWarning("[FileManager] No path given for search.");
				return null;
			}
			string searchKey = name.ToLower();
			if(FileManager.cache.ContainsKey(searchKey)){
				return FileManager.cache[searchKey];
			}
			if(name.StartsWith("!")){name = name.ReplaceFirst("!","");}
			if(name.ContainsAny("<",">","?",":","|")){
				if(name[1] != ':') {
					if(FileManager.debug){Debug.LogWarning("[FileManager] Path has invalid characters -- " + name);}
					return new FileData[0];
				}
			}
			if(!name.Contains(".") && name.EndsWith("*")){name = name + ".*";}
			if(name.Contains("*")){firstOnly = false;}
			else if(name.Contains(":")){firstOnly = true;}
			string fileName = name.GetFileName();
			string path = name.GetDirectory();
			string type = name.GetFileExtension().ToLower();
			var results = new List<FileData>();
			var types = new List<string>();
			var allTypes = FileManager.filesByType.Keys;
			if(type.IsEmpty() || type == "*"){types = allTypes.ToList();}
			else if(type.StartsWith("*")){types.AddRange(allTypes.Where(x=>x.EndsWith(type.Remove("*"),true)));}
			else if(type.EndsWith("*")){types.AddRange(allTypes.Where(x=>x.StartsWith(type.Remove("*"),true)));}
			else if(FileManager.filesByType.ContainsKey(type)){types.Add(type);}
			foreach(var typeName in types){
				FileManager.SearchType(fileName,typeName,path,firstOnly,ref results);
			}
			if(results.Count == 0){
				foreach(var item in FileManager.folders){
					FileData folder = item.Value;
					string folderPath = item.Key;
					if(folderPath.Matches(name,true)){
						results.Add(folder);
					}
				}
			}
			if(results.Count == 0 && showWarnings){Debug.LogWarning("[FileManager] Path [" + name + "] could not be found.");}
			FileManager.cache[searchKey] = results.ToArray();
			if(FileManager.clock){Debug.Log("[FileManager] : Find [" + name + "] complete (" + results.Count + ") -- " + (FileManager.GetTime()-time) + " seconds.");}
			return results.ToArray();
		}
		public static void SearchType(string name,string type,string path,bool firstOnly,ref List<FileData> results){
			bool pathSearch = !path.IsEmpty() && FileManager.filesByPath.ContainsKey(path);
			var files = pathSearch ? FileManager.filesByPath[path] : FileManager.filesByType[type];
			if(FileManager.debug){Debug.Log("[FileManager] Search -- " + name + " -- " + type + " -- " + path);}
			foreach(FileData file in files){
				bool correctPath = pathSearch ? true : file.path.Contains(path+"/",true);
				bool correctType = !pathSearch ? true : file.extension.Matches(type,true);
				bool wildcard = name.IsEmpty() || name == "*";
				wildcard = wildcard || name.StartsWith("*") && file.name.EndsWith(name.Remove("*"),true);
				wildcard = wildcard || (name.EndsWith("*") && file.name.StartsWith(name.Remove("*"),true));
				if(correctPath && correctType && (wildcard || file.name.Matches(name,true))){
					results.Add(file);
					if(firstOnly){return;}
				}
			}
		}
		public static string GetPath(UnityObject target,bool relative=true){
			#if UNITY_EDITOR
			if(Application.isEditor){
				string assetPath = AssetDatabase.GetAssetPath(target);
				if(!relative){assetPath = FileManager.dataPath.Replace("Assets","") + assetPath;}
				return assetPath;
			}
			#endif
			return "";
		}
		public static T GetAsset<T>(UnityObject target){
			#if UNITY_EDITOR
			if(Application.isEditor){
				if(!FileManager.assets.ContainsKey(target)){
					string assetPath = FileManager.GetPath(target);
					object asset = AssetDatabase.LoadAssetAtPath(assetPath,typeof(T));
					if(asset == null){return default(T);}
					FileManager.assets[target] = Convert.ChangeType(asset,typeof(T));
				}
				return (T)FileManager.assets[target];
			}
			#endif
			return default(T);
		}
		public static FileData Create(string path){
			path = Path.GetFullPath(path).Replace("\\","/");
			var data = new FileData(path);
			if(!data.name.IsEmpty()){
				File.Create(path).Dispose();
			}
			else{
				data.isFolder = true;
				Directory.CreateDirectory(path);
			}
			FileManager.BuildCache(data);
			return data;
		}
		public static void Copy(string path,string destination){
			File.Copy(path,destination,true);
		}
		public static void Delete(string path){
			var file = FileManager.Find(path);
			if(!file.IsNull()){
				file.Delete();
			}
		}
		public static void WriteFile(string path,byte[] bytes){
			var folder = path.GetDirectory();
			if(!FileManager.Exists(folder)){FileManager.Create(folder);}
			FileStream stream = new FileStream(path,FileMode.Create);
			BinaryWriter file = new BinaryWriter(stream);
			file.Write(bytes);
			file.Close();
			stream.Close();
		}
		//===============
		// Shorthand
		//===============
		public static bool Exists(string path){return File.Exists(path) || Directory.Exists(path);}
		public static FileData Find(string name,bool showWarnings=true){
			name = !name.ContainsAny("*") ? "!"+name : name;
			var results = FileManager.FindAll(name,showWarnings,true);
			if(results.Length > 0){return results[0];}
			return null;
		}
		public static FileData Get(UnityObject target,bool showWarnings=false){
			string path = FileManager.GetPath(target,false);
			return FileManager.Find(path,showWarnings);
		}
		public static string GetGUID(string name,bool showWarnings=true){
			FileData file = FileManager.Find(name,showWarnings);
			if(file != null){return file.GetGUID();}
			return "";
		}
		public static string GetGUID(UnityObject target){
			#if UNITY_EDITOR
			return AssetDatabase.AssetPathToGUID(FileManager.GetPath(target));
			#else
			return "";
			#endif
		}
		public static T GetAsset<T>(string name,bool showWarnings=true) where T : UnityObject{
			FileData file = FileManager.Find(name,showWarnings);
			if(file != null){return file.GetAsset<T>();}
			return default(T);
		}
		public static T[] GetAssets<T>(string name="*",bool showWarnings=true) where T : UnityObject{
			var files = FileManager.FindAll(name,true,showWarnings);
			if(files.Length < 1){return new T[0];}
			return files.Select(x=>x.GetAsset<T>()).Where(x=>!x.IsNull()).ToArray();
		}
		public static Dictionary<string,T> GetNamedAssets<T>(string name="*",bool showWarnings=true) where T : UnityObject{
			if(!FileManager.namedAssets.AddNew(typeof(T)).ContainsKey(name)){
				var files = FileManager.GetAssets<T>(name,showWarnings).GroupBy(x=>x.name).Select(x=>x.First());
				FileManager.namedAssets[typeof(T)][name] = files.ToDictionary(x=>x.name,x=>(UnityObject)x);
			}
			return FileManager.namedAssets[typeof(T)][name].ToDictionary(x=>x.Key,x=>(T)x.Value);
		}
	}
	public class FileMonitor{
		public string path;
		public DateTime lastModify;
		public FileMonitor(string path){
			this.path = path;
			this.lastModify = Directory.GetLastWriteTime(this.path);
		}
		public bool WasChanged(){
			var modifyTime = Directory.GetLastWriteTime(this.path);
			if(this.lastModify != modifyTime){
				this.lastModify = modifyTime;
				return true;
			}
			return false;
		}
	}
	[Serializable]
	public class FileData{
		public string path;
		public string directory;
		public string name;
		public string fullName;
		public string extension;
		public bool isFolder;
		public FileData(){}
		public FileData(string path,bool isFolder=false){
			this.path = path;
			this.directory = path.GetDirectory();
			this.name = path.GetFileName();
			this.extension = isFolder ? "" : path.GetFileExtension();
			this.fullName = isFolder ? this.name : this.name + "." + this.extension;
			this.isFolder = isFolder;
		}
		public string GetText(){return File.ReadAllText(this.path);}
		public void WriteText(string contents){File.WriteAllText(this.path,contents);}
		public void Delete(bool cacheOnly=false){
			foreach(var item in FileManager.cache.Copy()){
				if(item.Value.Contains(this)){
					FileManager.cache[item.Key] = item.Value.Remove(this);
					if(FileManager.cache[item.Key].Length < 1){
						FileManager.cache.Remove(item.Key);
					}
				}
			}
			if(!this.isFolder){
				if(!cacheOnly){File.Delete(this.path);}
				FileManager.filesByType[this.extension].Remove(this);
				FileManager.filesByPath[this.directory].Remove(this);
				return;
			}
			if(!cacheOnly){Directory.Delete(this.path);}
			FileManager.folders.Remove(this.path);
		}
		public void MarkDirty(){File.SetLastWriteTime(this.path,DateTime.Now);}
		public string GetModifiedDate(string format="M-d-yy"){return File.GetLastWriteTime(this.path).ToString(format);}
		public string GetAccessedDate(string format="M-d-yy"){return File.GetLastAccessTime(this.path).ToString(format);}
		public string GetCreatedDate(string format="M-d-yy"){return File.GetCreationTime(this.path).ToString(format);}
		public string GetChecksum(){return this.GetText().ToMD5();}
		public long GetSize(){return new FileInfo(this.path).Length;}
		public T GetAsset<T>() where T : UnityObject{
			#if UNITY_EDITOR
			if(Application.isEditor && this.path.IndexOf("Assets") != -1){
				return AssetDatabase.LoadAssetAtPath<T>(this.GetAssetPath());
			}
			#endif
			return default(T);
		}
		public string GetGUID(){
			#if UNITY_EDITOR
			if(Application.isEditor){
				return AssetDatabase.AssetPathToGUID(this.GetAssetPath());
			}
			#endif
			return "";
		}
		public string GetAssetPath(){return this.path.GetAssetPath();}
		public string GetFolderPath(){
			return this.path.Substring(0,this.path.LastIndexOf("/")) + "/";
		}
	}
}