using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    [SerializeField] private StatBlock statBlock;

    private readonly List<IEffectRuntime> activeEffects = new List<IEffectRuntime>();

    private void Awake()
    {
        if (statBlock == null)
        {
            statBlock = GetComponent<StatBlock>();
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            IEffectRuntime effect = activeEffects[i];
            effect.OnTick(deltaTime);

            if (!effect.IsExpired)
            {
                continue;
            }

            RemoveEffectAt(i);
        }
    }

    public void ApplyEffect(EffectAsset effectAsset, GameObject source = null)
    {
        if (effectAsset == null)
        {
            return;
        }

        if (!effectAsset.Stackable)
        {
            RemoveEffectsById(effectAsset.EffectId);
        }

        List<WeaponBase> weapons = ResolveWeapons();
        List<IModifiableStatProvider> weaponStatsProviders = ResolveWeaponStatsProviders(weapons);
        EffectContext context = new EffectContext(source, gameObject, statBlock, weapons, weaponStatsProviders, this);
        IEffectRuntime runtime = effectAsset.CreateRuntime(context);
        if (runtime == null)
        {
            return;
        }

        activeEffects.Add(runtime);
        runtime.OnApply();
        EventBus.Publish(new EffectAppliedEvent(gameObject, runtime.EffectId));
    }

    public int RemoveEffectsById(string effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            return 0;
        }

        int removed = 0;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].EffectId != effectId)
            {
                continue;
            }

            RemoveEffectAt(i);
            removed++;
        }

        return removed;
    }

    public int RemoveEffectsByPrefix(string effectIdPrefix)
    {
        if (string.IsNullOrWhiteSpace(effectIdPrefix))
        {
            return 0;
        }

        int removed = 0;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            string effectId = activeEffects[i].EffectId;
            if (!effectId.StartsWith(effectIdPrefix))
            {
                continue;
            }

            RemoveEffectAt(i);
            removed++;
        }

        return removed;
    }

    public bool HasEffect(string effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            return false;
        }

        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].EffectId == effectId)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetEffectRemaining(string effectId, out float remainingTime, out float duration)
    {
        remainingTime = 0f;
        duration = 0f;
        if (string.IsNullOrWhiteSpace(effectId))
        {
            return false;
        }

        for (int i = 0; i < activeEffects.Count; i++)
        {
            IEffectRuntime runtime = activeEffects[i];
            if (runtime.EffectId != effectId)
            {
                continue;
            }

            remainingTime = runtime.RemainingTime;
            duration = runtime.Duration;
            return true;
        }

        return false;
    }

    private void RemoveEffectAt(int index)
    {
        IEffectRuntime runtime = activeEffects[index];
        runtime.OnRemove();
        activeEffects.RemoveAt(index);
        EventBus.Publish(new EffectRemovedEvent(gameObject, runtime.EffectId));
    }

    private List<WeaponBase> ResolveWeapons()
    {
        List<WeaponBase> weapons = new List<WeaponBase>();
        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat == null)
        {
            return weapons;
        }

        IReadOnlyList<WeaponBase> slots = playerCombat.GetWeaponSlots();
        if (slots == null)
        {
            return weapons;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            WeaponBase weapon = slots[i];
            if (weapon == null || weapons.Contains(weapon))
            {
                continue;
            }

            weapons.Add(weapon);
        }

        return weapons;
    }

    private static List<IModifiableStatProvider> ResolveWeaponStatsProviders(IReadOnlyList<WeaponBase> weapons)
    {
        List<IModifiableStatProvider> providers = new List<IModifiableStatProvider>();
        if (weapons == null)
        {
            return providers;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            IModifiableStatProvider provider = weapons[i] as IModifiableStatProvider;
            if (provider == null || providers.Contains(provider))
            {
                continue;
            }

            providers.Add(provider);
        }

        return providers;
    }
}
