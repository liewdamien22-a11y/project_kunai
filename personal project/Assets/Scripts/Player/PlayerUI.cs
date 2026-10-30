using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI healthText;
    public PlayerStats PlayerStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (PlayerStats == null)
        {
            PlayerStats = FindFirstObjectByType<PlayerStats>();
        }
    }
    private void OnEnable()
    {
        if(PlayerStats != null)
        {
            PlayerStats.OnStaminaChanged += UpdateStaminaUI;//subscribe to stamina change event in playerstats
            PlayerStats.OnHealthChanged += UpdateHealthUI;//subscribe to health change event in playerstats
        }
    }
    private void OnDisable()
    {
        if(PlayerStats != null)
        {
            PlayerStats.OnStaminaChanged -= UpdateStaminaUI;//unsubscribe just in case
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        if (staminaText != null)
        {
            //round stamina value and display
            staminaText.text = $"STAMINA: {Mathf.CeilToInt(currentStamina)} / {Mathf.CeilToInt(maxStamina)}";
        }
    }
    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if(healthText != null)
        {
            healthText.text = $"Health: {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
