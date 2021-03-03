using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentHealth {private set; get;}
    public int maxHealth {private set; get;}

    public bool isRestricted = false;
    private float restrictionTimeout;
    private float timeSinceRestrictionStart;

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
