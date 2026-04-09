using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Spell Slots")]
    [SerializeField] private SpellData slot1Spell; // Assign your Fireball ScriptableObject here

    [Header("UI References")]
    [SerializeField] private Image spell1CooldownImage; // Dark overlay that fills during cooldown

    private SpellData selectedSpell; // The currently "armed" spell ready for LMB click
    private float lastCastTime = float.NegativeInfinity;

    private static readonly int CastTriggerHash = Animator.StringToHash("CastSpell");

    private void Start()
    {
        spell1CooldownImage.fillAmount = 0;
    }

    private void Update()
    {
        // Update the visual cooldown on the HUD every frame
        HandleCooldownUI();
    }

    /// <summary>
    /// Selects the spell in the first slot, making it ready to cast.
    /// </summary>
    public void SelectSpellSlot1()
    {
        if (slot1Spell != null)
        {
            selectedSpell = slot1Spell;
            Debug.Log("Spell Selected: " + selectedSpell.spellName);
        }
    }

    /// <summary>
    /// Checks conditions and triggers the spell cast if resources and cooldowns allow.
    /// </summary>
    /// <param name="stats">Reference to the stats module for mana consumption</param>
    /// <param name="mainCamera">Reference to the camera for screen-center aiming</param>
    public void TryCastSelectedSpell(PlayerStats stats, Transform mainCamera)
    {
        // Prevent casting if no spell is selected
        if (selectedSpell == null) return;

        float timeSinceCast = Time.time - lastCastTime;

        // Verify cooldown state and mana availability before executing the cast
        if (timeSinceCast >= selectedSpell.cooldown && stats.ConsumeMana(selectedSpell.manaCost))
        {
            ExecuteCast(mainCamera);
        }
    }

    private void ExecuteCast(Transform mainCamera)
    {
        lastCastTime = Time.time;
        animator.SetTrigger(CastTriggerHash);

        // Aiming Logic: Create a ray from the exact center of the screen
        Ray ray = mainCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        // Check if the crosshair is hovering over an object; otherwise, aim at a point in the distance
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        // Spawn the projectile at chest height (+1 unit up) to avoid floor collisions
        Vector3 spawnPos = transform.position + transform.up;
        Vector3 castDirection = (targetPoint - spawnPos).normalized;

        // Instantiate the prefab and rotate it toward the target
        // The Projectile script on the prefab will handle forward velocity
        Instantiate(selectedSpell.spellPrefab, spawnPos, Quaternion.LookRotation(castDirection));
    }

    private void HandleCooldownUI()
    {
        if (selectedSpell == null || spell1CooldownImage == null) return;

        float timeSinceCast = Time.time - lastCastTime;

        // Calculate radial fill: 1 is fully on cooldown, 0 is ready to use
        float cooldownProgress = Mathf.Clamp01(1 - (timeSinceCast / selectedSpell.cooldown));
        spell1CooldownImage.fillAmount = cooldownProgress;
    }
}