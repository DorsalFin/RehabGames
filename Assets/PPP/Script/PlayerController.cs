using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;

	[Tooltip("Speed moving")]
	public float speed = 10f;
	public AudioClip soundTurn;
	public AudioClip soundHitWall;
	public AudioClip soundCorrectTurn;
	public AudioClip soundCollectPoint;
	public AudioClip soundFinish;
	[Tooltip("Auto change color when earn point")]
	public Color[] randomColor;

    public List<float> actionTimes = new List<float>();

    public GameObject pointFx;
	public GameObject finishFx;

	private bool allowCreatePath = true;	//allow create new path after first tap on screen
	private Vector3 moving;		//step translate of the ball
	private bool moveHorizontal = true;		//first, the ball will moving along with X axis
	private bool allowMoving = false;		//just allow the ball moving when it already lay on the platform
	private Renderer rend;

    void Awake()
    {
        Instance = this;
    }

	// Use this for initialization
	void Start ()
    {
        // speed changes for difficulty settings
        if (UIManager.Instance.currentDifficulty == 0)
            speed = 2;
        else if (UIManager.Instance.currentDifficulty == 1)
            speed = 4;
        else
            speed = 6;

        speed = speed * Time.fixedDeltaTime;		
		moving = new Vector3 (speed * (-1), 0, speed);	//moving along the X asix and to the left side
		rend = GetComponent<Renderer>();
		GetComponent<Rigidbody> ().isKinematic = true;
		StartCoroutine (DelayAndFall (1f));		// set isKinematic to false after time
	}

    public void Action()
    {
        if (allowMoving && PopManager.CurrentState == PopManager.GameState.Playing)
        {
            SoundManager.PlaySfx(soundTurn);
            allowCreatePath = true;     //allow create new path
            if (moveHorizontal)
            {
                moveHorizontal = false;     //moving along with Z axis
                moving = new Vector3(speed, 0, speed);
            }
            else
            {
                moveHorizontal = true;		///moving along with X axis
				moving = new Vector3(speed * (-1), 0, speed);
            }

            actionTimes.Add(Time.time);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // for testing
        if (allowMoving && Input.anyKeyDown)
            Action();
    }

	void FixedUpdate(){
		if (allowMoving && PopManager.CurrentState == PopManager.GameState.Playing)
			transform.Translate (moving);		//the ball moving when in Playing mode
	}

//	void OnCollisionEnter(Collision other){
//		allowMoving = true;
//		if (other.gameObject.CompareTag ("Wall")) {
//			SoundManager.PlaySfx (soundHitWall);
//			other.transform.parent.gameObject.SendMessage ("HitWall", SendMessageOptions.DontRequireReceiver);
//			if (allowCreatePath) {
//				CreatePlatform.instance.CreateWall ();		//just create new path when user Tap to change the direction of the ball and the ball collide with first wall
//				allowCreatePath = false;
//			}
//			moving *= -1;	//change direction of the ball
//		}
//	}

	void OnTriggerEnter(Collider other){
		if (other.CompareTag ("Finish")) {
            PopManager.instance.GameOver();
		} 
		else if (other.CompareTag ("CorrectTurn")) {
			SoundManager.PlaySfx (soundCorrectTurn,0.75f);
			Destroy (other.gameObject);
			GlobalValue.levelPlayingPathLeft--;
		}
		else if (other.CompareTag ("Point")) {
			SoundManager.PlaySfx (soundCollectPoint);
//			GameManager.Score += 1;
			rend.material.color = randomColor[Random.Range(0,randomColor.Length)];
			Instantiate (pointFx, other.transform.position, Quaternion.identity);
			Destroy (other.gameObject);
		}

		else if (other.CompareTag ("Door")) {
			SoundManager.PlaySfx (soundFinish);
            PopManager.instance.GameSuccess ();
			Instantiate (finishFx, other.transform.position, Quaternion.identity);
			gameObject.SetActive (false);
		}else if (other.CompareTag ("Wall")) {
			SoundManager.PlaySfx (soundHitWall, 0.45f);
			other.transform.parent.gameObject.SendMessage ("HitWall", SendMessageOptions.DontRequireReceiver);
			if (allowCreatePath) {
				CreatePlatform.instance.CreateWall ();		//just create new path when user Tap to change the direction of the ball and the ball collide with first wall
				allowCreatePath = false;
			}
			moving *= -1;	//change direction of the ball
		}
	}

	IEnumerator DelayAndFall(float time){
		yield return new WaitForSeconds (time);
		allowMoving = true;
		GetComponent<Rigidbody> ().isKinematic = false;
	}
}
