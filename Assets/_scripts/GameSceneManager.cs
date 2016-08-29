using UnityEngine;
using System.Collections;
//using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    void Awake()
    {
        Instance = this;
    }


    public void ResetPopPop()
    {
        GameManager.Instance.ExitGame(false);
        //SceneManager.LoadSceneAsync(UIManager.Instance.CurrentGameString, LoadSceneMode.Additive);
        Application.LoadLevelAdditiveAsync(UIManager.Instance.CurrentGameString);
    }
}
