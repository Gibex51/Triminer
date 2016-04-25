using UnityEngine;
using System.Collections;
using System.IO;

public class ScoreData {
	public int Easy;
	public int Normal;
	public int Hard;
	public int Endless;
}

public class FileManager : MonoBehaviour {

	public ScoreData LoadScores(){
		ScoreData scoreData = new ScoreData();
		scoreData.Easy = PlayerPrefs.GetInt ("Score.Easy");
		scoreData.Normal = PlayerPrefs.GetInt ("Score.Normal");
		scoreData.Hard = PlayerPrefs.GetInt ("Score.Hard");
		scoreData.Endless = PlayerPrefs.GetInt ("Score.Endless");

		if (scoreData.Easy == 0) scoreData.Easy = 59999;
		if (scoreData.Normal == 0) scoreData.Normal = 59999;
		if (scoreData.Hard == 0) scoreData.Hard = 59999;
		if (scoreData.Endless < 0) scoreData.Endless = 0;
		return scoreData;
	}

	public void SaveScores(ScoreData scoreData){
		PlayerPrefs.SetInt ("Score.Easy", scoreData.Easy);
		PlayerPrefs.SetInt ("Score.Normal", scoreData.Normal);
		PlayerPrefs.SetInt ("Score.Hard", scoreData.Hard);
		PlayerPrefs.SetInt ("Score.Endless", scoreData.Endless);
		PlayerPrefs.Save ();
	}
}
