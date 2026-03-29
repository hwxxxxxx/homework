using System;
using UnityEngine;

public class DamageEventReceiver : MonoBehaviour
{
    [SerializeField] private MonoBehaviour damageableComponent;

    private IDamageable damageable;
    private IDisposable subscription;

    private void Awake()
    {
        if (damageableComponent != null)
        {
            damageable = damageableComponent as IDamageable;
        }

        if (damageable == null)
        {
            damageable = GetComponent<IDamageable>();
        }
    }

    private void OnEnable()
    {
        subscription = EventBus.Subscribe<DamageRequestedEvent>(HandleDamageRequested);
    }

    private void OnDisable()
    {
        subscription?.Dispose();
        subscription = null;
    }

    private void HandleDamageRequested(DamageRequestedEvent damageEvent)
    {
        if (damageable == null || damageEvent.Target != gameObject)
        {
            return;
        }

        int amount = Mathf.RoundToInt(damageEvent.Amount);
        if (amount <= 0)
        {
            return;
        }

        damageable.TakeDamage(amount);
    }
}
