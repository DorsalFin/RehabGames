using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatePlatform : MonoBehaviour {

	public static CreatePlatform instance;

	public GameObject platformLeft;
	public GameObject platformLeftDoor;
	public GameObject platformRight;
	public GameObject platformRightDoor;
	[Tooltip("The % chance to have bonus Point on the Path")]
	public float rateCreatePoint = 20f;
	private int createPointAt = 0; 	//0 mean no bunus Point

	private Vector3 currentPos;
	private int randomPathLength;
	private int randomWallPos;

	private Vector3 stepRight;		//step spawn on left side
	private Vector3 stepLeft;		//step spawn on right side
	private bool isLeft = true;		//create left path first
	private int countPathCreate=0;
	private bool isCreating = false;
	private float speedCreating =0.07f;
	private List<GameObject> listPath;
	private List<GameObject> listPathOld;
	private List<GameObject> listPathRemove;


	// Use this for initialization
	void Start () {
		instance = this;
		currentPos = transform.position;

		float step = Mathf.Sqrt (2) * 0.5f;
		stepRight = new Vector3 (step, 0, step);
		stepLeft = new Vector3 (step*(-1), 0, step);

		GlobalValue.levelPathLeft = GlobalValue.levelPlaying;

		listPath = new List<GameObject> ();
		randomPathLength = Random.Range (7, 11);	//first path
		CreatePath (); 	//create first path on start
	}


	public void CreatePath(){
		if (GlobalValue.levelPathLeft >= 0) {
			//get Point if lucky
			if (Random.Range (0f, 100f) < rateCreatePoint)
				createPointAt = Random.Range (2, randomPathLength - 1);
			else
				createPointAt = 0;

			listPath.Clear ();		//clear list to setup
			StartCoroutine (CreatePathInDelay (speedCreating));	//begin create the path with platform delay 0.1f

		}
	}

	//called when the ball hit the wall
	public void CreateWall(){
		if (!isCreating) {
			isCreating = true; 		//tell that system is creating

			randomWallPos = Random.Range (0, randomPathLength - 4);
			listPath [randomWallPos].GetComponent<Animator> ().SetTrigger ("wall");	//active wall

			int posNextPath = Random.Range (randomWallPos + 2, listPath.Count - 2);
			currentPos = listPath [posNextPath].transform.position;

			listPathOld = new List<GameObject> (listPath);
			RemoveOld ();

			GlobalValue.levelPathLeft--;
			randomPathLength = Random.Range (4, 12);	//get random how much platform on path
			CreatePath ();
		}
	} 

	//this will remove the path we created after the old list: listPath -> listPathOld -> listPathRemove
	private void RemoveOld(){
		if (listPathRemove != null) {
			float time = 0f;
			foreach (GameObject ob in listPathRemove) {
				time += 0.1f;
				ob.SendMessage ("Remove", time, SendMessageOptions.DontRequireReceiver);
			}
		} 

		listPathRemove = new List<GameObject>(listPathOld);
	}

	IEnumerator CreatePathInDelay(float time){
		yield return new WaitForSeconds (time);

		GameObject path;
		if (isLeft) {
			currentPos += stepLeft;
			path = Instantiate (platformLeft, currentPos, Quaternion.identity) as GameObject;
		} else {
			currentPos += stepRight;
			 path = Instantiate (platformRight, currentPos, Quaternion.identity) as GameObject;
		}
        path.transform.parent = GameManager.Instance.transform;

        countPathCreate++;
		if (countPathCreate == 1) {
			path.SendMessage ("FirstPlatformOfPaths", SendMessageOptions.DontRequireReceiver);
		}

		//create point if createPointAt != 0
		if(countPathCreate == createPointAt)
			path.SendMessage ("PointOn", SendMessageOptions.DontRequireReceiver);

		if (countPathCreate >= randomPathLength || Mathf.Abs (path.transform.position.x) > 4)
        {
			//replace the PlatformMid by the FlatformEnd if end
			if (GlobalValue.levelPathLeft == 0) {
				if (isLeft)
					path = Instantiate (platformLeftDoor, currentPos, Quaternion.identity) as GameObject;
				else
					path = Instantiate (platformRightDoor, currentPos, Quaternion.identity) as GameObject;

                path.transform.parent = GameManager.Instance.transform;
            }

			isCreating = false;

			randomPathLength = countPathCreate;	//in case Mathf.Abs (path.transform.position.x) > 4
			path.SendMessage ("WallOn", SendMessageOptions.DontRequireReceiver);	//tell that platform is final flatform, so turn the wall on
			countPathCreate = 0;
			isLeft = !isLeft;//change side path
		}
		else
        {
			StartCoroutine (CreatePathInDelay (speedCreating));
		}

		listPath.Add (path);
	}
}
