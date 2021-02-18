using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BatEnemy_AI : MonoBehaviour 
{
    public Slider enemyHealthbar;
    public Gradient gradient;
    public Image barColor;

    [SerializeField] private State state = State.Harvesting;
    private float roamingSpeed = 8f;
    private float chaseSpeed = 11f;

    private float currentHealth;
    private float maxHealth = 100f; 
    [SerializeField] private float essenceCount;
    private float essenceMaximum = 100f;
    private float essenceCollectionCooldown = 0.5f;
    private float essenceDeliveryCooldown = 1f;

    private float attackDamage = 3;
    [SerializeField] private float attackRange = 1;
    private float attackCooldown = 1.5f;
    private float secsToNextAttack = 0.0f;

    private float secsToNextDelivery = 0.0f;
    private float deliveryCooldown = 1.5f;
    private float secsToNextHarvest = 0.0f;
    private float harvestCooldown = 1.5f;

    NavMeshAgent agent;
    GameObject attackTarget;
    private Transform navmeshTarget;

    [SerializeField] private List<Transform> essenceCollectionSpots;
    [SerializeField] private Transform currentEssenceCollectionSpot;
    [SerializeField] private Transform essenceDeliverySpot;

    void Start() 
    {
        agent = GetComponentInParent<NavMeshAgent>();
        currentHealth = maxHealth;
        enemyHealthbar.value = currentHealth;
        barColor.color = gradient.Evaluate(1f);
        essenceCount = 0;
        attackTarget = null;
        agent.speed = roamingSpeed;

        // Add all essenceCollectionSpots
        System.Array.ForEach<GameObject>(
            GameObject.FindGameObjectsWithTag("EssenceCollector"), 
            spot => essenceCollectionSpots.Add(spot.transform)
        );

        // Find and add the essenceDeliverySpot
        essenceDeliverySpot = GameObject.FindGameObjectWithTag("EssenceDelivery").transform;

        // Make sure these spots really are set
        if(essenceDeliverySpot == null) 
            Debug.LogError("No GameObject with the tag \"EssenceDelivery\" in the scene. Make sure there is one.");

        if(essenceCollectionSpots.Count == 0) 
            Debug.LogError("No GameObjects with the tag \"EssenceCollector\" in the scene. Make sure there is at least one.");
    }

    void Update() 
    {
        switch(state) {
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
    }

    private void HandleHarvesting() {
        agent.speed = roamingSpeed;
        if(essenceCount < essenceMaximum) {
            if(currentEssenceCollectionSpot == null)
            {
                // Select essence Collection Spot
                int essenceIndex = Random.Range(0, essenceCollectionSpots.Count);
                currentEssenceCollectionSpot = essenceCollectionSpots[essenceIndex];
                navmeshTarget = currentEssenceCollectionSpot;
                agent.SetDestination(navmeshTarget.position);
            }

            //Collect essence
            float distanceToEssence = Vector3.Distance(this.transform.position, currentEssenceCollectionSpot.position);
            if(distanceToEssence <= attackRange){
                secsToNextHarvest -= Time.deltaTime;
                if(secsToNextHarvest <= 0) {
                    essenceCount += Random.Range(5,20);
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
        if(distanceToDeliverySpot <= attackRange && essenceCount > 0){
            secsToNextDelivery -= Time.deltaTime;
            if(secsToNextDelivery <= 0) {
                essenceCount -= Random.Range(10,20);
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
            //Attack
            //TODO: Implement cooldown
            secsToNextAttack -= Time.deltaTime;
            if(secsToNextAttack <= 0) {
                Debug.Log(this.gameObject.name + ": Attacked " + attackTarget.name);
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
            agent.SetDestination(navmeshTarget.position);
            agent.speed = chaseSpeed;
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag == "Player") {
            state = State.Idle;
        }
    }

    public void TakeDamage(float _damage, Spell.SpellType _spellType)
    {
        Debug.Log("Enemy took " + _damage.ToString() + " of damage");

        if(_spellType == Spell.SpellType.Ice)
        {
            Debug.Log("spell type is Ice");
            currentHealth -= _damage;
            enemyHealthbar.value = currentHealth;
            barColor.color = gradient.Evaluate(enemyHealthbar.normalizedValue);
        }

        if (_spellType == Spell.SpellType.Fire)
        {
            Debug.Log("spell type is Fire");
            currentHealth -= _damage;
            enemyHealthbar.value = currentHealth;
        }

    }

    private void IsDead() {
        if(currentHealth <= 0) {
            // destroy gameobject
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