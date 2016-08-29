using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {
	public GameObject Point;
	public GameObject CorrectTurn;

	// Use this for initialization
	void Start () {
	}

	//called by SendMessage function in CreatePlatform
	public void WallOn(){
		StartCoroutine (SetTriggerDelay1Frame ("wall"));
	}

	//called by SendMessage function in CreatePlatform
	public void HitWall(){
		if (GlobalValue.levelPlaying >= 10)
			GetComponent<Animator>().SetTrigger ("hit wall");
	}

	//called by SendMessage function in CreatePlatform
	public void PointOn(){
		Point.SetActive (true);
	}

	//called by SendMessage function in CreatePlatform
	public void FirstPlatformOfPaths(){
		CorrectTurn.SetActive (true);
	}

	public void Remove(float time){
//		GetComponent<Animator>().SetTrigger ("remove");
		StartCoroutine (delayRemove (0.1f));
	}

	//sent by animator
	public void Destroy(){
		Destroy (gameObject);
	}

	IEnumerator delayRemove(float time){
		yield return new WaitForSeconds (time);

		GetComponent<Animator>().SetTrigger ("remove");
	}

	IEnumerator SetTriggerDelay1Frame(string trigger){
		yield return 0;

		GetComponent<Animator>().SetTrigger (trigger);
	}
}
