using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour {

	string inputBox;
	string languageTo;
	public Dropdown dropdown;
	public Text translation;
	public Text langDetected;
	public static int valueSelected = -1;

	//When the Send Button is clicked
	public void onClick()
	{
		//Finds the input field in the scene
		inputBox = GameObject.FindGameObjectWithTag("InputBox").GetComponent<InputField>().text;
		languageTo = GameObject.FindGameObjectWithTag("LanguageTo").GetComponent<InputField>().text;
		Debug.Log("Text: " + inputBox);
		Debug.Log("Translation language code: " + languageTo);

		//Detects the action selected from the dropdown
		if (!string.IsNullOrEmpty(inputBox))
		{
			//Select action:
			switch (valueSelected)
			{
				//Translation case
				case 1:
					StartCoroutine(APIHandler.instance.TranslateFromTo(inputBox, languageTo));
					break;
				//Detection of the language
				case 2:
					StartCoroutine(APIHandler.instance.DetectTextLanguage(inputBox));
					break;
				case 3:
					StartCoroutine(APIHandler.instance.LUISModelCall(inputBox));
					break;
				//No action required
				default:
					Debug.Log("No selection...");
					break;
			}
		}

		//StartCoroutine(APIHandler.instance.GetTranslationAuth(inputBox));
	}

	//Selects the value of the dropdown menu
	public void onValueChanged()
	{
		valueSelected = dropdown.value;
		switch (valueSelected) {
			case 1:
				Debug.Log("Translate!");
				break;
			case 2:
				Debug.Log("Detect Language");
				break;
			case 3:
				Debug.Log("Sending text to LUIS...");
				break;
			default:
				Debug.Log("No selection done");
				break;
		}
	}


}
