using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Spell Slots")]
    [SerializeField] private SpellData slot1Spell; // Assign your Fireball ScriptableObject here
    [SerializeField] private SpellData slot2Spell; // For hollow purple, we will handle it differently since it has a unique casting process

    [Header("UI References")]
    [SerializeField] private Image spell1CooldownImage; // Dark overlay that fills during cooldown
    [SerializeField] private Image spell2CooldownImage; // For hollow purple, we can use this to show the "charging" state

    [Header("Aiming Settings")]
    [SerializeField] private LayerMask ignoreLayers;

    public SpellData selectedSpell { get; private set; } // The currently "armed" spell ready for LMB click
    private float lastCastTime = float.NegativeInfinity;

    [Header("Hollow Purple Settings")]
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;
    [SerializeField] private GameObject bluePrefab;
    [SerializeField] private GameObject redPrefab;

    [Header("Melee Settings")]
    [SerializeField] private float comboLeeway = 0.8f; // Time allowed between clicks to continue combo
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private Transform attackPoint; // Created empty GameObject on Gojo's fist
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;

    private int comboIndex = 0;
    private float lastMeleeTime;


    private GameObject activeBlue;
    private GameObject activeRed;
    private GameObject activePurple;
    private Vector3 savedTargetPoint; // To remember where we aimed when the animation started
    private CinemachineImpulseSource impulseSource; // For camera shake on impact

    private static readonly int CastTriggerPrepareHollowPurpleHash = Animator.StringToHash("castPrepareHollowPurple");
    private static readonly int CastTriggerReleaseHollowPurpleHash = Animator.StringToHash("castReleaseHollowPurple");
    private static readonly int CastTriggerFireballHash = Animator.StringToHash("castFireball");
    private static readonly int MeleeAttackTriggerHash = Animator.StringToHash("meleeAttack");
    private static readonly int ComboIndexHash = Animator.StringToHash("comboIndex");
    public bool IsSpellSelected => selectedSpell != null;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        spell1CooldownImage.fillAmount = 0;
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

    public void SelectSpellSlot1()
    {
        if (slot1Spell != null)
        {
            selectedSpell = slot1Spell;
            Debug.Log("Spell Selected: " + selectedSpell.spellName);
        }
    }

    public void SelectSpellHollowPurple()
    {
        if (bluePrefab != null && redPrefab != null)
        {
            selectedSpell = slot2Spell; // This will be our "Hollow Purple" spell data, even though the actual casting process is unique
            animator.SetTrigger(CastTriggerPrepareHollowPurpleHash);

            Debug.Log("Spell Selected: " + selectedSpell.spellName);
        }
    }

    public void DeselectSpell()
    {
        selectedSpell = null;
        
        if (activeBlue) Destroy(activeBlue);
        if (activeRed) Destroy(activeRed);

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

        float timeSinceCast = Time.time - lastCastTime;

        // Verify cooldown state and mana availability before executing the cast
        if (timeSinceCast >= selectedSpell.cooldown && stats.CanConsumeMana(selectedSpell.manaCost))
        {
            stats.ConsumeMana(selectedSpell.manaCost);
            ExecuteCast(mainCamera);
        }
    }

    private void ExecuteCast(Transform mainCamera)
    {
        lastCastTime = Time.time;
        if (selectedSpell == slot1Spell)
        {
            animator.SetTrigger(CastTriggerFireballHash); // Trigger the Fireball animation

        }
        else if (selectedSpell == slot2Spell)
        {
            animator.SetTrigger(CastTriggerReleaseHollowPurpleHash); // Trigger the Hollow Purple animation
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
        // Get information about the current state on Layer 3 (Fight Mode)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(3);

        // Prevent skipping: If we are already playing an attack, 
        // don't allow a new one until the current one is mostly finished (e.g., 70%)
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
        activeBlue = Instantiate(bluePrefab, leftHandAnchor.position, Quaternion.identity, leftHandAnchor);
    }

    // Called by Animation Event mid-way
    public void OnSpawnRed()
    {
        activeRed = Instantiate(redPrefab, rightHandAnchor.position, Quaternion.identity, rightHandAnchor);
    }

    // Called when hands come together
    public void OnMergePurple()
    {
        if (activeBlue) Destroy(activeBlue);
        if (activeRed) Destroy(activeRed);

        // Spawn the "Charging" version of purple at the chest
        Vector3 mergePos = (leftHandAnchor.position + rightHandAnchor.position) / 2;
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

    // Called by Animation Event
    public void HitDetected()
    {
        // Create an invisible sphere at Gojo's hand to see who we hit
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
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
        if (selectedSpell == null || spell1CooldownImage == null) return;

        float timeSinceCast = Time.time - lastCastTime;

        // Calculate radial fill: 1 is fully on cooldown, 0 is ready to use
        float cooldownProgress = Mathf.Clamp01(1 - (timeSinceCast / selectedSpell.cooldown));
        spell1CooldownImage.fillAmount = cooldownProgress;
    }

    public SpellData GetSlot1Spell()
    {
        return slot1Spell;
    }

    public SpellData GetSlot2Spell()
    {
        return slot2Spell;
    }
}