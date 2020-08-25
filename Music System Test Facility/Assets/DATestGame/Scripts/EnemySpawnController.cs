using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    public float frequencyRateOfIncrease = 2;         // How gradually the spawn rate will increase
     
    public GameObject player;                         // Reference to our player

    public GameObject enemyPrefab;                    // The prefab enemies are based on
    public Transform[] spawnPoints;                   // The points from which enemies will spawn

    public float spawnFrequency = 3;                  // How often a enemy will spawn

    public float increaseSpawnAmountFrequency = 10;   // The number of spawns after which the spawn amount will increase
    public float maxSpawnNumber = 3;                  // The maximum nuber of enemies that will spawn in a single wave

    private int currentSpawnNumber = 1;               // How many enemies spawn at each spawn interval
    private int currentSpawnCount = 0;                // keeps track of the number of spawns since the last SpawnNumber increase

    private float lastSpawnIndex = -1;                // Reference to the last place an enemy was spawned
    private float spawnTimer = 0;                     // Used to keep track of how much time has passed between spawns
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnTimer > spawnFrequency)
        {
            // We want to repeat the spawning process currentSpawnNumber times
            for (int i = 0; i < currentSpawnNumber; i++)
            {
                // Let's choose a random spawn point
                int randomIndex = Random.Range(0, spawnPoints.Length - 1);

                // and ensure it isn't the same as the one we spawned from previously
                while (randomIndex == lastSpawnIndex)
                {
                    // this just rerandomises if we chose the same index as before
                    randomIndex = Random.Range(0, spawnPoints.Length - 1);
                }

                // We update our lastSpawnIndex value for the next time this runs
                lastSpawnIndex = randomIndex;


                // We instantiate a new enemy at the selected spawn point, but keep a reference to it...
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity) as GameObject;
                EnemyController newEnemyController = newEnemy.GetComponent<EnemyController>();

                // ... so that we can assign its player gameObject 
                newEnemyController.player = player;
            }

            // We want to check if we even need to be counting spawn waves anymore 
            // (as this will no longer be necessary if we've reached our max spawn number already)
            if (currentSpawnNumber < maxSpawnNumber)
            {
                // we wnat to increase our spawnCount, and check if the spawn number should increase
                currentSpawnCount++;

                if (currentSpawnCount >= increaseSpawnAmountFrequency)
                {
                    // If we've reached the point where the spawn amount should increase, we want to reset our counter
                    currentSpawnCount = 0;
                    // and increase our spawnNumber variable by 1
                    currentSpawnNumber++;
                }
            }
            


            // Finally, we need to reset our timing variable
            spawnTimer = 0;

            // and adjust our frequency value
            spawnFrequency -= Time.deltaTime * frequencyRateOfIncrease;
        }
        else
        {
            spawnTimer += Time.deltaTime;
        }
    }
}
