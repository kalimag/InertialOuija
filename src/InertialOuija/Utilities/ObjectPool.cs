extern alias GameScripts;
using System;
using System.Collections.Generic;
using GameScripts.Assets.Source.SaveData;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace InertialOuija.Utilities;

internal static class ObjectPool
{
	public static ObjectPool<MemoryStream> MemoryStreams { get; } = new(() => new MemoryStream(), stream => stream.SetLength(0));

	public static ObjectPool<BinaryFormatter> UnitySerializers { get; } = new(() =>
	{
		var serializer = new BinaryFormatter();
		SaveHelpers.AddUnitySerialisationSurrogates(serializer);
		return serializer;
	});
}

internal class ObjectPool<T>
{
	private readonly Stack<T> _pool;
	private readonly int _maxPoolSize;
	private readonly Func<T> _createItem;
	private readonly Action<T> _resetItem;

	public ObjectPool(Func<T> createItem, Action<T> resetItem = null, int maxPoolSize = 10)
	{
		_pool = new Stack<T>(maxPoolSize);
		_maxPoolSize = maxPoolSize;
		_createItem = createItem;
		_resetItem = resetItem;
	}

	public T Get()
	{
		lock (_pool)
		{
			if (_pool.TryPop(out var item))
				return item;
			else
				return _createItem();
		}
	}

	public void Return(T item)
	{
		if (item == null)
			return;

		_resetItem?.Invoke(item);
		lock (_pool)
		{
			if (_pool.Count < _maxPoolSize)
				_pool.Push(item);
		}
	}

	public LeasedObject Lease()
		=> new(this, Get());

	public struct LeasedObject : IDisposable
	{
		private ObjectPool<T> _pool;
		private T _value;

		public T Value => _value;

		public LeasedObject(ObjectPool<T> pool, T item)
		{
			_pool = pool;
			_value = item;
		}

		public static implicit operator T(LeasedObject borrowedItem)
			=> borrowedItem._pool != null ? borrowedItem.Value : throw new ObjectDisposedException(nameof(LeasedObject));

		void IDisposable.Dispose()
		{
			if (_pool != null)
			{
				_pool.Return(_value);
				_pool = null;
				_value = default;
			}
		}
	}
}
