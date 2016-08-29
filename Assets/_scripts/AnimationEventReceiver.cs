using UnityEngine;
using System.Collections;

public class AnimationEventReceiver : MonoBehaviour 
{

    public GameObject receiver;

    public void FinishedStep()
    {
        receiver.SendMessage("FinishedStep");
    }

    public void DeathComplete()
    {
        GameManager.Instance.GameOver(false, "Oh no! The monster caught you!");
    }

    public void WinComplete()
    {
        GameManager.Instance.GameOver(true, RunningMan.Instance.newRecord ? "That's a new record! Good job!" : "You made it! Well done!");
    }
}
