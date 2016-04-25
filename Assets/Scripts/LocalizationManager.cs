using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SmartLocalization;

public class LocalizationManager : MonoBehaviour {

	private LanguageManager langManager;

	public Text textWin;
	public Text textGameOver;
	public Text textHighScore;
	public Text textMenuEasy;
	public Text textMenuNormal;
	public Text textMenuHard;
	public Text textMenuCustom;
	public Text textMenuEndless;
	public Text textMenuScores;
	public Text textInfoDev;
	public Text textInfoVersion;
	public Text textScoresEasy;
	public Text textScoresNormal;
	public Text textScoresHard;
	public Text textScoresEndless;
	public Text textCustomFieldRadius;
	public Text textCustomMinesPercent;
	public Text textCustomPlay;

	// Use this for initialization
	void Start () {
		langManager = LanguageManager.Instance;
		LanguageManager.Instance.OnChangeLanguage += OnLanguageChanged;

		switch (Application.systemLanguage) {
			case SystemLanguage.Russian: langManager.ChangeLanguage("ru"); break;
			case SystemLanguage.German: langManager.ChangeLanguage("de"); break;
			default: langManager.ChangeLanguage("en"); break;
		}
	}

	void OnLanguageChanged(LanguageManager thisLanguageManager)
	{
		textWin.text = langManager.GetTextValue("gameover.win");
		textGameOver.text = langManager.GetTextValue("gameover.lose");
		textHighScore.text = langManager.GetTextValue("gameover.highscore");
		textMenuEasy.text = langManager.GetTextValue("menu.easy");
		textMenuNormal.text = langManager.GetTextValue("menu.normal");
		textMenuHard.text = langManager.GetTextValue("menu.hard");
		textMenuCustom.text = langManager.GetTextValue("menu.custom");
		textMenuEndless.text = langManager.GetTextValue("menu.endless");
		textMenuScores.text = langManager.GetTextValue("menu.scores");
		textInfoDev.text = langManager.GetTextValue("info.developers");
		textInfoVersion.text = langManager.GetTextValue("info.version");
		textScoresEasy.text = langManager.GetTextValue("menu.easy");
		textScoresNormal.text = langManager.GetTextValue("menu.normal");
		textScoresHard.text = langManager.GetTextValue("menu.hard");
		textScoresEndless.text = langManager.GetTextValue("menu.endless");
		textCustomFieldRadius.text = langManager.GetTextValue("custom.fieldradius");
		textCustomMinesPercent.text = langManager.GetTextValue("custom.minespercent");
		textCustomPlay.text = langManager.GetTextValue("custom.play");
	}

	void OnDestroy(){
		if (LanguageManager.HasInstance)
			LanguageManager.Instance.OnChangeLanguage -= OnLanguageChanged;
	}
}
