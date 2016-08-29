using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public float smooth = 5f;
	public float offset = 15f;

	private Transform player;

	// Use this for initialization
	void Start () {
		player = FindObjectOfType<PlayerController> ().transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (player.position.z - offset > transform.position.z) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, Mathf.Lerp (transform.position.z, player.position.z - offset, smooth * Time.deltaTime));
		}
	}
}
