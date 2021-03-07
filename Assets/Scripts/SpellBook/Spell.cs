using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(SphereCollider))]

public class Spell : MonoBehaviour {

    private Rigidbody myBody;
    private MeshRenderer myRend;
    private Collider myCollider;
    private GameObject firstChild;//trail renderer

    private ParticleSystem spellParticle;
    private ParticleSystem[] spellParticleChildren;

    private List<IEnemyAI> enemyHealth = new List<IEnemyAI>();

    public int damage;
    public float radius;
    public float lifeTimeTimer;

    public enum SpellType { Ice, Fire};
    public SpellType spellType;

    private List<GameObject> spellParticles = new List<GameObject>();

    private void Start()
    {
        myBody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        spellParticle = GetComponent<ParticleSystem>();
        spellParticleChildren = GetComponentsInChildren<ParticleSystem>();

        spellParticles.Add((GameObject)Resources.Load("IceExplosion"));
        spellParticles.Add((GameObject)Resources.Load("FireExplosion"));
    }

    private void Update()
    {
        lifeTimeTimer -= Time.deltaTime;

        if (lifeTimeTimer <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" || other.tag == "Ground")
        {   
            HideStuff();
            SpawnParticles(other);
            DoDamage();
        }
    }

    private void HideStuff()
    {
        // myBody.constraints = RigidbodyConstraints.FreezeAll;
        myCollider.enabled = false;
    }

    private void SpawnParticles(Collider collider)
    {
        switch(spellType) 
        {
            case SpellType.Fire:
                Instantiate(spellParticles[1], collider.transform.position, Quaternion.identity);
                break;
            case SpellType.Ice:
                Instantiate(spellParticles[0], collider.transform.position, Quaternion.identity);
                break;
        }
    }

    private void DoDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, radius);

        for (int i = 0; i < colls.Length; i++)
        {
            Collider coll = colls[i];

            if (colls[i].tag == "Enemy")
            {
                IEnemyAI currentHealth = coll.GetComponentInParent<IEnemyAI>();
                currentHealth.TakeDamage(damage, spellType);
            }
        }
    }

    private void Die()
    {
        Debug.Log("--Spell Destroyed--");
        Destroy(gameObject);
    }
}