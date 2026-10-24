using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



public class StartMenuController : MonoBehaviour
{
    public Image borderTop;
    public Image borderBottom;
    public Image borderLeft;
    public Image borderRight;

    public TMP_Text level1RecordText;
    public TMP_Text level2RecordText;

    private Color startColor = new Color32(64, 224, 255, 255);
    private Color endColor = Color.white;

    private float changeTime = 1f;
    private float colorTime = 0f;
    private bool turnWhite = true;

    void Start()
    {
        int level1Score = PlayerPrefs.GetInt("L1BestScore", 0);
        float level1Time = PlayerPrefs.GetFloat("L1BestTime", 0f);

        int level2Score = PlayerPrefs.GetInt("L2BestScore", 0);
        float level2Time = PlayerPrefs.GetFloat("L2BestTime", 0f);

        level1RecordText.text = "Best Score: " + level1Score.ToString("D6")
            + "\nTime: " + GetTimeText(level1Time);

        level2RecordText.text = "Best Score: " + level2Score.ToString("D6")
            + "\nTime: " + GetTimeText(level2Time);
    }

    string GetTimeText(float saveTime)
    {
        // 先换成百分之一秒
        int totalTime = Mathf.FloorToInt(saveTime * 100f);

        int minute = totalTime / 6000;
        int second = totalTime / 100 % 60;
        int smallSecond = totalTime % 100;

        return minute.ToString("D2") + ":"
            + second.ToString("D2") + ":"
            + smallSecond.ToString("D2");
    }

    public void OpenLevel1()
    {
        SceneManager.LoadScene("RecreatedLevel");
    }

    public void OpenLevel2()
    {
        SceneManager.LoadScene("InnovationScene");
    }



    
    void Update()
    {
        // 每秒换一次方向
        colorTime = colorTime + Time.deltaTime;

        while (colorTime >= changeTime)
        {
            colorTime = colorTime - changeTime;

            if (turnWhite == true)
            {
                turnWhite = false;
            }
            else
            {
                turnWhite = true;
            }
        }

        float colorPercent = colorTime / changeTime;
        Color nowColor;

        if (turnWhite == true)
        {
            nowColor = Color.Lerp(startColor, endColor, colorPercent);
        }
        else
        {
            nowColor = Color.Lerp(endColor, startColor, colorPercent);
        }

        // 四条边一起变色
        borderTop.color = nowColor;
        borderBottom.color = nowColor;
        borderLeft.color = nowColor;
        borderRight.color = nowColor;
    }
}