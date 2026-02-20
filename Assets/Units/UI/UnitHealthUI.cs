using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitHealthUI : UnitComponent
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private UnitHealth unitHealth;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        unitHealth = Controller.GetComponent<UnitHealth>();
        if (unitHealth)
        {
            unitHealth.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(unitHealth.CurrentHealth, unitHealth.MaxHealth);
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthText)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        if (healthSlider)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthFillImage)
        {
            float healthPercent = currentHealth / maxHealth;
            healthFillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                healthPercent > lowHealthThreshold ? 1f : healthPercent / lowHealthThreshold);
        }
    }

    private void OnDestroy()
    {
        if (unitHealth)
        {
            unitHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }
}
