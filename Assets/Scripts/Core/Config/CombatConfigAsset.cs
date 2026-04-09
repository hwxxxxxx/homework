using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/Combat Config", fileName = "CombatConfig")]
public class CombatConfigAsset : ScriptableObject
{
    [SerializeField] private string playerTag;

    public string PlayerTag => playerTag;
}
