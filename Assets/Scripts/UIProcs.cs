using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIProcs : MonoBehaviour {

	public GameObject Field;
	public GameObject TitlePanel;
	public GameObject MenuWindow;
	public GameObject ScoreWindow;
	public GameObject InfoWindow;
	public GameObject HelpWindow;
	public GameObject CustomGameWindow;
	public AdMobPlugin AdMob;

	public GameObject CustomFieldRadius;
	public GameObject CustomMinesPercent;
	public Text FieldRadiusValueText;
	public Text MinesPercentValueText;

	public GameObject Waiter;
	public GameObject FlagsCounterText;

	public GameObject MenuButton;
	public Sprite menuSprite;
	public Sprite playSprite;

	public GameObject mineIcon;
	public Sprite mineSprite;
	public Sprite triSprite;

	private bool currGameIsEndless;

	float WaiterTime = 0;
	bool WaiterVisible = false;
	int flagsCount = 0;
	float WaiterTimeLen = 0.75f;

	public FieldProcs fieldProcs;
	public FieldMover fieldMover;

	public void ResetFlagsCount(){
		flagsCount = fieldProcs.GetMinesCount ();
		if (currGameIsEndless)
			flagsCount = 0;
		FlagsCounterText.GetComponent<Text> ().text = flagsCount.ToString ();
	}

	public bool IsEndlessGame(){
		return currGameIsEndless;
	}

	public void IncOpenedCells(){
		fieldMover.openedCells++;
		if (currGameIsEndless)
			FlagsCounterText.GetComponent<Text> ().text = fieldMover.openedCells.ToString ();
	}

	public bool ChangeFlagsCount(int change_value){
		if (flagsCount + change_value < 0) return false;
		if (flagsCount + change_value > 9999) return false;
		flagsCount += change_value;
		if (!currGameIsEndless)
			FlagsCounterText.GetComponent<Text> ().text = flagsCount.ToString ();
		return true;
	}

	public void updateCustomGameValues(){
		FieldRadiusValueText.text = CustomFieldRadius.GetComponent<Slider> ().value.ToString();
		MinesPercentValueText.text = CustomMinesPercent.GetComponent<Slider> ().value.ToString() + '%';
	}

	public void resetGame(){
		if (currGameIsEndless) {
			mineIcon.GetComponent<Image>().sprite = triSprite;
		} else {
			mineIcon.GetComponent<Image>().sprite = mineSprite;
		}
		fieldProcs.BuildField (currGameIsEndless);
		fieldMover.ResetFieldMover (currGameIsEndless);
		ResetFlagsCount ();
		hideMenu ();
		fieldProcs.StartGame ();  
		MenuButton.SetActive (true);
		AdMob.Show();
	}

	public void exitGame(){
		fieldProcs.ClearBlocks ();
		Application.Quit ();
	}

	public void newGame(int level){
		currGameIsEndless = false;
		fieldProcs.SetLevel (level);
		resetGame ();
	}

	public void newEndlessGame(int level){
		currGameIsEndless = true;
		fieldProcs.SetLevel (level);
		resetGame ();
	}

	public void newCustomGame(){
		currGameIsEndless = false;
		int fieldRadius = Mathf.RoundToInt(CustomFieldRadius.GetComponent<Slider>().value);
		int minesPercent = Mathf.RoundToInt(CustomMinesPercent.GetComponent<Slider>().value);
		fieldProcs.SetCustomLevel (fieldRadius, minesPercent);
		resetGame ();
	}

	public void pauseGame(bool isPause){
		fieldMover.SetGamePause (isPause);
		if (isPause)
			MenuButton.GetComponent<Image>().sprite = playSprite;
		else
			MenuButton.GetComponent<Image>().sprite = menuSprite;
	}

	public void backButtonPress(){
		if (InfoWindow.activeSelf || ScoreWindow.activeSelf || HelpWindow.activeSelf || CustomGameWindow.activeSelf) {
			ScoreWindow.SetActive (false);
			InfoWindow.SetActive (false);
			HelpWindow.SetActive (false);
			CustomGameWindow.SetActive(false);
			MenuWindow.SetActive (true);
		} else {
			if (fieldMover.IsPaused()){
				pauseGame (false);
				MenuWindow.SetActive (false);
				TitlePanel.SetActive (false);
				AdMob.Show();
			}
		}
	}

	public void hideMenu(){
		ScoreWindow.SetActive (false);
		MenuWindow.SetActive (false);
		InfoWindow.SetActive (false);
		TitlePanel.SetActive (false);
		HelpWindow.SetActive (false);
		CustomGameWindow.SetActive(false);
	}

	public void customGameButtonPress(){
		CustomGameWindow.SetActive(true);
		MenuWindow.SetActive (false);
	}

	public void scoresButtonPress(){
		ScoreWindow.SetActive (true);
		MenuWindow.SetActive (false);
	}

	public void infoButtonPress(){
		InfoWindow.SetActive (true);
		MenuWindow.SetActive (false);
		ScoreWindow.SetActive (false);
		HelpWindow.SetActive (false);
		CustomGameWindow.SetActive(false);
	}

	public void helpButtonPress(){
		HelpWindow.SetActive (true);
		InfoWindow.SetActive (false);
		MenuWindow.SetActive (false);
		ScoreWindow.SetActive (false);
		CustomGameWindow.SetActive(false);
	}

	public void menuButtonPress(){
		if (fieldProcs.GetGameStarted ()) {
			bool gamePaused = fieldMover.IsPaused ();

			pauseGame (!gamePaused);

			ScoreWindow.SetActive (false);
			InfoWindow.SetActive (false);
			HelpWindow.SetActive (false);
			CustomGameWindow.SetActive (false);
			MenuWindow.SetActive (!gamePaused);
			TitlePanel.SetActive (!gamePaused);

			if (!gamePaused)
				AdMob.Hide();
			else
				AdMob.Show();
		}
	}

	public void ShowWaiter(float x, float z, float time){
		WaiterVisible = true;
		WaiterTime = 0;
		WaiterTimeLen = time;
		Vector3 pos = Waiter.transform.position;
		pos.x = x;
		pos.z = z;
		Waiter.transform.position = pos;
	}

	public void HideWaiter(){
		WaiterVisible = false;
		Waiter.SetActive (false);
	}

	public bool isWaiterActive(){
		return Waiter.activeSelf;
	}

	void Start(){
		AdMob.Load ();
		AdMob.Hide ();
	}

	void FixedUpdate() {
		if (WaiterVisible) {
			WaiterTime += Time.deltaTime;
			if (WaiterTime > 0.25f) {
				Waiter.SetActive (true);
			}
			if (WaiterTime > WaiterTimeLen) {
				WaiterVisible = false;
				Waiter.SetActive (false);
			}
			float ox, oy;
			ox = Mathf.FloorToInt (WaiterTime / WaiterTimeLen * 16) % 4 * 0.25f;
			oy = 0.75f - Mathf.FloorToInt (WaiterTime / WaiterTimeLen / 0.25f) % 4 * 0.25f;
			Waiter.GetComponent<Renderer>().materials [0].SetTextureOffset ("_MainTex", new Vector2 (ox, oy));
		}
	}		
}
