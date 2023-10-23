using System;
using UnityEngine;
using System.Collections;

namespace GuCore
{
	internal class GCoroutine : GUnitySingleton<GCoroutine>
	{
	}

	public static class GCoroutineUtil
	{
		public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
		public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

		public static Coroutine StartCoroutine(IEnumerator routine)
		{
			return GCoroutine.Instance.StartCoroutine(routine);
		}

		public static void StopCorutinue(Coroutine routine)
		{
			GCoroutine.Instance.StopCoroutine(routine);
		}

		public static IEnumerator WaitForSeconds(float seconds)
		{
			var endTime = Time.time + seconds;
			while (Time.time < endTime)
				yield return null;
		}

		public static IEnumerator WaitForSecondsOnFixedUpdate(float seconds)
		{
			var endTime = Time.fixedTime + seconds;
			while (Time.fixedTime < endTime)
			{
				yield return WaitForFixedUpdate;
			}
		}

		public static IEnumerator WaitForSecondsOnFixedUpdate(float seconds, Func<bool> cancelCondition)
		{
			var endTime = Time.fixedTime + seconds;
			while (Time.fixedTime < endTime)
			{
				yield return WaitForFixedUpdate;
				if (!cancelCondition())
					yield break;
			}
		}

		public static IEnumerator WaitForSecondsOnIndependentTimeScale(float seconds)
		{
			var endtime = Time.realtimeSinceStartup + seconds;
			while (Time.realtimeSinceStartup < endtime)
			{
				yield return null;
			}
		}
	}
}