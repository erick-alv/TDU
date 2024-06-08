
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public FlyingEnemy flyingEnemyPrefab;
    public GroundEnemy groundEnemyPrefab;
    public float countDownTime = 2f;

    private int spawnedEnemiesCurrentWave = 0;
    public bool isSpawning = false;
    private float currentActiveTime = 0.0f;

    public void StartSpawning()
    {
        isSpawning = true;
        spawnedEnemiesCurrentWave = 0;
        currentActiveTime = countDownTime;
    }

    void Update()
    {

        if (!GameManager.Instance.Paused && isSpawning)
        {

            if (currentActiveTime >= countDownTime)
            {
                SpawnGroundEnenemy();
                spawnedEnemiesCurrentWave++;
                if (spawnedEnemiesCurrentWave < GameManager.Instance.AmountEnemiesCurrentWave())
                {
                    SpawnFlyingEnemy();
                    spawnedEnemiesCurrentWave++;
                }
                currentActiveTime = 0.0f;
            }

            currentActiveTime += Time.deltaTime * GameManager.Instance.SpeedUp;
            if (spawnedEnemiesCurrentWave >= GameManager.Instance.AmountEnemiesCurrentWave())
            {
                isSpawning = false;
            }
        }

    }

    public void SpawnGroundEnenemy()
    {
        Vector3 positionGround = gameObject.transform.position + gameObject.transform.forward * -4;
        positionGround[1] = groundEnemyPrefab.transform.position[1];
        SpawnEnemy(groundEnemyPrefab, positionGround);
    }

    public void SpawnFlyingEnemy()
    {
        Vector3 positionAir = gameObject.transform.position;
        positionAir[1] = flyingEnemyPrefab.transform.position[1];
        SpawnEnemy(flyingEnemyPrefab, positionAir);
    }

    void SpawnEnemy(Enemy enemy, Vector3 position)
    {
        Debug.Log("Enemy Spawned");
        Instantiate(enemy, position, enemy.transform.rotation);
    }

}