using UnityEngine;
using UnityEngine.InputSystem;






public class PacStudentController : MonoBehaviour
{
    public LevelGenerator map;
    public GameManager game;
    public float moveSpeed = 5f;

    public AudioClip moveSound;
    public AudioClip eatSound;
    public ParticleSystem moveDust;


    public ParticleSystem wallHit;
    public AudioClip wallSound;

    private bool wallStopped = false;




    
    private int lastInput = -1;
    private int currentInput = -1;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float movePercent = 0f;

    private bool isMoving = false;
    private bool waitInput = true;

    private Animator playerAnimator;
    private AudioSource moveAudio;

   



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
                game.EatFood(transform.position);

                               if (!StartMove())
                {
                    playerAnimator.speed = 0f;
                    StopMoveShow();

                    if (!wallStopped)
                    {
                        PlayWallHit();
                        wallStopped = true;
                    }

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

                CheckTeleport();
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





    void CheckTeleport()
    {
        if (transform.position.y != -14f)
        {
            return;
        }

        if (currentInput == 2 && transform.position.x == 0f)
        {
            transform.position = new Vector3(27f, -14f, 0f);
            game.ResetCherryCheck();
        }
        else if (currentInput == 0 && transform.position.x == 27f)
        {
            transform.position = new Vector3(0f, -14f, 0f);
            game.ResetCherryCheck();
        }
    }



    public bool HasStarted()
    {
        return !waitInput;
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
        wallStopped = false;

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

        if (game.HasFood(row, column))
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



    void PlayWallHit()
    {
        int hitDirection = currentInput;

        if (hitDirection == -1)
        {
            hitDirection = lastInput;
        }

        wallHit.transform.position = transform.position
            + GetDirection(hitDirection) * 0.5f;

        wallHit.Play();
        wallHit.Emit(8);
        game.eventAudio.PlayOneShot(wallSound);
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