using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    [SerializeField] private EffectAsset cooldownEffect;
    public EffectAsset CooldownEffect => cooldownEffect;

    public bool TryActivate(GameObject owner)
    {
        if (owner == null)
        {
            return false;
        }

        EffectController effectController = owner.GetComponent<EffectController>();
        if (!IsReady(effectController))
        {
            return false;
        }

        Activate(owner);
        StartCooldown(effectController, owner);
        return true;
    }

    public bool IsReady(EffectController effectController)
    {
        if (cooldownEffect == null)
        {
            return true;
        }

        return effectController != null && !effectController.HasEffect(cooldownEffect.EffectId);
    }

    private void StartCooldown(EffectController effectController, GameObject source)
    {
        if (cooldownEffect == null || effectController == null)
        {
            return;
        }

        effectController.ApplyEffect(cooldownEffect, source);
    }

    protected abstract void Activate(GameObject owner);
}
