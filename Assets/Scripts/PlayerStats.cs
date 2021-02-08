using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentHealth {private set; get;}
    public int maxHealth {private set; get;}


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

public enum DamageType
{
    Damage,
    Healing    
}
