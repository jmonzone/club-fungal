using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null, UnityAction<T> initialize = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Pre-instantiate objects
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            initialize?.Invoke(obj);
        }
    }

    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T obj = Object.Instantiate(prefab, parent);
            return obj;
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
