using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BuffPickupItem : MonoBehaviour, IPoolable
{
    [Header("Presentation")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 2.2f;

    private EffectAsset effectAsset;
    private int stackCount;
    private EffectController targetEffectController;
    private bool collected;
    private Vector3 spawnPosition;
    private GameObject activeVisual;

    private void OnValidate()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void Awake()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (!trigger.isTrigger)
        {
            throw new InvalidOperationException("BuffPickupItem requires SphereCollider.isTrigger = true.");
        }

        if (visualRoot == null)
        {
            throw new InvalidOperationException("BuffPickupItem requires visualRoot reference.");
        }
    }

    private void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = spawnPosition + Vector3.up * bobOffset;
        visualRoot.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    public void InitializeDrop(EffectAsset effect, int stacks, EffectController targetController, string statId)
    {
        if (effect == null)
        {
            throw new ArgumentNullException(nameof(effect));
        }

        if (stacks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stacks), "Buff pickup stacks must be > 0.");
        }

        if (targetController == null)
        {
            throw new ArgumentNullException(nameof(targetController));
        }

        effectAsset = effect;
        stackCount = stacks;
        targetEffectController = targetController;
        spawnPosition = transform.position;
        collected = false;
        ApplyVisual(statId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag(CombatConfigProvider.Config.PlayerTag))
        {
            return;
        }

        collected = true;
        for (int i = 0; i < stackCount; i++)
        {
            targetEffectController.ApplyEffect(effectAsset, gameObject);
        }

        PoolService.Despawn(gameObject);
    }

    public void OnSpawnedFromPool()
    {
        spawnPosition = transform.position;
        collected = false;
    }

    public void OnDespawnedToPool()
    {
        effectAsset = null;
        stackCount = 0;
        targetEffectController = null;
        collected = false;

        if (activeVisual != null)
        {
            Destroy(activeVisual);
            activeVisual = null;
        }
    }

    private void ApplyVisual(string statId)
    {
        if (activeVisual != null)
        {
            Destroy(activeVisual);
            activeVisual = null;
        }

        GameObject visualPrefab = ResolveVisualPrefab(statId);
        if (visualPrefab == null)
        {
            return;
        }

        activeVisual = Instantiate(visualPrefab, visualRoot);
        activeVisual.transform.localPosition = Vector3.zero;
        activeVisual.transform.localRotation = Quaternion.identity;
        activeVisual.transform.localScale = Vector3.one;

        Collider[] colliders = activeVisual.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }

        Rigidbody[] rigidbodies = activeVisual.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Destroy(rigidbodies[i]);
        }
    }

    private static GameObject ResolveVisualPrefab(string statId)
    {
        RunLootConfigAsset config = RunLootConfigProvider.Config;
        if (statId == StatIds.WeaponDamage)
        {
            return config.DamageBuffVisualPrefab;
        }

        if (statId == StatIds.WeaponFireRate)
        {
            return config.FireRateBuffVisualPrefab;
        }

        if (statId == StatIds.WeaponReloadTime)
        {
            return config.ReloadBuffVisualPrefab;
        }

        return null;
    }
}
