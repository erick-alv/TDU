using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    


    public float speed = 4;
    public float reachDistance = 0.3f;
    public int damageResistance = 1;

    protected PathPoint target;
    protected int pathIndex = 0;
    private int damageReceived = 0;
    private bool died = false;


    private void Start()
    {
        
        StartPathTarget();
    }

    protected virtual void StartPathTarget()
    {

    }

    private void Update()
    {
        if (!GameManager.Instance.paused && !died)
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
        if (distance <= reachDistance) {
            GetNextPoint();
        }
        
    }

    protected void MoveTowardsTarget()
    {
        Vector3 dir = target.transform.position - transform.position;
        dir[1] = 0.0f;
        dir = dir.normalized;
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    void GetNextPoint()
    {
        if(pathIndex >= GroundPath.points.Length - 1)
        {
            EndPointReached();
            return;
        }

        pathIndex++;
        target = GroundPath.points[pathIndex];
    }

    void EndPointReached()
    {
        Destroy(gameObject);
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
        if(damageReceived >= damageResistance) {
            Die();
        }
    }

    private void Die()
    {
        died = true;
        //TODO increment gold
        Destroy(gameObject);
    }


}
