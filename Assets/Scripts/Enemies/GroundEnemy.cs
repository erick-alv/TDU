using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GroundEnemy : Enemy
{
    protected override void StartPathTarget()
    {
        target = GroundPath.points[0];
        pathIndex = 0;
    }
}
