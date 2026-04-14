public class WeaponAmmoGrantEffectRuntime : EffectRuntimeBase
{
    private readonly WeaponBase.WeaponKind targetWeaponKind;
    private readonly int reserveAmmoAmount;

    public WeaponAmmoGrantEffectRuntime(
        string effectId,
        EffectContext context,
        float duration,
        WeaponBase.WeaponKind targetWeaponKind,
        int reserveAmmoAmount
    ) : base(effectId, context, duration)
    {
        this.targetWeaponKind = targetWeaponKind;
        this.reserveAmmoAmount = reserveAmmoAmount;
    }

    public override void OnApply()
    {
        if (Context.Weapons == null)
        {
            Expire();
            return;
        }

        for (int i = 0; i < Context.Weapons.Count; i++)
        {
            WeaponBase weapon = Context.Weapons[i];
            if (weapon == null || weapon.GetWeaponKind() != targetWeaponKind)
            {
                continue;
            }

            weapon.AddReserveAmmo(reserveAmmoAmount);
        }

        Expire();
    }
}
