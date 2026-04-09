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
        progressService.TryUnlockAchievement(AchievementId.DefeatFirstBoss);
    }

    private void HandleLevelUnlocked(LevelUnlockedEvent _)
    {
        progressService.TryUnlockAchievement(AchievementId.UnlockFirstLevel);
    }

    private void HandlePlayerDied(PlayerDiedEvent _)
    {
        progressService.TryUnlockAchievement(AchievementId.FirstDeath);
    }
}
