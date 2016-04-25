using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LogoShow : MonoBehaviour {

	public Image logo;
	bool Inverse;
	float HideTime;

	// Use this for initialization
	void Start () {
		Color logoColor = logo.color;
		logoColor.a = 0;
		logo.color = logoColor;

		Inverse = false;
		HideTime = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!Inverse) {
			if (logo.color.a < 1) {
				Color logoColor = logo.color;
				logoColor.a += 0.01f;
				logo.color = logoColor;
			} else 
				Inverse = true;
		} else {
			if (logo.color.a > 0) {
				Color logoColor = logo.color;
				logoColor.a -= 0.01f;
				logo.color = logoColor;
				if (logo.color.a <= 0.02) HideTime = Time.time;
			}
		};
		if ((HideTime != 0) && (Time.time - HideTime > 0.10f))
			Application.LoadLevel ("MainScene");
	}
}
