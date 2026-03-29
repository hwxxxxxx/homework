public interface IModifiableStatProvider : IStatProvider
{
    string AddModifier(string statId, StatModifier modifier);
    bool RemoveModifier(string statId, string modifierId);
    int RemoveModifiersBySource(object source);
    void SetBaseValue(string statId, float baseValue);
}
