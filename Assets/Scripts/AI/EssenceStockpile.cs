using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceStockpile : MonoBehaviour
{
    public int essencePile;

    public void DeliverEssence(int amount)
    {
        essencePile += amount;
    }

    public int RetrieveEssence(int amount)
    {
        int retrieveAmount = 0;

        if(essencePile >= amount) {
            retrieveAmount = amount;
            essencePile -= amount;
        }
        else {
            retrieveAmount = essencePile;
            essencePile = 0;
        }

        return retrieveAmount;
        
    }
}
