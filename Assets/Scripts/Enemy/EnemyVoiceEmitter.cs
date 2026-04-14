using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyBase))]
public class EnemyVoiceEmitter : MonoBehaviour
{
    private EnemyBase enemyBase;
    private Coroutine emitRoutine;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (emitRoutine == null)
        {
            emitRoutine = StartCoroutine(EmitLoop());
        }
    }

    private void OnDisable()
    {
        if (emitRoutine != null)
        {
            StopCoroutine(emitRoutine);
            emitRoutine = null;
        }
    }

    private IEnumerator EmitLoop()
    {
        while (enabled && gameObject.activeInHierarchy)
        {
            AudioRuntimeService audio = RuntimeShell.Instance != null ? RuntimeShell.Instance.AudioRuntimeService : null;
            float delay = audio != null ? audio.GetEnemyVoiceInterval() : 4f;
            yield return new WaitForSeconds(delay);

            if (enemyBase != null && !enemyBase.IsDead && audio != null)
            {
                audio.PlayCueAt(AudioCueId.EnemyVoice, transform.position);
            }
        }
    }
}
