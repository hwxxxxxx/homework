using System;

[Serializable]
public struct StatModifierDescriptor
{
    public string statId;
    public float value;
    public StatModifierOperation operation;
    public int order;
}
