using System;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum GameFlowState
    {
        Start,
        Combat,
        Victory,
        Fail
    }

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Flow")]
    [SerializeField] private bool autoStartCombat = true;

    private GameFlowState currentState = GameFlowState.Start;

    public event Action<GameFlowState> OnStateChanged;
    public event Action OnCombatStarted;

    public GameFlowState CurrentState => currentState;
    public PlayerStats PlayerStatsRef => playerStats;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDied += HandlePlayerDied;
        }
    }

    private void Start()
    {
        SetState(GameFlowState.Start);

        if (autoStartCombat)
        {
            StartCombat();
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDied -= HandlePlayerDied;
        }
    }

    public void StartCombat()
    {
        if (currentState != GameFlowState.Start)
        {
            return;
        }

        SetState(GameFlowState.Combat);
        OnCombatStarted?.Invoke();
    }

    public void NotifyAllWavesCleared()
    {
        if (currentState != GameFlowState.Combat)
        {
            return;
        }

        SetState(GameFlowState.Victory);
    }

    public void TriggerFail()
    {
        if (currentState == GameFlowState.Victory || currentState == GameFlowState.Fail)
        {
            return;
        }

        SetState(GameFlowState.Fail);
    }

    private void HandlePlayerDied()
    {
        TriggerFail();
    }

    private void SetState(GameFlowState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(currentState);
    }
}
