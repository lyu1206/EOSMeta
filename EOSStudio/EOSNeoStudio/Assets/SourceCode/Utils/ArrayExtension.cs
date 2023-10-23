using System;
using System.Collections.Generic;

public static class ArrayExtention
{
	public static T[] Add<T>(this T[] array, T add)
	{
		var list = new List<T>(array);
		list.Add(add);
		return list.ToArray();
	}
	public static T[] Remove<T>(this T[] array, T remove)
	{
		var list = new List<T>(array);
		list.Remove(remove);
		return list.ToArray();
	}
	public static T Find<T>(this T[] array, Predicate<T> p)
	{
		foreach(var a in array)
		{
			if (p(a) == true) return a;
		}
		return default(T);
	}
	public static int FindIndex<T>(this T[] array, Predicate<T> p)
	{
		for (int i=0;i<array.Length;i++)
		{
			if (p(array[i]) == true) return i;
		}
		return -1;
	}
	public static int FindIndex<T>(this T[] array, T item)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(item)) return i;
		}
		return -1;
	}

	public static void ForEach<T>(this T[] array, Action<T> rhs)
	{
		if (array == null) return;
		foreach (var val in array)
		{
			rhs(val);
		}
	}
	public static T[] Sort<T>(this T[] array,Comparison<T> comparison)
    {
		var list = new List<T>(array);
		list.Sort(comparison);
		return list.ToArray();
    }
}
