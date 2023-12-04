using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
	public class SegmentedLruCache<TKey, TValue> where TKey : notnull
	{
		readonly ConcurrentDictionary<TKey, Entry<TKey, TValue>> _dictionary;

		readonly Queue<Entry<TKey, TValue>> _hotSegment;
		readonly Queue<Entry<TKey, TValue>> _warmSegment;
		readonly Queue<Entry<TKey, TValue>> _coldSegment;

		readonly int _hotCapcity;
		readonly int _warmCapacity;
		readonly int _coldCapacity;

		private readonly object _putSync = new();

		public SegmentedLruCache(int capacity)
		{
			Capacity = capacity;
			_dictionary = new ConcurrentDictionary<TKey, Entry<TKey, TValue>>();

			// partition capacity into 3 equal segments
			_hotCapcity = _warmCapacity = _coldCapacity = Convert.ToInt32(Math.Round((double)capacity / 3));
			_coldCapacity += capacity - (_hotCapcity + _warmCapacity + _coldCapacity);

			_hotSegment = new Queue<Entry<TKey, TValue>>(_hotCapcity);
			_warmSegment = new Queue<Entry<TKey, TValue>>(_warmCapacity + 1);
			_coldSegment = new Queue<Entry<TKey, TValue>>(_coldCapacity);
		}

		public int Capacity { get; }

		public int Count
		{
			get
			{
				return _dictionary.Count;
			}
		}

		public void Remove(TKey key)
		{
			lock (_putSync)
			{
				if (_dictionary.TryRemove(key, out Entry<TKey, TValue> v))
				{
					MoveToCold(v);
				}
			}
		}

		public void Clear()
		{
			lock (_putSync)
			{
				_hotSegment.Clear();
				_warmSegment.Clear();
				_coldSegment.Clear();
				_dictionary.Clear();
			}
		}

		public TValue Get(TKey key)
		{
			// gets are not locked
			if (_dictionary.TryGetValue(key, out var entry))
			{
				entry.WasAccessed = true;
				return entry.Value;
			}

			return default;
		}

		public void Put(TKey key, TValue value)
		{
			// puts are still locked
			lock (_putSync)
			{
				_dictionary.AddOrUpdate(key,
						k =>
						{
							// add new and place in hot segment, cycling all segments
							var entry = new Entry<TKey, TValue>(k, value);
							MoveToHot(entry);
							return entry;
						},
						(k, entry) =>
						{
							// update
							//   set new value
							//   and treat entry as having been accessed same as a get
							entry.Value = value;
							entry.WasAccessed = true;
							return entry;
						});
			}
		}

		void MoveToHot(Entry<TKey, TValue> entry)
		{
			entry.WasAccessed = false;

			// if hot segment is full, take oldest hot item
			if (_hotSegment.Count >= _hotCapcity && _hotSegment.TryDequeue(out var dequeued))
			{
				// if it was touched since last cycle or we're still in a cache warm-up, keep in warm
				if (dequeued.WasAccessed || _warmSegment.Count < _warmCapacity)
					MoveToWarm(dequeued);
				// otherwise, in probation
				else
					MoveToCold(dequeued);
			}

			_hotSegment.Enqueue(entry);
		}

		void MoveToWarm(Entry<TKey, TValue> entry)
		{
			entry.WasAccessed = false;

			// handle any overflows from previously recycled warm to warm nodes
			// in this edge case, oldest warm node is forcibly pushed to cold
			if (_warmSegment.Count > _warmCapacity)
			{
				if (_warmSegment.TryDequeue(out var dequeued))
					MoveToCold(dequeued);
				// try again, as alterations will change destination of entry
				MoveToWarm(entry);
			}
			// then proceed with normal warm cycling...
			// if warm segment full, take oldest warm item...
			else if (_warmSegment.Count == _warmCapacity)
			{
				if (_warmSegment.TryDequeue(out var dequeued))
				{
					// if it was used since last segment, keep it wam
					if (dequeued.WasAccessed)
						// recycling a warm node to warm can result in an intentional
						// breif overflow which will be handled in next cycle
						MoveToWarm(dequeued);
					// otherwise, in probation
					else
						MoveToCold(dequeued);
				}
				// try again, as alterations will change destination of entry
				MoveToWarm(entry);
			}
			else
			{
				_warmSegment.Enqueue(entry);
			}
		}

		void MoveToCold(Entry<TKey, TValue> entry)
		{
			entry.WasAccessed = false;
			// if cold is full, take oldest cold item
			if (_coldSegment.Count >= _coldCapacity)
			{
				if (_coldSegment.TryDequeue(out var dequeued))
				{
					// rewarm out of cold if was accessed
					if (dequeued.WasAccessed)
						MoveToWarm(dequeued);
					// otherwise, discard and evict from cache
					else
						_dictionary.TryRemove(dequeued.Key, out _);
				}
				// try again, as alterations will change destination of entry
				MoveToCold(entry);
			}
			else
			{
				_coldSegment.Enqueue(entry);
			}
		}

		class Entry<K, V> where K : notnull
		{
			public Entry(K key, V value)
			{
				Key = key;
				Value = value;
			}

			public K Key { get; set; }
			public V Value { get; set; }
			public bool WasAccessed { get; set; }
		}
	}
}
