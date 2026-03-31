using UnityEngine;

public class BaseInteractionController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private BaseSceneUIController uiController;
    [SerializeField] private Transform interactionRoot;
    [SerializeField] private float interactDistance = 2.8f;
    private BaseInteractionTarget[] interactionTargets;

    private void Awake()
    {
        if (gameInput == null || uiController == null || interactionRoot == null)
        {
            Debug.LogError("BaseInteractionController references are not fully assigned.", this);
            enabled = false;
            return;
        }

        interactionTargets = interactionRoot.GetComponentsInChildren<BaseInteractionTarget>(true);
        if (interactionTargets.Length == 0)
        {
            Debug.LogError("BaseInteractionController interactionRoot has no BaseInteractionTarget.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (uiController.IsModalOpen)
        {
            uiController.SetInteractionHint("Press E to interact");
        }

        BaseInteractionTarget nearestTarget = GetNearestTarget();
        if (nearestTarget != null && !uiController.IsModalOpen)
        {
            uiController.SetInteractionHint($"Press E to interact: {nearestTarget.InteractionLabel}");
        }
        else if (!uiController.IsModalOpen)
        {
            uiController.SetInteractionHint(string.Empty);
        }

        if (!gameInput.IsInteractPressed() || nearestTarget == null)
        {
            return;
        }

        switch (nearestTarget.Type)
        {
            case BaseInteractionTarget.TargetType.Battle:
                uiController.ShowBattlePanel();
                break;
            case BaseInteractionTarget.TargetType.Upgrade:
                uiController.ShowUpgradePanel();
                break;
        }
    }

    private BaseInteractionTarget GetNearestTarget()
    {
        if (interactionTargets == null || interactionTargets.Length == 0)
        {
            return null;
        }

        BaseInteractionTarget nearest = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 origin = transform.position;
        float maxDistanceSqr = interactDistance * interactDistance;

        for (int i = 0; i < interactionTargets.Length; i++)
        {
            BaseInteractionTarget target = interactionTargets[i];
            if (target == null)
            {
                continue;
            }

            float distanceSqr = (target.transform.position - origin).sqrMagnitude;
            if (distanceSqr > maxDistanceSqr || distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearest = target;
            nearestDistanceSqr = distanceSqr;
        }

        return nearest;
    }
}
