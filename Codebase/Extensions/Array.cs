using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Zios{
	public static class ArrayExtension{
		//=======================
		// Default
		//=======================
		public static T[] Convert<T>(this Array current){
			List<T> casted = new List<T>();
			Type type = typeof(T);
			foreach(var item in current){
				if(type == typeof(Single)){
					float value = System.Convert.ToSingle(item);
					casted.Add((T)System.Convert.ChangeType(value,type));
				}
				else{
					casted.Add((T)item);
				}
			}
			return casted.ToArray();
		}
		public static List<T> ToList<T>(this T[] current){
			return new List<T>(current);
		}
		public static int IndexOf<T>(this Array current,T value){
			return Array.IndexOf(current,value);
		}
		public static int IndexOf<T>(this Array current,Enum enumerable){
			string name = enumerable.ToString();
			return current.IndexOf(name);
		}
		public static T[] Copy<T>(this T[] current){
			T[] result = new T[current.Length];
			current.CopyTo(result,0);
			return result;
		}
		public static T[] Concat<T>(this T[] current,T[] list){
			T[] result = new T[current.Length + list.Length];
			current.CopyTo(result,0);
			list.CopyTo(result,current.Length);
			return result;
		}
		public static bool Exists<T>(this T[] current,Predicate<T> predicate){
			return Array.Exists(current,predicate);
		}
		public static T Find<T>(this T[] current,Predicate<T> predicate){
			return Array.Find(current,predicate);
		}
		public static void Clear<T>(this T[] current){
			Array.Clear(current,0,current.Length);
		}
		public static T[] Add<T>(this T[] current,T element){
			T[] extra = new T[]{element};
			return current.Concat(extra);
		}
		public static T[] Remove<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.Remove(value);
			return copy.ToArray();
		}
		public static T[] RemoveAt<T>(this T[] current,int index){
			List<T> copy = new List<T>(current);
			copy.RemoveAt(index);
			return copy.ToArray();
		}
		public static T[] RemoveAll<T>(this T[] current,T value){
			List<T> copy = new List<T>(current);
			copy.RemoveAll(x=>x.Equals(value));
			return copy.ToArray();
		}
		public static T[] Resize<T>(this T[] current,int newSize){
			while(current.Length > newSize){
				current = current.RemoveAt(current.Length-1);
			}
			while(current.Length < newSize){
				current = current.Add(default(T));
			}
			return current;
		}
		public static T[] Order<T>(this T[] current){
			var copy = current.Copy().ToList();
			copy.Sort();
			return copy.ToArray();
		}
		public static bool HasAny<T>(this T[] current,params T[] values){return current.ContainsAny(values);}
		public static bool HasAll<T>(this T[] current,params T[] values){return current.ContainsAll(values);}
		public static bool ContainsAny<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(current.Contains(values[index])){return true;}
			}
			return false;
		}
		public static bool ContainsAll<T>(this T[] current,params T[] values){
			for(int index=0;index<values.Length;++index){
				if(!current.Contains(values[index])){return false;}
			}
			return true;
		}
		public static void ForEach<T>(this T[] current,Action<T> method){
			for(int index=0;index<current.Length;++index){
				method(current[index]);
			}
		}
		//=======================
		// Float
		//=======================
		public static float[] Scale(this float[] current,float scalar){
			float[] result = current;
			for(int index=0;index<current.Length;++index){
				result[index] = current[index] * scalar;
			}
			return result;
		}
		public static Color ToColor(this float[] current){
			if(current.Length >= 3){
				float r = current[0];
				float g = current[1];
				float b = current[2];
				if(current.Length > 3){
					return new Color(r,g,b,current[3]);
				}
				return new Color(r,g,b);
			}
			return Color.white;
		}
		public static Vector2 ToVector2(this float[] current){
			if(current.Length >= 2){
				float a = current[0];
				float b = current[1];
				return new Vector2(a,b);
			}
			return Vector2.zero;
		}
		public static Vector3 ToVector3(this float[] current){
			if(current.Length >= 3){
				float a = current[0];
				float b = current[1];
				float c = current[2];
				return new Vector3(a,b,c);
			}
			return Vector3.zero;
		}
		public static Vector4 ToVector4(this float[] current){
			if(current.Length >= 4){
				float a = current[0];
				float b = current[1];
				float c = current[2];
				float d = current[3];
				return new Vector4(a,b,c,d);
			}
			return Vector4.zero;
		}
		public static Rect ToRect(this float[] current){
			Rect result = new Rect();
			for(int index=0;index<current.Length;++index){
				if(index > 3){break;}
				if(index == 0){result.x = current[index];}
				if(index == 1){result.y = current[index];}
				if(index == 2){result.width = current[index];}
				if(index == 3){result.height = current[index];}
			}
			return result;
		}
	}
}