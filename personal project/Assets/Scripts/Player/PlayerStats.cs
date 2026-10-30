using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health variables")]//just a simple header in the inspector
    [SerializeField] private float maxHealth = 100f;
    public float CurrentHealth { get; private set; }//this makes the variable read only to everything else except this script

    [Header("Stamina variables")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.5f;
    public float CurrentStamina { get; private set; }

    private float staminaRegenTimer;

    //events for decoupled script interaction(no direct function calls between scripts) 
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaDepleted;
    public event Action OnPlayerDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentStamina = maxStamina;
    }

    private void Start()
    {
        //shout initial health and stamina values for UI script
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    private void Update()
    {
        HandleStaminaRegen();
    }

    private void HandleStaminaRegen()//stamina regeneration logic
    {
        if (CurrentStamina < maxStamina)
        {
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else
            {
                CurrentStamina += staminaRegenRate * Time.deltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
                OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
            }
        }
    }

    public bool HasEnoughStamina(float cost)//check if there is enough stamina for __ action
    {
        return CurrentStamina >= cost;
    }

    public void ConsumeStamina(float amount)
    {
        if (amount <= 0f) return;

        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        staminaRegenTimer = staminaRegenDelay;

        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);

        if (CurrentStamina <= 0f)
        {
            OnStaminaDepleted?.Invoke();
        }
    }

    public void TakeDamage(float amount) //to be called when other scripts shout damage event
    {
        if (amount <= 0f || CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public void Heal(float amount) //to be called when other scripts shout heal event
    {
        if (amount <= 0f || CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}