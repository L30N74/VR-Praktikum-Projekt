using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class GestureHandler : MonoBehaviour
{
    public int healEffectDuration;
    public int healEffectValue;
    public float healEffectInterval;

    private IEnumerator waterBubbleRoutine;
    private Transform player;
    private PlayerStats playerStatsScript;

    // For spells
    public GameObject fireSpellPrefab;
    public GameObject iceSpellPrefab;
    public GameObject healSpellPrefab;
    private GameObject healEffectParticles;
    public float throwForce;
    public Transform spawnPoint;

    private Camera mainCamera;

    public TextMeshProUGUI debugText;
	
    public SpellBook spellbookScript;

    public void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.Find("Player").transform;
        playerStatsScript = player.GetComponent<PlayerStats>();
    }

    public void ExecuteSpellByName(string spellName)
    {
        switch(spellName) 
        {
            case "fire":
                debugText.text = "Used fire spell";
                FireSpell();
                break;
            case "ice":
                debugText.text = "Used ice spell";
                IceSpell();
                break;
            case "heal":
                debugText.text = "Used healing spell";
                Heal();
                break;
            case "swipe left":
            case "swipe right":
                if(spellbookScript)
                    spellbookScript.displayNextPage();
                break;
        }
    }

    /// <summary>
    /// Creates a healing-effect for the player that, 
    /// for a set time, periodically heals him
    /// </summary>
    public void Heal() {
        // Stop the effect in case it's already active so it doesn't stack
        if (waterBubbleRoutine != null) {
            StopCoroutine(waterBubbleRoutine);
        }

        // Get player position
        Vector3 spawnPosition = new Vector3(mainCamera.transform.position.x, player.position.y, mainCamera.transform.position.z);
        if (healEffectParticles) Destroy(healEffectParticles);

        // Spawn particle system
        healEffectParticles = Instantiate(healSpellPrefab, spawnPosition, Quaternion.identity);
        healEffectParticles.transform.SetParent(player);
        Destroy(healEffectParticles, healEffectDuration);

        waterBubbleRoutine = HealBubbleRoutine();
        StartCoroutine(waterBubbleRoutine);
    }

    /// <summary>
    /// The Coroutine that controlls healing the player
    /// </summary>
    /// <returns>Time to wait for the next healing-effect to take place</returns>
    private IEnumerator HealBubbleRoutine()
    {
        float startTime = Time.time;

        while(true) {
            playerStatsScript.AlterHealth(healEffectValue, DamageType.Healing);

            // Break after the effect's duration has elapsed
            if (Time.time - startTime > healEffectDuration) {
                waterBubbleRoutine = null;
                break;
            }

            yield return new WaitForSeconds(healEffectInterval);
        }
    }

    public void FireSpell() {
        GameObject spellGo = Instantiate(fireSpellPrefab, spawnPoint.position, Quaternion.identity);
        spellGo.GetComponent<Rigidbody>().AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
    }

    public void IceSpell() {
        GameObject spellGo = Instantiate(iceSpellPrefab, spawnPoint.position, Quaternion.identity);
        spellGo.GetComponent<Rigidbody>().AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
    }
}
