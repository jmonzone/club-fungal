using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ScriptableObject definition for a health component.
/// Define health parameters that can be shared across multiple units.
/// </summary>
[CreateAssetMenu(fileName = "HealthComponent", menuName = "Club Fungal/Units/Components/Health Component")]
public class HealthComponentDefinition : UnitComponentDefinition
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float regenRate = 0f;
    [SerializeField] private bool canDie = true;

    public float MaxHealth => maxHealth;
    public float RegenRate => regenRate;
    public bool CanDie => canDie;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new HealthComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of health component.
/// Manages actual health values and damage/healing logic.
/// </summary>
[System.Serializable]
public class HealthComponentInstance : UnitComponentInstance
{
    [SerializeField] private float currentHealth;

    public HealthComponentDefinition HealthDefinition => definition as HealthComponentDefinition;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => HealthDefinition.MaxHealth;
    public float HealthPercent => currentHealth / MaxHealth;
    public bool IsDead => currentHealth <= 0f && HealthDefinition.CanDie;

    public event UnityAction<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event UnityAction OnDeath;

    public HealthComponentInstance(HealthComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        currentHealth = MaxHealth;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Auto-regen if enabled
        if (HealthDefinition.RegenRate > 0f && currentHealth < MaxHealth)
        {
            Heal(HealthDefinition.RegenRate * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        float oldMax = MaxHealth;
        currentHealth = Mathf.Min(currentHealth, newMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, newMaxHealth);
    }

    public void FullHeal()
    {
        currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }
}
