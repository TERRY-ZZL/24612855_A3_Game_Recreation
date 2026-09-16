using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Vector3 pointA = new Vector3(1f, -1f, 0f);
    private Vector3 pointB = new Vector3(6f, -1f, 0f);
    private Vector3 pointC = new Vector3(6f, -5f, 0f);
    private Vector3 pointD = new Vector3(1f, -5f, 0f);

    private Vector3 startPoint;
    private Vector3 endPoint;

    private int currentLine = 0;
    private float lineTime = 0f;

    private Animator playerAnimator;

    void Start()
    {
        transform.position = pointA;




        playerAnimator = GetComponent<Animator>();
        playerAnimator.SetBool("PreviewMode", false);
        playerAnimator.SetBool("IsDead", false);
        playerAnimator.SetInteger("Direction", 0);
    }

    void Update()
    {
        lineTime = lineTime + Time.deltaTime;

        SetCurrentLine();

        float lineDistance = Vector3.Distance(startPoint, endPoint);
        float lineNeedTime = lineDistance / moveSpeed;

        while (lineTime >= lineNeedTime)
        {
            lineTime = lineTime - lineNeedTime;
            currentLine = currentLine + 1;

            if (currentLine > 3)
            {
                currentLine = 0;
            }

            SetCurrentLine();

            lineDistance = Vector3.Distance(startPoint, endPoint);
            lineNeedTime = lineDistance / moveSpeed;
        }

        float movePercent = lineTime / lineNeedTime;
        transform.position = Vector3.Lerp(startPoint, endPoint, movePercent);
    }

    void SetCurrentLine()
    {
        if (currentLine == 0)
        {
            startPoint = pointA;
            endPoint = pointB;
            playerAnimator.SetInteger("Direction", 0);
        }
        else if (currentLine == 1)
        {
            startPoint = pointB;
            endPoint = pointC;
            playerAnimator.SetInteger("Direction", 1);
        }
        else if (currentLine == 2)
        {
            startPoint = pointC;
            endPoint = pointD;
            playerAnimator.SetInteger("Direction", 2);
        }
        else
        {
            startPoint = pointD;
            endPoint = pointA;
            playerAnimator.SetInteger("Direction", 3);
        }
    }
}