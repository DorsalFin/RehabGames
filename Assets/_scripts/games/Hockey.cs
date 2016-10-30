using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hockey : MonoBehaviour 
{
    public static Hockey Instance;

    public GameObject topPosition;
    public GameObject bottomPosition;
    public GameObject playerBasher;
    public GameObject cpuBasher;
    public GameObject puckPrefab;

    public AudioSource AS_back;
    public AudioSource AS_shot;
    public AudioSource AS_win;
    public AudioSource AS_point;
    public AudioSource AS_hit;

    public float ballSpeed = 50;
    public float basherSpeed = 1.5f;
    public float cpuBasherSpeed = 1000.0f;
    public float cpuHitChance = 30f;

    public Transform canvasParent;
    public Text countdownText;
    public Text playerScoreText;
    public Text cpuScoreText;
    public Text tt;
    public Text tt1;

    public int playerScore = 0;
    public int cpuScore = 0;
    public bool newRecord;

    /////////////////////////////////////////////////////////////////////////////

    GameObject _puck;
    Vector3 _currentPosition;
    Vector3 _targetPosition;
    Vector3 _currentCpuPosition;
    Vector3 _targetCpuPosition;
    Vector3 _target;
    float _bottomY;
    float _topY;
    bool _willHit;
    bool _positionsSet;
    float _cpuTargetX;
    RectTransform _cpuRectTransform;

    /////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        Instance = this;
        _cpuRectTransform = cpuBasher.GetComponent<RectTransform>();

        if (UIManager.Instance.currentDifficulty == 1)
        {
            ballSpeed = 350;
            cpuHitChance = 60;
        }
        else if (UIManager.Instance.currentDifficulty == 2)
        {
            ballSpeed = 500;
            cpuHitChance = 75;
        }
    }

    void Start()
    {
        _topY = topPosition.transform.position.y;
        _bottomY = bottomPosition.transform.position.y;
        _currentPosition = playerBasher.transform.position;
        _targetPosition = playerBasher.transform.position;
        _currentCpuPosition = cpuBasher.transform.position;
        _targetCpuPosition = cpuBasher.transform.position;

        _positionsSet = true;
    }

    void Update()
    {
        // lock the cpu basher to a rect x position of -25, the rect being anchored to screen right
        // that is 25 pixels in from the right hand screen edge
        if (_cpuRectTransform.anchoredPosition.x != -25)
        {
            _positionsSet = false;
            _cpuRectTransform.anchoredPosition = new Vector2(-25, _cpuRectTransform.anchoredPosition.y);
            _currentCpuPosition.x = cpuBasher.transform.position.x;
            _targetCpuPosition.x = cpuBasher.transform.position.x;
            _positionsSet = true;
        }

        if (_positionsSet)
        {
            _currentPosition = Vector2.Lerp(_currentPosition, _targetPosition, basherSpeed * Time.deltaTime);
            Mathf.Clamp(_currentPosition.y, _bottomY, _topY);

            if (_puck != null)
            {
                _targetCpuPosition.y = _puck.transform.position.y;
                if (!_willHit)
                    _targetCpuPosition.y = _puck.transform.position.y > cpuBasher.transform.position.y ? _targetCpuPosition.y - 85 : _targetCpuPosition.y + 85;
                Mathf.Clamp(_targetCpuPosition.y, _bottomY, _topY);

                ballSpeed += 0.03f;
            }

            _currentCpuPosition = Vector2.Lerp(_currentCpuPosition, _targetCpuPosition, cpuBasherSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
       if (_positionsSet)
       {
           playerBasher.GetComponent<Rigidbody2D>().MovePosition(_currentPosition);
           cpuBasher.GetComponent<Rigidbody2D>().MovePosition(_currentCpuPosition);
       }
    }

    /// <summary>
    /// randomise the cpu's hit chance eevry time the player hits it
    /// </summary>
    public void RandomiseCpuHit()
    {
        _willHit = Random.Range(0, 100) < cpuHitChance;
        //Debug.Log("cpu will " + (_willHit ? "HIT" : "MISS"));
    }

    public void UpdateFrequency(float amt)
    {
        float newYPos = Mathf.Lerp(_bottomY, _topY, amt);
        _targetPosition = new Vector2(playerBasher.transform.position.x, newYPos);
    }

    public void StartGame()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.text = "";

        if (!GameManager.Instance.GameInProgress)
            GameManager.Instance.GameStarting(Time.time);

        ShootNewPuck();
        AS_shot.Play();
        AS_back.volume *= 0.9f;
    }

    void ShootNewPuck()
    {
        if (_puck != null)
            Destroy(_puck);
        _puck = (GameObject)Instantiate(puckPrefab, Vector2.zero, Quaternion.identity);
        _puck.transform.parent = canvasParent;
        _puck.transform.localPosition = Vector2.zero;
        Vector3 forceDir = -Vector3.right;// Random.Range(0, 100) > 50 ? Vector3.right : -Vector3.right;
        _puck.GetComponent<Rigidbody2D>().velocity = forceDir * ballSpeed;
        _puck.GetComponent<Ball>().TurnOn();
    }

    public void Score(int playerNum)
    {
        Destroy(_puck);

        if (playerNum == 0)
        {
            AS_point.Play();
            playerScore++;
            ballSpeed *= 1.1f; // 20;
            playerScoreText.text = playerScore.ToString();
        }
        else 
        {
            AS_point.Play();
            cpuScore++;
            ballSpeed *= 1.1f; // 20;
            cpuScoreText.text = cpuScore.ToString();
        }

        if (playerScore == 4 || cpuScore == 4)
        {
            AS_back.mute = true;
            AS_win.Play();
            AS_win.loop = true;

            EndGame();
        }
        else
            StartCoroutine(Countdown());
    }

    void EndGame()
    {
        bool winner = playerScore == 4;

        if (winner && GameManager.Instance.endTime - GameManager.Instance.startTime > UIManager.Instance.hockeyHighScores[UIManager.Instance.currentDifficulty])
            newRecord = true;

        if (newRecord)
        {
            UIManager.Instance.hockeyHighScores[UIManager.Instance.currentDifficulty] = GameManager.Instance.endTime - GameManager.Instance.startTime;
            UIManager.Instance.UpdateTopResultTexts();
        }

        GameManager.Instance.GameEnding(Time.time, newRecord);
        GameManager.Instance.GameOver(winner, winner ? (newRecord ? "You beat your old record! Great job!" : "Well done! You beat the computer!") : "Oh no! The computer won!");
    }
    public void ExitGame()
    {
        Calibration.Instance.ExitCurrentGame();
    }
}
