using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FieldMover : MonoBehaviour {

	private bool mousePressed;
	private Vector3 mouseDownPos;
	private Vector3 prevFieldPos;
	public Transform fieldTransform;
	public Camera mainCamera;

	private Vector3 minBound, maxBound;
	private bool gameOver;
	private bool winState;
	private bool gamePause;
	private bool firstOpen;
	private bool movingFieldStart;
	private float bottomMenuSize;

	public GameObject buttonRestart;
	public GameObject textGameOver;
	public GameObject textWin;
	public GameObject blockHighScore;
	public GameObject valueHighScore;

	public GameObject textTimer;
	private float time;

	ScoreData scoreData;
	public GameObject textEasy;
	public GameObject textNormal;
	public GameObject textHard;
	public GameObject textEndless;
	public GameObject textMineCounter;

	public GameObject textVersion;

	public int openedCells;

	public FieldProcs fieldProcs;
	public UIProcs uiProcs;

	//private float touch_dist = 0;

	public void SetWin(bool iswin){
		winState = iswin;
		textWin.SetActive(iswin);
		buttonRestart.SetActive(iswin);
		GetComponent<UIProcs> ().HideWaiter ();
	}

	public void SetGameOver (bool isgameover){
		gameOver = isgameover;
		textGameOver.SetActive(isgameover);
		buttonRestart.SetActive(isgameover);
		if (!isgameover)
			blockHighScore.SetActive(false);
		GetComponent<UIProcs> ().HideWaiter ();
	}

	public void SetGamePause (bool ispaused){
		gamePause = ispaused;
	}

	public void FirstOpen(){
		firstOpen = true;
	}

	public bool IsFirstOpen(){
		return firstOpen;
	}

	public bool IsPaused(){
		return gamePause;
	}

	string TimeToText(int time){
		int tMin = Mathf.FloorToInt (time / 60);
		int tSec = time % 60;
		return string.Format("{0,2:00}:{1,2:00}", tMin, tSec);
	}

	void FillScoreText(){
		textEasy.GetComponent<Text> ().text = TimeToText(scoreData.Easy);
		textNormal.GetComponent<Text> ().text = TimeToText(scoreData.Normal);
		textHard.GetComponent<Text> ().text = TimeToText(scoreData.Hard);
		textEndless.GetComponent<Text> ().text = scoreData.Endless.ToString();
	}

	public void ResetScores(){
		scoreData.Easy = 59999;
		scoreData.Normal = 59999;
		scoreData.Hard = 59999;
		scoreData.Endless = 0;

		FillScoreText ();
		
		FileManager fileManager = GetComponent<FileManager> ();
		fileManager.SaveScores (scoreData);
	}

	public void LoadScore(){
		FileManager fileManager = GetComponent<FileManager> ();
		scoreData = fileManager.LoadScores ();

		FillScoreText ();
	}

	public void ShowHighscoreBlock(string hsText){
		blockHighScore.SetActive(true);
		valueHighScore.GetComponent<Text> ().text = hsText;
	}

	public void SaveScore(){
		if (uiProcs.IsEndlessGame ()) {
			if (openedCells > scoreData.Endless){
				scoreData.Endless = openedCells;
				ShowHighscoreBlock(scoreData.Endless.ToString ());
			}
		} else {
			int currTime = Mathf.FloorToInt (time);

			switch (fieldProcs.currLevel) {
				case 1:{
					if (currTime < scoreData.Easy){
						scoreData.Easy = currTime;
						ShowHighscoreBlock(TimeToText (scoreData.Easy));						
					}
				}break;
				case 2:{
					if (currTime < scoreData.Normal){
						scoreData.Normal = currTime;
						ShowHighscoreBlock(TimeToText (scoreData.Normal));
					}
				}break;
				case 3:{
					if (currTime < scoreData.Hard){
						scoreData.Hard = currTime;
						ShowHighscoreBlock(TimeToText (scoreData.Hard));
					}
				}break;
			}
		}

		FillScoreText ();

		FileManager fileManager = GetComponent<FileManager> ();
		fileManager.SaveScores (scoreData);
	}

	public bool GetWin(){
		return winState;
	}

	public bool GetGameOver(){
		return gameOver;
	}

	void UpdateTimer(int timeSeconds){
		int tMin = Mathf.FloorToInt (timeSeconds / 60);
		int tSec = timeSeconds % 60;
		textTimer.GetComponent<Text> ().text = string.Format("{0,2:00}:{1,2:00}", tMin, tSec);
	}

	public void ResetFieldMover(bool isEndless){
		SetGameOver (false);
		SetWin (false);
		UpdateTimer(0);

		time = 0;
		openedCells = 0;
		firstOpen = false;
		movingFieldStart = false;

		minBound = mainCamera.ScreenToWorldPoint (new Vector3 (mainCamera.pixelRect.x, mainCamera.pixelRect.y, 0)); 
		maxBound = mainCamera.ScreenToWorldPoint (new Vector3 (mainCamera.pixelRect.width, mainCamera.pixelRect.height, 0));

		fieldProcs = fieldTransform.gameObject.GetComponent<FieldProcs> ();
		bottomMenuSize = fieldProcs.triSideLen * 1.1f;
		
		if (fieldProcs != null) {
			if (isEndless) {
				minBound.x = minBound.z = -2000000;
				maxBound.x = maxBound.z = 2000000;
			}else{
				float fieldWidth = (fieldProcs.GetFieldSize() + 0.5f) * fieldProcs.triSideLen;
				float fieldHeight = fieldProcs.GetFieldSize() * fieldProcs.triHeight + bottomMenuSize;
				
				if (minBound.x > -fieldWidth) 
					minBound.x = - (minBound.x + fieldWidth); 
				else
					minBound.x = (minBound.x + fieldWidth);
				
				if (maxBound.x < fieldWidth) 
					maxBound.x = - (maxBound.x - fieldWidth);
				else
					maxBound.x = (maxBound.x - fieldWidth);
				
				if (minBound.z > -fieldHeight) 
					minBound.z = - (minBound.z + fieldHeight);
				else
					minBound.z = (minBound.z + fieldHeight);
				
				if (maxBound.z < fieldHeight) 
					maxBound.z = - (maxBound.z - fieldHeight);
				else
					maxBound.z = (maxBound.z - fieldHeight);
			}
			fieldTransform.position = new Vector3((minBound.x + maxBound.x) / 2.0f, 0, (minBound.z + maxBound.z) / 2.0f);
		}
	}

	void Start() {
		textVersion.GetComponent<Text> ().text = "1.1";
		scoreData = new ScoreData();
		LoadScore ();
	}

	void Update() {
		if (Application.platform == RuntimePlatform.Android) {
			if (Input.GetKeyDown (KeyCode.Escape))
				uiProcs.backButtonPress ();

			if (Input.GetKeyDown(KeyCode.Menu)) 
				uiProcs.menuButtonPress ();
		}

		if (gamePause) return;

		if (!gameOver && !winState && firstOpen) {
			time += Time.deltaTime;
			UpdateTimer(Mathf.RoundToInt(time));
		}

		/*if ((Input.touchCount >= 2) && 
		    ((Input.GetTouch(0).phase == TouchPhase.Moved) || (Input.GetTouch(1).phase == TouchPhase.Moved))) {
			Vector2 t0 = Input.GetTouch(0).position;
			Vector2 t1 = Input.GetTouch(1).position;
			Vector2 diff_vec = t0 - t1;
			float dist = Mathf.Sqrt(diff_vec.x*diff_vec.x + diff_vec.y*diff_vec.y);
			if (Mathf.Abs (touch_dist) > 0){
				float diff = dist - touch_dist;
				Vector3 single_vec = new Vector3(1,1,1);
				transform.parent.localScale += (single_vec * diff/100);
				touch_dist = dist;
			}else{
				touch_dist = dist;
			}
		}else{
			touch_dist = 0;
		}*/

		if (Input.GetButtonDown ("Fire1")) {
			if (Input.mousePosition.y > 60){
				mousePressed = true;
				mouseDownPos = Input.mousePosition;
				prevFieldPos = fieldTransform.position;
			}
			movingFieldStart = false;
		}

		if (Input.GetButtonUp ("Fire1")) {
			mousePressed = false;
			movingFieldStart = false;
		}

		if (mousePressed) {
			if ((Mathf.Abs(mouseDownPos.x - Input.mousePosition.x) > 15) || 
			    (Mathf.Abs(mouseDownPos.y - Input.mousePosition.y) > 15) || (movingFieldStart)){
				movingFieldStart = true;

				GameObject currCell = fieldProcs.GetCurrentCell();
				if (currCell != null){
					OpenCell currOpenCell = currCell.GetComponentInChildren<OpenCell>();
					if (currOpenCell != null)
						currOpenCell.CancelOpen();
				}
			
				Vector3 old_pos = mainCamera.ScreenToWorldPoint(mouseDownPos);
				Vector3 new_pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
				/*Vector3 newPosition = new Vector3 (prevFieldPos.x - (mouseDownPos.x - Input.mousePosition.x) / 10,
	        										0,
	        										prevFieldPos.z - (mouseDownPos.y - Input.mousePosition.y) / 10);*/
				Vector3 newPosition = new Vector3 (prevFieldPos.x - (old_pos.x - new_pos.x),
				                                   0,
				                                   prevFieldPos.z - (old_pos.z - new_pos.z));

				if (newPosition.x < minBound.x)  newPosition.x = minBound.x;
				if (newPosition.z < minBound.z)  newPosition.z = minBound.z;
				if (newPosition.x > maxBound.x)  newPosition.x = maxBound.x;
				if (newPosition.z > maxBound.z)  newPosition.z = maxBound.z;

				fieldTransform.position = newPosition;
			}
		}
	}
}
