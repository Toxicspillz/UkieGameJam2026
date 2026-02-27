using UnityEngine;

/// <summary>
/// Central player controller that listens to health events and handles player death
/// </summary>
public class PlayerController : MonoBehaviour
{
    private HealthComponent m_HealthComponent;

    private void Awake()
    {
        m_HealthComponent = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        m_HealthComponent.OnDamaged += Handle_OnHealthDamage;
        m_HealthComponent.OnDead += Handle_OnPlayerDies;
    }

    private void OnDestroy()
    {
        m_HealthComponent.OnDamaged -= Handle_OnHealthDamage;
        m_HealthComponent.OnDead -= Handle_OnPlayerDies;
    }

    void Update()
    {

    }

    private void Handle_OnHealthDamage(float currentHealth, float maxHealth, float inBoundDamage)
    {
        Debug.Log($"Current Health is: {currentHealth} out of: {maxHealth} and Damage received was: {inBoundDamage}");
    }

    private void Handle_OnPlayerDies(MonoBehaviour causer)
    {
        Debug.Log($"Our player died, due to {causer.gameObject.name}");
    }
}
