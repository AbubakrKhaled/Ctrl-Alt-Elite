using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject normalEnemy;
    public int baseCount = 3; //number of enemies of first wave
    public Transform[] spawnPoints;
    private int waveNumber = 1;
    private List<GameObject> currentEnemies = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(5f); // initial delay

        while (waveNumber <= 3)
        {
            int enemyCount = baseCount * waveNumber;
            currentEnemies.Clear();

            for (int i = 0; i < enemyCount; i++)
            {
                Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemy = Instantiate(normalEnemy, spawn.position, Quaternion.identity);
                currentEnemies.Add(enemy);

                // 50% chance
                if (Random.value < 0.5f)
                {
                    enemy.AddComponent<EnemyPath>(); //Chase player
                }
                else
                {
                    enemy.AddComponent<EnemyShooter>(); //Stay still and shoot player
                }

            }

            yield return new WaitUntil(() => currentEnemies.TrueForAll(e => e == null)); //All enemies are dead
            yield return new WaitForSeconds(5f); //Delay before next wave
            waveNumber++;
        }
    }
}
