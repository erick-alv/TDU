using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    [Header("Turret Fire Logic")]
    public Bullet bulletPrefab;
    public float range = 15f;
    public float fireTimeInterval = 1f;
    public string enemyTag = "Enemy";
    public Transform[] firePoints;

    private Transform target;
    private Enemy targetEnemy;
    private float activeFireTime = 0.0f;

    [Header("For visualizations")]
    public Renderer[] renderers;
    public GameObject rangeVisualization;

    private Material[] originalMaterials;

    [Header("For Construction Logic")]
    public int goldPrice;
    public Transform placementPoint;
    public Vector2Int[] otherFieldCoordinates;

    private void Awake()
    {
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }
        rangeVisualization.transform.localScale = new Vector3(2 * range, 0.5f, 2 * range);
    }

    private void Start()
    {
        activeFireTime = fireTimeInterval;//So that it starts shooting right away
        StartCoroutine(SeekEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused)
        {
            if (target == null)
            {
                return;
            }

            if (activeFireTime >= fireTimeInterval)
            {
                Shoot();
                activeFireTime = 0.0f;
            }
            activeFireTime += Time.deltaTime * GameManager.Instance.SpeedUp;
        }
    }

    IEnumerator SeekEnemies()
    {
        while (true)
        {
            if (!GameManager.Instance.Paused)
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
                float shortestDistance = Mathf.Infinity;
                GameObject nearestEnemy = null;
                foreach (GameObject enemy in enemies)
                {
                    if (enemy != null)
                    {
                        Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);
                        Vector2 enemyPos = new Vector2(enemy.transform.position.x, enemy.transform.position.z);
                        float distanceToEnemy = Vector2.Distance(thisPos, enemyPos);
                        if (distanceToEnemy <= range && distanceToEnemy < shortestDistance)
                        {
                            shortestDistance = distanceToEnemy;
                            nearestEnemy = enemy;
                        }
                    }
                }

                if (nearestEnemy != null)
                {
                    target = nearestEnemy.transform;
                    targetEnemy = nearestEnemy.GetComponent<Enemy>();
                }
                else
                {
                    target = null;
                    targetEnemy = null;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Shoot()
    {
        foreach (Transform firePoint in firePoints)
        {
            Bullet bullet = Instantiate(bulletPrefab, firePoint.position, bulletPrefab.transform.rotation);
            if (bullet != null)
            {
                bullet.SetEnemyTarget(targetEnemy);
            }
        }

    }

    public void Rotate90AntiHour()
    {
        this.transform.RotateAround(placementPoint.position, Vector3.up, -90);
        for (int i = 0; i < otherFieldCoordinates.Length; i++)
        {
            Vector2Int c = otherFieldCoordinates[i];
            //equivalent of applying 90 rotation in counterclock direction
            otherFieldCoordinates[i].x = -c.y;
            otherFieldCoordinates[i].y = c.x;
        }
    }

    public void SetMaterial(Material newMaterial)
    {
        foreach (var rend in renderers)
        {
            rend.material = newMaterial;
        }
    }

    public void SetBackOriginalMaterial()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }

    }

}
