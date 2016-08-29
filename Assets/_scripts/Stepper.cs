using UnityEngine;
using System.Collections.Generic;

public class Stepper : MonoBehaviour 
{
    public bool acceptsInput;

    public float movementSpeed = 0.01f;
    public float growthRate = 0.0025f;
    public List<float> stepTimes = new List<float>();

    public float StepsTaken { get { return _stepsTaken; } }

    Animator _anim;
    bool _leftStep;
    bool _stepping;
    int _stepsTaken = 0;


    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }
	
	void Update () 
    {
        if (_stepping)
            transform.Translate(movementSpeed * Time.deltaTime, 0, 0);
	}

    public void Step()
    {
        if (_stepping)
            return;

        stepTimes.Add(Time.time);

        _stepping = true;
        _stepsTaken++;

        string animToPlay = _leftStep ? "left_step" : "right_step";
        _anim.Play(animToPlay);
        _leftStep = !_leftStep;
    }

    public void FinishedStep()
    {
        _stepping = false;

        // report our finished step to the manager
        RunningMan.Instance.FinishedStep(this);
    }

    public void Die()
    {
        _stepping = false;
        _anim.Play("fall");
    }

    public void Win()
    {
        _stepping = false;
        _anim.Play("win");
    }
}
