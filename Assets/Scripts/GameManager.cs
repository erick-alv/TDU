using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    //Properties
    public bool Paused { get; private set; } = false;
    public float SpeedUp { get; private set; } = 1.0f;
    public int WaveNumber { get; private set; } = 0;

    public int Gold { get; private set; } = 8;

    public int Lives { get; private set; } = 3;


    private int destroyedEnemiesCurrentWave = 0;

    private const float upLimitSpeed = 16.0f;
    private const float downLimitSpeed = 0.25f;
    private const int factorEnemiesWave = 4;

    [Header("References to UI elements")]
    public GameObject slowDownButton;
    public GameObject speedUpButton;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;
    public GameObject fieldDimensionUI;
    public GameObject buildOrWaveUI;
    public GameObject SpeedUI;
    public GameObject gameOverCanvas;
    public GameObject pauseCanvas;

    [Header("References to other Instances")]
    [SerializeField]
    private Spawner spawner;
    [SerializeField]
    private FieldCreator fieldCreator;
    [SerializeField]
    private GroundPath groundPath;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        livesText.SetText($"Lives: {Lives}");
        goldText.SetText($"Gold: {Gold}");
        waveText.SetText($"Lives: {WaveNumber}");
    }

    public void RestartValues()
    {
        Paused = false;
        SpeedUp = 1.0f;
        WaveNumber = 0;
        Gold = 8;
        Lives = 3;
        destroyedEnemiesCurrentWave = 0;

        livesText.SetText($"Lives: {Lives}");
        goldText.SetText($"Gold: {Gold}");
        waveText.SetText($"Wave: {WaveNumber}");

        fieldCreator.EliminatePreviousField();
        spawner.isSpawning = false;
        fieldDimensionUI.SetActive(true);
        SpeedUI.SetActive(false);

        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");
        foreach(GameObject obj in turrets)
        {
            Destroy(obj);
        }
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in enemies)
        {
            Destroy(obj);
        }
        //Bullets Destroy themselves
    }
    
    public void StartWave()
    {
        if(fieldCreator.fieldInitialized)
        {
            List<Vector3> pathPositions = fieldCreator.GetPathPointsPositions();
            if (pathPositions.Count > 0)
            {
                groundPath.CreatePathPoints(pathPositions);

                WaveNumber += 1;
                destroyedEnemiesCurrentWave = 0;
                waveText.SetText($"Wave: {WaveNumber}");
                spawner.StartSpawning();
            }
        } else
        {
            WaveNumber += 1;
            destroyedEnemiesCurrentWave = 0;
            waveText.SetText($"Wave: {WaveNumber}");
            spawner.StartSpawning();
        }
    }

    public int AmountEnemiesCurrentWave()
    {
        return factorEnemiesWave * WaveNumber;
    }

    public void ReportDestroyedEnemy()
    {
        destroyedEnemiesCurrentWave++;
        if(destroyedEnemiesCurrentWave >= AmountEnemiesCurrentWave())
        {
            buildOrWaveUI.SetActive(true);
            SpeedUI.SetActive(false);
        }
    }

    public void SpeedUpFunction()
    {
        this.SpeedUp *=2;
        //Deactivate if already reached 
        if( this.SpeedUp >= upLimitSpeed) { 
            speedUpButton.SetActive(false);
        }

        if(!slowDownButton.activeSelf && this.SpeedUp > downLimitSpeed)
        {
            slowDownButton.SetActive(true);
        }
    }

    public void SlowDownFunction()
    {
        this.SpeedUp /= 2;
        //Deactivate if already reached 
        if (this.SpeedUp <= downLimitSpeed)
        {
            slowDownButton.SetActive(false);
        }

        if (!speedUpButton.activeSelf && this.SpeedUp < upLimitSpeed)
        {
            speedUpButton.SetActive(true);
        }
    }

    public void DecreaseLives(int livePoints)
    {
        Lives -= livePoints;
        livesText.SetText($"Lives: {Lives}");
        if(Lives <= 0)
        {
            Paused = true;
            gameOverCanvas.SetActive(true);
        }
    }

    public void IncreaseGold(int goldAmount)
    {
        Gold += goldAmount;
        goldText.SetText($"Gold: {Gold}");
    }

    public void DecreaseGold(int goldAmount)
    {
        Gold -= goldAmount;
        goldText.SetText($"Gold: {Gold}");
    }

    public void PauseGame()
    {
        Paused = true;
        pauseCanvas.SetActive(true);
    }

    public void UnpauseGame()
    {
        Paused = false;
        pauseCanvas.SetActive(false);
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
