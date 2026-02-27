using UnityEngine;
using System;

/// <summary>
/// Generic health component that tracks damage and death
/// Broadcasts events when damaged or health reaches zero
/// </summary>
public class HealthComponent : MonoBehaviour
{
    public event Action<float, float, float> OnDamaged;
    public event Action<MonoBehaviour> OnDead;

    [SerializeField] float m_maxHealth;
    private float m_CurrentHealth;

    void Start()
    {
        m_CurrentHealth = m_maxHealth;
    }

    public void ApplyDamage(float damage, MonoBehaviour causer)
    {
        float change = Mathf.Min(m_CurrentHealth, damage);
        m_CurrentHealth -= change;

        OnDamaged?.Invoke(m_CurrentHealth, m_maxHealth, damage);

        if (m_CurrentHealth == 0)
        {
            OnDead?.Invoke(causer);
        }
    }

    public float GetCurrentHealth() => m_CurrentHealth;
    public float GetMaxHealth() => m_maxHealth;

    void Update()
    {

    }
}
