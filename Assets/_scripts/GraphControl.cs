using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;
using System;
using Vectrosity;

public class Result
{
    public int id = -1;
    public int userId = -1;
    public string game;
    public int difficulty = 0;
    public bool win;
    public float minAngle;
    public float maxAngle;
    public float startTime;
    public float endTime;
    public List<float> actionTimes = new List<float>();
    public List<float> inputFrequencies = new List<float>();
    public List<float> inputFrequencyMaximums = new List<float>();
    public DateTime creationDate;
}

public class GraphControl : MonoBehaviour
{
    public static GraphControl Instance;

    List<Result> results = new List<Result>();
    Result topResult;
    public GameObject resultButtonPrefab;

    public Button[] gameButtons;

    public GameObject graphRoot;
    public GameObject graphPanel;
    public GameObject searchPanel;
    public GameObject adminPanel;
    public GameObject loadingIndicator;
    public GameObject zeroPos;
    public Image horizBarImage;
    public Image vertBarImage;
    public Text horizBarExtentText;
    public Text vertBatExtentText;
    public GameObject freqBlip;
    public GameObject resetLine;
    public GameObject backButton;
    public InputField userSearchInput;
    public Text searchedUsersNameText;

    public UILineRenderer uiLineRenderer;
    public UILineRenderer uiLineRendererActions;
    public UILineRenderer uiLineRendererSuccess;

    public GameObject basketballActionPrefab, runningManActionPrefab, popPopActionPrefab;

    public Transform listParentTransform;
    public GameObject selectedGameImage;

    public GameObject topResultsRoot;
    public Transform topResultsParent;
    public GameObject overallButton;
    public GameObject recentResultsRoot;
    public Transform recentResultsParent;
    public Text noResultsText;
    public GameObject userDoesNotExistPanel;

    public string SelectedGame { get { return _selectedGame; } }

    /////////////////////////////////////////////////////////////////////

    string _selectedGame = "";
    List<GameObject> _actionMarkers = new List<GameObject>();
    string _lastSearchedName = "";
    Color[] _gameButtonColours;
    Image[] _gameButtonIcons;
    Color[] _gameButtonIconUnselectedColours;

    //VectorLine graphVectorLine;

    /////////////////////////////////////////////////////////////////////

    void Awake()
    {
        Instance = this;
        _gameButtonColours = new Color[gameButtons.Length];
        _gameButtonIcons = new Image[gameButtons.Length];
        _gameButtonIconUnselectedColours = new Color[gameButtons.Length];
        for (int i = 0; i < gameButtons.Length; i++)
        {
            _gameButtonColours[i] = gameButtons[i].GetComponent<Image>().color;
            _gameButtonIcons[i] = gameButtons[i].transform.Find("Image - icon").GetComponent<Image>();
            _gameButtonIconUnselectedColours[i] = _gameButtonIcons[i].color;
        }

        GameButtonPressed(gameButtons[0].gameObject);
    }

    void Update()
    {
        if (loadingIndicator.activeInHierarchy)
            loadingIndicator.transform.Rotate(0, 0, -100 * Time.deltaTime);
    }

    public Result CreateResult(int id, int userId, string game, int difficulty, bool win, float minAngle, float maxAngle, float startTime, float endTime, List<float> actionTimes, List<float> inputFrequencies, List<float> inputFreqMaxes, DateTime creationDate)
    {
        Result result = new Result();

        result.id = id;
        result.userId = userId;
        result.game = game;
        result.difficulty = difficulty;
        result.win = win;
        result.minAngle = minAngle;
        result.maxAngle = maxAngle;
        result.startTime = startTime;
        result.endTime = endTime;
        result.actionTimes = actionTimes;
        result.inputFrequencies = inputFrequencies;
        result.inputFrequencyMaximums = inputFreqMaxes;
        result.creationDate = creationDate;

        results.Add(result);
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newResults"></param>
    /// <param name="type">0 = topResults, 1 = recentResults</param>
    public void AddResults(List<Result> newResults, int type)
    {
        loadingIndicator.SetActive(false);

        if (type == 0 && !topResultsRoot.activeSelf)
            topResultsRoot.SetActive(true);
        else if (type == 1 && !recentResultsRoot.activeSelf)
            recentResultsRoot.SetActive(true);

        results.AddRange(newResults);
        if (_selectedGame == newResults[0].game)
        {
            // update panel here
            foreach (Result newResult in newResults)
            {
                GameObject resultButton = (GameObject)Instantiate(resultButtonPrefab);
                resultButton.name = newResult.id.ToString();
                resultButton.GetComponentInChildren<Text>().text = newResult.creationDate.ToLongDateString() + " / " + newResult.creationDate.ToShortTimeString() + " / " + GetColouredDifficultyStringFromInt(newResult.difficulty) + " / " + (newResult.game != "basketball" ? (newResult.win ? "win" : "lose") : newResult.actionTimes.Count.ToString() + " shots");
                resultButton.transform.parent = type == 0 ? topResultsParent : recentResultsParent;

                resultButton.GetComponent<Button>().onClick.AddListener(delegate { ClickedResultsButton(resultButton); });
            }
        }
    }

    public void AddTopResult(Result result)
    {
        topResult = result;
        overallButton.SetActive(true);
    }

    public void ClickedTopResult()
    {
        ShowGraph(topResult, true);
    }

    public void ClickedResultsButton(GameObject button)
    {
        int matchingId = int.Parse(button.name);
        foreach (Result result in results)
        {
            if (result.id == matchingId)
            {
                ShowGraph(result, true);
                return;
            }
        }
    }

    string GetColouredDifficultyStringFromInt(int difficulty)
    {
        if (difficulty == 0)
            return "<color=#7EE276FF><b>EASY</b></color>";
        else if (difficulty == 1)
            return "<color=#FFB876FF><b>MEDIUM</b></color>";
        else
            return "<color=#FF8676FF><b>HARD</b></color>";
    }

    public void BackButtonPressed()
    {
        if (!graphPanel.activeSelf || GameManager.Instance != null)
        {
            ExitGraph(PopManager.instance == null);
        }
        else
        {
            graphPanel.SetActive(false);
            searchPanel.SetActive(true);
            ResetGraph();
        }
    }

    public void ZeroResults(string msg)
    {
        loadingIndicator.SetActive(false);
        topResultsRoot.SetActive(false);
        overallButton.SetActive(false);
        recentResultsRoot.SetActive(false);
        noResultsText.text = msg;
        noResultsText.gameObject.SetActive(true);
    }

    public void ShowResults()
    {
        graphRoot.SetActive(true);
        graphPanel.SetActive(false);
        searchPanel.SetActive(true);
        adminPanel.SetActive(NetworkManager.Instance.isAdmin);

        FetchNewResults();
    }

    public void GameButtonPressed(GameObject button)
    {
        if (button.name == _selectedGame)
            return;

        bool skipNetworkCall = _selectedGame == "";

        ClearList();
        results.Clear();
        _selectedGame = button.name;

        Vector2 pos = selectedGameImage.transform.localPosition;
        pos.y = button.transform.localPosition.y + 30;
        selectedGameImage.transform.localPosition = pos;

        for (int i = 0; i < gameButtons.Length; i++)
        {
            int index = Array.IndexOf(gameButtons, button.GetComponent<Button>());
            bool selected = i == index;
            gameButtons[i].GetComponent<Image>().color = selected ? Color.white : _gameButtonColours[i];
            _gameButtonIcons[i].color = selected ? Color.white : _gameButtonIconUnselectedColours[i];
            gameButtons[i].transform.localScale = Vector3.one;

            if (selected)
                iTween.ScaleBy(gameButtons[i].gameObject, iTween.Hash("name", "gameButtonTweenScale", "amount", Vector3.one * 1.05f, "time", 1f, "easetype", "linear", "looptype", "pingPong"));
            else
            {
                iTween.Stop(gameButtons[i].gameObject);
                gameButtons[i].transform.localScale = Vector3.one;
            }
        }

        if (!skipNetworkCall)
        {
            if (NetworkManager.Instance.currentUserName != searchedUsersNameText.text)
                FetchNewResults(searchedUsersNameText.text);
            else
                FetchNewResults();
        }
    }

    int ResultsOfGameStored(string game)
    {
        int count = 0;
        foreach (Result result in results)
        {
            if (result.game == game)
                count++;
        }
        return count;
    }

    public void MeButtonPressed()
    {
        results.Clear();
        ClearList();
        userSearchInput.text = "";
        searchedUsersNameText.text = NetworkManager.Instance.currentUserName;
        FetchNewResults();
    }

    public void FetchResultsWithUsername()
    {
        if (userSearchInput.text != _lastSearchedName)
        {
            results.Clear();
            ClearList();
        }

        FetchNewResults(userSearchInput.text);
    }

    void FetchNewResults(string username = "")
    {
        userDoesNotExistPanel.SetActive(false);
        noResultsText.gameObject.SetActive(false);
        loadingIndicator.SetActive(true);
        topResultsRoot.SetActive(false);
        overallButton.SetActive(false);
        recentResultsRoot.SetActive(false);
        NetworkManager.Instance.GetResultData(NetworkManager.Instance.currentUserId, username, _selectedGame, -1, ResultsOfGameStored(_selectedGame));
        _lastSearchedName = username;
    }

    void ClearList()
    {
        foreach (Transform child in topResultsParent)
            Destroy(child.gameObject);
        foreach (Transform child in recentResultsParent)
            Destroy(child.gameObject);
    }

    public void ShowUserDoesNotExist()
    {
        userDoesNotExistPanel.transform.localScale = Vector3.zero;
        userDoesNotExistPanel.SetActive(true);
        iTween.ScaleTo(userDoesNotExistPanel, iTween.Hash("scale", Vector3.one, "time", 1.50f, "easetype", "easeInOutElastic"));
    }

    public void ShowGraph(Result result, bool showBackArrow, bool closeAdminPanel = false)
    {
        graphRoot.SetActive(true);
        searchPanel.SetActive(false);
        graphPanel.SetActive(true);
        //backButton.SetActive(showBackArrow);

        if (closeAdminPanel && adminPanel.activeSelf)
            adminPanel.SetActive(false);

        float totalHeight = 390;
        float totalWidth = 530;
        // x axis extent is total time
        float totalTime = result.endTime - result.startTime;
        float horizBySecond = totalWidth / totalTime;

        List<float> actionTimes = result.actionTimes;//new List<float>(GetCurrentGameActionTimes());

        // y axis extent is total amount of steps taken
        float totalActions = actionTimes != null ? actionTimes.Count : 0;
        uiLineRendererActions.gameObject.SetActive(totalActions > 0);

        if (result != topResult)
        {
            //Generating information:   
            //domain:
            float[] temp = result.inputFrequencies.ToArray();
            int input_count = result.inputFrequencies.Count;
            Array.Sort(temp);
            int domain = (int)(temp[input_count - 1] - temp[0]);
            //max up and down speed:
            temp = result.inputFrequencies.ToArray();
            for (int i = 0; i < input_count - 1; i++)
                temp[i] = temp[i + 1] - temp[i];
            temp[input_count - 1] = 0;
            Array.Sort(temp);

            float upspeed = 0;
            for (int i = 1; i < input_count / 30; i++)
                upspeed += temp[input_count - i];
            upspeed /= ((input_count / 30) - 2);
            upspeed /= horizBySecond;

            float downspeed = 0;
            for (int i = 0; i < input_count / 30; i++)
                downspeed += temp[i];
            downspeed /= ((input_count / 30) - 1);
            downspeed /= horizBySecond;

            horizBarExtentText.text = totalTime.ToString("00.00");
            vertBatExtentText.text = "Movement domain: " + domain.ToString() + " Deg " +
                                     "-- Initial end angle: " + ((int)(result.inputFrequencyMaximums[0])).ToString() + " Deg " +
                                     "-- Final end angle: " + ((int)(result.inputFrequencyMaximums[result.inputFrequencyMaximums.Count - 1])).ToString() + " Deg \n" +
                                     "Avarage upward speed:" + upspeed.ToString("0.00") + " Deg/Sec " +
                                     "-- Avarage downward speed:" + downspeed.ToString("0.00") + " Deg/Sec \n " +
                                     "Total rewards: " + totalActions.ToString();
        }

        if (totalActions > 0)
        {
            uiLineRendererActions.Points = new Vector2[(int)totalActions];

            int actionIndex = 1;
            foreach (float actionTime in actionTimes)
            {
                float xPosition = (actionTime - result.startTime) * horizBySecond;
                float yPosition = (actionIndex / totalActions) * totalHeight;

                uiLineRendererActions.Points[actionIndex - 1] = new Vector2(xPosition, yPosition);

                GameObject blip = (GameObject)Instantiate(GetGraphBlipForGame(result.game), Vector2.zero, Quaternion.identity);
                blip.transform.parent = graphPanel.transform;
                blip.transform.localPosition = (Vector2)zeroPos.transform.localPosition + new Vector2(xPosition, yPosition);
                _actionMarkers.Add(blip);

                actionIndex++;
            }
        }

        // now work out frequency points
        uiLineRenderer.Points = new Vector2[result.inputFrequencies.Count];//microphone.Instance.frequencies.Count];
        float freqEveryStep = totalWidth / result.inputFrequencies.Count;//microphone.Instance.frequencies.Count;
        int freqIndex = 0;
        foreach (float freq in result.inputFrequencies)//microphone.Instance.frequencies)
        {
            float xPosition = (freqIndex + 1) * freqEveryStep;
            float yPosition = Mathf.InverseLerp(result.minAngle, result.maxAngle, freq) * totalHeight;
            uiLineRenderer.Points[freqIndex] = new Vector2(xPosition, yPosition);
            freqIndex++;
        }

        //inputFreqData.myStupidList.Clear();
        //inputFreqData.myStupidList.AddRange(uiLineRenderer.Points);

        if (uiLineRendererSuccess != null)
        {
            //Vector2[] linePoints = new Vector2[result.inputFrequencyMaximums.Count];

            // show success line
            uiLineRendererSuccess.Points = new Vector2[result.inputFrequencyMaximums.Count];//microphone.Instance.maxAngles.Count];
            freqIndex = 0;
            foreach (float angle in result.inputFrequencyMaximums)//microphone.Instance.maxAngles)
            {
                float xPosition = (freqIndex + 1) * freqEveryStep;
                float yPosition = Mathf.InverseLerp(result.minAngle, result.maxAngle, angle) * totalHeight * 0.9f;
                uiLineRendererSuccess.Points[freqIndex] = new Vector2(xPosition, yPosition);

                //linePoints[freqIndex] = new Vector2(xPosition, yPosition);

                freqIndex++;
            }

            //// Make a VectorLine object using the above points and the default material, with a width of 2 pixels
            //VectorLine line = new VectorLine("Line", linePoints, null, 2.0f, LineType.Continuous);

            //// Draw the line
            //line.Draw();
        }

        if (result == topResult)
            resetLine.SetActive(false);
        else
        {
            // and set reset bar height
            resetLine.SetActive(true);
            float resetY = 0.10f * totalHeight;
            if (resetLine != null)
                resetLine.transform.localPosition = new Vector2(resetLine.transform.localPosition.x, horizBarImage.transform.localPosition.y + resetY);
        }

        graphPanel.SetActive(true);
    }

    void ResetGraph()
    {
        foreach (GameObject obj in _actionMarkers)
            Destroy(obj);

        _actionMarkers.Clear();
    }

    GameObject GetGraphBlipForGame(string game)
    {
        if (game == "running_man")
            return runningManActionPrefab;
        else if (game == "basketball")
            return basketballActionPrefab;
        else
            return popPopActionPrefab;
    }

    public void ExitGraph(bool exitGame = true)
    {
        if (GameManager.Instance != null && exitGame)
            GameManager.Instance.ExitGame();

        ResetGraph();
        noResultsText.gameObject.SetActive(false);
        graphRoot.SetActive(false);
        ClearList();
    }


}
