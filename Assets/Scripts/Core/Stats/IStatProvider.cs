public interface IStatProvider
{
    bool HasStat(string statId);
    float GetStatValue(string statId);
}
