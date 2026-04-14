using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Weapon Ammo Grant Effect", fileName = "WeaponAmmoGrantEffect")]
public class WeaponAmmoGrantEffectAsset : EffectAsset
{
    [SerializeField] private WeaponBase.WeaponKind targetWeaponKind = WeaponBase.WeaponKind.RocketLauncher;
    [SerializeField] private int reserveAmmoAmount = 3;

    public override IEffectRuntime CreateRuntime(EffectContext context)
    {
        return new WeaponAmmoGrantEffectRuntime(EffectId, context, Duration, targetWeaponKind, reserveAmmoAmount);
    }
}
