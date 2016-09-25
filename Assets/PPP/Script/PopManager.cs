/*
 * This script control almost the game: State, UI, Ads
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_3
using UnityEngine.SceneManagement;
#endif
public class PopManager : MonoBehaviour {
	public static PopManager instance;
	public enum GameState{
		Menu,
		Playing,
		Pause,
		Dead
	};

    public bool winStatus;
    private GameState state;
	private int score = 0;

	public static GameState CurrentState{
		get{ return instance.state; }
		set{ instance.state = value; }
	}

	public static int Score{
		get{ return instance.score; }
		set{ instance.score = value; }
	}

	public static int HighLevel{
		get{ return PlayerPrefs.GetInt(GlobalValue.levelHighest,1); }
		set{ PlayerPrefs.SetInt(GlobalValue.levelHighest, value); }
	}

	// Use this for initialization
	void Awake () {
		instance = this;
		state = GameState.Menu;

        HighLevel = UIManager.Instance.popPopHighScores[UIManager.Instance.currentDifficulty];
        if (HighLevel == 0) HighLevel = 1;
        GlobalValue.levelPlaying = HighLevel;
    }

	public void Play(){

        GameManager.Instance.GameStarting(Time.time);
		state = GameState.Playing;
		GlobalValue.levelPlayingPathLeft = GlobalValue.levelPlaying;	//for PlayerController
		GlobalValue.levelPathLeft = GlobalValue.levelPlaying;		//for CreatePlatform

//		AdsController.HideAds ();

	}

	public void GameSuccess()
    {
		GlobalValue.levelPlaying++;
        winStatus = true;
        bool newRecord = false;

        if (GlobalValue.levelPlaying >= UIManager.Instance.popPopHighScores[UIManager.Instance.currentDifficulty])//HighLevel)
        {
            HighLevel++;        //save playerPref
            UIManager.Instance.popPopHighScores[UIManager.Instance.currentDifficulty] = HighLevel;
            NetworkManager.Instance.SetHighLevel(UIManager.Instance.currentDifficulty, HighLevel);
            UIManager.Instance.UpdateTopResultTexts();
            newRecord = true;
        }

        GameManager.Instance.GameEnding(Time.time, newRecord);
        //StartCoroutine (WaitForRestart (1.5f));

//		AdsController.ShowAds ();
	}

	public void GameOver(){
        winStatus = false;
		state = GameState.Dead;
        GameManager.Instance.GameEnding(Time.time, showUI: false);
        StartCoroutine (WaitForRestart (1f));

//		AdsController.ShowAds ();
	}

	public void Restart(){
		StartCoroutine (WaitForRestart (0f));
	}

    public void NextButtonPressed()
    {
        GlobalValue.isRestart = true;
        GameSceneManager.Instance.ResetPopPop();
    }

	IEnumerator WaitForRestart(float time){
		yield return new WaitForSeconds (time);
		GlobalValue.isRestart = true;

        GameSceneManager.Instance.ResetPopPop();
        //#if UNITY_5_3
        //SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
        //#else
        //Application.LoadLevel (Application.loadedLevel);
        //#endif
    }
}
