using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine.EventSystems;
//using UnityEngine.SceneManagement;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Camera mainCamera;
    public EventSystem eventSystem;
    public CanvasGroup uiCanvasGroupLogin;
    public CanvasGroup uiCanvasGroupGame;
    public CanvasGroup uiCanvasGroupCalibration;
    public GameObject calibrationParentObject;
    public GameObject[] calibrationStepObjecs;
    public GameObject worldUiCavasObject;

    public GameObject login_loadingPanel;
    public GameObject login_loadingImage;

    public GameObject login_errorPanel;
    public Text errorPanel_text;

    //public GameObject login_autoLoginPanels;
    public Toggle autoLoginToggle;
    public GameObject login_newUserPanel;
    public InputField newUserPanel_usernameInput;
    public InputField newUserPanel_password1Input;
    public InputField newUserPanel_password2Input;
    public GameObject login_loginPanel;
    public InputField loginUserPanel_usernameInput;
    public InputField loginUserPanel_passwordInput;

    public Image tabButtonLogin;
    public Image tabButtonNewUser;
    public Color selectedColour;
    public Color disabledColour;

    public GameObject padlockObject;

    public GameObject centerProfileCircle;
    public Text usernameText;
    public Text[] topScoreTexts;

    public Text userLoggedInText;
    public GameObject skipErrorObject;

    public GameObject userPanelLoggedIn;
    public GameObject userPanelNotLoggedIn;

    public Button[] gameButtons;
    public GameObject highScoreParentObject;
    public Image[] tickDifficultyImages;
    public Sprite tickSprite;
    public Sprite crossSprite;
    public int[] popPopHighScores = new int[3];
    public int[] basketballHighScores = new int[3];
    public float[] hockeyHighScores = new float[3];
    public int currentDifficulty = 0;
    public GameObject notConnectedGameErrorObject;
    public Toggle setMinToggle;
    public Toggle setMaxToggle;

    public Color tickColour;
    public Color xColour;

    public GameObject[] tweenScaleObjects;

    //public GameObject login_loggedInPanel;
    //public Text loggedInPanel_userDetailsText;

    ///////////////////////////////////////////////////////////////////////////////

    public bool SetUiShowState
    {
        set
        {
            if (value) { _fadingUiIn = true; _fadingUiOut = false; }
            else { _fadingUiOut = true; _fadingUiIn = false; }
        }
    }

    public string CurrentGameString { get { return _currentGame; } }

    bool _fadingUiOut;
    bool _fadingUiIn;
    string _currentGame = "";
    MD5CryptoServiceProvider md5Hash = new MD5CryptoServiceProvider();
    Color[] _gameButtonColours;
    Image[] _gameButtonIcons;
    Color[] _gameButtonIconUnselectedColours;

    ///////////////////////////////////////////////////////////////////////////////

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

        OnGameButtonPressed(gameButtons[0].gameObject);

        //foreach (GameObject tweenScaleObject in tweenScaleObjects)
        //    iTween.ScaleBy(tweenScaleObject, iTween.Hash("amount", Vector3.one * 1.05f, "time", 1f, "easetype", "linear", "looptype", "pingPong"));
    }

    void Update()
    {
        if (_fadingUiOut)
        {
            uiCanvasGroupGame.alpha -= 0.025f;
            if (uiCanvasGroupGame.alpha <= 0)
            {
                _fadingUiOut = false;
                uiCanvasGroupGame.gameObject.SetActive(false);
                worldUiCavasObject.SetActive(false);
                //SceneManager.LoadSceneAsync(_currentGame, LoadSceneMode.Additive);
                Application.LoadLevelAdditiveAsync(_currentGame);
            }
        }
        else if (_fadingUiIn)
        {
            if (!uiCanvasGroupGame.gameObject.activeSelf)
                uiCanvasGroupGame.gameObject.SetActive(true);
            worldUiCavasObject.SetActive(true);

            uiCanvasGroupGame.alpha += 0.025f;
            if (uiCanvasGroupGame.alpha >= 1)
            {
                _fadingUiIn = false;
                //Calibration.Instance.ShowMicrophones();
            }
        }

        if (login_loadingImage.activeInHierarchy)
            login_loadingImage.transform.Rotate(0, 0, -100 * Time.deltaTime);
    }

    public void SetLoginPanel(string show)
    {
        login_newUserPanel.SetActive(show == "newUser");
        login_loginPanel.SetActive(show == "loginUser");
        login_loadingPanel.SetActive(show == "loading");

        autoLoginToggle.gameObject.SetActive(show == "newUser" || show == "loginUser");

        if (show == "loginUser")
        {
            tabButtonLogin.color = selectedColour;
            tabButtonNewUser.color = disabledColour;
        }
        else if (show == "newUser")
        {
            tabButtonLogin.color = disabledColour;
            tabButtonNewUser.color = selectedColour;
        }
    }

    public void SetAccountExpiredStatus(bool hasExpired)
    {
        padlockObject.SetActive(hasExpired);
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    public void NewUser()
    {
        // check the two passwords match each other
        if (newUserPanel_password1Input.text != newUserPanel_password2Input.text)
        {
            StartCoroutine(ShowLoginError("passwords do not match!"));
            return;
        }
        else if (newUserPanel_password1Input.text.Length < 5)
        {
            StartCoroutine(ShowLoginError("your password must be at least 5 characters long!"));
            return;
        }
        else if (newUserPanel_usernameInput.text.Length < 3)
        {
            StartCoroutine(ShowLoginError("your username needs to be at least 3 characters long!"));
            return;
        }

        SetLoginPanel("loading");

        NetworkManager.Instance.NewUser(newUserPanel_usernameInput.text, newUserPanel_password1Input.text);
    }

    public void LoginUser()
    {
        if (loginUserPanel_usernameInput.text.Length < 3)
        {
            StartCoroutine(ShowLoginError("usernames are at least 3 characters long!"));
            return;
        }
        else if (loginUserPanel_passwordInput.text.Length == 0)
        {
            StartCoroutine(ShowLoginError("you haven't entered a password!"));
            return;
        }

        SetLoginPanel("loading");
        string sPassword = GetMd5Hash(loginUserPanel_passwordInput.text);

        // store in playerprefs if autologin is checked
        if (autoLoginToggle.isOn)
        {
            PlayerPrefs.SetString("username", loginUserPanel_usernameInput.text);
            PlayerPrefs.SetString("password", sPassword);
        }

        NetworkManager.Instance.LoginUser(loginUserPanel_usernameInput.text, sPassword);
    }

    public void Logout()
    {
        NetworkManager.Instance.currentUserId = -1;
        NetworkManager.Instance.currentUserName = "";
        NetworkManager.Instance.isAdmin = false;

        loginUserPanel_passwordInput.text = "";
        newUserPanel_password1Input.text = "";
        newUserPanel_password2Input.text = "";

        if (skipErrorObject.activeSelf)
            skipErrorObject.SetActive(false);

        uiCanvasGroupCalibration.gameObject.SetActive(false);
        uiCanvasGroupCalibration.alpha = 0;
        uiCanvasGroupGame.gameObject.SetActive(false);
        uiCanvasGroupGame.alpha = 0;
        uiCanvasGroupLogin.gameObject.SetActive(true);

        SetLoginPanel("loginUser");
        PlayerPrefs.SetInt("auto_login", 0);
    }

    public IEnumerator ShowLoginError(string msg)
    {
        StopCoroutine("ShowLoginError");
        errorPanel_text.text = msg;
        login_errorPanel.transform.localScale = Vector3.zero;
        login_errorPanel.SetActive(true);
        iTween.ScaleTo(login_errorPanel, iTween.Hash("scale", Vector3.one, "time", 1.50f, "easetype", "easeInOutElastic"));

        yield return new WaitForSeconds(5);

        login_errorPanel.SetActive(false);
    }

    public string GetMd5Hash(string input)
    {
        return GetMd5Hash(Encoding.UTF8.GetBytes(input));
    }

    private string GetMd5Hash(byte[] input)
    {
        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(input);

        // Create a new Stringbuilder to collect the bytes and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data and format each one as a hexadecimal string. 
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string. 
        return sBuilder.ToString();
    }

    public void LoggedIn(int user_id, string username)
    {
        //userPanelLoggedIn.SetActive(user_id != -1);
        //userPanelNotLoggedIn.SetActive(user_id == -1);

        if (user_id != -1)
            usernameText.text = username;
            //userLoggedInText.text = "You are logged in as " + username;

        uiCanvasGroupLogin.gameObject.SetActive(false);
        _fadingUiIn = true;
    }

    public void OnSkipLoginButtonPressed()
    {
        skipErrorObject.transform.localScale = Vector3.zero;
        skipErrorObject.SetActive(true);
        iTween.ScaleTo(skipErrorObject, iTween.Hash("scale", Vector3.one, "time", 1.50f, "easetype", "easeInOutElastic"));
    }

    public void OnSkipOkButtonPresed()
    {
        LoggedIn(-1, "");
    }

   

    void SteadyCenterCircle()
    {
        //iTween.RotateTo(centerProfileCircle, Vector3.zero, 1.0f);
    }

    public void OnGameButtonPressed(GameObject button)
    {
        //if (microphone.Instance.minangle < microphone.Instance.maxangle && microphone.Instance.maxangle != 360)
        //{
            //iTween.StopByName("gameButtonTweenScale");

            for (int i = 0; i < gameButtons.Length; i++)
            {
                int index = System.Array.IndexOf(gameButtons, button.GetComponent<Button>());
                bool selected = i == index;
                gameButtons[i].GetComponent<Image>().color = selected ? Color.white : _gameButtonColours[i];
                _gameButtonIcons[i].color = selected ? Color.white : _gameButtonIconUnselectedColours[i];
                gameButtons[i].transform.localScale = Vector3.one;

                //if (selected)
                //    iTween.ScaleBy(gameButtons[i].gameObject, iTween.Hash("name", "gameButtonTweenScale", "amount", Vector3.one * 1.05f, "time", 1f, "easetype", "linear", "looptype", "pingPong"));
            }

            if (button.name.Contains("pop_pop"))
            {
                _currentGame = "pop_pop";
                eventSystem.SetSelectedGameObject(gameButtons[0].gameObject);
            }
            else if (button.name.Contains("basketball"))
            {
                _currentGame = "basketball";
                eventSystem.SetSelectedGameObject(gameButtons[1].gameObject);
            }
            else if (button.name.Contains("hockey"))
            {
                _currentGame = "hockey";
                eventSystem.SetSelectedGameObject(gameButtons[2].gameObject);
            }

            UpdateTopResultTexts();
        //}
    }

    public void OnDifficultyButtonPressed(GameObject difficultyButton)
    {
        // don't do anything if account has expired
        if (NetworkManager.Instance.accountExpiry < DateTime.UtcNow)
            return;

#if !UNITY_EDITOR
        if (!BytesTerminal.Instance.fullyCalibrated)
        {
            iTween.Stop(notConnectedGameErrorObject);
            iTween.ScaleTo(notConnectedGameErrorObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", 0.5f, "easetype", "linear", "oncomplete", "ScaleDownError", "oncompletetarget", gameObject));
            return;
        }
#endif

        if (difficultyButton.name.Contains("easy"))
            currentDifficulty = 0;
        else if (difficultyButton.name.Contains("medium"))
            currentDifficulty = 1;
        else if (difficultyButton.name.Contains("hard"))
            currentDifficulty = 2;

        // start game
        _fadingUiOut = true;
    }

    void ScaleDownError()
    {
        iTween.ScaleTo(notConnectedGameErrorObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", "linear"));
    }

    public void SetTopScores(Dictionary<string, object> topResults)
    {
        if (topResults == null)
        {
            // new user, set scores to 0
            for (int i = 0; i < popPopHighScores.Length; i++)
            {
                popPopHighScores[i] = 0;
                basketballHighScores[i] = 0;
                hockeyHighScores[i] = 0;
            }
        }
        else
        {
            Dictionary<string, object>[] popPopHighestLevels = (Dictionary<string, object>[])topResults["pop_pop"];
            popPopHighScores[0] = int.Parse((string)popPopHighestLevels[0]["easy_top"]);
            popPopHighScores[1] = int.Parse((string)popPopHighestLevels[0]["medium_top"]);
            popPopHighScores[2] = int.Parse((string)popPopHighestLevels[0]["hard_top"]);

            object[] basketballResults = (object[])topResults["basketball"];
            for (int i = 0; i < basketballResults.Length; i++)
                basketballHighScores[i] = basketballResults[i] is string ? int.Parse((string)basketballResults[i]) : 0;

            object[] hockeyResults = (object[])topResults["hockey"];
            for (int i = 0; i < hockeyResults.Length; i++)
                hockeyHighScores[i] = hockeyResults[i] is string ? float.Parse((string)hockeyResults[i]) : 0;
        }

        UpdateTopResultTexts();
    }

    public void UpdateTopResultTexts()
    {
        if (_currentGame == "")
        {
            OnGameButtonPressed(gameButtons[0].gameObject);
            return;
        }

        //foreach (Text tex in topScoreTexts)
        //    tex.gameObject.SetActive(_currentGame == "basketball");
        //highScoreParentObject.SetActive(_currentGame == "basketball");

        StopAllCoroutines();

        for (int i = 0; i < tickDifficultyImages.Length; i++)
        {
            Image tickParentImage = tickDifficultyImages[i].transform.parent.GetComponent<Image>();
            if (_currentGame == "pop_pop")
            {
                tickDifficultyImages[i].sprite = popPopHighScores[i] > 0 ? tickSprite : crossSprite;
                tickParentImage.color = popPopHighScores[i] > 0 ? tickColour : xColour;
                if (popPopHighScores[i] > 0)
                {
                    if (topScoreTexts[i].gameObject.activeInHierarchy)
                        StartCoroutine(StartTypewriterText(topScoreTexts[i], "------ level " + popPopHighScores[i].ToString(), 0.85f));
                    else
                        topScoreTexts[i].text = "------ level " + popPopHighScores[i].ToString();
                }
                else
                    topScoreTexts[i].text = "";
            }
            else if (_currentGame == "basketball")
            {
                tickDifficultyImages[i].sprite = basketballHighScores[i] > 0 ? tickSprite : crossSprite;
                tickParentImage.color = basketballHighScores[i] > 0 ? tickColour : xColour;
                StartCoroutine(StartTypewriterText(topScoreTexts[i], basketballHighScores[i] > 0 ? "-------" + (basketballHighScores[i] < 10 ? "-- " : " ") + basketballHighScores[i].ToString() : "", 0.85f));
            }
            else if (_currentGame == "hockey")
            {
                tickDifficultyImages[i].sprite = hockeyHighScores[i] > 0 ? tickSprite : crossSprite;
                tickParentImage.color = hockeyHighScores[i] > 0 ? tickColour : xColour;
                if (hockeyHighScores[i] > 0)
                    StartCoroutine(StartTypewriterText(topScoreTexts[i], "------- " + Mathf.RoundToInt(hockeyHighScores[i]).ToString() + " seconds", 0.85f));
                else
                    topScoreTexts[i].text = "";
            }
        }
    }

    IEnumerator StartTypewriterText(Text textObject, string newText, float time)
    {
        float timeStep = time / newText.Length;
        for (int i = 0; i < newText.Length; i++)
        {
            textObject.text = newText.Substring(0, i + 1);
            yield return new WaitForSeconds(timeStep);
        }
    }

    public void SetCalibrationScreen(int stage)
    {
        //for (int i = 0; i < calibrationStepObjecs.Length; i++)
        //    calibrationStepObjecs[i].SetActive(i == stage);

        uiCanvasGroupGame.gameObject.SetActive(false);
        //calibrationParentObject.SetActive(true);
        uiCanvasGroupCalibration.gameObject.SetActive(true);
        //Calibration.Instance.ShowMicrophones();
    }

    public void OnBackSettingButtonPressed()
    {
        //if (calibrationStepObjecs[0].activeSelf)
        //{
        //    uiCanvasGroupGame.gameObject.SetActive(true);
        //    calibrationParentObject.SetActive(false);
        //}
        //else
        //{
        //    for (int i = 1; i < calibrationStepObjecs.Length; i++)
        //        if (calibrationStepObjecs[i].activeSelf)
        //        {
        //            calibrationStepObjecs[i - 1].SetActive(true);
        //            calibrationStepObjecs[i].SetActive(false);
        //            i = calibrationStepObjecs.Length;
        //        }

        //}

        //Calibration.Instance.HideMicrophones();
        uiCanvasGroupCalibration.gameObject.SetActive(false);
        uiCanvasGroupGame.gameObject.SetActive(true);
    }

    public void OnStepButtonPressed(int code)
    {
        int nextstep = code / 10;
        if(nextstep<3)
        {
            calibrationStepObjecs[nextstep-1].SetActive(false);
            calibrationStepObjecs[nextstep].SetActive(true);
        }

    }
}
