using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public Image playerHealthbar;

    public bool isRestricted = false;
    private float restrictionTimeout;
    private float timeSinceRestrictionStart;

    private void Start() 
    {
        currentHealth = maxHealth;
        //playerHealthbar.fillAmount = currentHealth;
    }       
    private void Update()
    {
        if (isRestricted) {
            timeSinceRestrictionStart += Time.deltaTime;
            if (timeSinceRestrictionStart >= restrictionTimeout) {
                isRestricted = false;
            }
        }
    }

    /// <summary>
    /// Add or subract health from the player
    /// </summary>
    /// <param name="value">Amount of Health to add or subtract to/from the player's current total</param>
    /// <param name="type">Type of Damage e.g. Gain(Healing) or Damage</param>
    public void AlterHealth(int value, DamageType type) {
        if (type == DamageType.Healing)
            this.currentHealth += Mathf.Clamp(currentHealth + value, 0, maxHealth);
        else
            this.currentHealth += Mathf.Clamp(currentHealth - value, 0, maxHealth);

        //playerHealthbar.fillAmount =(float)(currentHealth/maxHealth);
    }

    public void RestrictMovement(float time)
    {
        isRestricted = true;
        restrictionTimeout = time;
        timeSinceRestrictionStart = 0;
    }
}

public enum DamageType
{
    Damage,
    Healing    
}
