using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [Header("Necessary References")]
    public Transform body;
    public Transform eye;
    public Transform startingPosition;
    public Vector3 startingRotation;

    [Space(10)]


    [Header("Audio sources")]
    public AudioClip normalAttack;
    public AudioClip specialAttack;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer_eye;
    private MeshRenderer meshRenderer_body;

    private Color eyeColor_normal = new Color(204, 204, 204);
    private Color eyeColor_rage = new Color(207, 20, 20);

    private NavMeshAgent agent;

    //** AI **\\
    private Transform player;
    private PlayerStats playerStatsScript;


    #region Variables
    //** ATTACKS **\\
    private readonly int specialAttackThreshold = 80;    // If a random number is above this threshold, a special attack will be executed (when possible by cooldown)
    private readonly float normalAttackCooldown = 3f;
    private float timeSinceLastNormalAttack;
    private readonly float specialAttackCooldown = 20f;
    private float timeSinceLastSpecialAttack;

    private float attackRange = 1.5f;

    private readonly int screechDamage = 10;
    private readonly int biteDamage = 10;
    private readonly int laserDamage = 10;

    private float bindDuration = 3f;



    //** SHIELD AND HEALTH **\\
    private int currentHealth;
    private readonly int maxHealth;
    private float shieldAmount;
    private float maxShieldAmount = 300f;
    private float shieldStockpile = 0;      // Mana brought in from minions. IDEA: Mana builds in crystals from which the boss can drain mid-fight when own sield is low
    private List<GameObject> manaStockpiles;

    //** AGGREVATION **\\
    private bool isAggro = false;
    private bool playerLeftZone = false;
    private readonly float aggroTimeout = 30f;
    private float secondsUntilDeAggro = 0f;

    private bool secondPhaseEntered = false;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        meshRenderer_eye = eye.GetComponent<MeshRenderer>();
        meshRenderer_body = body.GetComponent<MeshRenderer>();
        agent = GetComponent<NavMeshAgent>();
        startingRotation = transform.rotation.eulerAngles;

        shieldAmount = maxShieldAmount;
        manaStockpiles = new List<GameObject>();
        audioSource = transform.GetChild(0).GetComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerStatsScript = player.GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        // Aggro timeout
        if(playerLeftZone) {
            secondsUntilDeAggro -= Time.deltaTime;
            if (secondsUntilDeAggro <= 0) {
                // Player left area for an extended period of time. 
                // De-Aggro and return to start
                secondsUntilDeAggro = aggroTimeout;
                isAggro = false;
                meshRenderer_eye.material.color = eyeColor_normal;
                agent.destination = startingPosition.position;
            }
        }

        if (!isAggro)
            return;

        // Rotate eye towards player when in fov
        TargetEyeTracker();

        HandleAttack();

        /*if(!secondPhaseEntered)
            if(currentHealth < maxHealth / 2 && shieldAmount < maxShieldAmount / 3 && GetManaReservoirsAmount() == 0) {
                // Enter Phase 2 -> deal more damage, reduced cooldown on special attacks
                secondPhaseEntered = true;

                // Call for help -> Spawn multiple minions around the area
                    // Loop
                        // Determine spawn position
                        // Spawn minion
            }*/
    }

    private void TargetEyeTracker()
    {

    }

    /// <summary>
    /// Determines which attack the boss should execute based on a sequence of random numbers.
    /// </summary>
    private void HandleAttack()
    {
        // Normal Attacks: Screech, Bite
        // Special Attacks: Eye Laser, Bind

        int attackTypeDecisionValue = Random.Range(1, 101);
        int attackVariantValue = Random.Range(1,101);

        timeSinceLastSpecialAttack += Time.deltaTime;
        timeSinceLastNormalAttack += Time.deltaTime;

        if (attackTypeDecisionValue <= specialAttackThreshold || 
            (attackTypeDecisionValue <= specialAttackThreshold && timeSinceLastSpecialAttack < specialAttackCooldown)) {
            if (timeSinceLastNormalAttack < normalAttackCooldown)
                return;

            // Determine which normal attack should be executed
            if(attackVariantValue <= 50) {
                // Bite the player
                ExecuteBite();
            }
            else {
                // Screech at the player
                ExecuteScreech();
            }
            timeSinceLastNormalAttack = 0f;
        }
        else {                
            // Determine which special attack should be executed
            if (attackVariantValue <= 50) {
                // Bite the player
                ExecuteLaser();
            }
            else {
                // Screech at the player
                ExecuteBind();
            }
            timeSinceLastSpecialAttack = 0f;
        }
    }

    //** ATTACKS **\\
    private void ExecuteScreech()
    {
        // Play audio source

        // Play special effect and animation

        // Deal damage
        playerStatsScript.AlterHealth(screechDamage, DamageType.Damage);
    }

    private void ExecuteBite()
    {
        // Play audio source

        // Play animation

        // Deal damage
        playerStatsScript.AlterHealth(biteDamage, DamageType.Damage);
    }

    private void ExecuteLaser()
    {
        // Play audio source

        // Play animation

        // Deal damage
        playerStatsScript.AlterHealth(laserDamage, DamageType.Damage);
    }

    private void ExecuteBind()
    {
        // TODO: Special effect to signify this state to the player i.e. vines/tendrils around him
        playerStatsScript.RestrictMovement(bindDuration);
    }

    private int GetManaReservoirsAmount()
    {
        // TODO: Implement if we implement reservoirs
        return 0;
    }

    #region Aggro Handling with Triggers
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") {
            isAggro = true;
            playerLeftZone = false;
            meshRenderer_eye.material.color = eyeColor_rage;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") {
            playerLeftZone = true;
        }
    }
    #endregion
}
