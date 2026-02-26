using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    #region
    [Header("Health System Control")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    private bool isAlive = true;

    #endregion

    private void takeDamage(int Damage)
    {
        // basic outline of taking damage, you can add more complex logic here such as adding invincibility frames etc.
        currentHealth -= Damage;
        if curentHealth <= 0)
        {
            isAlive = false
            DeathHandling();
            return;

        }
    }

private void DeathHandling()
    {
        // add full kill logic here, such as playing death animation etc.
        Destroy(gameObject);
    }

}
