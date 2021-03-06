using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour, IEnemyAI
{
    [Header("Necessary References")]
    public Transform body;
    public Transform eye;
    public Transform startingPosition;
    public Quaternion startingRotation;
       
    [Header("Audio sources")]
    public AudioClip normalAttack;
    public AudioClip specialAttack;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer_eye;
    private MeshRenderer meshRenderer_body;

    private Color eyeColor_normal = new Color(204, 204, 204);
    private Color eyeColor_rage = new Color(207, 20, 20);

    private readonly float eyeFollowSpeed = 2f;

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

    private readonly float attackRange = 20f;

    private readonly int screechDamage = 10;
    private readonly int biteDamage = 10;
    private readonly int laserDamage = 10;

    private readonly float bindDuration = 3f;

    private Transform bindPrefab;
    public Vector3 bindOffset;
    private Transform screechPrefab; 
    private Transform bitePrefab;
    private Transform laserPrefab;



    //** SHIELD AND HEALTH **\\
    public int currentHealth;
    public int maxHealth;
    private float shieldAmount;
    private readonly float maxShieldAmount = 300f;

    private float shieldRefillCooldown = 5f;
    private float timeSinceLastRefill = 5f;

    private EssenceStockpile essenceStockpileScript;

    private Image enemyShieldBar;
    private Image enemyHealthBar;

    //** AGGREVATION **\\
    private bool isAggro = false;
    private bool playerLeftZone = false;
    private readonly float aggroTimeout = 30f;
    private float secondsUntilDeAggro = 0f;

    private readonly bool secondPhaseEntered = false;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        meshRenderer_eye = eye.GetComponent<MeshRenderer>();
        meshRenderer_body = body.GetComponent<MeshRenderer>();
        agent = GetComponent<NavMeshAgent>();
        startingRotation = transform.rotation;

        currentHealth = maxHealth;
        shieldAmount = maxShieldAmount;
        audioSource = transform.GetChild(0).GetComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerStatsScript = player.GetComponent<PlayerStats>();

        enemyHealthBar = GetComponentsInChildren<Image>()[1];
        enemyShieldBar = GetComponentsInChildren<Image>()[3];

        essenceStockpileScript = GameObject.FindWithTag("EssenceDelivery").GetComponent<EssenceStockpile>();

        bindPrefab = ((GameObject)Resources.Load("Tendrils")).transform;
        screechPrefab = ((GameObject)Resources.Load("Cry")).transform;
        bitePrefab = ((GameObject)Resources.Load("Bite")).transform;
        laserPrefab = ((GameObject)Resources.Load("Laser")).transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Aggro timeout
        if(playerLeftZone) {
            secondsUntilDeAggro += Time.deltaTime;
            if (secondsUntilDeAggro >= aggroTimeout) {
                // Player left area for an extended period of time. 
                // De-Aggro and return to start
                secondsUntilDeAggro = 0f;
                isAggro = false;
                meshRenderer_eye.material.color = eyeColor_normal;
                transform.rotation = Quaternion.Lerp(transform.rotation, startingRotation, 3f);
            }
        }

        if (!isAggro)
            return;

        RotateTowardsTarget();

        HandleAttack();


        timeSinceLastRefill += Time.deltaTime;
        // Refill Shield
        if (shieldAmount < maxShieldAmount/3f)
            RefillShield();

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

    private void RefillShield()
    {
        if (timeSinceLastRefill >= shieldRefillCooldown) {
            shieldAmount += essenceStockpileScript.RetrieveEssence(150);
            enemyShieldBar.fillAmount = (float)(shieldAmount / maxShieldAmount);
            timeSinceLastRefill = 0f;
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = player.position - body.position;
        //Quaternion rotation = direction; //Quaternion.LookRotation(direction, Vector3.up);

        Quaternion rotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, eyeFollowSpeed * Time.deltaTime);
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

        ExecuteBind();
        isAggro = false;
        /*if (attackTypeDecisionValue <= specialAttackThreshold || 
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
        }*/
    }

    //** ATTACKS **\\
    private void ExecuteScreech()
    {   
        Quaternion spawnRotation = new Quaternion(0, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        // Play audio source

        // Play special effect and animation
        Instantiate(screechPrefab, spawnPosition, spawnRotation);

        // Deal damage
        playerStatsScript.AlterHealth(screechDamage, DamageType.Damage);
    }

    private void ExecuteBite()
    {   
        Quaternion spawnRotation = new Quaternion(0, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        // Play audio source

        // Play animation
        Instantiate(bitePrefab, spawnPosition, spawnRotation);

        // Deal damage
        playerStatsScript.AlterHealth(biteDamage, DamageType.Damage);
    }

    private void ExecuteLaser()
    {
        Quaternion spawnRotation = new Quaternion(0, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        // Play audio source

        // Play animation
        Instantiate(laserPrefab, spawnPosition, spawnRotation);
        // Deal damage
        playerStatsScript.AlterHealth(laserDamage, DamageType.Damage);
    }

    private void ExecuteBind()
    {
        Quaternion spawnRotation = new Quaternion(180, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        Destroy((Instantiate(bindPrefab, spawnPosition, spawnRotation)).gameObject, bindDuration);
        playerStatsScript.RestrictMovement(bindDuration);
    }

    public void TakeDamage(int _damage, Spell.SpellType _spellType)
    {
        if (shieldAmount > 0) {
            shieldAmount -= _damage;
            enemyShieldBar.fillAmount = (float)(shieldAmount / maxShieldAmount);
        }
        else {
            currentHealth -= _damage;
            enemyHealthBar.fillAmount = (float)(currentHealth / maxHealth);
            Debug.Log("Fill: " + enemyHealthBar.fillAmount);
        }

        IsDead();
    }

    private void IsDead()
    {
        if (currentHealth <= 0) {
            Destroy(this.gameObject);
            //TODO: Spawn smoke
        }
    }

    #region Aggro Handling with Triggers
    
    public void ZoneTriggerEnter()
    {
        isAggro = true;
        playerLeftZone = false;
        meshRenderer_eye.material.color = eyeColor_rage;
    }
    public void ZoneTriggerExit()
    {
        playerLeftZone = true;
    }
    #endregion
}
