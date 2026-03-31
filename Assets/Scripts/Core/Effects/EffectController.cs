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

        EffectContext context = new EffectContext(source, gameObject, statBlock, this);
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
}
