using UnityEngine;
using System.Collections;

public class GlobalValue : MonoBehaviour {
	public static bool isSound = true;
	public static bool isMusic = true;
	public static bool isRestart = false;

	public static int levelPlaying = 1;	//default = 1
	public static int levelPlayingPathLeft = 1;		//for PlayerController
	public static int levelPathLeft = 1;		//for CreatePlatform


	public static string levelHighest = "levelHighest";
	public static string totalScore = "totalScore";
}
