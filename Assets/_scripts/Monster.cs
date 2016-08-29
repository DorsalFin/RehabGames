using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            RunningMan.Instance.MonsterCaughtPlayer();
    }
}
