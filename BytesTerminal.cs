using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BytesTerminal : MonoBehaviour
{
    public static BytesTerminal Instance;

    public GameObject q1, q2, q3, qdiff2;
    public GameObject arm, toparm, sp;
    public Text Angletext;
    Quaternion qq1, qq2, qq1_0, qq2_0, qq3, diff2, qqzero, qmin, qmax;
    bool ifcal;
    float currentAngle = 0f;
    char sensor;
    /// 
    Vector3 v3min, v3max, v3cur, v3cur_;
    Vector3 point;
    float angold;
    int dis_count = 0;
    int try_count = 0;
    int sent_count = 0;
    int sema = 0;

    public bool fullyCalibrated;

    // for result data
    public float minAngle = 0f;
    public float maxAngle = 0f;
    public float NewmaxAngle = 0f;
    public List<float> thisGamesFrequencies = new List<float>();
    public List<float> thisGamesMaxAngles = new List<float>();

    public Color connectedButtonColour;
    public Color connectedButtonHoverColour;
    public Color notConnectedButtonColour;
    public Color notConnectedButtonHoverColour;
    public Button connectButton;
    public Image connectButtonImage;
    public Text connectButtonText;
    bool _connectButtonStatus;
    string _lastNameEnteredToConnect = "";

    ///
    //int t='\n';
    public Vector2 scrollPosition = Vector2.zero;
    bool connected = false;// equals true when connected
    public InputField deviceNameInput;
    byte[] messageFromMC; //temporary string to hold BtConnector.read() value
    string controlData = ""; //will contain data from the plugin to check the status of the whole process

    List<string> messages = new List<string>();// messages from Microcontroller in bytes

    int labelHeight;//height of a single label inside the ScrollView
    int height;//ScrollView Height
    float speed;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //use one of the following two methods to change the default bluetooth module.
        //BtConnector.moduleMAC("00:13:12:09:55:17");
        //BtConnector.moduleName ("HC-05");
        //BtConnector.autoConnect(100);
        sensor = 'U'; // U=Unknown, I=IMU9250, B=Body, F=Felex.    
        height = (int)(Screen.height * 0.8f);
        labelHeight = (int)(0.06f * height);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        qqzero.w = 1;
        qqzero.x = qqzero.y = qqzero.z = 0;
        ifcal = false;
        angold = 0;
        point = Vector3.one;
        speed = 0.03f;
        InvokeRepeating("ontime", 0, speed);

    }

    void Update()
    {
        Angletext.text =
        "Sensor : " + sensor.ToString() +
        "\n Min angle : " + minAngle.ToString() +
        "\n CurrentAngle :" + currentAngle.ToString() +
        "\n Max angle : " + maxAngle.ToString() +
        "\n dis_count :" + dis_count.ToString() +
        "\n sema :" + sema.ToString() +
        "\n or.readControlData:" + BtConnector.readControlData().ToString() +
        "\n " + BtConnection.readControlData().Length.ToString() +
        "\n on.isConnected:" + BtConnection.isConnected().ToString();

        //"\nNewmaxAngle : " + NewmaxAngle.ToString();
    }
    /*
        //q1.transform.Rotate(0.1f, 0, 0.1f);
        //q2.transform.Rotate(-0.1f, 0, -0.1f);

        connected = BtConnector.isConnected();//check connection status
        if (BtConnector.isConnected() != _connectButtonStatus)
        {
            _connectButtonStatus = BtConnector.isConnected();
            connectButtonImage.color = _connectButtonStatus == true ? connectedButtonColour : notConnectedButtonColour;
            connectButtonText.text = _connectButtonStatus == true ? "connected!" : "connect";

            if (_connectButtonStatus == true)
            {
                PlayerPrefs.SetString("lastConnectedDeviceName", _lastNameEnteredToConnect);

                if (ifcal)
                    UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
            }
            else
            {
                UIManager.Instance.notConnectedGameErrorObject.SetActive(true);
                //UIManager.Instance.setMinToggle.isOn = false;
                //UIManager.Instance.setMaxToggle.isOn = false;
                //fullyCalibrated = false;

                // TODO : pause game and allow reconnection
                if (GameManager.Instance != null)
                    GameManager.Instance.ExitGame();
            }
        }

        if (BtConnector.isConnected() && BtConnector.available())//check connection status
        {
            //UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
            messageFromMC = BtConnector.readBuffer();//read bytes till a new line or a max of 100 bytes
            if (messageFromMC.Length > 0)
            {
                if (messageFromMC.Length == 16)
                {
                    sensor = 'I';// IMU6050, IMU 9250, etc.
                    qq1.w = (float)(messageFromMC[0] * 256 + messageFromMC[1]);// (10000.0f);
                    qq1.x = (float)(messageFromMC[2] * 256 + messageFromMC[3]);// (10000.0f);
                    qq1.y = (float)(messageFromMC[4] * 256 + messageFromMC[5]);// (10000.0f);
                    qq1.z = (float)(messageFromMC[6] * 256 + messageFromMC[7]);// (10000.0f);

                    qq2.w = (float)(messageFromMC[8] * 256 + messageFromMC[9]);// (10000.0f);
                    qq2.x = (float)(messageFromMC[10] * 256 + messageFromMC[11]);// (10000.0f);
                    qq2.y = (float)(messageFromMC[12] * 256 + messageFromMC[13]);// (10000.0f);
                    qq2.z = (float)(messageFromMC[14] * 256 + messageFromMC[15]);// (10000.0f);

                    if (qq1.w >= 32768)
                        qq1.w = qq1.w - 65536;

                    if (qq1.x >= 32768)
                        qq1.x = qq1.x - 65536;

                    if (qq1.y >= 32768)
                        qq1.y = qq1.y - 65536;

                    if (qq1.z >= 32768)
                        qq1.z = qq1.z - 65536;

                    if (qq2.w >= 32768)
                        qq2.w = qq2.w - 65536;

                    if (qq2.x >= 32768)
                        qq2.x = qq2.x - 65536;

                    if (qq2.y >= 32768)
                        qq2.y = qq2.y - 65536;

                    if (qq2.z >= 32768)
                        qq2.z = qq2.z - 65536;
                    //////////////////////////////////////////////////////////
                    if (qq1.w > 10000)
                        qq1.w -= 10000;
                    if (qq1.x > 10000)
                        qq1.x -= 10000;
                    if (qq1.y > 10000)
                        qq1.y -= 10000;
                    if (qq1.z > 10000)
                        qq1.z -= 10000;

                    if (qq2.w > 10000)
                        qq2.w -= 10000;
                    if (qq2.x > 10000)
                        qq2.x -= 10000;
                    if (qq2.y > 10000)
                        qq2.y -= 10000;
                    if (qq2.z > 10000)
                        qq2.z -= 10000;

                    qq1.w /= 10000;
                    qq1.x /= 10000;
                    qq1.y /= 10000;
                    qq1.z /= 10000;

                    qq2.w /= 10000;
                    qq2.x /= 10000;
                    qq2.y /= 10000;
                    qq2.z /= 10000;
                    float s1, s2;
                    s1 = (qq1.w * qq1.w) + (qq1.x * qq1.x) + (qq1.y * qq1.y) + (qq1.z * qq1.z);
                    s2 = (qq2.w * qq2.w) + (qq2.x * qq2.x) + (qq2.y * qq2.y) + (qq2.z * qq2.z);

                    if ((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100)
                        q1.transform.rotation = qq1;

                    if ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100)
                        q2.transform.rotation = qq2;

                    if (((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100) && ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100))
                    {
                        Angletext.text = "Not Set!";

                        if (ifcal)
                        {

                            q3.transform.rotation = qq3 = product(divideQbyR(qq2, qq2_0), qq1_0);
                            diff2 = divideQbyR(qq1, qq3);
                            //currentAngle = Quaternion.Angle(qqzero, diff2);
                            arm.transform.localRotation = diff2;//current position

                            v3cur = (toparm.transform.position - sp.transform.position);
                            v3cur_ = Vector3.ProjectOnPlane(v3cur, Vector3.Cross(v3max, v3min));
                            currentAngle = Vector3.Angle(v3min, v3cur_);

                            if (Mathf.Abs((Vector3.Angle(v3min, v3max) + Vector3.Angle(v3min, v3cur_)) - Vector3.Angle(v3max, v3cur_)) < 1)
                                currentAngle *= -1;
                            else if (Mathf.Abs(Vector3.Angle(v3min, v3max) + Vector3.Angle(v3min, v3cur_) + Vector3.Angle(v3max, v3cur_) - 360) < 1)
                                currentAngle *= -1;

                            currentAngle = (angold * 3 + currentAngle) / 4.0f;




                        }
                        else
                        {
                            q3.transform.rotation = qq2;
                            //q3.transform.rotation = qq2;
                        }

                    }
                }
                else if (messageFromMC.Length == 3 && messageFromMC[0] == 'F')
                {
                    sensor = 'F';// Felex/Bending sensors.
                    currentAngle = 256 * messageFromMC[1] + messageFromMC[2];
                    currentAngle = (angold * 3 + currentAngle) / 4.0f;

                    // arm.transform.localEulerAngles.Set(arm.transform.localEulerAngles.x, getFangle(currentAngle), arm.transform.localEulerAngles.z);
                    arm.transform.localRotation = Quaternion.Euler(0.0f, getFangle(currentAngle), 0.0f);

                }
                else if (messageFromMC.Length == 3 && messageFromMC[0] == 'B')
                {
                    sensor = 'B';// MyWare body sensor
                    currentAngle = 256 * messageFromMC[1] + messageFromMC[2];
                    currentAngle = (angold * 3 + currentAngle) / 4.0f;
                    if (fullyCalibrated)
                        arm.transform.localRotation = Quaternion.Euler(0.0f, (currentAngle - minAngle) * (90.0f / (maxAngle - minAngle)), 0.0f);


                }
                else
                {
                    sensor = 'U';
                    arm.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }


            }
            if (ifcal && GameManager.Instance != null)
            {

                // don't want to increase max angle in hockey or pop pop
                //if (currentAngle > maxAngle && (Hockey.Instance == null || PopManager.instance != null))
                //    maxAngle = currentAngle;

                // if there is a gamemmanger in scene, it means we're acting on input

                // update image of current value
                GameManager.Instance.UpdateCurrentValue(minAngle, currentAngle, maxAngle);

                // check if we've reset
                bool hasReset = GameManager.Instance.hasResetToggle != null ? GameManager.Instance.hasResetToggle.isOn : true;

                // accept input within 10 % of the maximum angle
                if (GameManager.Instance.GameInProgress && !GameManager.Instance.locked && hasReset && (Mathf.InverseLerp(minAngle, maxAngle, currentAngle) > 0.90f || Input.GetKeyDown(KeyCode.Space)))
                {
                    GameManager.Instance.Action();
                }
                // else register a reset if we're within 10% of the minimum angle
                else if (GameManager.Instance.GameInProgress && !hasReset && Mathf.InverseLerp(minAngle, maxAngle, currentAngle) < 0.10f)
                {
                    GameManager.Instance.hasResetToggle.isOn = true;
                }

                if (GameManager.Instance.GameInProgress)
                {
                    thisGamesFrequencies.Add(currentAngle);
                    thisGamesMaxAngles.Add(maxAngle);
                }
            }

        }
        //text:
        Angletext.text =
            "Sensor : " + sensor.ToString() +
            "\n\n Min angle : " + minAngle.ToString() +
            "\n CurrentAngle :" + currentAngle.ToString() +
            "\n Max angle : " + maxAngle.ToString() +

            "\n\n count : " + count.ToString() +
            "\n angold : " + angold.ToString();

        //auto-reconnecting
        if (angold == currentAngle && count < 100)
            count++;
        else
            count = 0;

        if ((count == 100 && BtConnector.isConnected()))//|| (!BtConnector.available()&& BtConnector.isConnected()))
        {

            Connect();
            count = 0;
        }

        if (count % 50 == 2 && BtConnector.isConnected())
        {
            BtConnector.sendChar('F');
        }


        angold = currentAngle;
        //read control data from the Module.
        controlData = BtConnector.readControlData();
    }
    */

    void ontime()
    {

        connected = BtConnector.isConnected();//check connection status

        if (BtConnector.isConnected() != _connectButtonStatus)
        {
            _connectButtonStatus = BtConnector.isConnected();
            connectButtonImage.color = _connectButtonStatus == true ? connectedButtonColour : notConnectedButtonColour;
            connectButtonText.text = _connectButtonStatus == true ? "connected!" : "connect";

            if (_connectButtonStatus == true)
            {
                PlayerPrefs.SetString("lastConnectedDeviceName", _lastNameEnteredToConnect);

                if (ifcal)
                    UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
            }
            else
            {
                UIManager.Instance.notConnectedGameErrorObject.SetActive(true);
                // UIManager.Instance.setMinToggle.isOn = false;
                // UIManager.Instance.setMaxToggle.isOn = false;
                // fullyCalibrated = false;

                // TODO : pause game and allow reconnection
                if (GameManager.Instance != null)
                    GameManager.Instance.ExitGame();

            }
        }
        ////////////////////////////keep 2 way communication//////////////////
        if (sent_count == 15)//(int)(1.0f/(2.0f*speed)))
        {
            if (BtConnector.isConnected())
                BtConnector.sendChar('F');
            sent_count = 0;
        }
        sent_count++;
        //////////////////////////////////////////////////////////////////////////////




        if (BtConnector.isConnected() && BtConnector.available())//check connection status
        {
            //UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
            messageFromMC = BtConnector.readBuffer();//read bytes till a new line or a max of 100 bytes
            //Angletext.text += "\n messageFromMC.Length" + messageFromMC.Length.ToString();
            if (messageFromMC.Length > 0)
            {
                if (messageFromMC.Length == 16)
                {
                    sensor = 'I';// IMU6050, IMU 9250, etc.
                    qq1.w = (float)(messageFromMC[0] * 256 + messageFromMC[1]);// (10000.0f);
                    qq1.x = (float)(messageFromMC[2] * 256 + messageFromMC[3]);// (10000.0f);
                    qq1.y = (float)(messageFromMC[4] * 256 + messageFromMC[5]);// (10000.0f);
                    qq1.z = (float)(messageFromMC[6] * 256 + messageFromMC[7]);// (10000.0f);

                    qq2.w = (float)(messageFromMC[8] * 256 + messageFromMC[9]);// (10000.0f);
                    qq2.x = (float)(messageFromMC[10] * 256 + messageFromMC[11]);// (10000.0f);
                    qq2.y = (float)(messageFromMC[12] * 256 + messageFromMC[13]);// (10000.0f);
                    qq2.z = (float)(messageFromMC[14] * 256 + messageFromMC[15]);// (10000.0f);

                    if (qq1.w >= 32768)
                        qq1.w = qq1.w - 65536;

                    if (qq1.x >= 32768)
                        qq1.x = qq1.x - 65536;

                    if (qq1.y >= 32768)
                        qq1.y = qq1.y - 65536;

                    if (qq1.z >= 32768)
                        qq1.z = qq1.z - 65536;

                    if (qq2.w >= 32768)
                        qq2.w = qq2.w - 65536;

                    if (qq2.x >= 32768)
                        qq2.x = qq2.x - 65536;

                    if (qq2.y >= 32768)
                        qq2.y = qq2.y - 65536;

                    if (qq2.z >= 32768)
                        qq2.z = qq2.z - 65536;
                    //////////////////////////////////////////////////////////
                    if (qq1.w > 10000)
                        qq1.w -= 10000;
                    if (qq1.x > 10000)
                        qq1.x -= 10000;
                    if (qq1.y > 10000)
                        qq1.y -= 10000;
                    if (qq1.z > 10000)
                        qq1.z -= 10000;

                    if (qq2.w > 10000)
                        qq2.w -= 10000;
                    if (qq2.x > 10000)
                        qq2.x -= 10000;
                    if (qq2.y > 10000)
                        qq2.y -= 10000;
                    if (qq2.z > 10000)
                        qq2.z -= 10000;

                    qq1.w /= 10000;
                    qq1.x /= 10000;
                    qq1.y /= 10000;
                    qq1.z /= 10000;

                    qq2.w /= 10000;
                    qq2.x /= 10000;
                    qq2.y /= 10000;
                    qq2.z /= 10000;
                    float s1, s2;
                    s1 = (qq1.w * qq1.w) + (qq1.x * qq1.x) + (qq1.y * qq1.y) + (qq1.z * qq1.z);
                    s2 = (qq2.w * qq2.w) + (qq2.x * qq2.x) + (qq2.y * qq2.y) + (qq2.z * qq2.z);

                    if ((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100)
                        q1.transform.rotation = qq1;

                    if ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100)
                        q2.transform.rotation = qq2;

                    if (((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100) && ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100))
                    {
                        //Angletext.text = "Not Set!";

                        if (ifcal)
                        {

                            q3.transform.rotation = qq3 = product(divideQbyR(qq2, qq2_0), qq1_0);
                            diff2 = divideQbyR(qq1, qq3);
                            //currentAngle = Quaternion.Angle(qqzero, diff2);
                            arm.transform.localRotation = diff2;//current position

                            v3cur = (toparm.transform.position - sp.transform.position);
                            v3cur_ = Vector3.ProjectOnPlane(v3cur, Vector3.Cross(v3max, v3min));
                            currentAngle = Vector3.Angle(v3min, v3cur_);

                            if (Mathf.Abs((Vector3.Angle(v3min, v3max) + Vector3.Angle(v3min, v3cur_)) - Vector3.Angle(v3max, v3cur_)) < 1)
                                currentAngle *= -1;
                            else if (Mathf.Abs(Vector3.Angle(v3min, v3max) + Vector3.Angle(v3min, v3cur_) + Vector3.Angle(v3max, v3cur_) - 360) < 1)
                                currentAngle *= -1;

                            currentAngle = (angold * 3 + currentAngle) / 4.0f;


                        }
                        else
                        {
                            q3.transform.rotation = qq2;
                            currentAngle = Quaternion.Angle(qq1, qq2);
                            arm.transform.localRotation = Quaternion.Euler(0.0f, currentAngle, 0.0f);
                        }
                    }
                }
                else if (messageFromMC.Length == 3 && messageFromMC[0] == 'F')
                {
                    sensor = 'F';// Felex/Bending sensors.
                    currentAngle = 256 * messageFromMC[1] + messageFromMC[2];
                    currentAngle = 170 - getFangle(currentAngle);
                    currentAngle = (angold * 3 + currentAngle) / 4.0f;

                    // arm.transform.localEulerAngles.Set(arm.transform.localEulerAngles.x, getFangle(currentAngle), arm.transform.localEulerAngles.z);
                    arm.transform.localRotation = Quaternion.Euler(0.0f, -1 * currentAngle, 0.0f);

                }
                else if (messageFromMC.Length == 3 && messageFromMC[0] == 'B')
                {
                    sensor = 'B';// MyWare body sensor
                    currentAngle = 256 * messageFromMC[1] + messageFromMC[2];
                    currentAngle = (angold * 3 + currentAngle) / 4.0f;
                    if (fullyCalibrated)
                        arm.transform.localRotation = Quaternion.Euler(0.0f, (currentAngle - minAngle) * (90.0f / (maxAngle - minAngle)), 0.0f);


                }
                else
                {
                    sensor = 'U';
                    sema++;
                }


            }
            if (ifcal && GameManager.Instance != null)
            {

                // don't want to increase max angle in hockey or pop pop
                if (currentAngle > NewmaxAngle && sensor == 'F') //&& (Hockey.Instance == null || PopManager.instance != null))
                    NewmaxAngle = currentAngle;
                if (maxAngle < NewmaxAngle)
                {
                    //maxAngle += (NewmaxAngle - maxAngle) / 300;
                    maxAngle += 0.04f;
                }
                // if there is a gamemmanger in scene, it means we're acting on input

                // update image of current value
                GameManager.Instance.UpdateCurrentValue(minAngle, currentAngle, maxAngle);

                // check if we've reset
                bool hasReset = GameManager.Instance.hasResetToggle != null ? GameManager.Instance.hasResetToggle.isOn : true;

                // accept input within 10 % of the maximum angle
                if (GameManager.Instance.GameInProgress && !GameManager.Instance.locked && hasReset && (Mathf.InverseLerp(minAngle, maxAngle, currentAngle) > 0.90f || Input.GetKeyDown(KeyCode.Space)))
                {
                    GameManager.Instance.Action();
                }
                // else register a reset if we're within 10% of the minimum angle
                else if (GameManager.Instance.GameInProgress && !hasReset && Mathf.InverseLerp(minAngle, maxAngle, currentAngle) < 0.10f)
                {
                    GameManager.Instance.hasResetToggle.isOn = true;
                }

                if (GameManager.Instance.GameInProgress)
                {
                    thisGamesFrequencies.Add(currentAngle);
                    thisGamesMaxAngles.Add(maxAngle);
                }
            }

        }

        //////////////////////auto-reconnecting//////////////////////////////////////////

        if (angold == currentAngle)
            dis_count++;
        else
            dis_count = 0;

        if (dis_count > 30)
        {
            dis_count = -30;
            //if (BtConnector.isConnected())
            if (controlData.Length == 9)
                BtConnector.close();
            Connect();

        }

        angold = currentAngle;
        //read control data from the Module.
        controlData = BtConnector.readControlData();
    }


    public void LoggedIn()
    {
        if (PlayerPrefs.HasKey("lastConnectedDeviceName"))
        {
            deviceNameInput.text = PlayerPrefs.GetString("lastConnectedDeviceName");
            Connect();
        }
    }

    public void ResetValues()
    {
        thisGamesFrequencies.Clear();
        thisGamesMaxAngles.Clear();
    }

    public void Connect()
    {


        int result = 0;
        if (!BtConnector.isBluetoothEnabled())
        {
            BtConnector.askEnableBluetooth();
        }
        else
        {
            _lastNameEnteredToConnect = deviceNameInput.text;
            BtConnector.moduleName(deviceNameInput.text); //incase User Changed the Bluetooth Name
            result = BtConnector.connect();
        }

        connected = BtConnector.isConnected();//check connection status

    }

    public void SetZeroPosition()
    {
        if (sensor == 'I')
        {
            qq1_0 = qq1;
            qq2_0 = qq2;
            /////
            qq3 = product(divideQbyR(qq2, qq2_0), qq1_0);
            qmin = diff2 = divideQbyR(qq1, qq3);
            arm.transform.localRotation = diff2;
            v3min = toparm.transform.position - sp.transform.position;
            minAngle = 0;
        }
        else
        {
            minAngle = currentAngle;
        }

        ifcal = true;
        UIManager.Instance.setMinToggle.isOn = true;
        if (UIManager.Instance.setMaxToggle.isOn)
        {
            fullyCalibrated = true;
            UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
        }
    }

    public void SetMaxPosition()
    {
        if (sensor == 'I')
        {//to calculate negative angles:
            qq3 = product(divideQbyR(qq2, qq2_0), qq1_0);
            qmax = divideQbyR(qq1, qq3);
            v3max = toparm.transform.position - sp.transform.position;
        }
        maxAngle = currentAngle;

        UIManager.Instance.setMaxToggle.isOn = true;
        if (UIManager.Instance.setMinToggle.isOn)
        {
            fullyCalibrated = true;
            UIManager.Instance.notConnectedGameErrorObject.SetActive(false);
        }
    }

    Quaternion product(Quaternion r, Quaternion q)
    {
        Quaternion t;
        t.w = (r.w * q.w) - (r.x * q.x) - (r.y * q.y) - (r.z * q.z);
        t.x = (r.w * q.x) + (r.x * q.w) - (r.y * q.z) + (r.z * q.y);
        t.y = (r.w * q.y) + (r.x * q.z) + (r.y * q.w) - (r.z * q.x);
        t.z = (r.w * q.z) - (r.x * q.y) + (r.y * q.x) + (r.z * q.w);

        return t;
    }

    Quaternion divideQbyR(Quaternion q, Quaternion r)// t = Q/R
    {
        Quaternion t;
        float makhraj = (r.w * r.w) + (r.x * r.x) + (r.y * r.y) + (r.z * r.z);
        t.w = ((r.w * q.w) + (r.x * q.x) + (r.y * q.y) + (r.z * q.z)) / makhraj;
        t.x = ((r.w * q.x) - (r.x * q.w) - (r.y * q.z) + (r.z * q.y)) / makhraj;
        t.y = ((r.w * q.y) + (r.x * q.z) - (r.y * q.w) - (r.z * q.x)) / makhraj;
        t.z = ((r.w * q.z) - (r.x * q.y) + (r.y * q.x) - (r.z * q.w)) / makhraj;

        return t;
    }

    Quaternion inverse(Quaternion r)// t = Q/R
    {
        Quaternion t;
        float makhraj = (r.w * r.w) + (r.x * r.x) + (r.y * r.y) + (r.z * r.z);
        t.w = r.w / makhraj;
        t.x = -1 * r.x / makhraj;
        t.y = -1 * r.y / makhraj;
        t.z = -1 * r.z / makhraj;

        return t;
    }
    float dot(Vector3 a, Vector3 b)
    {
        return ((a.x * b.x) + (a.y * b.y) + (a.z * b.z));
    }
    float getFangle(float value)
    {
        /*
        float val = value - 175;
        if (val >= -69)
            val *= 0.761f;
        else if (val >= -83)
            val *= 1.062f;
        else if (val >= -93)
            val *= 1.394f;
        else
            val *= 1.735f;
        val = -1 * val;
        return val;
       */
        float val;
        if (value < 101)
            val = value * 0.96f;
        else if (value < 111)
            val = value * 0.98f;
        else if (value < 123)
            val = value * 0.99f;
        else if (value < 140)
            val = value * 0.97f;
        else if (value < 155)
            val = value * 0.92f;
        else if (value < 167)
            val = value * 0.90f;
        else if (value < 177)
            val = value * 0.89f;
        else if (value < 191)
            val = value * 0.90f;
        else if (value < 201)
            val = value * 0.88f;
        else if (value < 217)
            val = value * 0.89f;
        else if (value < 234)
            val = value * 0.87f;
        else if (value < 256)
            val = value * 0.85f;
        else if (value < 266)
            val = value * 0.81f;
        else if (value < 272)
            val = value * 0.82f;
        else if (value < 290)
            val = value * 0.84f;
        else
            val = value * 0.86f;

        return val;

    }
    public void BLnamechange()
    {

    }
}
