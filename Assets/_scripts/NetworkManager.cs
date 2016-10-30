using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class NetworkManager : MonoBehaviour 
{
    public static NetworkManager Instance;

    public string serviceUrl = "gateway.php";
    public string appServerAddress = "rhar.cloudapp.net";
    public string protocol = "https://";
    public string appVersion = "";

    public int currentUserId;
    public string currentUserName;
    public bool isAdmin = false;

    /////////////////////////////////////////////////////////////////////////////////////////

    string _applicationServerAddress = "";
    string _serviceUrl = "";
    bool _requestInProgress;
    float _lastQueryTime;

    /////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {
        Instance = this;

        _applicationServerAddress = protocol + appServerAddress + "/";
        _serviceUrl = _applicationServerAddress + serviceUrl;
    }

    void Start()
    {
        bool autoLogin = PlayerPrefs.GetInt("auto_login", 0) == 1;
        UIManager.Instance.autoLoginToggle.isOn = autoLogin;
        if (autoLogin)
            AutoLogin();
    }

    void AutoLogin()
    {
        string username = PlayerPrefs.GetString("username");
        string password = PlayerPrefs.GetString("password");
        UIManager.Instance.SetLoginPanel("loading");
        LoginUser(username, password);
    }

    public void NewUser(string username, string password)
    {
        WWWForm formRequest = new WWWForm();

        formRequest.AddField("username", username);
        formRequest.AddField("password", password);
        formRequest.AddField("method", "NewUser");

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "NewUser"));
    }

    public void LoginUser(string username, string password)
    {
        WWWForm formRequest = new WWWForm();

        formRequest.AddField("username", username);
        formRequest.AddField("password", password);
        formRequest.AddField("method", "LoginUser");

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "LoginUser"));
    }

    /// <summary>
    /// fetches results via id or username, depending on which argument you pass
    /// </summary>
    /// <param name="limit">maximum count of results to fetch - if you pass -1 there will be NO limit</param>
    public void GetResultData(int user_id, string username, string game, int limit, int offset)
    {
        WWWForm formRequest = new WWWForm();

        if (username == "")
            formRequest.AddField("user_id", user_id);
        else
            formRequest.AddField("username", username);

        formRequest.AddField("versionNum", appVersion);
        formRequest.AddField("game", game);
        formRequest.AddField("limit", limit);

        if (limit != -1)
            formRequest.AddField("offset", offset);

        formRequest.AddField("method", "GetResultData");

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "GetResultData"));
    }

    public void GetResultDataByUsername(string username, string game, int limit, int offset)
    {
        WWWForm formRequest = new WWWForm();

        formRequest.AddField("username", username);
        formRequest.AddField("game", game);
        formRequest.AddField("limit", limit);
        formRequest.AddField("offset", offset);
        formRequest.AddField("method", "GetResultData");

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "GetResultData"));
    }

    public void SaveResultData(string game, int difficulty, bool win, bool record, float minAngle, float maxAngle, float startTime, float endTime, float[] actionTimes, float[] inputFrequencies, float[] inputFrequencyMaximums)
    {
        WWWForm formRequest = new WWWForm();

        // convert numeric arrays to server friendly string format
        string iFreqs = string.Join(",", inputFrequencies.Select(x => x.ToString()).ToArray());
        string iMaxFreqs = string.Join(",", inputFrequencyMaximums.Select(x => x.ToString()).ToArray());

        formRequest.AddField("user_id", currentUserId);
        formRequest.AddField("game", game);
        formRequest.AddField("difficulty", difficulty);
        formRequest.AddField("win", win.ToString());
        formRequest.AddField("record", record.ToString());
        formRequest.AddField("min_angle", minAngle.ToString());
        formRequest.AddField("max_angle", maxAngle.ToString());
        formRequest.AddField("start_time", startTime.ToString());
        formRequest.AddField("end_time", endTime.ToString());
        formRequest.AddField("input_frequencies", iFreqs);
        formRequest.AddField("input_frequency_maximums", iMaxFreqs);
        formRequest.AddField("method", "SaveResultData");

        if (actionTimes != null)
        {
            string aTimes = string.Join(",", actionTimes.Select(x => x.ToString()).ToArray());
            formRequest.AddField("action_times", aTimes);
        }

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "SaveResultData"));
    }

    public void SetHighLevel(int currentDifficulty, int highLevel)
    {
        WWWForm formRequest = new WWWForm();
        formRequest.AddField("user_id", currentUserId);
        formRequest.AddField("difficultyColumnName", currentDifficulty == 0 ? "easy_top" : currentDifficulty == 1 ? "medium_top" : "hard_top");
        formRequest.AddField("highLevel", highLevel);

        formRequest.AddField("method", "SetHighLevel");

        StartCoroutine(PerformQuery(_serviceUrl, formRequest, "SetHighLevel"));
    }


    IEnumerator PerformQuery(string sUrl, WWWForm sQuery, string sMethod, float wwwTimeout = 20, int retries = 3, string clientData = null)
    {
        sUrl += "?" + appVersion + "-" + currentUserId + "-" + sMethod;

        int connectionAttempts = 0;
        bool connected = false;
        bool aborted = false;
        while (!connected && !aborted)
        {
            _requestInProgress = true;
            connectionAttempts++;
            if (connectionAttempts > 1)
                Debug.Log(System.DateTime.Now.ToString("HH:mm:ss") + " - Retry " + (connectionAttempts - 1) + " - PerformQuery method " + sMethod);

            _lastQueryTime = Time.time;

                WWW www = new WWW(sUrl, sQuery != null ? sQuery.data : null);

                float timer = 0;
                bool timedOut = false;
                while (!www.isDone)
                {
                    if (timer > wwwTimeout)
                    {
                        timedOut = true;
	                    Debug.Log("Timeout for request " + sMethod);
                        break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (timedOut)
                {
                    www.Dispose();

                    // abort or wait a frame and retry
                    if (connectionAttempts <= retries)
                        yield return null;
                    else
                        aborted = true;
                }
                else
                {
                    string responseData = (www.error == null ? " " + System.Text.Encoding.UTF8.GetString(www.bytes) : " ERROR: " + www.error);
                    //Debug.Log ("Response: " + responseData);
                    if (responseData.Contains("you have to let us know which method you want to call"))
                    {
                        // retry count is hardcoded for 'no method' workaround, as we want to retry with all types of request
                        if (connectionAttempts < 5 || connectionAttempts <= retries)
                        {
                            // wait two frames and retry
                            yield return null;
                            yield return null;
                        }
                        else
                        {
                            // abort
                            aborted = true;
                        }
                    }
                    else if (www.error != null)
                    {
	                    Debug.Log("www.error for request " + sMethod + ": " + www.error);

                        if (connectionAttempts <= retries)
                        {
                            // wait a frame and retry
                            yield return null;
                        }
                        else
                        {
                            // abort
                            aborted = true;
                        }
                    }
                    else
                    {
                        HandleReply(sMethod, responseData, www, clientData);
                        connected = true;
                    }
                }

            if (aborted)
            {
				Debug.Log ("Aborted request for " + sMethod);
                HandleReply(sMethod, null, null, clientData);
            }

            _requestInProgress = false;
        }
    }

    void HandleReply(string sMethod, string responseData, WWW www, string clientData)
    {
        Dictionary<string, object> aData = null;

        // www is null if we have timed out in performquery
        if (www == null)
        {
            //Debug.LogError("Request timed out");
            aData = new Dictionary<string, object>();
            aData.Add("request_type", sMethod);
            aData.Add("timeout", true);
            aData.Add("msg", "Request timed out");
            aData.Add("msgKey", "ServerMessage104");
            aData.Add("connectionError", true);
            aData.Add("result", 0);
        }
        else
        {
            string debugText = "HandleReply " + sMethod + responseData;
            if (debugText.Length > 256)
                debugText = debugText.Substring(0, 256) + "...";
            Debug.Log(debugText);

            if (www.error == null)
            {
                try
                {
                    // workaround for unicode characters in the 'surrogate pair range'
                    string cleanedText = CleanUnicode(responseData);
                    aData = Pathfinding.Serialization.JsonFx.JsonReader.Deserialize(cleanedText) as Dictionary<string, object>;
                }
                catch (System.Exception e)
                {
                    Debug.Log("Exception " + e.Message);
                    aData = null;
                }

                if (aData == null)
                {
                    // maybe it's url encoded?
                    try
                    {
                        Dictionary<string, object> urlData = new Dictionary<string, object>();
                        string[] urlParts = responseData.Trim().Split(new char[] { '&' });
                        foreach (string part in urlParts)
                        {
                            string[] keyAndValue = part.Split(new Char[] { '=' });
                            if (keyAndValue.Length == 2)
                                urlData.Add(keyAndValue[0], keyAndValue[1]);
                        }
                        aData = urlData;
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log("Exception (url encoding attempt) " + e.Message);
                    }
                }

                if (aData == null)
                {
                    aData = new Dictionary<string, object>();
                    aData.Add("request_type", sMethod);
                    //aData.Add("msg", "Null result from json decode - raw text " + www.text);
                    string tmpMsg = "Null result from json decode - raw text: ";
                    aData.Add("msg", tmpMsg + responseData);
                    aData.Add("result", 0);
                }
                else
                {
                    aData.Add("request_type", sMethod);
                    if (!aData.ContainsKey("result"))
                    {
                        aData.Add("result", 1);
                    }
                }
            }
            else
            {
                aData = new Dictionary<string, object>();
                aData.Add("request_type", sMethod);
                string sErrorToLower = responseData.ToLower();

                if (sErrorToLower.Contains("resolve host") || sErrorToLower.Contains("offline") || sErrorToLower.Contains("timed out") || sErrorToLower.Contains("no network connection") || sErrorToLower.Contains("connect to host"))
                {
                    aData.Add("msg", "No network connection available.");
                    aData.Add("msgKey", "PopupContent5");
                    aData.Add("connectionError", true);

                }
                else
                {
                    string tmpMsg = "WWW error has occurred - ";
                    aData.Add("msg", tmpMsg + sErrorToLower);
                }
                aData.Add("result", 0);
            }
        }

        if (clientData != null)
            aData.Add("clientData", clientData);


        // HANDLING REPLIES //
        if (sMethod == "NewUser" || sMethod == "LoginUser")
        {
            if ((int)aData["result"] == 1)
            {
                string username = (string)aData["username"];
                int user_id = int.Parse((string)aData["user_id"]);
                if (aData.ContainsKey("admin"))
                {
                    string admin = (string)aData["admin"];
                    isAdmin = admin.Contains("t");
                }
                currentUserId = user_id;
                currentUserName = username;
                UIManager.Instance.LoggedIn(user_id, username);
                PlayerPrefs.SetInt("auto_login", UIManager.Instance.autoLoginToggle.isOn ? 1 : 0);

                GraphControl.Instance.searchedUsersNameText.text = currentUserName;
                BytesTerminal.Instance.LoggedIn();

                // set the top score displays
                if (aData.ContainsKey("top_results"))
                {
                    Dictionary<string, object> topResults = (Dictionary<string, object>)aData["top_results"];
                    UIManager.Instance.SetTopScores(topResults);
                }
                else
                    UIManager.Instance.SetTopScores(null);
            }
            else
            {
                string msg = (string)aData["msg"];
                Debug.Log("logging in user failed with msg - " + msg);

                StartCoroutine(UIManager.Instance.ShowLoginError(msg));

                UIManager.Instance.SetLoginPanel(sMethod == "NewUser" ? "newUser" : "loginUser");
            }
        }
        else if (sMethod == "GetResultData")
        {
            if ((int)aData["result"] == 1)
            {
                string gamename = (string)aData["game"];
                if (aData.ContainsKey("username_searched"))
                {
                    string username_searched = (string)aData["username_searched"];
                    if (gamename == GraphControl.Instance.SelectedGame)
                    {
                        GraphControl.Instance.searchedUsersNameText.text = username_searched;
                    }
                }

                if (aData.ContainsKey("topResults"))
                {
                    List<Result> topResults = new List<Result>();
                    Dictionary<string, object>[] results = (Dictionary<string, object>[])aData["topResults"];
                    foreach (Dictionary<string, object> matchingResult in results)
                        topResults.Add(ParseResult(matchingResult));
                    GraphControl.Instance.AddResults(topResults, 0);
                }

                if (aData.ContainsKey("recentResults"))
                {
                    List<Result> recentResults = new List<Result>();
                    Dictionary<string, object>[] results = (Dictionary<string, object>[])aData["recentResults"];
                    foreach (Dictionary<string, object> matchingResult in results)
                        recentResults.Add(ParseResult(matchingResult));
                    GraphControl.Instance.AddResults(recentResults, 1);
                }

                if (aData.ContainsKey("recordResults"))
                {
                    List<Result> recordResults = new List<Result>();
                    Dictionary<string, object>[] results = (Dictionary<string, object>[])aData["recordResults"];
                    Result topResult = new Result();
                    topResult.game = (string)results[0]["game"];
                    topResult.userId = int.Parse((string)results[0]["user_id"]);

                    // end time should be number of days between lowest/highest result
                    // we set this first
                    DateTime earliestDate = new DateTime(2099, 1, 1);
                    DateTime latestDate = new DateTime(1900, 1, 1);
                    float minAction = Mathf.Infinity;
                    float maxAction = 0f;
                    foreach (Dictionary<string, object> matchingResult in results)
                    {
                        string createdAtString = (string)matchingResult["created_at"];
                        DateTime creationDate = DateTime.Parse(createdAtString);
                        if (DateTime.Compare(creationDate, earliestDate) < 0)
                            earliestDate = creationDate;
                        if (DateTime.Compare(creationDate, latestDate) > 0)
                            latestDate = creationDate;

                        // TODO: scaling on the Y axis
                        //if (topResult.game == "basketball")
                        //{
                        //    // basketball goes by actiontimes (shots made)
                        //    string actionTimesString = (string)matchingResult["action_times"];
                        //    if (actionTimesString != "{}")
                        //    {
                        //        actionTimesString = actionTimesString.TrimStart('{');
                        //        actionTimesString = actionTimesString.TrimEnd('}');
                        //        int actionCount = actionTimesString.Split(',').Select(t => float.Parse(t)).ToList().Count;
                        //        if (actionCount < minAction)
                        //            minAction = actionCount;
                        //        if (actionCount > maxAction)
                        //            maxAction = actionCount;
                        //    }
                        //}
                        //else if (topResult.game == "hockey")
                        //{
                        //    // hockey goes by time
                        //    float length = float.Parse((string)matchingResult["end_time"]) - float.Parse((string)matchingResult["start_time"]);
                        //    if (length < minAction)
                        //        minAction = length;
                        //    if (length > maxAction)
                        //        maxAction = length;
                        //}
                        //else if (topResult.game == "pop_pop")
                        //{
                        //    // pop pop goes by levels, so if there is an entry here we can just add a top result (since it's a record result)
                        //}
                    }

                    topResult.earliestDate = earliestDate;
                    topResult.latestDate = latestDate;

                    List<float> actionTimes = new List<float>();
                    foreach (Dictionary<string, object> matchingResult in results)
                    {
                        // add the day of this result for horizontal positioning
                        string createdAtString = (string)matchingResult["created_at"];
                        DateTime creationDate = DateTime.Parse(createdAtString);
                        actionTimes.Add((creationDate - earliestDate).Days);
                    }

                    // get total amount of days between lowest/highest result
                    int totalDays = (latestDate - earliestDate).Days;
                    topResult.endTime = totalDays + 1;

                    actionTimes.Sort();
                    topResult.actionTimes = actionTimes;

                    GraphControl.Instance.AddTopResult(topResult);
                }
                else
                    GraphControl.Instance.playMoreObject.SetActive(true);

                if (!aData.ContainsKey("topResults") && !aData.ContainsKey("recentResults"))
                {
                    Debug.Log("no results for search");

                    if (gamename == GraphControl.Instance.SelectedGame)
                    {
                        GraphControl.Instance.ZeroResults("There are no results here!");
                    }
                }
            }
            else
            {
                string msg = (string)aData["msg"];
                Debug.Log("error retrieving result data - " + msg);

                if (msg.Contains("does not exist"))
                {
                    // user doesn't exist with username given
                    GraphControl.Instance.ShowUserDoesNotExist();
                    GraphControl.Instance.ZeroResults("");
                }
                else
                    GraphControl.Instance.ZeroResults("There was an error getting data");
            }
        }
    }

    Result ParseResult(Dictionary<string, object> resultData)
    {
        Result result = new Result();
        result.id = int.Parse((string)resultData["id"]);
        result.game = (string)resultData["game"];
        result.minAngle = float.Parse((string)resultData["min_angle"]);
        result.maxAngle = float.Parse((string)resultData["max_angle"]);

        string actionTimesString = (string)resultData["action_times"];
        if (actionTimesString != "{}")
        {
            actionTimesString = actionTimesString.TrimStart('{');
            actionTimesString = actionTimesString.TrimEnd('}');
            result.actionTimes = actionTimesString.Split(',').Select(t => float.Parse(t)).ToList();
        }

        string inputFreqsString = (string)resultData["input_frequencies"];
        inputFreqsString = inputFreqsString.TrimStart('{');
        inputFreqsString = inputFreqsString.TrimEnd('}');
        result.inputFrequencies = inputFreqsString.Split(',').Select(t => float.Parse(t)).ToList();

        string inputFreqMaxesString = (string)resultData["input_frequency_maximums"];
        inputFreqMaxesString = inputFreqMaxesString.TrimStart('{');
        inputFreqMaxesString = inputFreqMaxesString.TrimEnd('}');
        result.inputFrequencyMaximums = inputFreqMaxesString.Split(',').Select(t => float.Parse(t)).ToList();

        result.userId = int.Parse((string)resultData["user_id"]);
        string createdAtString = (string)resultData["created_at"];
        result.creationDate = DateTime.Parse(createdAtString);
        string winString = (string)resultData["win"];
        result.win = winString.ToLower().Contains('t');
        result.startTime = float.Parse((string)resultData["start_time"]);
        result.endTime = float.Parse((string)resultData["end_time"]);
        result.difficulty = int.Parse((string)resultData["difficulty"]);

        return result;
    }

    // from http://forum.unity3d.com/threads/problem-converting-text-with-unescape-to-text-with-accents.108469/
    // and http://servicestack.googlecode.com/svn-history/r1296/trunk/Common/ServiceStack.Text/ServiceStack.Text/Json/JsonTypeSerializer.cs
    private string CleanUnicode(string textStore)
    {
        List<string> utfCodes = new List<string>();

        int tSystemStart = -1;
        while (true)
        {
            // Look for \\uXXXX chars
            tSystemStart = textStore.IndexOf("\\u", tSystemStart + 1);
            int tSystemEnd = tSystemStart+6;
            if ((tSystemStart > -1) && (tSystemEnd > tSystemStart)) {
                
                // Strip the system start/end elements
                int tStripStart = tSystemStart + 2;
                int tStripLength = tSystemEnd - tStripStart;
                utfCodes.Add(textStore.Substring(tStripStart, tStripLength));
            }
            else
            {
                break;
            }
        }

        foreach (string utfCode in utfCodes)
        {
            System.Int32 utf32 = System.Int32.Parse(utfCode, System.Globalization.NumberStyles.AllowHexSpecifier);

            if (0xD800 <= utf32 && utf32 <= 0xDFFF)
                textStore = textStore.Replace("\\u"+utfCode, ""); // replace
        }

        return textStore;
    }
}
