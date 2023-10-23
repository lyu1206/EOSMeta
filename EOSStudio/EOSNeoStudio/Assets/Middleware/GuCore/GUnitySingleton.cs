using System;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GuCore
{
	public abstract class GUnitySingleton<T> : GUnitySingleton<T, TBoolType.False>
		where T : GUnitySingleton<T, TBoolType.False>
	{
	}

	public abstract class GUnitySingleton<T, TIsDontDestory> : MonoBehaviour where T : GUnitySingleton<T, TIsDontDestory>
		where TIsDontDestory : TBoolType
	{
		private bool _isInit;
		private static T _instance = null;

		public static T Instance
		{
			get
			{
				// Instance requiered for the first time, we look for it
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType(typeof(T)) as T;

					// Object not found, we create a temporary one
					if (_instance == null)
					{
						_instance = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
						if (typeof(TIsDontDestory) == typeof(TBoolType.True))
							DontDestroyOnLoad(_instance.gameObject);
					}
					if (_instance != null && _instance._isInit == false)
						_instance.Init();
				}
				return _instance;
			}
		}

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this as T;
				if (_isInit == false)
					_instance.Init();
			}
		}

		protected virtual void Init()
		{
			_isInit = true;
		}

		protected virtual void Release()
		{
			_instance = null;
			_isInit = false;
		}

		public void OnDestroy()
		{
			Release();
		}
	}
}