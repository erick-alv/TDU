
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{

    public FlyingEnemy flyingEnemyPrefab;
    public GroundEnemy groundEnemyPrefab;
    public float countDownTime=2f;

    
    private float waveNumber = 0;
    private int spawnCallsCurrrentWave = 0;
    private bool isSpawning = false;
    private float currentActiveTime = 0.0f;

    public void Start()
    {
        //StartNextWave();//TODO delete this must be called by other script
    }

    public void StartNextWave()
    {
        waveNumber++;
        isSpawning = true;
        spawnCallsCurrrentWave = 0;
        currentActiveTime = countDownTime;

    }

    

    void Update()
    {

        if (!GameManager.Instance.paused && isSpawning)
        {

            if (currentActiveTime >= countDownTime)
            {
                SpawnEnenemies();
                spawnCallsCurrrentWave++;
                currentActiveTime = 0.0f;

            }

            currentActiveTime += Time.deltaTime * GameManager.Instance.speedUp;
            if (spawnCallsCurrrentWave >= 2*waveNumber)
            {
                isSpawning=false;
            }
        }
        
    }


    public void SpawnEnenemies()
    {
        Vector3 positionGround = gameObject.transform.position + gameObject.transform.forward * -4;
        positionGround[1] = groundEnemyPrefab.transform.position[1];
        SpawnEnemy(groundEnemyPrefab, positionGround);

        Vector3 positionAir = gameObject.transform.position;
        positionAir[1] = flyingEnemyPrefab.transform.position[1];
        SpawnEnemy(flyingEnemyPrefab, positionAir);

    }

    void SpawnEnemy(Enemy enemy, Vector3 position)
    {
        Instantiate(enemy, position, enemy.transform.rotation);
    }

}