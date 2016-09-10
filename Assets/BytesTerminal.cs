using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BytesTerminal : MonoBehaviour
{
    public static BytesTerminal Instance;

    public GameObject q1, q2, q3, qdiff2;
    public GameObject arm;
    //public Text tcal;
    Quaternion qq1, qq2, qq1_0, qq2_0, diff1, qq3, diff2, qqzero;
    bool ifcal;
    /// 
    Vector3 axold;
    Vector3 point;
    float angold;

    public bool fullyCalibrated;

    // for result data
    public float minAngle = 0f;
    public float maxAngle;
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
    string messageToMC = "message"; // string to sent to Microcontroller
	byte [] messageFromMC ; //temporary string to hold BtConnector.read() value
	string controlData = ""; //will contain data from the plugin to check the status of the whole process
	
	List<string> messages = new List<string>();// messages from Microcontroller in bytes
	
	int labelHeight;//height of a single label inside the ScrollView
	int height;//ScrollView Height

    bool hasReset;
	
    void Awake()
    {
        Instance = this;
    }

	void Start ()
    {
		//use one of the following two methods to change the default bluetooth module.
		//BtConnector.moduleMAC("00:13:12:09:55:17");
		//BtConnector.moduleName ("HC-05");
		height = (int)(Screen.height * 0.8f);
		labelHeight = (int)(0.06f*height);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        qqzero.w = 1;
        qqzero.x = qqzero.y = qqzero.z = 0;
        ifcal = false;
        //tcal.text = "NOT CALIBRATED!";
        angold = -1000;
        point = Vector3.one;
    }

	void Update ()
    {
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
                UIManager.Instance.setMinToggle.isOn = false;
                UIManager.Instance.setMaxToggle.isOn = false;
                fullyCalibrated = false;

                // TODO : pause game and allow reconnection
                if (GameManager.Instance != null)
                    GameManager.Instance.ExitGame();
            }
        }

        if (BtConnector.isConnected() && BtConnector.available())//check connection status
        {
            messageFromMC = BtConnector.readBuffer();//read bytes till a new line or a max of 100 bytes
            if (messageFromMC.Length > 0)
            {
                if (messageFromMC.Length == 16)
                {
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
                    /*
                    t.text += (messageFromMC.Length.ToString("00: ")
                            + "  ,w1: " + qq1.w.ToString("0.0000")
                            + "  ,x1: " + qq1.x.ToString("0.0000")
                            + "  ,y1: " + qq1.y.ToString("0.0000")
                            + "  ,z1: " + qq1.z.ToString("0.0000")
                            + "  ,s1: "+ ((int)(s1 * 100)).ToString("0.00")

                            + "  ,w2: " + qq2.w.ToString("0.0000")
                            + "  ,x2:" + qq2.x.ToString("0.0000")
                            + "  ,y2:" + qq2.y.ToString("0.0000")
                            + "  ,z2:" + qq2.z.ToString("0.0000")
                            + "  ,s2:"+ ((int)(s2 * 100)).ToString("0.00")+ "\n");
                    */
                    if ((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100)
                        q1.transform.rotation = qq1;

                    if ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100)
                        q2.transform.rotation = qq2;

                    if (((int)(s1 * 100) == 99 || (int)(s1 * 100) == 100) && ((int)(s2 * 100) == 99 || (int)(s2 * 100) == 100))
                    {
                        //t.text = "q1(t)  (x,y,z,w) : " + qq1.ToString() +
                        //         "\n q2(t)  (x,y,z,w) : " + qq2.ToString();

                        if (ifcal)
                        {
                            q3.transform.rotation = qq3 = product(divideQbyR(qq2, qq2_0), qq1_0);
                            diff2 = divideQbyR(qq1, qq3);
                            float currentAngle = Quaternion.Angle(qqzero, diff2);
                            //debugText2.text = currentAngle.ToString("0.0");

                            arm.transform.localRotation = diff2;

                            // don't want to increase max angle in hockey or pop pop
                            //if (currentAngle > maxAngle && (Hockey.Instance == null || PopManager.instance != null))
                            //    maxAngle = currentAngle;

                            // if there is a gamemmanger in scene, it means we're acting on input
                            if (GameManager.Instance != null)
                            {
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
                        else
                        {
                            q3.transform.rotation = qq2;
                            q3.transform.rotation = qq2;
                        }
 
                    }
                }

            }

            //if (t.text.Length > 500)
            //    t.text = "";

            //convert array of bytes into string
            ///////////////////////////////
            //if (labelHeight * messages.Count >= (height - labelHeight))
            //		scrollPosition.y += labelHeight;//slide the Scrollview down,when the screen filled with messages

            //if (labelHeight * messages.Count >= height * 2)
            //		messages.RemoveAt (0);//remove old messages,when ScrollView filled
        }
	
		//read control data from the Module.
		controlData = BtConnector.readControlData ();
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
        if (!BtConnector.isBluetoothEnabled())
            BtConnector.askEnableBluetooth();
        else
        {
            _lastNameEnteredToConnect = deviceNameInput.text;
            BtConnector.moduleName(deviceNameInput.text); //incase User Changed the Bluetooth Name
            int result = BtConnector.connect();
        }

        connected = BtConnector.isConnected();//check connection status
    }

    public void SetZeroPosition()
    {
        qq1_0 = qq1;
        qq2_0 = qq2;
        diff1 = product(qq1_0, inverse(qq2_0));
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
        float currentAngle = Quaternion.Angle(qqzero, diff2);
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
        t.x= (r.w * q.x) + (r.x * q.w) - (r.y * q.z) + (r.z * q.y);
        t.y= (r.w * q.y) + (r.x * q.z) + (r.y * q.w) - (r.z * q.x);
        t.z = (r.w * q.z) - (r.x * q.y) + (r.y * q.x) + (r.z * q.w);

        return t;
    }

    Quaternion divideQbyR(Quaternion q, Quaternion r)// t = Q/R
    {
        Quaternion t;
        float makhraj = (r.w * r.w) + (r.x * r.x) + (r.y * r.y) + (r.z * r.z);
        t.w = ((r.w * q.w) + (r.x * q.x) + (r.y * q.y) + (r.z * q.z))/makhraj;
        t.x = ((r.w * q.x) - (r.x * q.w) - (r.y * q.z) + (r.z * q.y))/makhraj;
        t.y = ((r.w * q.y) + (r.x * q.z) - (r.y * q.w) - (r.z * q.x))/makhraj;
        t.z = ((r.w * q.z) - (r.x * q.y) + (r.y * q.x) - (r.z * q.w))/makhraj;

        return t;
    }

    Quaternion inverse(Quaternion r)// t = Q/R
    {
        Quaternion t;
        float makhraj = (r.w * r.w) + (r.x * r.x) + (r.y * r.y) + (r.z * r.z);
        t.w = r.w / makhraj;
        t.x = -1* r.x / makhraj;
        t.y = -1 * r.y / makhraj;
        t.z = -1 * r.z / makhraj; 

        return t;
    }
}

















