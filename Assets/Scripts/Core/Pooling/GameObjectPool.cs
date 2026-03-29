using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform poolRoot;
    private readonly Queue<GameObject> inactiveQueue = new Queue<GameObject>();

    public GameObjectPool(GameObject prefab, Transform poolRoot, int preloadCount)
    {
        this.prefab = prefab;
        this.poolRoot = poolRoot;

        Warmup(preloadCount);
    }

    public void Warmup(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject instance = CreateNewInstance();
            instance.SetActive(false);
            inactiveQueue.Enqueue(instance);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject instance = inactiveQueue.Count > 0 ? inactiveQueue.Dequeue() : CreateNewInstance();

        Transform targetParent = parent == null ? null : parent;
        instance.transform.SetParent(targetParent, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        NotifySpawned(instance);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        NotifyDespawned(instance);

        instance.SetActive(false);
        instance.transform.SetParent(poolRoot, false);
        inactiveQueue.Enqueue(instance);
    }

    private GameObject CreateNewInstance()
    {
        GameObject instance = Object.Instantiate(prefab, poolRoot);
        instance.name = prefab.name;

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = instance.AddComponent<PooledObject>();
        }

        pooledObject.Initialize(prefab, this);
        return instance;
    }

    private static void NotifySpawned(GameObject instance)
    {
        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnSpawnedFromPool();
        }
    }

    private static void NotifyDespawned(GameObject instance)
    {
        IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < poolables.Length; i++)
        {
            poolables[i].OnDespawnedToPool();
        }
    }
}
