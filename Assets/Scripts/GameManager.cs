using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    
    
    public bool paused = false;
    public float speedUp = 1.0f;


    [Header("References to other Instances")]
    [SerializeField]
    private Spawner spawner;
    [SerializeField]
    private FieldCreator fieldCreator;
    [SerializeField]
    private GroundPath groundhPath;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartWave()
    {
        List<Vector3> pathPositions = fieldCreator.GetPathPointsPositions();
        if(pathPositions.Count > 0 )
        {
            groundhPath.CreatePathPoints(pathPositions);
            spawner.StartNextWave();
        }

    }

 
}
