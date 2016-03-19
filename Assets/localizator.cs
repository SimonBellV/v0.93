using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SmartLocalization;

public class localizator : MonoBehaviour {
	void OnGUI()
	{
		GameObject.Find ("Version1").GetComponentInChildren<Text> ().text = LanguageManager.Instance.GetTextValue ("Version 1");

		GameObject.Find ("Version2").GetComponentInChildren<Text> ().text = LanguageManager.Instance.GetTextValue ("Version 2");

		GameObject.Find ("Version3").GetComponentInChildren<Text> ().text = LanguageManager.Instance.GetTextValue ("Version 3");

		if (GUILayout.Button ("English")) {
			GetComponentInChildren<AudioSource> ().Play();
			LanguageManager.Instance.ChangeLanguage ("en");
		}
		if (GUILayout.Button ("Русский")) {
			GetComponentInChildren<AudioSource> ().Play();
			LanguageManager.Instance.ChangeLanguage ("ru");
		}
	}
}
