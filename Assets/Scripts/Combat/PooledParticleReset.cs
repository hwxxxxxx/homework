using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PooledParticleReset : MonoBehaviour, IPoolable
{
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trailRenderers;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    public void OnSpawnedFromPool()
    {
        ResetVisualState();
    }

    public void OnDespawnedToPool()
    {
        ResetVisualState();
    }

    private void ResetVisualState()
    {
        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (ps == null)
                {
                    continue;
                }

                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
        }

        if (trailRenderers == null)
        {
            return;
        }

        for (int i = 0; i < trailRenderers.Length; i++)
        {
            TrailRenderer trail = trailRenderers[i];
            if (trail != null)
            {
                trail.Clear();
            }
        }
    }
}
