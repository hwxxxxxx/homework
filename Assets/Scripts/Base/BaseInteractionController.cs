using UnityEngine;

public class BaseInteractionController : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private BaseSceneUIController uiController;
    [SerializeField] private Transform interactionRoot;
    [SerializeField] private float interactDistance = 2.8f;
    private BaseInteractionTarget[] interactionTargets;
    private UiTextConfigAsset textConfig;

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
        textConfig = UiTextConfigProvider.Config;
        RefreshInteractionTargets();
    }

    private void Update()
    {
        if (gameInput == null || uiController == null || interactionRoot == null)
        {
            return;
        }

        if (uiController.IsModalOpen)
        {
            uiController.SetInteractionHint(textConfig.InteractionPromptIdle);
        }

        BaseInteractionTarget nearestTarget = GetNearestTarget();
        if (nearestTarget != null && !uiController.IsModalOpen)
        {
            uiController.SetInteractionHint(string.Format(textConfig.InteractionPromptWithTargetTemplate, nearestTarget.InteractionLabel));
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
