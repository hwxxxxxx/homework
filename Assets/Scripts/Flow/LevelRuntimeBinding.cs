using UnityEngine;
using Unity.AI.Navigation;

public class LevelRuntimeBinding : MonoBehaviour
{
    [SerializeField] private LevelSpawnPoint[] spawnPoints;
    [SerializeField] private NavMeshSurface[] navMeshSurfaces;

    public LevelSpawnPoint[] SpawnPoints => spawnPoints;
    public NavMeshSurface[] NavMeshSurfaces => navMeshSurfaces;
}
