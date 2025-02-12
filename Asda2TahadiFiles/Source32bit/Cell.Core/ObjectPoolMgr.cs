﻿using System;
using WCell.Util.Collections;
using WCell.Util.ObjectPools;

namespace Cell.Core
{
  /// <summary>
  /// This class manages objects in a pool to maximize memory (de)allocation efficiency.
  /// </summary>
  public static class ObjectPoolMgr
  {
    /// <summary>A list of types of objects the pool contains.</summary>
    private static readonly SynchronizedDictionary<long, IObjectPool> Pools =
      new SynchronizedDictionary<long, IObjectPool>();

    /// <summary>Returns true if the specified type is registered.</summary>
    /// <typeparam name="T">The type to check registration with.</typeparam>
    /// <returns>True if the specified type is registered.</returns>
    public static bool ContainsType<T>()
    {
      return Pools.ContainsKey(GetTypePointer<T>());
    }

    /// <summary>Returns true if the specified type is registered.</summary>
    /// <param name="t">The type to check registration with.</param>
    /// <returns>True if the specified type is registered.</returns>
    public static bool ContainsType(Type t)
    {
      return Pools.ContainsKey(t.TypeHandle.Value.ToInt64());
    }

    /// <summary>Registers an object pool with the specified type.</summary>
    /// <param name="func">A pointer to a function that creates new objects.</param>
    /// <returns>True if the type already exists or was registered successfully. False if locking the internal pool list timed out.</returns>
    /// <remarks>The function waits 3000 milliseconds to aquire the lock of the internal pool list.</remarks>
    public static bool RegisterType<T>(Func<T> func) where T : class
    {
      long typePointer = GetTypePointer<T>();
      lock(typeof(ObjectPoolMgr))
      {
        if(!Pools.ContainsKey(typePointer))
        {
          Pools.Add(typePointer, new ObjectPool<T>(func));
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Sets the minimum number of hard references to be contained in the specified object pool.
    /// </summary>
    /// <param name="minSize">The minimum number of hard references to be contained in the specified object pool.</param>
    public static void SetMinimumSize<T>(int minSize) where T : class
    {
      long typePointer = GetTypePointer<T>();
      if(!Pools.ContainsKey(typePointer))
        return;
      ((ObjectPool<T>) Pools[typePointer]).MinimumSize = minSize;
    }

    /// <summary>Releases an object back into the object pool.</summary>
    /// <param name="obj">The object to be released.</param>
    public static void ReleaseObject<T>(T obj) where T : class
    {
      long typePointer = GetTypePointer<T>();
      IObjectPool objectPool;
      if(!Pools.TryGetValue(typePointer, out objectPool))
        return;
      objectPool.Recycle(obj);
    }

    /// <summary>Obtains an object from the specified object pool.</summary>
    /// <returns>If a lock could not be aquired on the object pool null is returned. Otherwise a hard reference to the object requested is returned.</returns>
    public static T ObtainObject<T>() where T : class
    {
      long typePointer = GetTypePointer<T>();
      IObjectPool objectPool;
      if(Pools.TryGetValue(typePointer, out objectPool))
        return ((ObjectPool<T>) objectPool).Obtain();
      return default(T);
    }

    /// <summary>Gets information about the specified object pool.</summary>
    /// <returns>An object of type <see cref="T:WCell.Util.ObjectPools.ObjectPoolInfo" /> if the function succeeded, otherwise an object with all values equal to 0 is returned.</returns>
    public static ObjectPoolInfo GetPoolInfo<T>() where T : class
    {
      long typePointer = GetTypePointer<T>();
      IObjectPool objectPool;
      if(Pools.TryGetValue(typePointer, out objectPool))
        return ((ObjectPool<T>) objectPool).Info;
      return new ObjectPoolInfo(0, 0);
    }

    private static long GetTypePointer<T>()
    {
      return typeof(T).TypeHandle.Value.ToInt64();
    }
  }
}