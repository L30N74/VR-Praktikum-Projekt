using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectHit : MonoBehaviour
{
    public Slider healthbar;

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Hit");
        healthbar.value -= 20;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
