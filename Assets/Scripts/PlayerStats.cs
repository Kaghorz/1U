using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Stamina UI")]
    [SerializeField] private GameObject staminaBarParent; // Object to hide/show
    [SerializeField] private Image staminaFillImage; // Visual fill for stamina

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDepleteRate = 15f;
    [SerializeField] private float staminaRegenRate = 15f;

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;

    [Header("Mana UI")]
    [SerializeField] private Image manaFillImage; // Visual fill for mana (New UI reference)

    private float currentStamina;
    private float currentMana;

    private void Start()
    {
        // Initialize resources to their maximum values at the start
        currentStamina = maxStamina;
        currentMana = maxMana;

        // Hide the stamina bar initially if it is full
        if (staminaBarParent != null)
            staminaBarParent.SetActive(false);
    }

    public void TickResources(bool isSprinting, bool isGrounded)
    {
        HandleStaminaLogic(isSprinting, isGrounded);
        HandleManaLogic();
        UpdateUI();
    }

    private void HandleStaminaLogic(bool isSprinting, bool isGrounded)
    {
        if (isSprinting)
        {
            // Deplete stamina while the player is sprinting
            currentStamina -= staminaDepleteRate * Time.deltaTime;
        }
        else if (isGrounded)
        {
            // Regenerate stamina only when touching the ground and not sprinting
            currentStamina += staminaRegenRate * Time.deltaTime;
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
    }

    public bool ConsumeStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
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
}