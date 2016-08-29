using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
	[Header("StartMenu")]
	public GameObject StartMenu;
	public Text Level;
	public GameObject NextButton;
	public GameObject PreButton;
	public Image soundImg;
	public Sprite soundOn;
	public Sprite soundOff;
	public string storeLink = "your store link";
	public string facebookLink = "your facebook link";
	
	[Header("UI")]
	public GameObject UI;
	public Text pathLeft;

	public static GUIManager instance;
	// Use this for initialization
	void Start () {
		instance = this;
		StartMenu.SetActive (true);
		UI.SetActive (false);

		if (!GlobalValue.isRestart) {
			GlobalValue.levelPlaying = PopManager.HighLevel;
		}
		init ();	//init start menu
		
		if (GlobalValue.isSound) 
			soundImg.sprite = soundOn;
		else
			soundImg.sprite = soundOff;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (PopManager.CurrentState == PopManager.GameState.Playing)
                PopManager.instance.Restart ();
			else
				Application.Quit ();
		}
		pathLeft.text = GlobalValue.levelPlayingPathLeft + "";
	}



	//called by Button

	public void Play(){
        PopManager.instance.Play ();
		StartMenu.SetActive (false);
		UI.SetActive (true);
	}

	//Button Next and Pre
	private void init(){
		
		Level.text = "Level " + GlobalValue.levelPlaying;
		if (PopManager.HighLevel == 1) {
			NextButton.SetActive (false);
			PreButton.SetActive (false);
		} else if (GlobalValue.levelPlaying == 1) {
			NextButton.SetActive (true);
			PreButton.SetActive (false);
		} else if (GlobalValue.levelPlaying == PopManager.HighLevel) {
			NextButton.SetActive (false);
			PreButton.SetActive (true);
		} else {
			NextButton.SetActive (true);
			PreButton.SetActive (true);
		}
	}

	public void Next(){
		SoundManager.PlaySfx ("Click");
		GlobalValue.levelPlaying++;
		init ();
	}

	public void Pre(){
		SoundManager.PlaySfx ("Click");
		GlobalValue.levelPlaying--;
		init ();
	}
	
	public void Sound(){
		GlobalValue.isSound = !GlobalValue.isSound;
		GlobalValue.isMusic = !GlobalValue.isMusic;
		if (GlobalValue.isSound) {
			soundImg.sprite = soundOn;
			SoundManager.SoundVolume = 1f;
			SoundManager.MusicVolume = 1f;
			SoundManager.PlaySfx ("Click");
		} else {
			soundImg.sprite = soundOff;
			SoundManager.SoundVolume = 0f;
			SoundManager.MusicVolume = 0f;
		}
	}

	public void OpenFacebook(){
		Application.OpenURL (facebookLink);
	}

	public void OpenStoreLink(){
		Application.OpenURL (storeLink);
	}

}
