using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Spell Slots")]
    [SerializeField] private SpellData slot1Spell; // For Blue
    [SerializeField] private SpellData slot2Spell; // For Red
    [SerializeField] private SpellData slot3Spell; // For hollow purple
    [SerializeField] private SpellData slotUltimativeSpell; // FUTURE: For domain expansion

    [Header("UI References")]
    [SerializeField] private Image spell1CooldownImage; // Overlay for Blue that fills during cooldown
    [SerializeField] private Image spell2CooldownImage; // Overlay for Red that fills during cooldown
    [SerializeField] private Image spell3CooldownImage; // Overlay for Hollow Purple that fills during cooldown
    [SerializeField] private Image ultimativeCooldownImage; // FUTURE: Overlay for Domain Expansion that fills during cooldown

    [Header("Aiming Settings")]
    [SerializeField] private LayerMask ignoreLayers;

    public SpellData selectedSpell { get; private set; } // The currently "armed" spell ready for LMB click
    private float lastCastTime = float.NegativeInfinity;

    [Header("Gojo's References")]
    [SerializeField] private Transform HP_leftHandAnchor;
    [SerializeField] private Transform HP_rightHandAnchor;
    [SerializeField] private Transform R_rightHandAnchor;

    [Header("Sphere Prefabs")]
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private GameObject HP_redPrefab;
    [SerializeField] private GameObject R_redPrefab;

    [Header("Melee Settings")]
    [SerializeField] private float comboLeeway = 0.8f; // Time allowed between clicks to continue combo
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private Transform leftHandAttackPoint;
    [SerializeField] private Transform rightHandAttackPoint;
    [SerializeField] private Transform rightLegAttackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;

    private int comboIndex = 0;
    private float lastMeleeTime;


    private GameObject activeBlue;
    private GameObject activeRed;
    private GameObject activePurple;
    private Vector3 savedTargetPoint; // To remember where we aimed when the animation started
    private CinemachineImpulseSource impulseSource; // For camera shake on impact

    private float spell1LastCastTime = float.NegativeInfinity;
    private float spell2LastCastTime = float.NegativeInfinity;
    private float spell3LastCastTime = float.NegativeInfinity;
    private float ultimativeSpellLastCastTime = float.NegativeInfinity;

    private static readonly int CastTriggerPrepareSlot1SpellHash = Animator.StringToHash("castPrepareSlot1Spell");
    private static readonly int CastTriggerReleaseSlot1SpellHash = Animator.StringToHash("castReleaseSlot1Spell");

    private static readonly int CastTriggerPrepareSlot2SpellHash = Animator.StringToHash("castPrepareSlot2Spell");
    private static readonly int CastTriggerReleaseSlot2SpellHash = Animator.StringToHash("castReleaseSlot2Spell");

    private static readonly int CastTriggerPrepareSlot3SpellHash = Animator.StringToHash("castPrepareSlot3Spell");
    private static readonly int CastTriggerReleaseSlot3SpellHash = Animator.StringToHash("castReleaseSlot3Spell");

    private static readonly int MeleeAttackTriggerHash = Animator.StringToHash("meleeAttack");
    private static readonly int ComboIndexHash = Animator.StringToHash("comboIndex");
    public bool IsSpellSelected => selectedSpell != null;

    public bool IsAttacking
    {
        get
        {
            if (animator == null) return false;
            // Treating it as attacking if playing the attack animation, or if an attack was just triggered instantly.
            return animator.GetCurrentAnimatorStateInfo(2).IsTag("Attack") || (Time.time - lastMeleeTime) < 0.1f;
        }
    }

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        spell1CooldownImage.fillAmount = 1;
        spell2CooldownImage.fillAmount = 1;
        spell3CooldownImage.fillAmount = 1;
    }

    private void Update()
    {
        // Update the visual cooldown on the HUD every frame
        HandleCooldownUI();

        // Reset combo if too much time has passed
        if (Time.time - lastMeleeTime > comboLeeway)
        {
            comboIndex = 0;
            animator.SetInteger(ComboIndexHash, comboIndex);
        }
        
        animator.SetBool("isSpellSelected", IsSpellSelected);
    }

    public bool SelectSpellSlot1(PlayerStats stats)
    {
        if (slot1Spell != null && bluePrefab != null)
        {
            float timeSinceCast = Time.time - spell1LastCastTime;
            if (timeSinceCast >= slot1Spell.cooldown && stats.CanConsumeMana(slot1Spell.manaCost))
            {
                DeselectSpell(); // Clear any previously selected spell
                selectedSpell = slot1Spell;
                animator.SetTrigger(CastTriggerPrepareSlot1SpellHash);
                Debug.Log("Spell Selected: " + selectedSpell.spellName);
                return true;
            }
            else
            {
                Debug.Log(slot1Spell.spellName + " is on cooldown.");
            }
        }
        return false;
    }

    public bool SelectSpellSlot2(PlayerStats stats)
    {
        if (slot2Spell != null && R_redPrefab != null)
        {
            float timeSinceCast = Time.time - spell2LastCastTime;
            if (timeSinceCast >= slot2Spell.cooldown && stats.CanConsumeMana(slot2Spell.manaCost))
            {
                DeselectSpell(); // Clear any previously selected spell
                selectedSpell = slot2Spell;
                animator.SetTrigger(CastTriggerPrepareSlot2SpellHash);
                Debug.Log("Spell Selected: " + selectedSpell.spellName);
                return true;
            }
            else
            {
                Debug.Log(slot2Spell.spellName + " is on cooldown.");
            }
        }
        return false;
    }

    public bool SelectSpellSlot3(PlayerStats stats)
    {
        if (slot3Spell != null && bluePrefab != null && HP_redPrefab != null)
        {
            float timeSinceCast = Time.time - spell3LastCastTime;
            if (timeSinceCast >= slot3Spell.cooldown && stats.CanConsumeMana(slot3Spell.manaCost))
            {
                DeselectSpell(); // Clear any previously selected spell
                selectedSpell = slot3Spell;
                animator.SetTrigger(CastTriggerPrepareSlot3SpellHash);
                Debug.Log("Spell Selected: " + selectedSpell.spellName);
                return true;
            }
            else
            {
                Debug.Log(slot3Spell.spellName + " is on cooldown.");
            }
        }
        return false;
    }

    public void DeselectSpell()
    {
        if (activeBlue) Destroy(activeBlue);
        if (activeRed) Destroy(activeRed);
        
        selectedSpell = null;
        Debug.Log("Spell Deselected");
    }

    public void TryPerformAttack(PlayerStats stats, Transform mainCamera)
    {
        if (IsSpellSelected)
        {
            TryCastSelectedSpell(stats, mainCamera);
        }
        else
        {
            PerformMeleeCombo();
        }
    }

    public void TryCastSelectedSpell(PlayerStats stats, Transform mainCamera)
    {
        // Prevent casting if no spell is selected
        if (selectedSpell == null) return;

        float timeSinceCast = 0f;
        if (selectedSpell == slot1Spell) timeSinceCast = Time.time - spell1LastCastTime;
        else if (selectedSpell == slot2Spell) timeSinceCast = Time.time - spell2LastCastTime;
        else if (selectedSpell == slot3Spell) timeSinceCast = Time.time - spell3LastCastTime;
        else if (selectedSpell == slotUltimativeSpell) timeSinceCast = Time.time - ultimativeSpellLastCastTime;
        else timeSinceCast = Time.time - lastCastTime;

        // Verify cooldown state and mana availability before executing the cast
        if (timeSinceCast >= selectedSpell.cooldown && stats.CanConsumeMana(selectedSpell.manaCost))
        {
            stats.ConsumeMana(selectedSpell.manaCost);
            ExecuteCast(mainCamera);
        }
        else
        {
            Debug.Log("Cannot cast " + selectedSpell.spellName + ". Cooldown or Mana not ready.");
        }
    }

    private void ExecuteCast(Transform mainCamera)
    {
        lastCastTime = Time.time;
        if (selectedSpell == slot1Spell)
        {
            spell1LastCastTime = Time.time;
            animator.SetTrigger(CastTriggerReleaseSlot1SpellHash); // Trigger the slot 1 spell animation
        }
        else if (selectedSpell == slot2Spell)
        {
            spell2LastCastTime = Time.time;
            animator.SetTrigger(CastTriggerReleaseSlot2SpellHash); // Trigger the slot 2 spell animation
        }
        else if (selectedSpell == slot3Spell)
        {
            spell3LastCastTime = Time.time;
            animator.SetTrigger(CastTriggerReleaseSlot3SpellHash); // Trigger the Slot 3 spell animation
        }
        else if (selectedSpell == slotUltimativeSpell)
        {
            ultimativeSpellLastCastTime = Time.time;
            // FUTURE: Trigger the ultimative spell animation
        }

        // Calculate aiming point immediately so the projectile knows where to go later
        Ray ray = mainCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~ignoreLayers))
        {
            savedTargetPoint = hit.point;
        }
        else
        {
            savedTargetPoint = ray.GetPoint(100f);
        }
    }


    private void PerformMeleeCombo()
    {
        // Get information about the current state on Layer 2 (Fight Mode)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(2);

        // Prevent skipping: If we are already playing an attack, 
        // don't allow a new one until the current one is 70% finished
        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime < 0.7f)
        {
            return;
        }

        // Reset or Increment logic
        if (Time.time - lastMeleeTime > comboLeeway)
        {
            comboIndex = 1; // Start fresh
        }
        else
        {
            comboIndex++;
            if (comboIndex > 5) comboIndex = 1; // Assuming a 5-hit combo
        }

        lastMeleeTime = Time.time;

        // Update Animator parameters
        animator.SetInteger(ComboIndexHash, comboIndex);
        animator.SetTrigger(MeleeAttackTriggerHash);
    }

    // The Hollow Purple casting process
    // Called by Animation Event at the start
    public void OnSpawnBlue()
    {
        activeBlue = Instantiate(bluePrefab, HP_leftHandAnchor.position, Quaternion.identity, HP_leftHandAnchor);
    }

    // Called by Animation Event mid-way
    public void HP_OnSpawnRed()
    {
        activeRed = Instantiate(HP_redPrefab, HP_rightHandAnchor.position, Quaternion.identity, HP_rightHandAnchor);
    }

    public void R_OnSpawnRed()
    {
        activeRed = Instantiate(R_redPrefab, R_rightHandAnchor.position, Quaternion.identity, R_rightHandAnchor);
    }

    // Called when hands come together
    public void OnMergePurple()
    {
        if (activeBlue) Destroy(activeBlue);
        if (activeRed) Destroy(activeRed);

        // Spawn the "Charging" version of purple at the chest
        Vector3 mergePos = (HP_leftHandAnchor.position + HP_rightHandAnchor.position) / 2;
        activePurple = Instantiate(selectedSpell.spellPrefab, mergePos, Quaternion.identity, transform);
    }

    // Called when hands push forward
    public void OnLaunchPurple()
    {
        if (activePurple != null)
        {
            activePurple.transform.SetParent(null); // Detach from player

            Vector3 launchDir = (savedTargetPoint - activePurple.transform.position).normalized;

            // Setup the projectile movement
            HollowPurpleProjectile projectileScript = activePurple.GetComponent<HollowPurpleProjectile>();
            if (projectileScript != null)
            {
                projectileScript.impulseSource.GenerateImpulse(); // Trigger camera shake on launch
                projectileScript.Launch(launchDir);
                DeselectSpell();
            }
        }
    }

    public void OnLaunchRed()
    {
        if (activeRed != null)
        {
            activeRed.transform.SetParent(null); // Detach from player
            Vector3 launchDir = (savedTargetPoint - activeRed.transform.position).normalized;
            RedProjectile projectileScript = activeRed.GetComponent<RedProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Launch(launchDir);
                selectedSpell = null; // Deselect after launch
            }
        }
    }

    public void OnLaunchBlue()
    {
        if (activeBlue != null)
        {
            activeBlue.transform.SetParent(null); // Detach from player
            Vector3 launchDir = (savedTargetPoint - activeBlue.transform.position).normalized;
            BlueProjectile projectileScript = activeBlue.GetComponent<BlueProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Launch(launchDir);
                selectedSpell = null; // Deselect after launch
                activeBlue = null; // Clear reference since it's now in the world
            }
        }
    }

    // Called by Animation Event
    public void HitDetected()
    {
        Transform currentAttackPoint = rightHandAttackPoint; // Default fallback

        // 1: Left Hand, 2: Right Hand, 3: Left Hand, 4: Right Leg, 5: Right Leg
        if (comboIndex == 1 || comboIndex == 3) currentAttackPoint = leftHandAttackPoint;
        else if (comboIndex == 2) currentAttackPoint = rightHandAttackPoint;
        else if (comboIndex >= 4) currentAttackPoint = rightLegAttackPoint;

        if (currentAttackPoint == null)
        {
            Debug.LogWarning("Current attack point is missing!");
            return;
        }

        // Create an invisible sphere at the active limb to see who we hit
        Collider[] hitEnemies = Physics.OverlapSphere(currentAttackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name + ", combo index: " + comboIndex);
            // TODO: enemy.GetComponent<EnemyHealth>().TakeDamage(meleeDamage);

            // Add a bit of force to make it feel "Gojo" strong
            if (enemy.TryGetComponent(out Rigidbody rb))
            {
                Vector3 pushDir = (enemy.transform.position - transform.position).normalized;
                rb.AddForce(pushDir * 5f, ForceMode.Impulse);
            }
        }
    }

    private void HandleCooldownUI()
    {
        // Update Slot 1 (Blue)
        if (spell1CooldownImage != null && slot1Spell != null)
        {
            float timeSinceCast1 = Time.time - spell1LastCastTime;
            // Fill goes from 0 (empty, on cooldown) to 1 (full, ready to use)
            float progress1 = Mathf.Clamp01(timeSinceCast1 / slot1Spell.cooldown);
            spell1CooldownImage.fillAmount = progress1;
        }

        // Update Slot 2 (Red)
        if (spell2CooldownImage != null && slot2Spell != null)
        {
            float timeSinceCast2 = Time.time - spell2LastCastTime;
            float progress2 = Mathf.Clamp01(timeSinceCast2 / slot2Spell.cooldown);
            spell2CooldownImage.fillAmount = progress2;
        }

        // Update Slot 3 (Hollow Purple)
        if (spell3CooldownImage != null && slot3Spell != null)
        {
            float timeSinceCast3 = Time.time - spell3LastCastTime;
            float progress3 = Mathf.Clamp01(timeSinceCast3 / slot3Spell.cooldown);
            spell3CooldownImage.fillAmount = progress3;
        }
    }

    public SpellData GetSlot1Spell()
    {
        return slot1Spell;
    }

    public SpellData GetSlot2Spell()
    {
        return slot2Spell;
    }

    public SpellData GetSlot3Spell() 
    { 
        return slot3Spell;
    }
}