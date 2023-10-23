using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace GuCore.Editors
{
	public partial class GEditorUtil
	{
		public enum EButton
		{
			None = 0,
			Add = 1 << 0,
			Duplicate = 1 << 1,
			Delete = 1 << 2,
			Up = 1 << 3,
			Down = 1 << 4,
		}

		private class Foldout
		{
			public bool IsFoldout = false;
			public object Object;

			public Foldout(object obj)
			{
				Object = obj;
			}
		}

		private static readonly float fieldSpace = 2;
		private static readonly float lebelWidth = 150;
		private static readonly GUIContent _moveDownButtonContent = new GUIContent("\u2193", "move down");
		private static readonly GUIContent _moveUpButtonContent = new GUIContent("\u2191", "move down");
		public static readonly GUIContent DuplicateButtonContent = new GUIContent("+", "duplicate");
		public static readonly GUIContent DeleteButtonContent = new GUIContent("-", "delete");
		//private GUIContent addButtonContent = new GUIContent( "+", "add element" );
		private static readonly GUILayoutOption miniButtonWidthLayoutOption = GUILayout.Width(20f);
		private static readonly GUILayoutOption miniFieldLabelWidthLayoutOption = GUILayout.Width(100f);
		private static readonly float fieldHeight = 20;
		public static readonly float ListButtonWidth = 20;
		public static readonly float MaxFoldoutSave = 2000;

		private static List<Foldout> _foldout = new List<Foldout>();

		public static string EditorPathSeparator
		{
			get { return "/"; }
		}

		public static string FisrtAssetPathString
		{
			get { return "/" + "Assets" + "/"; }
		}

		public static bool IsUnixLike
		{
			get
			{
				var platform = Environment.OSVersion.Platform;
				if (Application.platform == RuntimePlatform.OSXEditor)
				{
					platform = PlatformID.MacOSX; // seems to be some bug in Mono returning "Unix" when being on a mac.
				}
				return platform == PlatformID.MacOSX || platform == PlatformID.Unix;
			}
		}

		public static bool IsFoldout(object obj)
		{
			for (int i = 0; i < _foldout.Count; i++)
			{
				if (_foldout[i].Object == obj)
					return _foldout[i].IsFoldout;
			}
			return false;
		}

		public static void SetFoldout(object obj, bool isFoldout)
		{
			for (int i = 0; i < _foldout.Count; i++)
			{
				if (_foldout[i].Object == obj)
				{
					_foldout[i].IsFoldout = isFoldout;
					return;
				}
			}
			_foldout.Add(new Foldout(obj) {IsFoldout = isFoldout});
			if (_foldout.Count > MaxFoldoutSave)
				_foldout.RemoveRange(0, (int) MaxFoldoutSave/2);
		}

		public static void RegisterUndo(string name, params Object[] objects)
		{
			if (objects != null && objects.Length > 0)
			{
				Undo.RecordObjects(objects, name);
				foreach (Object obj in objects)
				{
					if (obj == null) continue;
					EditorUtility.SetDirty(obj);
				}
			}
		}

		public static T ReplaceAssetCreate<T>(string path, T obj) where T : Object
		{
			if (File.Exists(path))
			{
				AssetDatabase.DeleteAsset(path);
				AssetDatabase.Refresh();
				return CreateAsset(path, obj);
			}
			return CreateAsset(path, obj);
		}

		/// <summary>
		/// ?�디??모드?�서 ?�셋 ?�이?��? ?�로 ?�성?�거???�?�한??
		/// </summary>
		/// <typeparam name="T">?�셋 ?�??/typeparam>
		/// <param name="savePath">?�?�할 ?�스</param>
		/// <param name="obj">?�?�될 ?�셋 ?�이??/param>
		/// <param name="failMessage">메세지가 ?�이 ?�니�? obj??path?�, savePath가 ?��? ??savePath???�일??존재?�다�???�� ?�껏?��? 메시지 창을 출력?�다</param>
		/// <returns></returns>
		public static T ReplaceAssetSave<T>(string savePath, T obj, string failMessage = null) where T : Object
		{
			var existPath = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(existPath))
			{
				return CreateAsset(savePath, obj);
			}

			T newObj;
			if (existPath != savePath)
			{
				if (CopyAsset(existPath, savePath, failMessage) == false)
					return null;
				newObj = AssetDatabase.LoadAssetAtPath(savePath, typeof (T)) as T;
			}
			else
			{
				newObj = obj;
				EditorUtility.SetDirty(newObj);
			}
			AssetDatabase.Refresh();
			return newObj;
		}

		public static T ReplaceAssetCopy<T>(string path, T obj, string failMessage = null) where T : Object
		{
			var existPath = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(existPath))
			{
				return CreateAsset(path, obj);
			}
			if (CopyAsset(existPath, path, failMessage))
			{
				return AssetDatabase.LoadAssetAtPath(path, typeof (T)) as T;
			}
			AssetDatabase.Refresh();
			return null;
		}

		public static T CreateAsset<T>(string path, T obj) where T : Object
		{
			AssetDatabase.CreateAsset(obj, path);
			return AssetDatabase.LoadAssetAtPath(path, typeof (T)) as T;
		}

		/// <summary>
		/// Creates a custom asset at selected Object
		/// </summary>
		public static T CreateAsset<T>() where T : ScriptableObject
		{
			T asset = null;
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);

			if (path == "")
			{
				path = "Assets";
			}
			else if (System.IO.Path.GetExtension(path) != "")
			{
				path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
			string assetPathAndName =
				AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof (T).ToString().Split('.').Last() + ".asset");
			asset = CreateAsset<T>(assetPathAndName);
			return asset;
		}

		/// <summary>
		/// Creates a custom asset at path
		/// </summary>
		public static T CreateAsset<T>(string path) where T : ScriptableObject
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			T data = null;
			data = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(data, path);
			AssetDatabase.SaveAssets();
			return data;
		}

		public static bool CopyAsset(string sourcePath, string newpath, string failMessage = null)
		{
			if (File.Exists(newpath))
			{
				if (string.IsNullOrEmpty(failMessage) == false)
				{
					if (EditorUtility.DisplayDialog("existing file", "Write over the file?\n" + newpath, "ok", "cancel"))
						DeleteAsset(newpath);
					else
					{
						EditorUtility.DisplayDialog("notice", failMessage, "ok");
						return false;
					}
				}
				else
				{
					DeleteAsset(newpath);
				}
				AssetDatabase.Refresh();
			}
			var ret = AssetDatabase.CopyAsset(sourcePath, newpath);
			if (ret)
			{
				AssetDatabase.Refresh();
			}
			return ret;
		}

		private static void DeleteAsset(string newpath)
		{
			AssetDatabase.DeleteAsset(newpath);
			AssetDatabase.Refresh();
		}

		public static GameObject CreatePrefab(GameObject go, string path, bool isAtcive = false)
		{
			SetGameObjectActive(go, isAtcive);
			return PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
		}

		public static GameObject ReplacePrefab(GameObject go, string path, bool isAtcive = false)
		{
			if (File.Exists(path))
			{
				return PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
			}
			return CreatePrefab(go, path, isAtcive);
		}

		public static void CreatePath(params string[] aniPath)
		{
			foreach (var path in aniPath)
			{
				var fullPaht = Path.GetFullPath(path);
				if (Directory.Exists(fullPaht) == false)
					Directory.CreateDirectory(fullPaht);
			}
			AssetDatabase.Refresh();
		}


		public static T ReplaceAsset<T>(string path, T obj) where T : Object
		{
			if (File.Exists(path))
			{
				File.Delete(path);
				return CreateAsset(path, obj);
			}
			return CreateAsset(path, obj);
		}

		public static void SetGameObjectActive(GameObject go, bool active)
		{
#if UNITY_3_5
		go.SetActiveRecursively(active);
		#else
			go.SetActive(active);
#endif
		}

		public static GameObject AddChildGameObject(GameObject go, string name)
		{
			GameObject child;
			var type = PrefabUtility.GetPrefabInstanceStatus(go);
			if (type != PrefabInstanceStatus.NotAPrefab)
			{
				var prefabInstance = GameObject.Instantiate(go) as GameObject;
				child = new GameObject(name);
				child.transform.parent = prefabInstance.transform;
				var path = AssetDatabase.GetAssetPath(go);
				var go2 = PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstance, path, InteractionMode.AutomatedAction);
				GameObject.DestroyImmediate(prefabInstance);
				child = go2.transform.Find(name).gameObject;
			}
			else
			{
				child = new GameObject(name);
				child.transform.parent = go.transform;
			}
			return child;
		}

		public static T AddComponentChild<T>(GameObject go, string name, bool ifNotExistAddChild = true,
			bool force = false) where T : Component
		{
			return AddComponentChild(go, name, typeof (T), ifNotExistAddChild, force) as T;
		}

		public static Component AddComponentChild(GameObject go, string name, Type type, bool ifNotExistAddChild = true,
			bool force = false)
		{
			var trans = go.transform.FindChildDeep(name);
			if (trans == null && ifNotExistAddChild)
			{
				var newGo = AddChildGameObject(go, name);
				trans = newGo.transform;
			}
			if (trans != null)
			{
				if (force)
					return trans.gameObject.AddComponent(type);
				else
					return GComponentUtil.AddComponentIfNotContain(trans.gameObject, type);
			}
			return null;
		}

		public static void SetValue(object obj, PropertyInfo prop)
		{
			object value = null;
			if (prop.PropertyType == typeof (int))
				value = EditorGUILayout.IntField(prop.Name, (int) prop.GetValue(obj, null));
			else if (prop.PropertyType == typeof (string))
				value = EditorGUILayout.TextField(prop.Name, (string) prop.GetValue(obj, null));
			else if (prop.PropertyType == typeof (float))
				value = EditorGUILayout.FloatField(prop.Name, (float) prop.GetValue(obj, null));
			else if (prop.PropertyType == typeof (Enum))
				value = EditorGUILayout.EnumPopup(prop.Name, (Enum) prop.GetValue(obj, null));
			if (value != null)
				prop.SetValue(obj, value, null);
		}

		public static void SetValue(object obj, FieldInfo prop, bool drawLabel = true)
		{
			object value = null;
			if (prop.FieldType == typeof (int))
			{
				value = drawLabel
					? EditorGUILayout.IntField(prop.Name, (int) prop.GetValue(obj))
					: EditorGUILayout.IntField((int) prop.GetValue(obj));
			}
			else if (prop.FieldType == typeof (string))
			{
				if (drawLabel)
					value = EditorGUILayout.TextField(prop.Name, (string) prop.GetValue(obj));
				else
					value = EditorGUILayout.TextField((string) prop.GetValue(obj));
			}
			else if (prop.FieldType == typeof (float))
			{
				if (drawLabel)
					value = EditorGUILayout.FloatField(prop.Name, (float) prop.GetValue(obj));
				else
					value = EditorGUILayout.FloatField((float) prop.GetValue(obj));
			}
			else if (prop.FieldType == typeof (Transform))
			{
				if (drawLabel)
					value = EditorGUILayout.ObjectField(prop.Name, (Transform) prop.GetValue(obj), typeof (Transform), true);
				else
					value = EditorGUILayout.ObjectField((Transform) prop.GetValue(obj), typeof (Transform), true);
			}
			else if (prop.FieldType.BaseType == typeof (Enum))
			{
				if (drawLabel)
					value = EditorGUILayout.EnumPopup(prop.Name, (Enum) prop.GetValue(obj));
				else
					value = EditorGUILayout.EnumPopup((Enum) prop.GetValue(obj));
			}

			if (value != null)
				prop.SetValue(obj, value);
		}


		public static T GetBaseProperty<T>(SerializedProperty prop)
		{
			// Separate the steps it takes to get to this property
			string[] separatedPaths = prop.propertyPath.Split('.');

			// Go down to the root of this serialized property
			var reflectionTarget = prop.serializedObject.targetObject as object;
			// Walk down the path to get the target object
			foreach (var path in separatedPaths)
			{
				FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
				reflectionTarget = fieldInfo.GetValue(reflectionTarget);
			}
			return (T) reflectionTarget;
		}

		public static void SwapList(Array list, int index, int toIndex)
		{
			if (list.Length > toIndex && 0 <= toIndex)
			{
				var temp = list.GetValue(toIndex);
				list.SetValue(list.GetValue(index), toIndex);
				list.SetValue(temp, index);
			}
		}

		public static void SwapList(IList list, int index, int toIndex)
		{
			if (list.Count > toIndex && 0 <= toIndex)
			{
				var temp = list[toIndex];
				list[toIndex] = list[index];
				list[index] = temp;
			}
		}

		public static void RemoveAt<T>(ref T[] arr, int index)
		{
			for (int a = index; a < arr.Length - 1; a++)
			{
				// moving elements downwards, to fill the gap at [index]
				arr[a] = arr[a + 1];
			}
			// finally, let's decrement Array's size by one
			Array.Resize(ref arr, arr.Length - 1);
		}

		public static void InsertAt<T>(ref T[] arr, int index, T addObj)
		{
			Array.Resize(ref arr, arr.Length + 1);
			for (int a = index + 2; a < arr.Length; ++a)
				arr[a] = arr[a - 1];

			arr[index + 1] = addObj;
		}

		public static void Add<T>(ref T[] arr, int index)
		{
			for (int a = index; a < arr.Length - 1; a++)
			{
				arr[a] = arr[a + 1];
			}
			Array.Resize(ref arr, arr.Length - 1);
		}

		public static bool HasValue(EButton input, EButton mask)
		{
			return ((int) input & (int) mask) != 0;
		}

		public static Object FindAsset(string fileName)
		{
			string assetPath = GetAssetPathFromFileNameAndExtension(fileName);
			return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
		}

		private static string GetAssetPathFromFileNameAndExtension(string assetName)
		{
			string[] assets = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(assetName));
			string assetPath = null;

			foreach (string guid in assets)
			{
				string relativePath = AssetDatabase.GUIDToAssetPath(guid);

				if (Path.GetFileName(relativePath) == Path.GetFileName(assetName))
					assetPath = relativePath;
			}

			return assetPath;
		}
	}
}