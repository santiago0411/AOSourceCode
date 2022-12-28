using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AO.Core
{
	public sealed class Pool<T> where T : MonoBehaviour, IPoolObject
	{
		private readonly T prefab;
		private readonly List<T> objects = new();

		public Pool(T prefab)
		{
			AoDebug.Assert(prefab);
			this.prefab = prefab;
		}

		public void AddExistingObjectToPool(T obj)
        {
			objects.Add(obj);
        }

		/// <summary>Returns the first available <typeparamref name="T"/> object or instantiates a new one if there isn't one available.</summary>
		public T GetObject()
		{
			var obj = objects.FirstOrDefault(o => !o.IsBeingUsed);
			return obj ? obj : InstantiateNew();
		}

		private T InstantiateNew()
		{
			T newObject = Object.Instantiate(prefab);
			objects.Add(newObject);
			newObject.gameObject.SetActive(false);
			return newObject;
		}
	}

	public interface IPoolObject
	{
		bool IsBeingUsed { get; }
	}
}