using System.Collections.Generic;
using UnityEngine;

namespace GuAsset
{
	public class GAssetCache<TKey, T>
	{
		public int CacheCount = 20;
		private readonly Dictionary<TKey, T> _cache = new Dictionary<TKey, T>();
		private Queue<TKey> _cacheList = new Queue<TKey>();

		public bool TryGetValue(TKey key, out T outValue)
		{
			return _cache.TryGetValue(key, out outValue);
		}

		public T Add(TKey key, T value)
		{
			var removeCache = default(T);
			if (_cacheList.Count > CacheCount)
			{
				var cacheKey = _cacheList.Dequeue();
				removeCache = _cache[cacheKey];
				_cache.Remove(cacheKey);
			}
			_cache.Add(key, value);
			_cacheList.Enqueue(key);
			return removeCache;
		}
	}
}