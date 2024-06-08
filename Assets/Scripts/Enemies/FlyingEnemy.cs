using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FlyingEnemy : Enemy
{
    protected override void StartPathTarget()
    {
        target = GroundPath.points[GroundPath.points.Length - 1];
        pathIndex = GroundPath.points.Length - 1;
    }
}
