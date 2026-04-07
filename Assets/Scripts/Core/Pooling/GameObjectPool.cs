using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform poolRoot;
    private readonly Queue<GameObject> inactiveQueue = new Queue<GameObject>();
    private readonly List<GameObject> allInstances = new List<GameObject>();

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
        if (targetParent == null)
        {
            SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
        }

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
        allInstances.Add(instance);

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            throw new System.InvalidOperationException(
                $"Pooled prefab '{prefab.name}' is missing required PooledObject component."
            );
        }

        pooledObject.Initialize(prefab, this);
        return instance;
    }

    public void Clear()
    {
        for (int i = 0; i < allInstances.Count; i++)
        {
            if (allInstances[i] != null)
            {
                Object.Destroy(allInstances[i]);
            }
        }

        allInstances.Clear();
        inactiveQueue.Clear();
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
