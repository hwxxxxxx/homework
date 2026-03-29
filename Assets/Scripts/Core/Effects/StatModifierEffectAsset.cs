using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Stat Modifier Effect", fileName = "StatModifierEffect")]
public class StatModifierEffectAsset : EffectAsset
{
    [SerializeField] private StatModifierDescriptor[] modifiers;

    public override IEffectRuntime CreateRuntime(EffectContext context)
    {
        return new StatModifierEffectRuntime(EffectId, context, Duration, modifiers);
    }
}
