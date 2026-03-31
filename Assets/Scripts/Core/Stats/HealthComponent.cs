using System;
using UnityEngine;

public abstract class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected StatBlock statBlock;
    [SerializeField] protected string maxHealthStatId = StatIds.MaxHealth;

    protected int currentHealth;
    protected bool isDead;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    protected virtual void Awake()
    {
        if (statBlock == null)
        {
            statBlock = GetComponent<StatBlock>();
        }

        ResetStats();
    }

    protected virtual void OnEnable()
    {
        if (statBlock != null)
        {
            statBlock.OnStatChanged += HandleStatChanged;
        }
    }

    protected virtual void OnDisable()
    {
        if (statBlock != null)
        {
            statBlock.OnStatChanged -= HandleStatChanged;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        RaiseHealthChanged();

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public virtual void ResetStats()
    {
        ApplyMaxHealthFromStats();
        isDead = false;
        currentHealth = maxHealth;
        RaiseHealthChanged();
    }

    protected virtual void OnDiedInternal()
    {
    }

    protected void RaiseHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        OnDied?.Invoke();
        OnDiedInternal();
    }

    private void HandleStatChanged(string statId, float _, float __)
    {
        if (!string.Equals(statId, maxHealthStatId, StringComparison.Ordinal))
        {
            return;
        }

        ApplyMaxHealthFromStats();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        RaiseHealthChanged();
    }

    private void ApplyMaxHealthFromStats()
    {
        if (statBlock == null || string.IsNullOrWhiteSpace(maxHealthStatId) || !statBlock.HasStat(maxHealthStatId))
        {
            return;
        }

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(statBlock.GetStatValue(maxHealthStatId)));
    }
}
