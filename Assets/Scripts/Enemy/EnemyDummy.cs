using UnityEngine;

public class EnemyDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 50;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(name + " HP: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(name + " Died");
        Destroy(gameObject);
    }
}