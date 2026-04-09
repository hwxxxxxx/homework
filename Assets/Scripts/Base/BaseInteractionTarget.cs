using UnityEngine;

public class BaseInteractionTarget : MonoBehaviour
{
    public enum TargetType
    {
        Battle = 0,
        Upgrade = 1
    }

    [SerializeField] private TargetType targetType;

    public TargetType Type => targetType;
    public string InteractionLabel => targetType == TargetType.Battle
        ? UiTextConfigProvider.Config.BattleInteractionLabel
        : UiTextConfigProvider.Config.UpgradeInteractionLabel;
}
