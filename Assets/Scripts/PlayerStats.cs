using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenRate = 5f; // Health regained per second
    [SerializeField] private float healthRegenDelay = 5f; // Seconds without taking damage before regen starts
    [SerializeField] private float respawnDelay = 4f;
    [SerializeField] private string respawnSceneName = "JujutsuHigh";

    [Header("Health UI")]
    [SerializeField] private Image healthFillImage;

    [Header("Stamina UI")]
    [SerializeField] private GameObject staminaBarParent; // Object to hide/show
    [SerializeField] private Image staminaFillImage; // Visual fill for stamina

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDepleteRate = 15f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1.5f; // Seconds before stamina starts regenerating

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;

    [Header("Mana UI")]
    [SerializeField] private Image manaFillImage; // Visual fill for mana (New UI reference)

    private float currentHealth;
    private float currentStamina;
    private float currentMana;
    private float timeSinceLastDamage;
    private float timeSinceLastStaminaConsume;
    private bool isDead;

    public bool IsDead => isDead;

    private static readonly int IsDeadHash = Animator.StringToHash("isDead");
    private static readonly int HasDiedHash = Animator.StringToHash("hasDied");

    private void Start()
    {
        // Initialize resources to their maximum values at the start
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;

        // Hide the stamina bar initially if it is full
        if (staminaBarParent != null)
            staminaBarParent.SetActive(false);

        //Debug
        Debug.Log($"PlayerStats initialized. Health: {currentHealth}/{maxHealth}, Stamina: {currentStamina}/{maxStamina}, Mana: {currentMana}/{maxMana}");
    }

    public void TickResources(bool isSprinting, bool isGrounded, bool isShiftPressed)
    {
        HandleHealthLogic();
        HandleStaminaLogic(isSprinting, isGrounded, isShiftPressed);
        HandleManaLogic();
        UpdateUI();
    }

    private void HandleHealthLogic()
    {
        timeSinceLastDamage += Time.deltaTime;

        if (timeSinceLastDamage >= healthRegenDelay && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    private void HandleStaminaLogic(bool isSprinting, bool isGrounded, bool isShiftPressed)
    {
        if (isSprinting)
        {
            // Deplete stamina while the player is sprinting
            currentStamina -= staminaDepleteRate * Time.deltaTime;
            timeSinceLastStaminaConsume = 0f; // Reset regeneration delay
        }
        else
        {
            timeSinceLastStaminaConsume += Time.deltaTime;

            if (!isShiftPressed && isGrounded && timeSinceLastStaminaConsume >= staminaRegenDelay)
            {
                // Regenerate stamina only when touching the ground, not sprinting, and after the delay
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void HandleManaLogic()
    {
        // Continuously regenerate mana over time up to the maximum
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
    }

    private void UpdateUI()
    {
        // Toggle visibility: Hide stamina bar when full to reduce screen clutter
        if (staminaBarParent != null)
        {
            staminaBarParent.SetActive(currentStamina < maxStamina);
        }

        // Update fill amounts for the HUD images
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
        }

        if (manaFillImage != null)
        {
            manaFillImage.fillAmount = currentMana / maxMana;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            timeSinceLastStaminaConsume = 0f; // Reset regeneration delay
            return true;
        }
        return false;
    }

    public bool CanConsumeStamina(float amount) => currentStamina >= amount;

    public bool ConsumeMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
    }

    public bool CanConsumeMana(float amount) => currentMana >= amount;

    public bool HasStamina() => currentStamina > 0;

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        timeSinceLastDamage = 0f; // Reset delay timer
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }

        //Debug
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger(HasDiedHash);
            animator.SetBool(IsDeadHash, true);
        }
        Debug.Log("Player has died.");
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        SceneManager.LoadScene(respawnSceneName);

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;
        timeSinceLastDamage = 0f;
        timeSinceLastStaminaConsume = 0f;
        isDead = false;
        if (animator != null)
        {
            animator.SetBool(IsDeadHash, false);
        }
        UpdateUI();
    }
}