using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
//using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance;

    public float timer;
    public float startTime;
    public float endTime;

    public bool locked;

    public CanvasGroup canvasFadeGroup;
    public GameObject gameOverPanel;
    public Text gameOverText;

    public GameObject[] currentBarParts;
    public Image currentValueFillImage;
    public Image resetMarkerImage;
    public Image targetMarkerImage;
    public Toggle hasResetToggle;

    ///////////////////////////////////////////////////////////////////////////////////

    public bool GameInProgress { get { return _gameInProgress; } }

    public bool WaitingForConnection { get { return _waitingOnReconnection; } }

    ///////////////////////////////////////////////////////////////////////////////////

    bool _gameInProgress;
    bool _fadingIn;
    bool _waitingOnReconnection;

    ///////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartGame();
        Calibration.Instance.currentGameParentObject = transform.root.gameObject;
    }

	void Update () 
    {
        if (_gameInProgress)
            timer += Time.deltaTime;

        if (_fadingIn)
        {
            canvasFadeGroup.alpha += 0.005f;
            if (canvasFadeGroup.alpha == 1)
            {
                _fadingIn = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Action();
    }

    string GetCurrentGameName()
    {
        if (RunningMan.Instance != null)
            return "running_man";
        else if (PopManager.instance != null)
            return "pop_pop";
        else if (Basketball.Instance != null)
            return "basketball";
        else if (Hockey.Instance != null)
            return "hockey";

        return "";
    }

    List<float> GetCurrentGameActionTimes()
    {
        if (RunningMan.Instance != null)
            return RunningMan.Instance.playerStepper.stepTimes;
        else if (Basketball.Instance != null)
            return Basketball.Instance.shotTimes;
        else if (PopManager.instance != null)
            return PlayerController.Instance.actionTimes;

        return null;
    }

    void StartGame()
    {
        if (RunningMan.Instance != null)
            RunningMan.Instance.StartGame();
        else if (Basketball.Instance != null)
            Basketball.Instance.StartGame();
        else if (Hockey.Instance != null)
            Hockey.Instance.StartGame();
    }

    public void GameStarting(float sTime)
    {
        startTime = sTime;
        _gameInProgress = true;
    }

    public void PauseUntilReconnection()
    {
        _gameInProgress = false;
        _waitingOnReconnection = true;
        Time.timeScale = 0f;
    }

    public void DeviceConnectionFound()
    {
        _gameInProgress = true;
        _waitingOnReconnection = false;
        Time.timeScale = 1f;
    }

    public void Action()
    {
        if (RunningMan.Instance != null)
            RunningMan.Instance.playerStepper.Step();

        if (PopManager.instance != null)
            PlayerController.Instance.Action();

        if (Basketball.Instance != null)
            Basketball.Instance.Shoot();

        if (hasResetToggle != null)
            hasResetToggle.isOn = false;
    }

    public void GameEnding(float eTime, bool record = false, bool showUI = true)
    {
        endTime = eTime;
        _gameInProgress = false;

        //if (UIManager.Instance.CurrentGameString != "pop_pop")
        if (showUI)
        {
            canvasFadeGroup.gameObject.SetActive(true);
            _fadingIn = true;

            foreach (GameObject part in currentBarParts)
                if (part != null)
                    part.SetActive(false);
        }

        bool win = CheckWinStatus() || record;

        // log these results on the server
        if (NetworkManager.Instance.currentUserId != -1)
        {
            // maximum array length of 100 elements
            if (BytesTerminal.Instance.thisGamesFrequencies.Count > 100)
            {
                int nth = (int)((float)BytesTerminal.Instance.thisGamesFrequencies.Count / 100f);
                BytesTerminal.Instance.thisGamesFrequencies = new List<float>(BytesTerminal.Instance.thisGamesFrequencies.Where((elem, idx) => idx % nth == 0));
            }
            if (BytesTerminal.Instance.thisGamesMaxAngles.Count > 100)
            {
                int nth = (int)((float)BytesTerminal.Instance.thisGamesMaxAngles.Count / 100f);
                BytesTerminal.Instance.thisGamesMaxAngles = new List<float>(BytesTerminal.Instance.thisGamesMaxAngles.Where((elem, idx) => idx % nth == 0));
            }

            NetworkManager.Instance.SaveResultData(GetCurrentGameName(), UIManager.Instance.currentDifficulty, win, record, BytesTerminal.Instance.minAngle, BytesTerminal.Instance.maxAngle, startTime, endTime, GetCurrentGameActionTimes() != null ? GetCurrentGameActionTimes().ToArray() : null, BytesTerminal.Instance.thisGamesFrequencies.ToArray(), BytesTerminal.Instance.thisGamesMaxAngles.ToArray());
        }
    }

    public void GameOver(bool winner, string text)
    {
        gameOverText.text = text;
        gameOverPanel.SetActive(true);
    }

    public void ExitGame(bool quitToMenu = true)
    {
        if (RunningMan.Instance != null)
            UIManager.Instance.mainCamera.enabled = true;

        //SceneManager.UnloadScene(GetCurrentGameName());

        if (quitToMenu)
            Calibration.Instance.ExitCurrentGame();

        Destroy(gameObject);
    }

    public void UpdateCurrentValue(float reset, float current, float max)
    {
        float fillAmt = Mathf.InverseLerp(reset, max, current);

        if (Hockey.Instance != null)
        {
            Hockey.Instance.UpdateFrequency(fillAmt);
        }
        else
        {
            currentValueFillImage.fillAmount = fillAmt;

            //float rep = max - reset;
            //float oneDiv = currentValueFillImage.rectTransform.rect.width / rep;

            //float resetVal = rep * 0.10f;
            //resetMarkerImage.transform.localPosition = new Vector2(currentValueFillImage.transform.localPosition.x + (resetVal * oneDiv), resetMarkerImage.transform.localPosition.y);

            //float stepVal = rep * 0.90f;
            //targetMarkerImage.transform.localPosition = new Vector3(currentValueFillImage.transform.localPosition.x + (stepVal * oneDiv), targetMarkerImage.transform.localPosition.y);
        }
    }

    public bool CheckWinStatus()
    {
        if (Basketball.Instance != null)
            return true;
        else if (RunningMan.Instance != null)
        {
            if (GetCurrentGameActionTimes().Count >= RunningMan.Instance.stepsUntilVictory)
                return true;
            else
                return false;
        }
        else if (Hockey.Instance != null)
        {
            if (Hockey.Instance.playerScore > Hockey.Instance.cpuScore)
                return true;
            else
                return false;
        }
        else if (PopManager.instance != null)
            return PopManager.instance.winStatus;

        return false;
    }

    public void DisplayResult()
    {
        //Result result = GraphControl.Instance.CreateResult(-1, NetworkManager.Instance.currentUserId, GetCurrentGameName(), UIManager.Instance.currentDifficulty, CheckWinStatus(), microphone.Instance.minangle, microphone.Instance.maxangle, startTime, endTime, GetCurrentGameActionTimes(), microphone.Instance.frequencies, microphone.Instance.maxAngles, System.DateTime.Now);
        Result result = GraphControl.Instance.CreateResult(-1, NetworkManager.Instance.currentUserId, GetCurrentGameName(), UIManager.Instance.currentDifficulty, CheckWinStatus(), BytesTerminal.Instance.minAngle, BytesTerminal.Instance.maxAngle, startTime, endTime, GetCurrentGameActionTimes(), BytesTerminal.Instance.thisGamesFrequencies, BytesTerminal.Instance.thisGamesMaxAngles, System.DateTime.Now);

        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        GraphControl.Instance.ShowGraph(result, false, true);
    }
}
