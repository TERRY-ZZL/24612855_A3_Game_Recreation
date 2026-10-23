using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;



public class GameManager : MonoBehaviour
{
    public LevelGenerator map;
    public TMP_Text scoreText;

    public PacStudentController player;
    public CherryController cherry;
    public AudioSource eventAudio;
    public AudioClip cherrySound; 

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
    }

    public void AddScore(int number)
    {
        score = score + number;
        scoreText.text = "Score: " + score.ToString("D6");
    }



    void LateUpdate()
    {
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

        //看着一帧有没有当面撞到
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
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}