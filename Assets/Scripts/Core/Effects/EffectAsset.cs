using UnityEngine;

public abstract class EffectAsset : ScriptableObject
{
    [SerializeField] private string effectId;
    [SerializeField] private bool stackable;
    [SerializeField] private float duration = 3f;

    public string EffectId => effectId;
    public bool Stackable => stackable;
    public float Duration => duration;

    public abstract IEffectRuntime CreateRuntime(EffectContext context);
}
