using UnityEngine;
using System.Collections;

public class BasketballObject : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "score")
            Basketball.Instance.Score();
    }
}
