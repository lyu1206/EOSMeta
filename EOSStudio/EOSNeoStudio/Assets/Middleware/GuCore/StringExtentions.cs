using System;
using UnityEngine;
using System.Collections;

namespace GuCore
{
	public static class StringExtentions
	{
		public static string RemoveFirst(this string str, string fisrtWord)
		{
			return str.Substring( str.IndexOf(fisrtWord, System.StringComparison.Ordinal) + 1 );
		}
		public static string RemoveLast(this string str, string fisrtWord)
		{
			return str.Substring( str.LastIndexOf(fisrtWord, System.StringComparison.Ordinal) + 1 );
		}

		public static string RemoveLeft(this string str, string fisrtWord)
		{
			return str.Substring( str.LastIndexOf( fisrtWord, System.StringComparison.Ordinal) + 1 );
		}
		public static string RemoveRight(this string str, string fisrtWord)
		{
			return str.Remove( str.LastIndexOf( fisrtWord, System.StringComparison.Ordinal ) );
		}

		public static Vector3 ConvertVector3(this string str)
		{
			var floats = str.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
			var vec = Vector3.zero;
			for (var i = 0; i < floats.Length; i++)
			{
				float fvalue;
				if (float.TryParse(floats[i], out fvalue))
				{
					vec[i] = fvalue;
				}
			}
			return vec;
		}
	}
}