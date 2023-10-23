using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GuCore
{
	public static class GComponentUtil
	{
		public static TInterface GetInterface<TComponent, TInterface>(GameObject go)
			where TInterface : class where TComponent : Component
		{
			var com = go.GetComponent<TComponent>();
			if (com == null)
				return null;

			var type = typeof (TInterface);
			if (type.IsAssignableFrom(com.GetType()))
				return com as TInterface;
			return null;
		}

		public static TInterface GetInterface<TInterface>(MonoBehaviour com)
			where TInterface : class
		{
			var type = typeof (TInterface);
			if (type.IsAssignableFrom(com.GetType()))
				return com as TInterface;
			return null;
		}

		public static T GetInterface<T>(GameObject go) where T : class
		{
			var type = typeof (T);
			var coms = go.GetComponents(typeof (MonoBehaviour));
			if (coms == null)
				return null;
			for (int i = 0; i < coms.Length; i++)
			{
				Component com = coms[i];
				if (type.IsAssignableFrom(com.GetType()))
					return com as T;
			}
			return null;
		}

		// 부하가 많이 걸림 초기화 같은 자주 사용 하지 않는 곳에서 사용 
		public static T GetInterfaceInParent<T>(Transform trans) where T : class
		{
			var type = typeof (T);
			var coms = trans.GetComponents(typeof (MonoBehaviour));
			if (coms == null)
				return null;
			for (int i = 0; i < coms.Length; i++)
			{
				var com = coms[i];
				//Debug.Log( com );
				if (com != null && type.IsAssignableFrom(com.GetType()))
					return com as T;
			}
			var parent = trans.parent;
			if (parent == null)
				return null;
			return GetInterfaceInParent<T>(trans.parent);
		}

		public static T GetComponentInParent<T>(Transform trans) where T : Component
		{
			var com = trans.GetComponent<T>();
			if (com != null)
				return com;

			var parent = trans.parent;
			if (parent == null)
				return null;

			return GetComponentInParent<T>(trans.parent);
		}

		public static T GetComponentInChildrenFromParent<T>(Transform trans) where T : Component
		{
			var com = trans.GetComponentInChildren<T>();
			if (com != null)
				return com;

			var parent = trans.parent;
			if (parent == null)
				return null;

			return GetComponentInChildrenFromParent<T>(trans.parent);
		}

		public static T AddComponentIfNotContain<T>(GameObject go) where T : Component
		{
			var com = go.GetComponent<T>();
			if (com != null)
				return com;

			return go.AddComponent<T>();
		}

		public static Component AddComponentIfNotContain(GameObject go, System.Type type)
		{
			var com = go.GetComponent(type);
			if (com != null)
				return com;

			return go.AddComponent(type);
		}

		public static T FindChild<T>(Transform trans, string name) where T : Component
		{
			var found = trans.Find(name);
			return found ? found.GetComponent<T>() : null;
		}

		public static IList<T> GetComponentsInChildrenDeep<T>(this Transform trans, IList<T> outs = null) where T : Component
		{
			if(outs == null)
				outs = new List<T>();
			var com = trans.GetComponent<T>();
			if (com != null)
				outs.Add(com);

			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				child.GetComponentsInChildrenDeep<T>(outs);
			}
			return outs;
		}
	}
}