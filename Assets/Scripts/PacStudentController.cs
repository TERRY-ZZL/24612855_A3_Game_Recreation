using UnityEngine;
using UnityEngine.InputSystem;






public class PacStudentController : MonoBehaviour
{
    public LevelGenerator map;
    public float moveSpeed = 5f;

    public AudioClip moveSound;
    public AudioClip eatSound;
    public ParticleSystem moveDust;

    private int lastInput = -1;
    private int currentInput = -1;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float movePercent = 0f;

    private bool isMoving = false;
    private bool waitInput = true;

    private Animator playerAnimator;
    private AudioSource moveAudio;

    private GameObject[,] foodMap = new GameObject[29, 28];



    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        moveAudio = GetComponent<AudioSource>();

        playerAnimator.SetBool("PreviewMode", false);
        playerAnimator.SetBool("IsDead", false);
        playerAnimator.SetInteger("Direction", 0);

        playerAnimator.Play("PacStudent_WalkRight", 0, 0f);
        playerAnimator.Update(0f);
        playerAnimator.speed = 0f;

        moveAudio.Stop();
        moveDust.Stop(
            true, ParticleSystemStopBehavior.StopEmittingAndClear);

        FindFood();
    }



    void FindFood()
    {
        Transform[] mapObjects =
            map.sceneMap.GetComponentsInChildren<Transform>();

        foreach (Transform item in mapObjects)
        {
            if (item.name.EndsWith("_Pellet") ||
                item.name.EndsWith("_PowerPellet"))
            {
                // 镜像地图也用世界位置
                int column = Mathf.RoundToInt(item.position.x);
                int row = Mathf.RoundToInt(-item.position.y);

                foodMap[row, column] = item.gameObject;
            }
        }
    }



    void Update()
    {
        ReadInput();

        if (waitInput)
        {
            return;
        }

        float leftTime = Time.deltaTime;

        while (true)
        {
            // 到格点再选方向
            if (!isMoving)
            {
                if (!StartMove())
                {
                    playerAnimator.speed = 0f;
                    StopMoveShow();
                    return;
                }
            }

            if (leftTime <= 0f)
            {
                break;
            }

            float needTime = (1f - movePercent) / moveSpeed;

            if (leftTime >= needTime)
            {
                transform.position = endPoint;
                leftTime = leftTime - needTime;
                movePercent = 0f;
                isMoving = false;
            }
            else
            {
                movePercent = movePercent + leftTime * moveSpeed;

                transform.position = Vector3.Lerp(
                    startPoint, endPoint, movePercent);

                break;
            }
        }
    }



    void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        // 同帧按键按这个顺序选
        if (keyboard.dKey.wasPressedThisFrame)
        {
            lastInput = 0;
            waitInput = false;
        }
        else if (keyboard.sKey.wasPressedThisFrame)
        {
            lastInput = 1;
            waitInput = false;
        }
        else if (keyboard.aKey.wasPressedThisFrame)
        {
            lastInput = 2;
            waitInput = false;
        }
        else if (keyboard.wKey.wasPressedThisFrame)
        {
            lastInput = 3;
            waitInput = false;
        }
    }



    bool StartMove()
    {
        int nextInput;

        if (CanMove(lastInput))
        {
            nextInput = lastInput;
        }
        else if (CanMove(currentInput))
        {
            nextInput = currentInput;
        }
        else
        {
            return false;
        }

        currentInput = nextInput;

        startPoint = transform.position;
        endPoint = startPoint + GetDirection(currentInput);

        movePercent = 0f;
        isMoving = true;

        playerAnimator.SetInteger("Direction", currentInput);
        playerAnimator.speed = 1f;

        PlayMoveShow();

        return true;
    }



    void PlayMoveShow()
    {
        int column = Mathf.RoundToInt(endPoint.x);
        int row = Mathf.RoundToInt(-endPoint.y);

        AudioClip nextSound = moveSound;
        GameObject food = foodMap[row, column];

        if (food != null && food.activeInHierarchy)
        {
            nextSound = eatSound;
        }

        // 同一种声音接着播
        if (moveAudio.clip != nextSound)
        {
            moveAudio.Stop();
            moveAudio.clip = nextSound;
        }

        if (!moveAudio.isPlaying)
        {
            moveAudio.Play();
        }

        if (!moveDust.isPlaying)
        {
            moveDust.Play();
        }
    }



    void StopMoveShow()
    {
        if (moveAudio.isPlaying)
        {
            moveAudio.Stop();
        }

        if (moveDust.isPlaying || moveDust.particleCount > 0)
        {
            moveDust.Stop(
                true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }



    bool CanMove(int direction)
    {
        if (direction == -1)
        {
            return false;
        }

        Vector3 nextPoint =
            transform.position + GetDirection(direction);

        int column = Mathf.RoundToInt(nextPoint.x);
        int row = Mathf.RoundToInt(-nextPoint.y);

        int number = map.GetMapValue(row, column);

        // 空地和豆的位置能走
        if (number == 0 || number == 5 || number == 6)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    Vector3 GetDirection(int direction)
    {
        if (direction == 0)
        {
            return Vector3.right;
        }
        else if (direction == 1)
        {
            return Vector3.down;
        }
        else if (direction == 2)
        {
            return Vector3.left;
        }
        else if (direction == 3)
        {
            return Vector3.up;
        }
        else
        {
            return Vector3.zero;
        }
    }
}