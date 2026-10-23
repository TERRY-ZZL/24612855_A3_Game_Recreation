using UnityEngine;

public class GhostController : MonoBehaviour
{
    public GameManager game;
    public int ghostNumber = 1;

    private Animator ghostAnimator;
    private Vector3 bornPoint;
    private bool isDead = false;
    private string lastAnimation = "";





    void Awake()
    {
        ghostAnimator = GetComponent<Animator>();
        bornPoint = transform.position;
    }


    void Update()
    {
        if (!game.CanPlay())
        {
            ghostAnimator.speed = 0f;
            return;
        }

        ghostAnimator.speed = 1f;
        ShowAnimation();
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
        string animationName = "Ghost"
            + ghostNumber.ToString("D2");

        if (state == 3)
        {
            animationName = animationName + "_Dead";
        }
        else if (state == 2)
        {
            animationName = animationName + "_Recovering";
        }
        else if (state == 1)
        {
            animationName = animationName + "_ScaredRight";
        }
        else
        {
            animationName = animationName + "_WalkRight";
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
        ShowAnimation();
        return true;
    }


    public void ResetGhost()
    {
        transform.position = bornPoint;
        isDead = false;
        lastAnimation = "";

        ShowAnimation();
        ghostAnimator.Update(0f);
    }
}