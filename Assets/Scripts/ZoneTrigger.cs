using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    private bool zoneEntered = false;
    private BossAI bossAIScript;

    private List<MinionSpawner> spawner = new List<MinionSpawner>();

    // Start is called before the first frame update
    void Start()
    {
        bossAIScript = GameObject.Find("Boss").GetComponent<BossAI>();

        System.Array.ForEach<GameObject>(
            GameObject.FindGameObjectsWithTag("Spawner"),
            spot => spawner.Add(spot.GetComponent<MinionSpawner>())
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") {
            if (!zoneEntered) {
                zoneEntered = true;
                bossAIScript.ZoneTriggerEnter();

                UpdateSpawner(false);
            }
            else {
                zoneEntered = false;
                bossAIScript.ZoneTriggerExit();

                UpdateSpawner(true);
            }
        }
    }

    private void UpdateSpawner(bool value)
    {
        foreach (MinionSpawner s in spawner)
            s.spawnMinions = value;
    }
}
