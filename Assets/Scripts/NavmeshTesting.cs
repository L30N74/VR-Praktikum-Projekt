using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshTesting : MonoBehaviour
{
    public Transform destinationTransform;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<NavMeshAgent>().SetDestination(destinationTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
