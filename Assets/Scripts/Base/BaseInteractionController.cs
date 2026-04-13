using UnityEngine;

public class BaseInteractionController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private BaseSceneUIController uiController;
    [SerializeField] private Transform interactionRoot;
    [SerializeField] private float interactDistance = 2.8f;
    [SerializeField] private float promptHorizontalOffset = 0.8f;
    [SerializeField] private float promptVerticalOffset = 1.2f;
    private BaseInteractionTarget[] interactionTargets;

    public void ConfigureRuntime(GameInput runtimeInput, BaseSceneUIController runtimeUiController, Transform runtimeInteractionRoot)
    {
        gameInput = runtimeInput;
        uiController = runtimeUiController;
        interactionRoot = runtimeInteractionRoot;
        RefreshInteractionTargets();
        enabled = gameInput != null && uiController != null && interactionTargets != null && interactionTargets.Length > 0;
    }

    private void Awake()
    {
        RefreshInteractionTargets();
    }

    private void Update()
    {
        if (gameInput == null || uiController == null || interactionRoot == null)
        {
            return;
        }

        BaseInteractionTarget nearestTarget = GetNearestTarget();
        if (nearestTarget != null && !uiController.IsModalOpen)
        {
            Vector3 promptWorldPosition = transform.position
                                          + transform.right * promptHorizontalOffset
                                          + Vector3.up * promptVerticalOffset;
            uiController.SetInteractionPrompt(true, promptWorldPosition);
        }
        else
        {
            uiController.SetInteractionPrompt(false, Vector3.zero);
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

    private void RefreshInteractionTargets()
    {
        interactionTargets = interactionRoot == null
            ? null
            : interactionRoot.GetComponentsInChildren<BaseInteractionTarget>(true);
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
