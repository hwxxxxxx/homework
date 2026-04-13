using UnityEngine;
using UnityEngine.UIElements;

public sealed class UIDraggableWindowManipulator
{
    private readonly VisualElement root;
    private readonly VisualElement window;
    private readonly VisualElement handle;
    private bool isDragging;
    private int activePointerId = -1;
    private Vector2 pointerToWindowOffset;

    public UIDraggableWindowManipulator(VisualElement rootElement, VisualElement windowElement, VisualElement handleElement)
    {
        if (rootElement == null)
        {
            throw new System.ArgumentNullException(nameof(rootElement));
        }

        if (windowElement == null)
        {
            throw new System.ArgumentNullException(nameof(windowElement));
        }

        if (handleElement == null)
        {
            throw new System.ArgumentNullException(nameof(handleElement));
        }

        root = rootElement;
        window = windowElement;
        handle = handleElement;

        handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
        handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
        handle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    public void Dispose()
    {
        handle.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        handle.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        handle.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        handle.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0)
        {
            return;
        }

        MoveWindowToLeftTopAnchoring();
        Vector2 rootPointerPosition = root.WorldToLocal(evt.position);
        pointerToWindowOffset = rootPointerPosition - new Vector2(window.resolvedStyle.left, window.resolvedStyle.top);
        isDragging = true;
        activePointerId = evt.pointerId;
        handle.CapturePointer(activePointerId);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging || evt.pointerId != activePointerId || !handle.HasPointerCapture(activePointerId))
        {
            return;
        }

        Vector2 rootPointerPosition = root.WorldToLocal(evt.position);
        float targetLeft = rootPointerPosition.x - pointerToWindowOffset.x;
        float targetTop = rootPointerPosition.y - pointerToWindowOffset.y;

        float maxLeft = Mathf.Max(0f, root.resolvedStyle.width - window.resolvedStyle.width);
        float maxTop = Mathf.Max(0f, root.resolvedStyle.height - window.resolvedStyle.height);

        window.style.left = Mathf.Clamp(targetLeft, 0f, maxLeft);
        window.style.top = Mathf.Clamp(targetTop, 0f, maxTop);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.pointerId != activePointerId)
        {
            return;
        }

        StopDragging();
        evt.StopPropagation();
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent _)
    {
        StopDragging();
    }

    private void StopDragging()
    {
        if (activePointerId >= 0 && handle.HasPointerCapture(activePointerId))
        {
            handle.ReleasePointer(activePointerId);
        }

        isDragging = false;
        activePointerId = -1;
    }

    private void MoveWindowToLeftTopAnchoring()
    {
        Vector2 localTopLeft = root.WorldToLocal(window.worldBound.position);
        window.style.left = localTopLeft.x;
        window.style.top = localTopLeft.y;
        window.style.right = StyleKeyword.Auto;
        window.style.bottom = StyleKeyword.Auto;
    }
}
