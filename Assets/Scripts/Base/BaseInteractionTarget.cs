using UnityEngine;

public class BaseInteractionTarget : MonoBehaviour
{
    public enum TargetType
    {
        Battle = 0,
        Upgrade = 1
    }

    [SerializeField] private TargetType targetType;
    [SerializeField] private string interactionLabel = "Interact";

    public TargetType Type => targetType;
    public string InteractionLabel => interactionLabel;
}
