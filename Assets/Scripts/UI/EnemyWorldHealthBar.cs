using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyStats))]
public class EnemyWorldHealthBar : MonoBehaviour, IPoolable
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector2 size = new Vector2(72f, 8f);

    private EnemyStats enemyStats;
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform rectTransform;
    private Image fillImage;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        EnsureWidget();
    }

    private void OnEnable()
    {
        if (rectTransform == null || fillImage == null)
        {
            EnsureWidget();
        }

        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(enemyStats.CurrentHealth, enemyStats.MaxHealth);
        }

        if (rectTransform != null)
        {
            rectTransform.gameObject.SetActive(true);
        }
    }

    private void LateUpdate()
    {
        if (rectTransform == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 worldPosition = transform.position + worldOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            rectTransform.gameObject.SetActive(false);
            return;
        }

        if (!rectTransform.gameObject.activeSelf)
        {
            rectTransform.gameObject.SetActive(true);
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint;
    }

    private void OnDisable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged -= HandleHealthChanged;
        }

        if (rectTransform != null)
        {
            rectTransform.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (rectTransform != null)
        {
            Destroy(rectTransform.gameObject);
        }
    }

    public void OnSpawnedFromPool()
    {
        if (rectTransform != null)
        {
            rectTransform.gameObject.SetActive(true);
        }

        if (enemyStats != null)
        {
            HandleHealthChanged(enemyStats.CurrentHealth, enemyStats.MaxHealth);
        }
    }

    public void OnDespawnedToPool()
    {
        if (rectTransform != null)
        {
            rectTransform.gameObject.SetActive(false);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (fillImage == null)
        {
            return;
        }

        float normalized = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
        fillImage.fillAmount = normalized;
        fillImage.rectTransform.localScale = new Vector3(Mathf.Max(0.001f, normalized), 1f, 1f);
    }

    private void EnsureWidget()
    {
        canvas = FindOverlayCanvas();
        if (canvas == null)
        {
            return;
        }

        string widgetName = $"HP_{GetInstanceID()}";
        Transform existing = canvas.transform.Find(widgetName);
        if (existing != null)
        {
            rectTransform = existing as RectTransform;
            fillImage = existing.Find("Fill")?.GetComponent<Image>();
            ConfigureFillImage(fillImage);
            return;
        }

        GameObject root = new GameObject(widgetName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        rectTransform = root.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;

        Image background = root.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(root.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);

        fillImage = fill.GetComponent<Image>();
        ConfigureFillImage(fillImage);
    }

    private static void ConfigureFillImage(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.color = new Color(0.15f, 0.95f, 0.25f, 0.95f);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillAmount = 1f;
        image.rectTransform.pivot = new Vector2(0f, 0.5f);
        image.rectTransform.localScale = Vector3.one;
    }

    private static Canvas FindOverlayCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvases[i];
            }
        }

        return null;
    }
}
