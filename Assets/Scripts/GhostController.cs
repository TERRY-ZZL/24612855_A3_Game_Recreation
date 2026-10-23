using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public GameManager game;
    public int ghostNumber = 1;

    private Animator ghostAnimator;
    private Vector3 bornPoint;
    private bool isDead = false;
    private bool hasLeft = false;
    private string lastAnimation = "";

    private int direction = -1;
    private bool isMoving = false;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float movePercent = 0f;
    private float moveLength = 1f;

    private float normalSpeed = 4.5f;
    private float slowSpeed = 2.25f;






    // 右、下、左、上，行号往下加
    private Vector2Int[] steps =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1)
    };

    private List<Vector2Int> road = new List<Vector2Int>();




    void Awake()
    {
        ghostAnimator = GetComponent<Animator>();
        bornPoint = transform.position;

        if (ghostNumber == 4)
        {
            MakeRoad();
        }
    }


    void Update()
    {
        if (!game.CanPlay())
        {
            ghostAnimator.speed = 0f;
            return;
        }

        ghostAnimator.speed = 1f;
        float leftTime = Time.deltaTime;

        while (leftTime > 0f)
        {
            if (!isMoving)
            {
                if (!StartMove())
                {
                    break;
                }
            }

            ShowAnimation();

            float speed;

            if (GetState() == 0)
            {
                speed = normalSpeed;
            }
            else
            {
                speed = slowSpeed;
            }

            float needTime =
                (1f - movePercent) * moveLength / speed;

            if (leftTime >= needTime)
            {
                transform.position = endPoint;
                leftTime = leftTime - needTime;
                FinishMove();
            }
            else
            {
                movePercent = movePercent
                    + leftTime * speed / moveLength;

                transform.position = Vector3.Lerp(
                    startPoint, endPoint, movePercent);

                leftTime = 0f;
            }
        }

        ShowAnimation();
    }


    bool StartMove()
    {
        if (isDead)
        {
            startPoint = transform.position;
            endPoint = bornPoint;
        }
        else
        {
            Vector2Int point = new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(-transform.position.y));

            if (!hasLeft)
            {
                if (point.y == 11 || point.y == 17)
                {
                    hasLeft = true;
                }
            }

            int nextDirection;

            if (!hasLeft)
            {
                nextDirection = GetExitDirection(point);
            }
            else
            {
                nextDirection = ChooseDirection(point);
            }

            if (nextDirection == -1)
            {
                return false;
            }

            direction = nextDirection;
            Vector2Int nextPoint = point + steps[direction];

            startPoint = transform.position;
            endPoint = new Vector3(
                nextPoint.x, -nextPoint.y, 0f);
        }

        moveLength = Vector3.Distance(startPoint, endPoint);
        movePercent = 0f;
        isMoving = true;

        return true;
    }


    void FinishMove()
    {
        isMoving = false;
        movePercent = 0f;

        if (isDead)
        {
            // 到家才复活
            isDead = false;
            hasLeft = false;
            direction = -1;
            game.ResetGhostPoint(this);
        }
    }


    int GetExitDirection(Vector2Int point)
    {
        int doorColumn;

        if (ghostNumber == 1 || ghostNumber == 2)
        {
            doorColumn = 13;
        }
        else
        {
            doorColumn = 14;
        }

        if (point.x < doorColumn)
        {
            return 0;
        }
        else if (point.x > doorColumn)
        {
            return 2;
        }
        else if (ghostNumber == 1 || ghostNumber == 3)
        {
            return 3;
        }
        else
        {
            return 1;
        }
    }


    bool CanWalk(Vector2Int point)
    {
        // 两端传送口不能走
        if (point.x <= 0 || point.x >= 27 ||
            point.y <= 0 || point.y >= 28)
        {
            return false;
        }

        // 出门后不能回出生区
        if (point.x >= 11 && point.x <= 16 &&
            point.y >= 13 && point.y <= 15)
        {
            return false;
        }

        int number = game.map.GetMapValue(point.y, point.x);

        if (number == 0 || number == 5 || number == 6)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    List<int> GetWays(Vector2Int point, int lastDirection)
    {
        List<int> ways = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            if (CanWalk(point + steps[i]))
            {
                ways.Add(i);
            }
        }

        // 有别的路就不回头
        if (ways.Count > 1 && lastDirection >= 0)
        {
            int backDirection = (lastDirection + 2) % 4;
            ways.Remove(backDirection);
        }

        return ways;
    }


    int ChooseDirection(Vector2Int point)
    {
        List<int> ways = GetWays(point, direction);

        if (ways.Count == 0)
        {
            return -1;
        }

        int state = GetState();

        if (state == 0 && ghostNumber == 4)
        {
            return FindRoad(point, ways);
        }

        bool runAway = false;
        bool chasePlayer = false;

        if (state == 1 || state == 2)
        {
            runAway = true;
        }
        else if (ghostNumber == 1)
        {
            runAway = true;
        }
        else if (ghostNumber == 2)
        {
            chasePlayer = true;
        }

        List<int> goodWays = new List<int>();

        Vector3 playerPoint = game.player.transform.position;
        Vector3 nowPoint = new Vector3(point.x, -point.y, 0f);
        float oldDistance = (nowPoint - playerPoint).sqrMagnitude;

        for (int i = 0; i < ways.Count; i++)
        {
            Vector2Int nextCell = point + steps[ways[i]];
            Vector3 nextPoint =
                new Vector3(nextCell.x, -nextCell.y, 0f);

            float nextDistance =
                (nextPoint - playerPoint).sqrMagnitude;

            if (runAway)
            {
                if (nextDistance >= oldDistance)
                {
                    goodWays.Add(ways[i]);
                }
            }
            else if (chasePlayer)
            {
                if (nextDistance <= oldDistance)
                {
                    goodWays.Add(ways[i]);
                }
            }
            else
            {
                goodWays.Add(ways[i]);
            }
        }

        if (goodWays.Count > 0)
        {
            return goodWays[Random.Range(0, goodWays.Count)];
        }
        else
        {
            return ways[Random.Range(0, ways.Count)];
        }
    }


    void MakeRoad()
    {
        Vector2Int[] corners =
        {
            new Vector2Int(1, 1),
            new Vector2Int(12, 1),
            new Vector2Int(12, 5),
            new Vector2Int(15, 5),
            new Vector2Int(15, 1),
            new Vector2Int(26, 1),
            new Vector2Int(26, 8),
            new Vector2Int(21, 8),
            new Vector2Int(21, 20),
            new Vector2Int(26, 20),
            new Vector2Int(26, 27),
            new Vector2Int(15, 27),
            new Vector2Int(15, 23),
            new Vector2Int(12, 23),
            new Vector2Int(12, 27),
            new Vector2Int(1, 27),
            new Vector2Int(1, 20),
            new Vector2Int(6, 20),
            new Vector2Int(6, 8),
            new Vector2Int(1, 8)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Vector2Int point = corners[i];
            Vector2Int target = corners[(i + 1) % corners.Length];

            while (point != target)
            {
                road.Add(point);

                if (point.x < target.x)
                {
                    point.x++;
                }
                else if (point.x > target.x)
                {
                    point.x--;
                }
                else if (point.y < target.y)
                {
                    point.y++;
                }
                else
                {
                    point.y--;
                }
            }
        }
    }


    int GetRoadDirection(Vector2Int point)
    {
        int number = road.IndexOf(point);

        if (number == -1)
        {
            return -1;
        }

        Vector2Int nextPoint = road[(number + 1) % road.Count];
        Vector2Int change = nextPoint - point;

        for (int i = 0; i < 4; i++)
        {
            if (steps[i] == change)
            {
                return i;
            }
        }

        return -1;
    }


    int FindRoad(Vector2Int point, List<int> ways)
    {
        int roadDirection = GetRoadDirection(point);

        if (ways.Contains(roadDirection))
        {
            return roadDirection;
        }

        // 按通道找回外圈，也记住进来的方向
        List<Vector3Int> checkPoints = new List<Vector3Int>();
        List<int> firstWays = new List<int>();
        bool[,,] checkedPoints = new bool[29, 28, 4];

        checkPoints.Add(new Vector3Int(
            point.x, point.y, direction));
        firstWays.Add(-1);

        for (int i = 0; i < checkPoints.Count; i++)
        {
            Vector3Int checkPoint = checkPoints[i];
            Vector2Int cell =
                new Vector2Int(checkPoint.x, checkPoint.y);

            List<int> nextWays = GetWays(cell, checkPoint.z);

            for (int j = 0; j < nextWays.Count; j++)
            {
                int nextDirection = nextWays[j];
                Vector2Int nextPoint = cell + steps[nextDirection];

                if (checkedPoints[
                    nextPoint.y, nextPoint.x, nextDirection])
                {
                    continue;
                }

                checkedPoints[
                    nextPoint.y, nextPoint.x, nextDirection] = true;

                int firstDirection;

                if (i == 0)
                {
                    firstDirection = nextDirection;
                }
                else
                {
                    firstDirection = firstWays[i];
                }

                int nextRoadDirection = GetRoadDirection(nextPoint);
                List<int> arriveWays =
                    GetWays(nextPoint, nextDirection);

                if (arriveWays.Contains(nextRoadDirection))
                {
                    return firstDirection;
                }

                checkPoints.Add(new Vector3Int(
                    nextPoint.x, nextPoint.y, nextDirection));
                firstWays.Add(firstDirection);
            }
        }

        return ways[0];
    }


    public int GetState()
    {
        if (isDead)
        {
            return 3;
        }
        else if (game.GetScaredTime() > 3f)
        {
            return 1;
        }
        else if (game.GetScaredTime() > 0f)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }


    void ShowAnimation()
    {
        int state = GetState();
        string animationName =
            "Ghost" + ghostNumber.ToString("D2");

        if (state == 3)
        {
            animationName = animationName + "_Dead";
        }
        else if (state == 2)
        {
            animationName = animationName + "_Recovering";
        }
        else
        {
            if (state == 1)
            {
                animationName = animationName + "_Scared";
            }
            else
            {
                animationName = animationName + "_Walk";
            }

            if (direction == 1)
            {
                animationName = animationName + "Down";
            }
            else if (direction == 2)
            {
                animationName = animationName + "Left";
            }
            else if (direction == 3)
            {
                animationName = animationName + "Up";
            }
            else
            {
                animationName = animationName + "Right";
            }
        }

        if (animationName != lastAnimation)
        {
            ghostAnimator.Play(animationName, 0, 0f);
            lastAnimation = animationName;
        }
    }


    public bool EatGhost()
    {
        int state = GetState();

        if (state != 1 && state != 2)
        {
            return false;
        }

        isDead = true;
        isMoving = false;
        ShowAnimation();

        return true;
    }


    public void ResetGhost()
    {
        transform.position = bornPoint;
        isDead = false;
        hasLeft = false;
        isMoving = false;
        movePercent = 0f;
        direction = -1;
        lastAnimation = "";

        ShowAnimation();
        ghostAnimator.Update(0f);
    }
}