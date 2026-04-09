using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimePhaseCoordinator : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] lifecycleParticipants;

    private readonly List<IRuntimePhaseParticipant> participants = new List<IRuntimePhaseParticipant>();

    public RuntimePhase CurrentPhase { get; private set; } = RuntimePhase.Stopped;
    public event Action<RuntimePhase> OnPhaseChanged;

    private void Awake()
    {
        participants.Clear();
        if (lifecycleParticipants == null)
        {
            return;
        }

        for (int i = 0; i < lifecycleParticipants.Length; i++)
        {
            if (lifecycleParticipants[i] is IRuntimePhaseParticipant participant)
            {
                participants.Add(participant);
            }
        }

        participants.Sort((a, b) => a.LifecycleOrder.CompareTo(b.LifecycleOrder));
    }

    public void EnterPhase(RuntimePhase phase)
    {
        if (CurrentPhase == phase)
        {
            return;
        }

        CurrentPhase = phase;
        for (int i = 0; i < participants.Count; i++)
        {
            participants[i].EnterRuntimePhase(phase);
        }

        OnPhaseChanged?.Invoke(phase);
    }
}
