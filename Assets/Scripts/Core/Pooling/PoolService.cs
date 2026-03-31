using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoolService
{
    private static readonly Dictionary<GameObject, GameObjectPool> Pools =
        new Dictionary<GameObject, GameObjectPool>();

    private static Transform poolRoot;
    private static PoolCoroutineRunner coroutineRunner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnDomainReload()
    {
        Pools.Clear();

        if (poolRoot != null)
        {
            Object.Destroy(poolRoot.gameObject);
        }

        poolRoot = null;
        coroutineRunner = null;
    }

    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null, int preloadCount = 0)
        where T : Component
    {
        if (prefab == null)
        {
            return null;
        }

        GameObjectPool pool = GetOrCreatePool(prefab.gameObject, preloadCount);
        GameObject instance = pool.Spawn(position, rotation, parent);
        return instance.GetComponent<T>();
    }

    public static void Warmup(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        GetOrCreatePool(prefab, count);
    }

    public static void Despawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject != null && pooledObject.IsInitialized && pooledObject.SourcePrefab != null)
        {
            if (Pools.TryGetValue(pooledObject.SourcePrefab, out GameObjectPool pool))
            {
                pool.Despawn(instance);
                return;
            }
        }

        Object.Destroy(instance);
    }

    public static void DespawnAfterDelay(GameObject instance, float delay)
    {
        if (instance == null)
        {
            return;
        }

        if (delay <= 0f)
        {
            Despawn(instance);
            return;
        }

        EnsureRunner();
        coroutineRunner.StartCoroutine(DespawnRoutine(instance, delay));
    }

    private static GameObjectPool GetOrCreatePool(GameObject prefab, int preloadCount)
    {
        EnsureRoot();

        if (!Pools.TryGetValue(prefab, out GameObjectPool pool))
        {
            pool = new GameObjectPool(prefab, poolRoot, preloadCount);
            Pools[prefab] = pool;
            return pool;
        }

        if (preloadCount > 0)
        {
            pool.Warmup(preloadCount);
        }

        return pool;
    }

    private static IEnumerator DespawnRoutine(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(instance);
    }

    private static void EnsureRoot()
    {
        if (poolRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("GlobalObjectPools");
        Object.DontDestroyOnLoad(rootObject);
        poolRoot = rootObject.transform;
    }

    private static void EnsureRunner()
    {
        if (coroutineRunner != null)
        {
            return;
        }

        EnsureRoot();
        coroutineRunner = poolRoot.gameObject.GetComponent<PoolCoroutineRunner>();
        if (coroutineRunner == null)
        {
            coroutineRunner = poolRoot.gameObject.AddComponent<PoolCoroutineRunner>();
        }
    }

    private class PoolCoroutineRunner : MonoBehaviour
    {
    }
}
