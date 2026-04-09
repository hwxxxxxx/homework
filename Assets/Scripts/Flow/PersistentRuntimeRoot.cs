using UnityEngine;

public class PersistentRuntimeRoot : MonoBehaviour
{
    private static PersistentRuntimeRoot instance;

    [Header("Player Runtime References")]
    [SerializeField] private GameObject battleSharedRoot;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerSkillSystem playerSkillSystem;
    [SerializeField] private EffectController playerEffectController;

    public GameObject BattleSharedRoot => battleSharedRoot;
    public PlayerCombat PlayerCombat => playerCombat;
    public PlayerStats PlayerStats => playerStats;
    public PlayerSkillSystem PlayerSkillSystem => playerSkillSystem;
    public EffectController PlayerEffectController => playerEffectController;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (battleSharedRoot == null ||
            playerCombat == null ||
            playerStats == null ||
            playerSkillSystem == null ||
            playerEffectController == null)
        {
            throw new System.InvalidOperationException("PersistentRuntimeRoot gameplay runtime references are not fully assigned.");
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
