using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10.0f;
    public int damage = 1;

    private Enemy enemyTarget;

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused)
        {
            if (enemyTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = enemyTarget.transform.position - transform.position;
            float distanceToTarget = dir.magnitude;
            dir = dir.normalized;

            float updateDistance = speed * Time.deltaTime * GameManager.Instance.SpeedUp;


            if (updateDistance >= distanceToTarget)
            {
                updateDistance = distanceToTarget;
            }

            transform.Translate(dir * updateDistance, Space.World);
        }

    }

    public void SetEnemyTarget(Enemy target)
    {
        enemyTarget = target;
    }
}
