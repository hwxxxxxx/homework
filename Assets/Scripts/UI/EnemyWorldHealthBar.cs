using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(EnemyStats))]
public class EnemyWorldHealthBar : MonoBehaviour, IPoolable
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector2 size = new Vector2(72f, 8f);
    [SerializeField] private Camera mainCamera;

    private static GameObject overlayRootObject;
    private static UIDocument overlayDocument;
    private static PanelSettings overlayPanelSettings;
    private static VisualElement overlayRoot;

    private EnemyStats enemyStats;
    private VisualElement barRoot;
    private VisualElement fillElement;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    private void OnEnable()
    {
        EnsureWidget();

        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(enemyStats.CurrentHealth, enemyStats.MaxHealth);
        }

        barRoot.style.display = DisplayStyle.Flex;
    }

    private void LateUpdate()
    {
        Vector3 worldPosition = transform.position + worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            barRoot.style.display = DisplayStyle.None;
            return;
        }

        barRoot.style.display = DisplayStyle.Flex;
        barRoot.style.left = screenPosition.x - (size.x * 0.5f);
        barRoot.style.top = (Screen.height - screenPosition.y) - (size.y * 0.5f);
    }

    private void OnDisable()
    {
        enemyStats.OnHealthChanged -= HandleHealthChanged;
        barRoot.style.display = DisplayStyle.None;
    }

    private void OnDestroy()
    {
        barRoot.RemoveFromHierarchy();
    }

    public void OnSpawnedFromPool()
    {
        barRoot.style.display = DisplayStyle.Flex;
        HandleHealthChanged(enemyStats.CurrentHealth, enemyStats.MaxHealth);
    }

    public void OnDespawnedToPool()
    {
        barRoot.style.display = DisplayStyle.None;
    }

    public void ConfigurePresentation(Camera targetCamera)
    {
        mainCamera = targetCamera;
        EnsureWidget();
    }

    private void HandleHealthChanged(int current, int max)
    {
        float normalized = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
        fillElement.style.width = Length.Percent(normalized * 100f);
    }

    private void EnsureWidget()
    {
        EnsureOverlay();

        if (barRoot != null)
        {
            return;
        }

        barRoot = new VisualElement();
        barRoot.style.position = Position.Absolute;
        barRoot.style.width = size.x;
        barRoot.style.height = size.y;
        barRoot.style.backgroundColor = new Color(0f, 0f, 0f, 0.65f);
        barRoot.style.paddingLeft = 1f;
        barRoot.style.paddingRight = 1f;
        barRoot.style.paddingTop = 1f;
        barRoot.style.paddingBottom = 1f;

        fillElement = new VisualElement();
        fillElement.style.height = Length.Percent(100f);
        fillElement.style.width = Length.Percent(100f);
        fillElement.style.backgroundColor = new Color(0.15f, 0.95f, 0.25f, 0.95f);
        barRoot.Add(fillElement);

        overlayRoot.Add(barRoot);
    }

    private static void EnsureOverlay()
    {
        if (overlayRootObject != null)
        {
            return;
        }

        overlayRootObject = new GameObject("EnemyHealthOverlay");
        overlayPanelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        overlayPanelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
        overlayPanelSettings.clearColor = false;
        overlayPanelSettings.sortingOrder = 250;

        overlayDocument = overlayRootObject.AddComponent<UIDocument>();
        overlayDocument.panelSettings = overlayPanelSettings;

        overlayRoot = overlayDocument.rootVisualElement;
        overlayRoot.style.flexGrow = 1f;
        overlayRoot.style.position = Position.Absolute;
        overlayRoot.style.left = 0f;
        overlayRoot.style.top = 0f;
        overlayRoot.style.right = 0f;
        overlayRoot.style.bottom = 0f;
        overlayRoot.pickingMode = PickingMode.Ignore;
    }
}
