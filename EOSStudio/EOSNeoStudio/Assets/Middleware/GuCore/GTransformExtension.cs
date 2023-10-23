using System.Diagnostics;
using UnityEngine;
using System.Collections;

namespace GuCore
{
	public static class GUnityExtension
	{
		public static Transform FindChildDeep(this Transform trans, string name)
		{
			var found = trans.Find(name);
			if (found == null)
			{
				for (int i = 0; i < trans.childCount; i++)
				{
					var child = trans.GetChild(i).FindChildDeep(name);
					if (child != null)
						return child;
				}
			}
			else
				return found;
			return null;
		}


		public static void AddComponentDeepChild<TFindComponent, TAddComponet>(this Transform trans)
			where TFindComponent : Component where TAddComponet : Component
		{
			var found = trans.GetComponent<TFindComponent>();
			if (found)
			{
				found.gameObject.AddComponent<TAddComponet>();
			}
			for (int i = 0; i < trans.childCount; i++)
			{
				trans.GetChild(i).AddComponentDeepChild<TFindComponent, TAddComponet>();
			}
		}

		public static void GetComponetnFromFindChild<T>(this Transform trans, string name, ref T destComponent)
			where T : MonoBehaviour
		{
			var found = trans.Find(name);
			if (found != null)
			{
				destComponent = found.GetComponentInChildren<T>();
			}
		}

		public static T GetComponentInOnlyChildren<T>(this MonoBehaviour mono) where T : Component
		{
			var trans = mono.transform;
			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				var com = child.GetComponent<T>();
				if (com != null)
					return com;
			}
			return null;
		}

		public static T AddComponentIfNotContain<T>(this GameObject go) where T : Component
		{
			var com = go.GetComponent<T>();
			if (com != null)
				return com;

			return go.AddComponent<T>();
		}

		public static void RemoveComponentIfContain<T>(this GameObject go) where T : Component
		{
			var com = go.GetComponent<T>();
			if (com != null)
			{
				Object.DestroyImmediate(com);
			}
		}

		public static void EnableComponent<T>(this GameObject go, bool isEnable) where T : Behaviour
		{
			var com = go.GetComponent<T>();
			if (com != null)
			{
				com.enabled = isEnable;
			}
		}

		public static void SetLayerRecursively(this GameObject go, int layer)
		{
			go.layer = layer;
			var trans = go.transform;
			for (int i = 0; i < trans.childCount; i++)
			{
				trans.GetChild(i).gameObject.SetLayerRecursively(layer);
			}
		}
	}
}