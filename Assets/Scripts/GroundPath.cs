using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPath : MonoBehaviour
{

    public PathPoint pathPointPrefab;
    public static PathPoint[] points;

    /*void Awake()
    {
        points = new PathPoint[transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i).GetComponent<PathPoint>();
        }
    }*/

    public void CreatePathPoints(List<Vector3> positions)
    {
        if(points != null) {
            for (int i = 0; i < points.Length; i++)
            {
                Destroy(points[i].gameObject);
            }
        }
        points = new PathPoint[positions.Count];
        for (int i = 0; i < positions.Count; i++) {
            points[i] = Instantiate(pathPointPrefab, positions[i], pathPointPrefab.transform.rotation);
        }
    }


}
