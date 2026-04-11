using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/Audio Config", fileName = "AudioConfig")]
public class AudioConfigAsset : ScriptableObject
{
    [Header("BGM")]
    [SerializeField] private AudioClip bgmMainLoop;
    [SerializeField] private float bgmVolume = 0.7f;

    [Header("Player SFX")]
    [SerializeField] private AudioClip playerWeaponFire;

    [Header("Enemy SFX - Normal")]
    [SerializeField] private AudioClip normalEnemyAttack;
    [SerializeField] private AudioClip normalEnemyDeath;

    [Header("Enemy SFX - Boss")]
    [SerializeField] private AudioClip bossEnemyAttack;
    [SerializeField] private AudioClip bossEnemyDeath;

    [Header("3D SFX")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 30f;

    public AudioClip BgmMainLoop => bgmMainLoop;
    public float BgmVolume => bgmVolume;
    public AudioClip PlayerWeaponFire => playerWeaponFire;
    public AudioClip NormalEnemyAttack => normalEnemyAttack;
    public AudioClip NormalEnemyDeath => normalEnemyDeath;
    public AudioClip BossEnemyAttack => bossEnemyAttack;
    public AudioClip BossEnemyDeath => bossEnemyDeath;
    public float SfxVolume => sfxVolume;
    public float SpatialBlend => spatialBlend;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
}
