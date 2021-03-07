using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public GameObject minionPrefab;
    private float timeSinceLastSpawn = 0f;
    public float spawnInterval = 4f;

    public int startingAmount = 5;
    public int maxMinionCount = 5;
    public int currentMinionCount;

    public bool spawnMinions = true;

    public Vector2 positionOffset = Vector2.zero;
    

    // Start is called before the first frame update
    void Start()
    {
        InitializeMinions();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentMinionCount < maxMinionCount && spawnMinions)
            SpawnMinion();
    }

    private void SpawnMinion()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if(timeSinceLastSpawn >= spawnInterval) {
            float randomX = Random.Range(transform.position.x - positionOffset.x, transform.position.x + positionOffset.x);
            float randomZ = Random.Range(transform.position.z - positionOffset.y, transform.position.z + positionOffset.y);

            Vector3 randomPosition = new Vector3(randomX, this.transform.position.y, randomZ);
            GameObject minion = Instantiate(minionPrefab, randomPosition, Quaternion.identity);
            minion.transform.SetParent(this.transform);
            minion.SendMessage("SetHome", GetComponent<MinionSpawner>());
            currentMinionCount++;
            timeSinceLastSpawn = 0f;
        }
    }

    private void InitializeMinions()
    {
        for (int i = 0; i < startingAmount; i++) {
            float randomX = Random.Range(transform.position.x - positionOffset.x, transform.position.x + positionOffset.x);
            float randomZ = Random.Range(transform.position.z - positionOffset.y, transform.position.z + positionOffset.y);

            Vector3 randomPosition = new Vector3(randomX, this.transform.position.y, randomZ);
            GameObject minion = Instantiate(minionPrefab, randomPosition, Quaternion.identity);
            minion.transform.SetParent(this.transform);
            currentMinionCount++;
        }
    }
}
