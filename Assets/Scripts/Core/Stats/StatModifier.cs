using System;

public readonly struct StatModifier
{
    public readonly string ModifierId;
    public readonly float Value;
    public readonly StatModifierOperation Operation;
    public readonly object Source;
    public readonly int Order;

    public StatModifier(string modifierId, float value, StatModifierOperation operation, object source = null, int order = 0)
    {
        ModifierId = string.IsNullOrWhiteSpace(modifierId) ? Guid.NewGuid().ToString("N") : modifierId;
        Value = value;
        Operation = operation;
        Source = source;
        Order = order;
    }
}
