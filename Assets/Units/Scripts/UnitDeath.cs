using UnityEngine;
using UnityEngine.Events;

public class UnitDeath : UnitComponent
{
    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private bool disableOnDeath = true;
    [SerializeField] private float destroyDelay = 2f;

    [Header("Animation")]
    [SerializeField] private string deathAnimationTrigger = "death";

    private UnitHealth unitHealth;
    private Animator animator;

    public bool IsDead { get; private set; }

    public event UnityAction OnUnitDeath;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        unitHealth = Controller.Health;
        animator = GetComponentInChildren<Animator>();

        if (unitHealth)
        {
            unitHealth.OnDeath += HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (IsDead) return;

        IsDead = true;

        Debug.Log($"[UnitDeath] {Controller.Instance.DisplayName} has died");

        // Trigger death animation if available
        if (animator && !string.IsNullOrEmpty(deathAnimationTrigger))
        {
            animator.SetTrigger(deathAnimationTrigger);
        }

        // Disable behaviours
        if (Controller.CurrentBehaviour)
        {
            Controller.CurrentBehaviour.enabled = false;
        }

        // Invoke death event
        OnUnitDeath?.Invoke();

        // Handle destruction or disabling
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
        else if (disableOnDeath)
        {
            // Disable after a delay to allow death animation to play
            Invoke(nameof(DisableUnit), destroyDelay);
        }
    }

    private void DisableUnit()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (unitHealth)
        {
            unitHealth.OnDeath -= HandleDeath;
        }
    }

    public void Revive()
    {
        IsDead = false;
        gameObject.SetActive(true);

        if (unitHealth)
        {
            unitHealth.ResetHealth();
        }

        if (Controller.CurrentBehaviour)
        {
            Controller.CurrentBehaviour.enabled = true;
        }
    }
}
