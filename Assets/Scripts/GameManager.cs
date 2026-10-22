using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void ExitGame()
    {
        SceneManager.LoadScene("StartScene");
    }
}