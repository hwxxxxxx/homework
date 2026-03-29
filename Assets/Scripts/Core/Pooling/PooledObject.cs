using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private GameObject sourcePrefab;
    private GameObjectPool ownerPool;
    private bool initialized;

    public GameObject SourcePrefab => sourcePrefab;
    public bool IsInitialized => initialized;

    public void Initialize(GameObject prefab, GameObjectPool pool)
    {
        sourcePrefab = prefab;
        ownerPool = pool;
        initialized = true;
    }

    public void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.Despawn(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }
}
