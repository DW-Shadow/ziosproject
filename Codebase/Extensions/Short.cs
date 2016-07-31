using System;
namespace Zios{
	public static class ShortExtension{
		//=====================
		// Conversion
		//=====================
		public static bool ToBool(this short current){return current != 0;}
		public static byte ToByte(this short current){return (byte)current;}
		public static int ToInt(this short current){return (int)current;}
		public static byte[] ToBytes(this short current){return BitConverter.GetBytes(current);}
		public static string Serialize(this short current){return current.ToString();}
		public static short Deserialize(this short current,string value){return value.ToShort();}
	}
}