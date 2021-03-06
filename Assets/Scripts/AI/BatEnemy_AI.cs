using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class BatEnemy_AI : MonoBehaviour, IEnemyAI
{
    public Image enemyHealthbar;
    public GameObject ice;

    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip attackSound;
    private AudioSource audioSource;

    [SerializeField] private State state = State.Harvesting;
    private float roamingSpeed = 8f;
    private float chaseSpeed = 11f;
    private new Rigidbody rigidbody;
    private new Collider collider;

    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth = 100; 
    [SerializeField] private int essenceCount;
    private float essenceMaximum = 100;
    private float essenceCollectionCooldown = 0.5f;
    private float essenceDeliveryCooldown = 1f;

    private int attackDamage = 3;
    private float harvestRange = 10f;
    [SerializeField] private float attackRange = 2.5f;
    private float attackCooldown = 1.5f;
    private float secsToNextAttack = 0.0f;

    private float secsToNextDelivery = 0.0f;
    private float deliveryCooldown = 1.5f;
    public float secsToNextHarvest = 0.0f;
    private float harvestCooldown = 1.5f;

    private float frozenTimeRemain = 0f;
    private float freezeTimer = 3f;
    private bool isFrozen = false;

    NavMeshAgent agent;
    GameObject attackTarget;
    private Transform navmeshTarget;

    private EssenceStockpile essenceStockpileScript;

    private PlayerStats playerStats;

    [SerializeField] private List<Transform> essenceCollectionSpots;
    [SerializeField] private Transform currentEssenceCollectionSpot;
    [SerializeField] private Transform essenceDeliverySpot;

    void Start() 
    {
        agent = GetComponentInParent<NavMeshAgent>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        currentHealth = maxHealth;
        enemyHealthbar = GetComponentsInChildren<Image>()[1];
        enemyHealthbar.fillAmount = currentHealth;
        essenceCount = 0;
        attackTarget = null;
        agent.speed = roamingSpeed;

        audioSource = GetComponent<AudioSource>();

        // Add all essenceCollectionSpots
        System.Array.ForEach<GameObject>(
            GameObject.FindGameObjectsWithTag("EssenceCollector"), 
            spot => essenceCollectionSpots.Add(spot.transform)
        );

        // Find and add the essenceDeliverySpot
        essenceDeliverySpot = GameObject.FindGameObjectWithTag("EssenceDelivery").transform;
        essenceStockpileScript = essenceDeliverySpot.GetComponent<EssenceStockpile>();
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

        // Make sure these spots really are set
        if(essenceDeliverySpot == null) 
            Debug.LogError("No GameObject with the tag \"EssenceDelivery\" in the scene. Make sure there is one.");
        
        if(essenceCollectionSpots.Count == 0) 
            Debug.LogError("No GameObjects with the tag \"EssenceCollector\" in the scene. Make sure there is at least one.");
    }

    void Update() 
    {
        secsToNextAttack -= Time.deltaTime;
        secsToNextHarvest -= Time.deltaTime;
        secsToNextDelivery -= Time.deltaTime;

        switch (state) {
            case State.Harvesting:
                HandleHarvesting();
            break;
            case State.Delivering:
                HandleDelivering();
            break;
            case State.Aggro:
                HandleAggro();
            break;
        }

        if (isFrozen) {
            frozenTimeRemain -= Time.deltaTime;

            if (frozenTimeRemain <= 0f) {
                agent.isStopped = false;
                isFrozen = false;
            }
        }
    }

    private void HandleHarvesting() {
        agent.speed = roamingSpeed;
        if (essenceCount < essenceMaximum) {
            if (currentEssenceCollectionSpot == null)
            {
                // Select essence Collection Spot
                int essenceIndex = Random.Range(0, essenceCollectionSpots.Count);
                currentEssenceCollectionSpot = essenceCollectionSpots[essenceIndex];
                navmeshTarget = currentEssenceCollectionSpot;
                agent.SetDestination(navmeshTarget.position);
            }

            //Collect essence
            float distanceToEssence = Vector3.Distance(this.transform.position, currentEssenceCollectionSpot.position);
            if (distanceToEssence <= harvestRange) {
                if (secsToNextHarvest <= 0f) {
                    essenceCount += Random.Range(5, 20);
                    secsToNextHarvest = harvestCooldown;
                }
            }
        }
        else {
            // Go deliver collected essence
            navmeshTarget = essenceDeliverySpot;
            agent.SetDestination(navmeshTarget.position);
            state = State.Delivering;
            currentEssenceCollectionSpot = null;
        }
    }

    private void HandleDelivering() {
        agent.speed = roamingSpeed;
        float distanceToDeliverySpot = Vector3.Distance(this.transform.position, essenceDeliverySpot.position);
        if(distanceToDeliverySpot <= harvestRange && essenceCount > 0){
            if(secsToNextDelivery <= 0) {
                int essenceDeliveryAmount = Random.Range(10, 50);
                essenceDeliveryAmount = essenceDeliveryAmount >= essenceCount ? essenceDeliveryAmount : essenceCount;
                essenceStockpileScript.DeliverEssence(essenceDeliveryAmount);
                essenceCount -= essenceDeliveryAmount;
                if (essenceCount <= 0) {
                    state = State.Harvesting;
                    return;
                }

                secsToNextDelivery = deliveryCooldown;
            }
        }            
        else {
            state = State.Harvesting;
        }
    }

    private void HandleAggro() {
        agent.speed = chaseSpeed;
        //if near enough -> attack
        float distanceToPlayer = Vector3.Distance(this.transform.position, attackTarget.transform.position);
        if(distanceToPlayer <= attackRange){
            // Rotate towards player
            Vector3 direction = attackTarget.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 3 * Time.deltaTime);

            // Attack
            if (secsToNextAttack <= 0) {
                playerStats.AlterHealth(attackDamage, DamageType.Damage);
                secsToNextAttack = attackCooldown;
            }
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player") {
            //Player entered aggro zone
            state = State.Aggro;

            // Set player as new navigation target and move towards him
            navmeshTarget = other.transform;
            attackTarget = other.gameObject;
            float randomX = Random.Range(other.transform.position.x - attackRange, other.transform.position.x + attackRange);
            float randomZ = Random.Range(other.transform.position.z - attackRange, other.transform.position.z + attackRange);
            Vector3 position = new Vector3(randomX, other.transform.position.y, randomZ);

            agent.SetDestination(position);
            agent.speed = chaseSpeed;
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag == "Player") {
            state = State.Idle;
        }
    }

    public void TakeDamage(int _damage, Spell.SpellType _spellType)
    {        
        if(_spellType == Spell.SpellType.Ice)
        {
            if (!isFrozen) {
                isFrozen = true;
                frozenTimeRemain = freezeTimer;
                agent.isStopped = true;
            }
        }

        currentHealth -= _damage;
        enemyHealthbar.fillAmount = (float)currentHealth / (float)maxHealth;

        IsDead();
    }

    private void IsDead() {
        if(currentHealth <= 0) {
            audioSource.PlayOneShot(deathSound);
            Destroy(this.gameObject);
            //TODO: Spawn smoke
        }
    }
}

public enum State 
{
    Idle,
    Harvesting,
    Delivering,
    Aggro
}