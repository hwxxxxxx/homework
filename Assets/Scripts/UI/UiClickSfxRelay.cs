using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class UiClickSfxRelay : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        RuntimeShell shell = RuntimeShell.Instance;
        AudioRuntimeService audioService = shell != null ? shell.AudioRuntimeService : null;
        if (audioService != null)
        {
            audioService.PlayUiClickSfx();
        }
    }
}
