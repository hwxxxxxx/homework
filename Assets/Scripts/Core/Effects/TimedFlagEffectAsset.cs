using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Timed Flag", fileName = "TimedFlagEffect")]
public class TimedFlagEffectAsset : EffectAsset
{
    public override IEffectRuntime CreateRuntime(EffectContext context)
    {
        return new TimedFlagEffectRuntime(EffectId, context, Duration);
    }
}
