using UnityEngine;

public class CherryController : MonoBehaviour
{
    public GameObject cherryPrefab;
    public Camera gameCamera;
    public float moveSpeed = 3f;
    public float spawnWait = 5f;

    private GameObject nowCherry;
    private Vector3 mapCenter = new Vector3(13.5f, -14f, 0f);
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float waitTime;
    private float movePercent;
    private float moveLength;
    private bool isPaused = false;
    private bool isStopped = false;





    void Start()
    {
        waitTime = spawnWait;
    }



    void Update()
    {
        if (isPaused || isStopped)
        {
            return;
        }



        float leftTime = Time.deltaTime;
        while (leftTime > 0f)
        {
            if (nowCherry == null)
            {
                if (leftTime < waitTime)
                {
                    waitTime = waitTime - leftTime;
                    return;
                }
                else
                {
                    leftTime = leftTime - waitTime;
                    CreateCherry();
                }
            }

            float needTime = (1f - movePercent) * moveLength / moveSpeed;
            if (leftTime >= needTime)
            {
                nowCherry.transform.position = endPoint;
                leftTime = leftTime - needTime;
                RemoveCherry();
            }
            else
            {
                movePercent = movePercent + leftTime * moveSpeed / moveLength;
                nowCherry.transform.position = Vector3.Lerp(startPoint, endPoint, movePercent);
                return;
            }
        }
    }




    void CreateCherry()
    {
        // 放在镜头外面
        float halfHeight = gameCamera.orthographicSize + 1f;
        float halfWidth = gameCamera.orthographicSize * gameCamera.aspect + 1f;
        int side = Random.Range(0, 4);
        float startX;
        float startY;

        if (side == 0)
        {
            startX = mapCenter.x - halfWidth;
            startY = mapCenter.y + Random.Range(-halfHeight, halfHeight);
        }
        else if (side == 1)
        {
            startX = mapCenter.x + halfWidth;
            startY = mapCenter.y + Random.Range(-halfHeight, halfHeight);
        }
        else if (side == 2)
        {
            startX = mapCenter.x + Random.Range(-halfWidth, halfWidth);
            startY = mapCenter.y + halfHeight;
        }
        else
        {
            startX = mapCenter.x + Random.Range(-halfWidth, halfWidth);
            startY = mapCenter.y - halfHeight;
        }

        startPoint = new Vector3(startX, startY, 0f);

        // 穿过中心，走到对面

        endPoint = mapCenter + (mapCenter - startPoint);
        moveLength = Vector3.Distance(startPoint, endPoint);
        movePercent = 0f;
        nowCherry = Instantiate(cherryPrefab, startPoint, Quaternion.identity);
    }

    void RemoveCherry()
    {
        nowCherry.SetActive(false);
        Destroy(nowCherry);
        nowCherry = null;
        waitTime = spawnWait;
        movePercent = 0f;
    }

    public GameObject GetCherry()
    {
        return nowCherry;
    }

    public bool EatCherry()
    {
        if (nowCherry == null || isPaused || isStopped)
        {
            return false;
        }
        RemoveCherry();
        return true;
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void StopCherry()
    {
        isStopped = true;
    }
}