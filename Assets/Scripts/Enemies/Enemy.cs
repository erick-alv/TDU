using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 4;
    public float reachDistance = 0.3f;
    public int damageResistance = 1;

    protected PathPoint target;
    protected int pathIndex = 0;
    private int damageReceived = 0;
    //To avoid calling methods of Finish or die multiple times due to delay of Destroy
    private bool died = false;
    private bool reachedEnd = false;

    private void Start()
    {
        StartPathTarget();
    }

    protected virtual void StartPathTarget()
    {

    }

    private void Update()
    {
        if (!GameManager.Instance.Paused && !died)
        {
            Move();
        }
    }

    protected virtual void Move()
    {
        MoveTowardsTarget();
        Vector3 distVec = target.transform.position - transform.position;
        distVec[1] = 0;
        float distance = distVec.magnitude;
        if (distance <= reachDistance)
        {
            GetNextPoint();
        }
    }

    protected void MoveTowardsTarget()
    {
        Vector3 dir = target.transform.position - transform.position;
        dir[1] = 0.0f;
        dir = dir.normalized;
        transform.Translate(dir * speed * GameManager.Instance.SpeedUp * Time.deltaTime, Space.World);
    }

    void GetNextPoint()
    {
        if (pathIndex >= GroundPath.points.Length - 1)
        {
            EndPointReached();
            return;
        }

        pathIndex++;
        target = GroundPath.points[pathIndex];
    }

    void EndPointReached()
    {
        if (!reachedEnd)
        {
            reachedEnd = true;
            GameManager.Instance.DecreaseLives(1);
            GameManager.Instance.ReportDestroyedEnemy();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            Bullet b = other.gameObject.GetComponent<Bullet>();
            HitByBullet(b);
            Destroy(other.gameObject);
        }
    }

    private void HitByBullet(Bullet bullet)
    {
        damageReceived += bullet.damage;
        if (damageReceived >= damageResistance)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!died)
        {
            died = true;
            GameManager.Instance.IncreaseGold(1);
            GameManager.Instance.ReportDestroyedEnemy();
            Destroy(gameObject);
        }
    }

}
