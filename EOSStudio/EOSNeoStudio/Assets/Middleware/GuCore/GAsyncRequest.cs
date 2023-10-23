using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GuAsset
{
	public interface GiAsyncRequest<T>
	{
		T Asset { get; }
		bool IsDone { get; }
		float Progress { get; }
		int Priority { get; }
		bool AllowSceneActivation { get; set; }
	}

	public class GEmptyRequest<T> : YieldInstruction, GiAsyncRequest<T> where T : class
	{
		public T Asset { get; }
		public bool IsDone => true;
		public float Progress => 1;
		public int Priority => 0;

		public bool AllowSceneActivation
		{
			get { return true; }
			set { }
		}

		public GEmptyRequest(T asset)
		{
			Asset = asset;
		}
	}

	public abstract class GAsyncRequest<T> : CustomYieldInstruction, GiAsyncRequest<T> where T : class
	{
		public string Path { get; }
		public T Asset { get; protected set; }
		protected AsyncOperation Operation { get; set; }

		public bool IsDone => Operation.isDone;
		public float Progress => Operation.progress;
		public int Priority => Operation.priority;
		public sealed override bool keepWaiting => IsDone == false;


		public bool AllowSceneActivation
		{
			get { return Operation.allowSceneActivation; }
			set { Operation.allowSceneActivation = value; }
		}

		protected GAsyncRequest(string path)
		{
			Path = path;
		}
	}

	public class GAsyncResourceRequest<T> : GAsyncRequest<T> where T : class
	{
		private Func<GAsyncRequest<T>, Object, T> _completed;
		private ResourceRequest _request;

		public GAsyncResourceRequest(string path, Func<GAsyncRequest<T>, Object, T> completed, Type loadType) : base(path)
		{
			Operation = _request = Resources.LoadAsync(path, loadType);
			Operation.completed += OnCompleted;
			_completed = completed;
		}

		private void OnCompleted(AsyncOperation operation)
		{
			Asset = _completed(this, _request.asset);
		}
	}
}