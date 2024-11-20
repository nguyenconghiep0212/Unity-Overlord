using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TimeManagement : MonoBehaviour
{

    public static TimeManagement Instance { get; set; }

    private void Awake()
    {
        if (Instance != null & Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public float incrementInterval = 5.0f; // Time in seconds to wait before incrementing
    public int turn = 0;
    public TurnProgressBar turnProgressBar;
    private float currentProgress;
 
    public TextMeshProUGUI turnUI;
    public GameObject pauseButton;
    public GameObject pauseMenu;
    public TextMeshProUGUI operationName;
    public TextMeshProUGUI difficulty;
    public GameObject playButton;
    public GameObject quitButton;

    public bool isPause = false;


    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        operationName.text = Setting.Instance.selectedMap.text;
        difficulty.text = Setting.Instance.difficulty.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartFunding()
    {
        StartCoroutine(ProgressBarIncrement());
        //StartCoroutine(IncrementTurn());
    }

    private IEnumerator ProgressBarIncrement()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            currentProgress += 0.25f;
            turnProgressBar.SetTurnProgresssBarPercentage(currentProgress / incrementInterval);
            if (currentProgress >= incrementInterval)
            {
                NextTurn();
                currentProgress = 0;
            }
        } 
    }

    //private IEnumerator IncrementTurn()
    //{
    //    while (true) // Infinite loop to keep incrementing
    //    {
    //        yield return new WaitForSeconds(incrementInterval); // Wait for the specified interval
    //        NextTurn();
    //    }
    //}

    public void NextTurn()
    {
        turn++;
        turnUI.text = turn.ToString();

        #region ---- || MAP TILE || ----
        foreach (MapTile ownTile in OperationManager.Instance.ownTiles)
        {
            if (ownTile.scanner)
            {
                foreach (MapTile tile in ownTile.neighborTiles)
                {
                    if (!tile.isScanned)
                    {
                        tile.Scanning();
                    }
                    if (TechTreeManager.Instance.purchasedUpgrades.Contains("SCAN_TOWR_2"))
                    {
                        foreach (MapTile tile2 in tile.neighborTiles)
                        {
                            if (!tile2.isScanned)
                            {
                                tile2.Scanning();
                            }
                        }
                    }
                }
            }
            if (ownTile.garrison || ownTile.occupiedAllyUnit || ownTile.HQ)
            {
                ownTile.RandomizeUprising();
                if (TechTreeManager.Instance.totalOperationSupport > 0)
                {
                    ownTile.supportPercentage += TechTreeManager.Instance.totalOperationSupport;
                }
                else
                {
                    ownTile.opposePercentage += Math.Abs(TechTreeManager.Instance.totalOperationSupport);
                }
            }
        }
        foreach (MapTile tile in EnemyOperationManager.Instance.ownTiles)
        {
            if (tile.opposePercentage < 5)
            {
                tile.opposePercentage += 0.1f;
            }
        }

        #endregion

        #region  ---- || OPERATION || ----
        OperationManager.Instance.IncreaseFundByTurn();
        OperationManager.Instance.headQuarter.AttackEnemy();
        #endregion

        #region ---- || PLAYER UNIT || ----
        foreach (Unit unit in OperationManager.Instance.totalDeployUnit)
        {
            unit.AttackEnemy();
        }
        foreach (Infrastruture infrastruture in OperationManager.Instance.Garrisions)
        {
            infrastruture.AttackEnemy();
        }
        #endregion

        #region ---- || ENEMY OPERATION || ----
        if (EnemyOperationManager.Instance.ai_agent.stateMachine.currentState != AI_StateID.Dormant && EnemyOperationManager.Instance.ai_agent.stateMachine.currentState != AI_StateID.Death)
        {
            EnemyOperationManager.Instance.IncreaseFundPerTurn();
        }
        if (EnemyOperationManager.Instance.headQuarter)
        {
            EnemyOperationManager.Instance.headQuarter.AttackPlayer();
        }
        #endregion

        #region ---- || ENEMY UNIT || ----
        foreach (Unit_Enemy unit in EnemyOperationManager.Instance.totalDeployUnit)
        {
            unit.AttackPlayer();
        }
        foreach (Infrastruture_Enemy infrastruture in EnemyOperationManager.Instance.Garrisions)
        {
            infrastruture.AttackPlayer();
        }
        #endregion
    }

    public void OpenTutorial()
    {
        Setting.Instance.tutorialUI.SetActive(true);

        PauseGame();
    }

    public void OpenPauseMenu()
    {
        pauseButton.SetActive(false);
        pauseMenu.SetActive(true);

        PauseGame();
    }
    public void ClosePauseMenu()
    {
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);

        ResumeGame();
    }

    public void PauseGame()
    {
        isPause = true;

        Time.timeScale = 0;

        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            audio.Pause();
        }


        Destroy(OperationManager.Instance.hqSelectorUI);
    }

    public void ResumeGame()
    {
        isPause = false;

        Time.timeScale = 1;

        AudioSource[] audios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            if (!audio.isPlaying)
                audio.Play();
        }
    }
}
