using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
namespace Zios{
	public static class IEnumerableExtension{
		//=======================
		// Conversion
		//=======================
		public static To[] ConvertAll<To>(this IEnumerable<string> current){
			return current.ConvertAll<string,To>();
		}
		public static To[] ConvertAll<From,To>(this IEnumerable<From> current){
			var source = current.ToArray<From>();
			return Array.ConvertAll(source,x=>x.Convert<To>()).ToArray();
		}
		public static Dictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> current){
			return current.ToDictionary(x=>x.Key,x=>x.Value);
		}
		public static string ToText<T>(this IEnumerable<T> current){
			var value = new StringBuilder();
			foreach(var item in current){
				value.Append(item.ToString());
				value.Append(" | ");
			}
			return value.ToString().TrimRight(" | ");
		}
		public static string ToString<T>(this IEnumerable<T> current,string separator=" ",string endTerm="or"){
			string result = "";
			foreach(var item in current){
				bool isLast = current.Last().Equals(item);
				if(isLast){result += endTerm;}
				result += item.ToString();
				if(!isLast){result += separator;}
			}
			return result;
		}
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> current){
		   return new HashSet<T>(current);
		}
		//=======================
		// General
		//=======================
		public static bool ContainsAll<T>(this IEnumerable<T> current,IEnumerable<T> other){
			return !other.Except(current).Any();
		}
		public static IEnumerable<T> Diff<T>(this IEnumerable<T> current,IEnumerable<T> other){
			return current.Except(other).Concat(other.Except(current));
		}
		public static string Serialize<T>(this IEnumerable<T> current){
			string output = "";
			foreach(var value in current){
				output += value.SerializeAuto()+"-";
			}
			return output.TrimRight("-");
		}
		public static IEnumerable<T> Deserialize<T>(this IEnumerable<T> current,string value){
			return value.Split("-").Select(x=>x.Deserialize<T>()).ToArray();
		}
		public static IEnumerable<T> Unshift<T>(this IEnumerable<T> current,T item){
			var result = current.ToList();
			result.Insert(0,item);
			return result;
		}
		public static IEnumerable<T> ReverseOrder<T>(this IEnumerable<T> current){
			current.Reverse();
			return current;
		}
		//=======================
		// LINQ-ish
		//=======================
		public static List<Type> If<Type>(this IEnumerable<Type> current,Func<Type,bool> comparer){
			var results = new List<Type>();
			foreach(var item in current){
				if(comparer(item)){
					results.Add(item);
				}
			}
			return results;
		}
		public static IEnumerable<Type> SkipLast<Type>(this IEnumerable<Type> current){
			return current.SkipRight(1);
		}
		public static IEnumerable<Type> SkipRight<Type>(this IEnumerable<Type> current,int amount){
			return current.Take(current.Count() - amount);
		}
		public static IEnumerable<Type> TakeRight<Type>(this IEnumerable<Type> current,int amount){
			return current.Skip(current.Count() - amount).Take(amount);
		}
		//=======================
		// String
		//=======================
		public static string Join(this IEnumerable<string> current,string separator=" "){
			return string.Join(separator,current.ToArray());
		}
		public static List<string> Filter(this IEnumerable<string> current,string text){
			List<string> newList = new List<string>();
			bool wildcard = text.Contains("*");
			text = text.Replace("*","");
			foreach(string item in current){
				if(wildcard && item.Contains(text)){
					newList.Add(item);
				}
				else if(item == text){
					newList.Add(item);
				}
			}
			return newList;
		}
		public static List<string> Replace(this IEnumerable<string> current,string replace,string with,bool ignoreCase=true){
			List<string> results = new List<string>();
			foreach(string item in current){
				results.Add(item.Replace(replace,with));
			}
			return results;
		}
		public static List<string> AddSuffix(this IEnumerable<string> current,string suffix){
			List<string> results = new List<string>();
			foreach(string item in current){
				results.Add(item+suffix);
			}
			return results;
		}
		public static string[] Trim(this IEnumerable<string> current,string values){return current.Select(x=>x.Trim(values)).ToArray();}
		public static string[] ToTitleCase(this IEnumerable<string> current){return current.Select(x=>x.ToTitleCase()).ToArray();}
		public static string[] ToCamelCase(this IEnumerable<string> current){return current.Select(x=>x.ToCamelCase()).ToArray();}
		public static string[] ToPascalCase(this IEnumerable<string> current){return current.Select(x=>x.ToPascalCase()).ToArray();}
		public static int[] ToInt(this IEnumerable<string> current){return current.Select(x=>x.ToInt()).ToArray();}
		public static bool[] ToBool(this IEnumerable<string> current){return current.Select(x=>x.ToBool()).ToArray();}
		public static float[] ToFloat(this IEnumerable<string> current){return current.Select(x=>x.ToFloat()).ToArray();}
		public static UnityEngine.Color[] ToColor(this IEnumerable<string> current){return current.Select(x=>x.ToColor()).ToArray();}
	}
}