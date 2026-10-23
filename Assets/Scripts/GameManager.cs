using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;



[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public LevelGenerator map;
    public TMP_Text scoreText;

    public PacStudentController player;
    public CherryController cherry;
    public AudioSource eventAudio;
    public AudioClip cherrySound;

    public TMP_Text timeText;
    public GameObject gameOverlay;
    public TMP_Text messageText;


    public AudioSource bgmAudio;
    public AudioClip introMusic;
    public AudioClip normalMusic;


    public GameObject life1;
    public GameObject life2;
    public GameObject life3;
    public Button exitButton;
    public AudioClip deathSound;

    public TMP_Text scaredTimeText;
    public AudioClip scaredMusic;

    public GhostController[] ghosts = new GhostController[4];
    public AudioClip eatGhostSound;
    public AudioClip deadMusic;

    private Vector3 oldPlayerPoint;
    private Vector3[] oldGhostPoints = new Vector3[4];
    private bool hasGhostPoints = false;


    private float scaredTime = 0f;

    private int life = 3;
    private float deathTime = 0f;
    private float endTime = 0f;


    

    public enum GameState
    {
        Ready,
        Playing,
        Dying,
        GameOver
    }


    private GameState gameState = GameState.Ready;
    private float readyTime = 0f;
    private float gameTime = 0f;

    private GameObject lastCherry;
    private Vector3 lastPlayerPoint;
    private Vector3 lastCherryPoint;

    private int score = 0;
    private int foodLeft = 0;
    private GameObject[,] foodMap = new GameObject[29, 28];




    void Awake()
    {
        Transform[] mapObjects = map.sceneMap.GetComponentsInChildren<Transform>();

        foreach (Transform item in mapObjects)
        {
            if (item.name.EndsWith("_Pellet") ||
                item.name.EndsWith("_PowerPellet"))
            {
                // 镜像也按世界位置找
                int column = Mathf.RoundToInt(item.position.x);
                int row = Mathf.RoundToInt(-item.position.y);

                foodMap[row, column] = item.gameObject;
                foodLeft++;
            }
        }

        scoreText.text = "Score: " + score.ToString("D6");
    }




    void Start()
    {
        gameOverlay.SetActive(true);
        messageText.text = "3";
        ShowTime();
        ClearScared();

        ShowLife();
        exitButton.interactable = true;

        bgmAudio.Stop();
        bgmAudio.clip = introMusic;
        bgmAudio.loop = false;
        bgmAudio.Play();
    }





    void Update()
    {
        if (gameState == GameState.Ready)
        {
            readyTime = readyTime + Time.deltaTime;

            if (readyTime < 1f)
            {
                messageText.text = "3";
            }
            else if (readyTime < 2f)
            {
                messageText.text = "2";
            }
            else if (readyTime < 3f)
            {
                messageText.text = "1";
            }
            else if (readyTime < 4f)
            {
                messageText.text = "GO!";
            }
            else
            {
                gameOverlay.SetActive(false);
                gameState = GameState.Playing;
                gameTime = readyTime - 4f;

                bgmAudio.Stop();
                bgmAudio.clip = normalMusic;
                bgmAudio.loop = true;
                bgmAudio.Play();
            }
        }

        else if (gameState == GameState.Playing)
        {
            gameTime = gameTime + Time.deltaTime;

            UpdateScared();
            UpdateMusic();
        }


        else if (gameState == GameState.Dying)
        {
            // 死亡期间也算用时
            gameTime = gameTime + Time.deltaTime;
            deathTime = deathTime + Time.deltaTime;

            if (deathTime >= 1f)
            {
                if (life > 0)
                {
                    player.ResetPlayer();

                    for (int i = 0; i < ghosts.Length; i++)
                    {
                        ghosts[i].ResetGhost();
                    }

                    cherry.SetPaused(false);


                    gameState = GameState.Playing;

                    bgmAudio.clip = normalMusic;
                    bgmAudio.loop = true;
                    bgmAudio.Play();
                }
                else
                {
                    EndGame();
                }
            }
        }
        else if (gameState == GameState.GameOver)
        {
            endTime = endTime + Time.deltaTime;

            if (endTime >= 3f)
            {
                SceneManager.LoadScene("StartScene");
            }
        }

        ShowTime();
    }


    void StartScared()
    {
        // 再吃一颗就重新算十秒
        scaredTime = 10f;
        scaredTimeText.gameObject.SetActive(true);
        scaredTimeText.text = "10";
        UpdateMusic();
    }


    void UpdateScared()
    {
        if (scaredTime > 0f)
        {
            scaredTime = scaredTime - Time.deltaTime;

            if (scaredTime <= 0f)
            {
                scaredTime = 0f;
                scaredTimeText.gameObject.SetActive(false);
            }
            else
            {
                scaredTimeText.text =
                    Mathf.CeilToInt(scaredTime).ToString();
            }
        }
    }


    public float GetScaredTime()
    {
        return scaredTime;
    }


    void ClearScared()
    {
        scaredTime = 0f;
        scaredTimeText.gameObject.SetActive(false);
    }


    void UpdateMusic()
    {
        bool hasDeadGhost = false;

        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i].GetState() == 3)
            {
                hasDeadGhost = true;
                break;
            }
        }


        AudioClip nextMusic;

        if (hasDeadGhost)
        {
            nextMusic = deadMusic;
        }
        else if (scaredTime > 0f)
        {
            nextMusic = scaredMusic;
        }
        else
        {
            nextMusic = normalMusic;
        }

        if (bgmAudio.clip != nextMusic)
        {
            bgmAudio.Stop();
            bgmAudio.clip = nextMusic;
            bgmAudio.loop = true;
            bgmAudio.Play();
        }
    }




    void ShowLife()
    {
        life1.SetActive(life >= 1);
        life2.SetActive(life >= 2);
        life3.SetActive(life >= 3);
    }


    public void PlayerHit()
    {
        // 同一次死亡只扣一次
        if (!CanPlay())
        {
            return;
        }

        gameState = GameState.Dying;
        deathTime = 0f;

        ClearScared();


        life = life - 1;
        ShowLife();

        ResetCherryCheck();
        cherry.SetPaused(true);
        player.PlayDeath();

        bgmAudio.Stop();
        eventAudio.Stop();
        eventAudio.PlayOneShot(deathSound);
    }


    public void EndGame()
    {
        if (gameState == GameState.GameOver)
        {
            return;
        }

        gameState = GameState.GameOver;
        endTime = 0f;

        ClearScared();

        player.StopPlayer();
        cherry.StopCherry();
        ResetCherryCheck();

        bgmAudio.Stop();
        exitButton.interactable = false;

        messageText.text = "Game Over";
        gameOverlay.SetActive(true);

        SaveRecord();
    }


    void SaveRecord()
    {
        string scoreKey;
        string timeKey;

        if (SceneManager.GetActiveScene().name == "InnovationScene")
        {
            scoreKey = "L2BestScore";
            timeKey = "L2BestTime";
        }
        else
        {
            scoreKey = "L1BestScore";
            timeKey = "L1BestTime";
        }

        bool needSave = false;

        if (!PlayerPrefs.HasKey(scoreKey))
        {
            needSave = true;
        }
        else
        {
            int oldScore = PlayerPrefs.GetInt(scoreKey);
            float oldTime = PlayerPrefs.GetFloat(timeKey);

            if (score > oldScore)
            {
                needSave = true;
            }
            else if (score == oldScore && gameTime < oldTime)
            {
                needSave = true;
            }
        }

        if (needSave)
        {
            PlayerPrefs.SetInt(scoreKey, score);
            PlayerPrefs.SetFloat(timeKey, gameTime);
            PlayerPrefs.Save();
        }
    }


    void ShowTime()
    {
        int totalTime = Mathf.FloorToInt(gameTime * 100f);

        int minute = totalTime / 6000;
        int second = totalTime / 100 % 60;
        int smallSecond = totalTime % 100;

        timeText.text = "Time: " + minute.ToString("D2")
            + ":" + second.ToString("D2")
            + ":" + smallSecond.ToString("D2");
    }



    public bool CanPlay()
    {
        return gameState == GameState.Playing;
    }



    public bool HasFood(int row, int column)
    {
        if (row < 0 || row >= 29 || column < 0 || column >= 28)
        {
            return false;
        }

        GameObject food = foodMap[row, column];

        if (food != null && food.activeInHierarchy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }





    public void EatFood(Vector3 playerPoint)
    {
        if (!CanPlay() || !player.HasStarted())
        {
            return;
        }

        int column = Mathf.RoundToInt(playerPoint.x);
        int row = Mathf.RoundToInt(-playerPoint.y);

        if (!HasFood(row, column))
        {
            return;
        }

        GameObject food = foodMap[row, column];
        int addScore;


        if (food.name.EndsWith("_PowerPellet"))
        {
            addScore = 50;
            StartScared();
        }
        else
        {
            addScore = 10;
        }



        // 清掉记录，避免重复吃
        foodMap[row, column] = null;
        food.SetActive(false);
        Destroy(food);

        foodLeft--;
        AddScore(addScore);

        if (foodLeft == 0 &&
            SceneManager.GetActiveScene().name == "RecreatedLevel")
        {
            EndGame();
        }
    }




    public void AddScore(int number)
    {
        score = score + number;
        scoreText.text = "Score: " + score.ToString("D6");
    }



    void CheckGhostContact()
    {
        Vector3 playerPoint = player.transform.position;

        for (int i = 0; i < ghosts.Length; i++)
        {
            Vector3 ghostPoint = ghosts[i].transform.position;
            Vector3 oldGap;

            if (hasGhostPoints)
            {
                oldGap = oldPlayerPoint - oldGhostPoints[i];
            }
            else
            {
                oldGap = playerPoint - ghostPoint;
            }

            Vector3 newGap = playerPoint - ghostPoint;
            Vector3 gapMove = newGap - oldGap;
            float nearTime = 0f;

            if (gapMove.sqrMagnitude > 0f)
            {
                nearTime = -Vector3.Dot(oldGap, gapMove)
                    / gapMove.sqrMagnitude;
                nearTime = Mathf.Clamp01(nearTime);
            }

            Vector3 nearGap = oldGap + gapMove * nearTime;
            oldGhostPoints[i] = ghostPoint;


            if (nearGap.sqrMagnitude <= 0.25f)
            {
                int state = ghosts[i].GetState();

                if (state == 0)
                {
                    PlayerHit();
                    return;
                }
                else if (state == 1 || state == 2)
                {
                    if (ghosts[i].EatGhost())
                    {
                        AddScore(300);
                        eventAudio.PlayOneShot(eatGhostSound);
                        UpdateMusic();
                    }
                }
            }
        }

        oldPlayerPoint = playerPoint;
        hasGhostPoints = true;
    }




        void LateUpdate()
    {
        if (!CanPlay())
        {
            lastCherry = null;
            hasGhostPoints = false;
            return;
        }

        CheckGhostContact();

        if (!CanPlay())
        {
            return;
        }

        GameObject nowCherry = cherry.GetCherry();



        if (!player.HasStarted() || nowCherry == null)
        {
            lastCherry = null;
            return;
        }

        Vector3 playerPoint = player.transform.position;
        Vector3 cherryPoint = nowCherry.transform.position;

        if (lastCherry != nowCherry)
        {
            lastPlayerPoint = playerPoint;
            lastCherryPoint = cherryPoint;
        }




        // 看这一帧有没有迎面撞到
        Vector3 oldGap = lastPlayerPoint - lastCherryPoint;
        Vector3 newGap = playerPoint - cherryPoint;
        Vector3 gapMove = newGap - oldGap;

        float nearTime = 0f;

        if (gapMove.sqrMagnitude > 0f)
        {
            nearTime = -Vector3.Dot(oldGap, gapMove)
                / gapMove.sqrMagnitude;
            nearTime = Mathf.Clamp01(nearTime);
        }


        Vector3 nearGap = oldGap + gapMove * nearTime;


        if (nearGap.sqrMagnitude <= 0.25f)
        {
            if (cherry.EatCherry())
            {
                AddScore(100);
                eventAudio.PlayOneShot(cherrySound);
                lastCherry = null;
                return;
            }
        }


        lastCherry = nowCherry;
        lastPlayerPoint = playerPoint;
        lastCherryPoint = cherryPoint;
    }




    public void ResetCherryCheck()
    {
        lastCherry = null;
        hasGhostPoints = false;
    }





    public void ExitGame()
    {
        if (gameState == GameState.GameOver)
        {
            return;
        }

        SceneManager.LoadScene("StartScene");
    }
}