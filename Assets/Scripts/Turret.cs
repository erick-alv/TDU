using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{

    public Bullet bulletPrefab;
    public float range = 15f;//TODO adjust ranges
    public float fireTimeInterval = 1f;
    public string enemyTag = "Enemy";

    private Transform target;
    private Enemy targetEnemy;//TODO used??
    private float activeFireTime = 0.0f;
 

    // Update is called once per frame
    void Update()
    {
        if( !GameManager.Instance.paused) {
            SeekEnemies();

            if (target == null)
            {
                return;
            }

            
            if (activeFireTime >= fireTimeInterval)
            {
                Shoot();
                activeFireTime = 0.0f;
            }

            activeFireTime += Time.deltaTime * GameManager.Instance.speedUp;

        }
    }

    void SeekEnemies()
    {
        //TODO rewrite this logic make a list when instantiating; iterate over it; see which one is in range
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
            targetEnemy = nearestEnemy.GetComponent<Enemy>();
        }
        else
        {
            target = null;
        }

    }

    void Shoot()
    {
        Bullet bullet = Instantiate(bulletPrefab, gameObject.transform.position, bulletPrefab.transform.rotation);
        if (bullet != null)
        {
            bullet.SetEnemyTarget(targetEnemy);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
