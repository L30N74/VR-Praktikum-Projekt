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

    private bool dealDamage = false;
    private List<BatEnemy_AI> enemyHealth = new List<BatEnemy_AI>();

    public float damage;
    public float radius;
    public float dealDamageTimer;
    private float timer;
    public float lifeTimeTimer;
    public GameObject collisionParticles;

    public enum SpellType { Ice, Fire};
    public SpellType spellType;

    private void Start()
    {
        myBody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        lifeTimeTimer -= Time.deltaTime;

        if (lifeTimeTimer <= 0)
        {
            Die();
        }

        if (dealDamage)
        {
            timer += Time.deltaTime;
            if (timer >= dealDamageTimer)
            {
               timer = 0;

                for (int i = 0; i < enemyHealth.Count; i++)
                {
                    enemyHealth[i].TakeDamage(damage, spellType);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Ground")
        {
            HideStuff();
            DoDamage();
        }
    }

    private void OnParticleCollision(Collision other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Ground")
        {
            HideStuff();
            DoDamage();
        }
    }

    private void HideStuff()
    {
        myBody.constraints = RigidbodyConstraints.FreezeAll;
        myCollider.enabled = false;
    }

    private void DoDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, radius);

        for (int i = 0; i < colls.Length; i++)
        {
            Collider current = colls[i];

            if (colls[i].tag == "Enemy")
            {
                GameObject go = Instantiate(collisionParticles, current.transform.position, Quaternion.identity);
                go.transform.parent = this.transform;

                BatEnemy_AI currentHealth = current.GetComponent<BatEnemy_AI>();
                currentHealth.TakeDamage(damage, spellType);
            }
        }
        dealDamage = true;
    }

    private void Die()
    {
        Debug.Log("--Spell Destroyed--");
        Destroy(gameObject);
    }
}