using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class BossAI : MonoBehaviour, IEnemyAI 
{
    #region Variables
    
    //** SHIELD AND HEALTH **\\
    [Header("Shield and Health")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    [SerializeField] private float shieldAmount;
    [SerializeField] private readonly float maxShieldAmount = 300f;

    private float shieldRefillCooldown = 5f;
    private float timeSinceLastRefill = 5f;

    private EssenceStockpile essenceStockpileScript;

    [SerializeField] private Image enemyShieldBar;
    [SerializeField] private Image enemyHealthBar;

    //** AGGREVATION **\\
    [SerializeField] private bool isAggro = false;
    private bool playerLeftZone = true;
    private readonly float aggroTimeout = 30f;
    private float secondsUntilDeAggro = 0f;

    private readonly bool secondPhaseEntered = false;

    [Header("Necessary References")]
    public Transform body;
    public Transform eye;
    public Quaternion startingRotation;
       
    [Header("Audio sources")]
    public AudioClip biteSound;
    public AudioClip screechSound;
    public AudioClip tendrilSound;
    public AudioClip laserSound;
    public AudioClip victorySound;

    private AudioSource audioSource;
    private MeshRenderer meshRenderer_eye;
    private MeshRenderer meshRenderer_body;

    [SerializeField] private Animator animator;

    private Color eyeColor_normal = new Color(204, 204, 204);
    private Color eyeColor_rage = new Color(207, 20, 20);

    private readonly float eyeFollowSpeed = 2f;

    //** AI **\\
    private Transform player;
    private PlayerStats playerStatsScript;


    //** ATTACKS **\\
    private readonly int specialAttackThreshold = 80;    // If a random number is above this threshold, a special attack will be executed (when possible by cooldown)
    private readonly float normalAttackCooldown = 3f;
    private float timeSinceLastNormalAttack;
    [SerializeField] private readonly float specialAttackCooldown = 20f;
    [SerializeField] private float timeSinceLastSpecialAttack;

    private readonly float attackRange = 20f;

    private readonly int screechDamage = 10;
    private readonly int biteDamage = 10;
    private readonly int laserDamage = 10;

    private readonly float meleeRange = 5f;

    private readonly float bindDuration = 3f;

    private Transform bindPrefab;
    private Transform screechPrefab; 
    private Transform bitePrefab;
    private Transform laserPrefab;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        meshRenderer_eye = eye.GetComponent<MeshRenderer>();
        meshRenderer_body = body.GetComponent<MeshRenderer>();
        startingRotation = transform.rotation;

        currentHealth = maxHealth;
        shieldAmount = maxShieldAmount;
        audioSource = transform.GetChild(0).GetComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerStatsScript = player.GetComponent<PlayerStats>();

        if(!enemyHealthBar)
            enemyHealthBar = GetComponentsInChildren<Image>()[1];
        if(!enemyShieldBar)
            enemyShieldBar = GetComponentsInChildren<Image>()[3];

        GameObject platform = GameObject.FindWithTag("EssenceDelivery");
        essenceStockpileScript = platform.GetComponent<EssenceStockpile>();

        bindPrefab = ((GameObject)Resources.Load("Tendrils")).transform;
        screechPrefab = ((GameObject)Resources.Load("Cry")).transform;
        bitePrefab = ((GameObject)Resources.Load("Bite")).transform;
        laserPrefab = ((GameObject)Resources.Load("Laser")).transform;

        if(!animator)
            animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            TakeDamage(50, Spell.SpellType.Fire);
        }

        // Aggro timeout
        if(playerLeftZone && isAggro) {
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

        // Refill Shield
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
        timeSinceLastRefill += Time.deltaTime;
        if (shieldAmount < maxShieldAmount / 3f) {
            if (timeSinceLastRefill >= shieldRefillCooldown) {
                shieldAmount += essenceStockpileScript.RetrieveEssence(150);
                enemyShieldBar.fillAmount = (float)(shieldAmount / maxShieldAmount);
                timeSinceLastRefill = 0f;
            }
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = player.position - body.position;
        Quaternion rotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, eyeFollowSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Determines which attack the boss should execute based on a sequence of random numbers.
    /// </summary>
    private void HandleAttack()
    {
        int attackTypeDecisionValue = Random.Range(1, 101);

        timeSinceLastSpecialAttack += Time.deltaTime;
        timeSinceLastNormalAttack += Time.deltaTime;

        if (attackTypeDecisionValue <= specialAttackThreshold || 
            (attackTypeDecisionValue <= specialAttackThreshold && timeSinceLastSpecialAttack < specialAttackCooldown)) {
            if (timeSinceLastNormalAttack < normalAttackCooldown)
                return;

            int attackVariantValue = Random.Range(1, 101);
            // Determine which normal attack should be executed
            if (attackVariantValue <= 50) {
                float dist = Vector3.Distance(body.position, player.position);

                // Bite the player
                if (dist < meleeRange)
                    ExecuteBite();
                else
                    ExecuteScreech();
            }
            else {
                // Screech at the player
                ExecuteScreech();
            }
            timeSinceLastNormalAttack = 0f;
        }
        else {
            if (timeSinceLastSpecialAttack < specialAttackCooldown)
                return;

            int attackVariantValue = Random.Range(1, 101);
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
        Debug.Log("Sreech attack");

        Quaternion spawnRotation = new Quaternion(0, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        // Play audio source
        audioSource.PlayOneShot(screechSound);

        // Play special effect and animation
        Instantiate(screechPrefab, spawnPosition, spawnRotation);

        // Deal damage
        playerStatsScript.AlterHealth(screechDamage, DamageType.Damage);
    }

    private void ExecuteBite()
    {
        Debug.Log("Bite attack");
        Quaternion spawnRotation = new Quaternion(0, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        // Play audio source
        audioSource.PlayOneShot(biteSound);

        // Play animation
        Destroy(Instantiate(bitePrefab, spawnPosition, spawnRotation), 1);

        // Deal damage
        playerStatsScript.AlterHealth(biteDamage, DamageType.Damage);
    }

    private void ExecuteLaser()
    {
        Debug.Log("Laser attack");
        Camera cam = Camera.main;
        Vector3 spawnPosition = eye.transform.position; 

        // Play audio source
        audioSource.PlayOneShot(laserSound);

        // Play animation
        Transform laserObject = Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
        Destroy(laserObject, 3f);

        Vector3 forceVector = eye.position - new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        laserObject.GetComponent<Rigidbody>().AddForce(forceVector, ForceMode.Impulse);

        // Deal damage
        playerStatsScript.AlterHealth(laserDamage, DamageType.Damage);
    }

    private void ExecuteBind()
    {
        Debug.Log("Bind attack");
        // Play audio
        audioSource.PlayOneShot(tendrilSound);
        
        // Spawn tendrils
        Quaternion spawnRotation = new Quaternion(180, 0, 0, 1);
        Camera cam = Camera.main;
        Vector3 spawnPosition = new Vector3(cam.transform.position.x, player.position.y, cam.transform.position.z);
        Destroy((Instantiate(bindPrefab, spawnPosition, spawnRotation)).gameObject, bindDuration);

        // Bind the player
        playerStatsScript.RestrictMovement(bindDuration);
    }

    public void TakeDamage(int _damage, Spell.SpellType _spellType)
    {
        //if (playerLeftZone) return; // No cheese in my house!

        if (shieldAmount > 0) {
            shieldAmount -= _damage;
            enemyShieldBar.fillAmount = (float)(shieldAmount / maxShieldAmount);
        }
        else {
            currentHealth -= _damage;
            enemyHealthBar.fillAmount = (float)currentHealth / (float)maxHealth;
        }

        IsDead();
    }

    private void IsDead()
    {
        if (currentHealth <= 0) {
            animator.SetBool("isDead", true);

            //TODO: Spawn smoke
            audioSource.PlayOneShot(victorySound);
            Destroy(this.gameObject, 2);
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
