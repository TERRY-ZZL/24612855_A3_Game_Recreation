using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InnovationController : MonoBehaviour
{
    public GameManager game;
    public PacStudentController player;

    public SpriteRenderer[] beacons = new SpriteRenderer[3];
    public SpriteRenderer exitPoint;

    public TMP_Text energyText;
    public TMP_Text beaconText;
    public TMP_Text hintText;
    public TMP_Text exitLabel;



    private int energy = 0;
    private int lightNum = 0;
    private bool[] lightOn = new bool[3];

    private Color darkColor = new Color32(64, 80, 96, 255);
    private Color lightColor = new Color32(64, 224, 255, 255);

    void Start()
    {
        for (int i = 0; i < beacons.Length; i++)
        {
            beacons[i].color = darkColor;
        }

        exitPoint.color = darkColor;


        exitLabel.text = "EXIT\n<size=28>LOCKED</size>";
        exitLabel.color = Color.white;

        ShowNumber();
        hintText.text = "Collect 20 energy per beacon, press E to light all 3, then reach the exit.";
    }


    

    void LateUpdate()
    {
        if (!game.CanPlay())
        {
            return;
        }

        string tip = "Collect 20 energy per beacon, press E to light all 3, then reach the exit.";

        if (lightNum == 3)
        {
            tip = "Exit open";
        }
        else if (energy == 20)
        {
            tip = "Energy full - charge a beacon";
        }

        Keyboard keyboard = Keyboard.current;
        bool pressE = false;

        if (keyboard != null)
        {
            pressE = keyboard.eKey.wasPressedThisFrame;
        }

        for (int i = 0; i < beacons.Length; i++)
        {
            float distance = Vector3.Distance(
                player.transform.position,
                beacons[i].transform.position);

            if (distance <= 1f)
            {
                if (lightOn[i])
                {
                    tip = "Beacon charged";
                }
                else if (energy < 20)
                {
                    tip = "Need 20 energy";
                }
                else
                {
                    tip = "Press E to charge";

                    if (pressE)
                    {
                        energy = energy - 20;
                        lightOn[i] = true;
                        lightNum = lightNum + 1;

                        beacons[i].color = lightColor;
                        game.eventAudio.PlayOneShot(game.cherrySound);
                        tip = "Beacon charged";

                        if (lightNum == 3)
                        {
                            exitPoint.color = lightColor;


                            exitLabel.text = "EXIT\n<size=28>OPEN</size>";
                            exitLabel.color = lightColor;

                            
                            tip = "Exit open";
                        }

                        ShowNumber();
                    }
                }

                break;
            }
        }

        float exitDistance = Vector3.Distance(
            player.transform.position,
            exitPoint.transform.position);

        if (exitDistance <= 1f)
        {
            if (lightNum == 3)
            {
                tip = "Exit open";
            }
            else
            {
                tip = "Exit locked";
            }
        }

        if (lightNum == 3)
        {
            tip = "Exit open - reach the exit";
        }

        hintText.text = tip;
    }

    public bool CanEatPellet()
    {
        if (lightNum == 3)
        {
            return true;
        }
        else if (energy < 20)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddEnergy()
    {
        if (lightNum < 3 && energy < 20)
        {
            energy = energy + 1;
            ShowNumber();
        }
    }

    public bool CheckExit(Vector3 playerPoint)
    {
        if (lightNum < 3)
        {
            return false;
        }

        // 到出口格点才结束
        if (playerPoint.x == exitPoint.transform.position.x &&
            playerPoint.y == exitPoint.transform.position.y)
        {
            game.EndGame();
            return true;
        }

        return false;
    }

    void ShowNumber()
    {
        energyText.text = "Energy: " + energy + "/20";
        beaconText.text = "Beacons: " + lightNum + "/3";
    }
}