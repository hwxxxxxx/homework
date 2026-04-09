using UnityEngine;

public class PlayerStats : HealthComponent, IDamageable
{
    [SerializeField] private bool disableComponentsOnDeath = true;

    protected override void OnDiedInternal()
    {
        EventBus.Publish(new PlayerDiedEvent());

        if (!disableComponentsOnDeath)
        {
            return;
        }

        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour != null && behaviour != this)
            {
                behaviour.enabled = false;
            }
        }
    }
}
