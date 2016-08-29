using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour {

    public static Calibration Instance;

    public GameObject audioDeviceButtonPrefab;
    public Transform audioDeviceParent;

    [HideInInspector]
    public GameObject currentGameParentObject;

    UIManager _uiManager;

    void Awake()
    {
        Instance = this;
        _uiManager = GetComponent<UIManager>();
    }

    //public void ShowMicrophones()
    //{
    //    // show list of audio devices if multiple exist
    //    if (Microphone.devices.Length > 0)
    //    {
    //        //clear any existing microphones
    //        foreach (Transform child in audioDeviceParent)
    //            Destroy(child.gameObject);

    //        // spawn button for each device, the vertical layout group will sort position automatically
    //        for (int i = 0; i < Microphone.devices.Length; ++i)
    //        {
    //            GameObject button = (GameObject)Instantiate(audioDeviceButtonPrefab, Vector3.zero, Quaternion.identity);
    //            button.transform.parent = audioDeviceParent;
    //            button.GetComponentInChildren<Text>().text = Microphone.devices[i].ToString();
    //            int thisIndex = i;
    //            button.GetComponent<Button>().onClick.AddListener(delegate { microphone.Instance.SetMicrophoneByIndex(thisIndex); });
    //        }
    //    }
    //    // this should never auto select - on android it was choosing a non existant microphone even though
    //    // there was a valid one (perhaps at a different index)
    //    else
    //        microphone.Instance.SetMicrophoneByIndex(0);
    //}

    public void HideMicrophones()
    {
        foreach (Transform child in audioDeviceParent)
            Destroy(child.gameObject);
    }

    public void ExitCurrentGame()
    {
        _uiManager.SetUiShowState = true;
        Destroy(currentGameParentObject);
    }
}
