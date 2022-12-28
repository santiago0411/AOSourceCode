using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AOClient.Core
{
	public sealed class Pool<T> where T : MonoBehaviour, IPoolObject
	{
		public int ActiveObjectsCount => objects.Count(o => o.IsBeingUsed);

		private readonly T prefab;
		private readonly List<T> objects = new();

		public Pool(T prefab)
		{
			this.prefab = prefab;
		}

		public Pool(T prefab, int startingCapacity)
			: this(prefab)
		{
			objects = new List<T>(startingCapacity);
		}

		/// <summary>Returns the first available object or instantiates a new one if there isn't one available.</summary>
		public T GetObject()
		{
			var obj = objects.FirstOrDefault(x => !x.IsBeingUsed);
			return obj ? obj : InstantiateNew();
		}

		/// <summary>Checks whether an object with the specified id exits in the pool.</summary>
		/// <param name="id">Instance id.</param>
		/// <param name="obj">Object with the specified instance id if found, otherwise null.</param>
		public bool TryFindObject(int id, out T obj)
		{
			return obj = objects.FirstOrDefault(x => x.InstanceId == id);
		}

		public T FindObject(int instanceId)
		{
			return objects.FirstOrDefault(x => x.InstanceId == instanceId);
		}
		
		public void ResetObjects()
		{
			foreach (var obj in objects)
				obj.ResetPoolObject();
		}

		public void ForEachActiveObject(Action<T> action)
		{
			foreach (var obj in objects)
				if (obj.IsBeingUsed)
					action(obj);
		}
		
		public void ForEachActiveObject<T1>(Action<T, T1> action, T1 state)
		{
			foreach (var obj in objects)
				if (obj.IsBeingUsed)
					action(obj, state);
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
		int InstanceId { get; }
		bool IsBeingUsed { get; }
		void ResetPoolObject();
	}
}
