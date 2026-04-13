using System;
using System.Collections;
using UnityEngine;

public class EnemyDeathDissolveView : MonoBehaviour
{
    private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    private static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
    private static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
    private static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");

    [SerializeField] private Renderer[] targetRenderers;

    private MaterialPropertyBlock propertyBlock;
    private Coroutine dissolveRoutine;
    private float currentEdgeWidth;
    private Color currentEdgeColor;
    private float currentNoiseScale;

    private void Awake()
    {
        InitializeIfNeeded();
        ClearPropertyBlocks();
    }

    public void Play(float duration, float edgeWidth, Color edgeColor, float noiseScale, Action onCompleted)
    {
        InitializeIfNeeded();

        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
            dissolveRoutine = null;
        }

        currentEdgeWidth = edgeWidth;
        currentEdgeColor = edgeColor;
        currentNoiseScale = noiseScale;
        ApplyProperties(0f);
        dissolveRoutine = StartCoroutine(DissolveRoutine(Mathf.Max(0.01f, duration), onCompleted));
    }

    public void ResetVisuals()
    {
        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
            dissolveRoutine = null;
        }

        InitializeIfNeeded();
        ClearPropertyBlocks();
    }

    private IEnumerator DissolveRoutine(float duration, Action onCompleted)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            ApplyProperties(t);
            yield return null;
        }

        ApplyProperties(1f);
        dissolveRoutine = null;
        onCompleted?.Invoke();
    }

    private void InitializeIfNeeded()
    {
        if (propertyBlock != null)
        {
            return;
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    private void ApplyProperties(float dissolveValue)
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DissolveId, dissolveValue);
            propertyBlock.SetFloat(EdgeWidthId, currentEdgeWidth);
            propertyBlock.SetColor(EdgeColorId, currentEdgeColor);
            propertyBlock.SetFloat(NoiseScaleId, currentNoiseScale);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void ClearPropertyBlocks()
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer != null)
            {
                renderer.SetPropertyBlock(null);
            }
        }
    }
}
