using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UI.Extensions;
using System.Collections;
using System.Collections.Generic;

public class Basketball : MonoBehaviour 
{
    public static Basketball Instance;

    public GameObject basketballRingParentObject;
    public GameObject blockingHandObject;
    public GameObject basketballPrefab;
    public Transform basketballPosition;
    public ParticleSystem readyGoParticle;
    public Cloth netCloth;
    public float roundTime = 60;
    public float forwardForce = 5;
    public float upwardForce = 9;
    public float ballForwardSpinStrength = 2;

    public float ringTweenTime = 5.0f;
    public float blockingHandTweenTime = 5.0f;

    public CanvasGroup readyCanvas;
    public Text readyText;
    public Text gameTimeText;
    public Text shotCountText;

    public AudioSource AS_back;
    public AudioSource AS_shot;
    public AudioSource AS_win;

    public List<float> shotTimes = new List<float>();

    GameObject _basketball;
    float _gameTimer;
    int _score = 0;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _gameTimer = roundTime;
        Calibration.Instance.currentGameParentObject = transform.root.gameObject;

        // medium difficulty introduces the blocking hand
        if (UIManager.Instance.currentDifficulty > 0)
        {
            blockingHandObject.SetActive(true);
            iTween.MoveTo(blockingHandObject, iTween.Hash("path", iTweenPath.GetPath("blocking_hand_path"), "time", blockingHandTweenTime, "easetype", "easeInOutQuad", "looptype", "pingPong"));
            iTween.RotateTo(blockingHandObject, iTween.Hash("z", -30, "time", blockingHandTweenTime, "easetype", "easeInOutQuad", "looptype", "pingPong"));
        }

        // hard difficulty tweens the ring
        if (UIManager.Instance.currentDifficulty == 2)
        {
            basketballRingParentObject.transform.localPosition += new Vector3(5, 0, 0);
            iTween.MoveTo(basketballRingParentObject, iTween.Hash("x", -4, "time", ringTweenTime, "islocal", true, "easetype", "linear", "looptype", "pingPong"));
        }
    }

    void Update()
    {
        if (GameManager.Instance.GameInProgress)
        {
            _gameTimer -= Time.deltaTime;
            gameTimeText.text = _gameTimer.ToString("0.00");

            if (_gameTimer <= 0)
            {
                AS_back.mute = true;
                AS_win.loop = true;
                AS_win.Play();
                GameOver();                
            }
        }
    }

    void GameOver()
    {
        bool newRecord = false;
        if (_score > UIManager.Instance.basketballHighScores[UIManager.Instance.currentDifficulty])
            newRecord = true;

        GameManager.Instance.GameEnding(Time.time, newRecord);
        GameManager.Instance.GameOver(true, newRecord ? "NEW RECORD! " + _score + " shots!" : "Great effort - You made " + _score + " shots!");

        if (newRecord)
        {
            UIManager.Instance.basketballHighScores[UIManager.Instance.currentDifficulty] = _score;
            UIManager.Instance.UpdateTopResultTexts();
        }
    }

    public void StartGame()
    {
        StartCoroutine(ReadyGo());
    }

    IEnumerator ReadyGo()
    {
        yield return new WaitForSeconds(3);

        CreateBasketball();
        GameManager.Instance.GameStarting(Time.time);
    }

    void CreateBasketball()
    {
        _basketball = (GameObject)Instantiate(basketballPrefab, basketballPosition.position, basketballPrefab.transform.rotation);
        _basketball.transform.parent = transform.root;
        ClothSphereColliderPair cscp = new ClothSphereColliderPair(_basketball.GetComponent<SphereCollider>());
        netCloth.sphereColliders = new ClothSphereColliderPair[] { cscp } ;
    }

    public void Shoot()
    {
        GameManager.Instance.locked = true;
        Rigidbody rigid = _basketball.GetComponent<Rigidbody>();
        rigid.AddForce(new Vector3(0, upwardForce, forwardForce));
        rigid.AddTorque(Vector3.right * ballForwardSpinStrength, ForceMode.Impulse);
        rigid.useGravity = true;
        StartCoroutine(CreateBasketballInSeconds(2));
        AS_shot.Play();
        AS_back.volume *= 0.9f;
    }

    public void Score()
    {
        if (GameManager.Instance.GameInProgress)
        {
            _score++;
            shotCountText.text = _score.ToString();
            shotTimes.Add(Time.time);
        }
    }

    IEnumerator CreateBasketballInSeconds(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);

        if (!GameManager.Instance.GameInProgress)
            yield break;

        CreateBasketball();
        GameManager.Instance.locked = false;
    }

    public void ExitGame()
    {
        Calibration.Instance.ExitCurrentGame();
    }
}
