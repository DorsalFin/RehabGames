using UnityEngine;
using System.Collections;

public class RunningMan : MonoBehaviour 
{
    public static RunningMan Instance;

    public Camera playerCamera;
    public Stepper playerStepper;
    //public CameraMove[] cameraMoves;

    // difficulty scroll speeds
    public float easyScrollSpeed = 0.04f;
    public float mediumScrollSpeed = 0.08f;
    public float hardScrollSpeed = 0.1f;

    public GameObject playerObject;
    public float fadeInStep = 0.025f;
    public GameObject monsterObject;
    public Animator monsterAnimator;
    public float timeUntilMonsterAppears = 10.0f;
    public float timeBetweenMonsterSteps = 5.0f;
    public int stepsUntilVictory = 10;

    bool _monsterSpawned;
    Stepper _monsterStepper;
    float _roarAnimationTime;

    public AudioSource AS_back;
    public AudioSource AS_monster;
    public AudioSource AS_win;

    public bool newRecord = false;

    /////////////////////////////////////////////////////////////////////////////

    SpriteRenderer[] _bodyParts;
    float _currentAlpha = 0;

    /////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        Instance = this;
        _bodyParts = monsterAnimator.GetComponentsInChildren<SpriteRenderer>();
        UIManager.Instance.mainCamera.enabled = false;
    }

    public void StartGame()
    {
        StartCoroutine(StartRunning());
    }

    IEnumerator StartRunning()
    {
        yield return new WaitForSeconds(1);
        GameManager.Instance.GameStarting(Time.time);
        playerObject.SetActive(true);
    }

    void SpawnMonster()
    {
        AS_monster.Play();
        AS_back.volume /= 1.5f;
        GameManager.Instance.timer = 0;
        _monsterSpawned = true;
        StartCoroutine(FadeInMonster());

        //foreach (CameraMove cM in cameraMoves)
        //    cM.speed = UIManager.Instance.currentDifficulty == 0 ? easyScrollSpeed : UIManager.Instance.currentDifficulty == 1 ? mediumScrollSpeed : hardScrollSpeed;
    }

    IEnumerator FadeInMonster()
    {
        while (_currentAlpha < 1)
        {
            _currentAlpha += fadeInStep;

            foreach (SpriteRenderer bodyPart in _bodyParts)
                bodyPart.color = new Color(bodyPart.color.r, bodyPart.color.g, bodyPart.color.b, _currentAlpha);

            yield return null;
        }
    }

    public void FinishedStep(Stepper stepper)
    {
        if (!_monsterSpawned)
            SpawnMonster();

        if (stepper.acceptsInput)
        {
            // check if we've reached the edge of the screen
            Vector3 viewPos = playerCamera.WorldToViewportPoint(playerObject.transform.position);
            if (viewPos.x > 0.9f)
            {
                // end game in victory for player
                Debug.Log("Player has won the game!");
                GameManager.Instance.GameEnding(Time.time);//, true, "You made it! Well done!");

                // play animations
                playerStepper.Win();
                AS_back.mute = true;
                AS_monster.mute = true;
                AS_win.Play();

                if (GameManager.Instance.endTime - GameManager.Instance.startTime > UIManager.Instance.popPopHighScores[UIManager.Instance.currentDifficulty])
                    newRecord = true;

                if (newRecord)
                {
                    //UIManager.Instance.popPopHighScores[UIManager.Instance.currentDifficulty] = GameManager.Instance.endTime - GameManager.Instance.startTime;
                    //UIManager.Instance.UpdateTopResultTexts();
                }
            }
        }
    }

    public void MonsterCaughtPlayer()
    {
        if (GameManager.Instance.GameInProgress)
        {
            //foreach (CameraMove cM in cameraMoves)
            //    cM.speed = 0;

            monsterAnimator.Play("Roar");

            // end game in defeat for player
            Debug.Log("The monster caught the player!");
            GameManager.Instance.GameEnding(Time.time);//, false, "Oh no! The monster caught you!");

            // play animations
            playerStepper.Die();
            AS_back.mute = true;
            AS_monster.loop = true;
        }
    }
}
