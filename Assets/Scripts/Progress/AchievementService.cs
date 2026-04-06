using System;
using UnityEngine;

public class AchievementService : MonoBehaviour
{
    [SerializeField] private ProgressService progressService;

    private IDisposable bossDefeatedSubscription;
    private IDisposable levelUnlockedSubscription;
    private IDisposable playerDiedSubscription;

    public void ConfigureProgressService(ProgressService service)
    {
        progressService = service;
    }

    private void Awake()
    {
        if (progressService == null)
        {
            progressService = FindObjectOfType<ProgressService>(true);
        }
    }

    private void OnEnable()
    {
        bossDefeatedSubscription = EventBus.Subscribe<BossDefeatedEvent>(HandleBossDefeated);
        levelUnlockedSubscription = EventBus.Subscribe<LevelUnlockedEvent>(HandleLevelUnlocked);
        playerDiedSubscription = EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDisable()
    {
        bossDefeatedSubscription?.Dispose();
        levelUnlockedSubscription?.Dispose();
        playerDiedSubscription?.Dispose();
        bossDefeatedSubscription = null;
        levelUnlockedSubscription = null;
        playerDiedSubscription = null;
    }

    private void HandleBossDefeated(BossDefeatedEvent _)
    {
        if (progressService != null)
        {
            progressService.TryUnlockAchievement(AchievementId.DefeatFirstBoss);
        }
    }

    private void HandleLevelUnlocked(LevelUnlockedEvent _)
    {
        if (progressService != null)
        {
            progressService.TryUnlockAchievement(AchievementId.UnlockFirstLevel);
        }
    }

    private void HandlePlayerDied(PlayerDiedEvent _)
    {
        if (progressService != null)
        {
            progressService.TryUnlockAchievement(AchievementId.FirstDeath);
        }
    }
}
