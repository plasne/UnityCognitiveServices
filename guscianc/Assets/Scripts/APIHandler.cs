using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APIHandler : MonoBehaviour {

	//Singleton
	public static APIHandler instance = null;
	public static APIHandler Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<APIHandler>();
				if (instance == null)
				{
					GameObject go = new GameObject();
					go.name = "SingletonControlle";
					instance = go.AddComponent<APIHandler>();
					DontDestroyOnLoad(go);
				}
				else
				{
					Debug.Log("[Singleton] Using instance already created!");
				}
			}
			return instance;
		}
	}



	//Const variables
	private const string TEXT_TRANSLATION_ENDPOINT = "https://api.microsofttranslator.com/v2/http.svc";
	private const string TEXT_TRANSLATION_AUTH = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
	private const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "54cf1d0363fb4a65a7de55a0764b093d";
	private const string LUIS_ENDPOINT = "https://westus.api.cognitive.microsoft.com/luis/v2.0/";
	private const string LUIS_IT_MODEL_APPID = "575be7a1-87eb-45aa-9001-f86494d630fc";
	private const string LUIS_EN_MODEL_APPID = "74a6ff8b-cd9b-4b5d-bdb0-14ee49a14c05";
	private const string LUIS_SUBSCRIPTION_KEY_IT = "1f9df5ce97d347d69d68d26759d092bd";
	private const string LUIS_SUBSCRIPTION_KEY_EN = "565197d3f8cc46efb4f676c8a7c6ff9a";
	//Static variables
	public static string authorizationToken = null; 
	public static string detectedLanguageCode;


	//Singleton implementation
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	//Translator API 

	//Get Authorization to run the Translation Text API
	public IEnumerator GetTranslationAuth()
	{
		//Checks if the variable is empty
		if (string.IsNullOrEmpty(authorizationToken))
		{
			string authToken = null;
			WWWForm form = new WWWForm();
			//API Request
			string uri = TEXT_TRANSLATION_AUTH + "?Subscription-Key=" + TEXT_TRANSLATION_API_SUBSCRIPTION_KEY;
			WWW web = new WWW(uri, form);
			while (!web.isDone)
			{
				yield return web;
			}
			//Saving the Auth Token
			authToken = web.text;
			Debug.Log("Authorization granted: " + authToken);

			//Sets the global variable
			authorizationToken = authToken;
		}
	}


	//Detect language used in the text
	public IEnumerator DetectTextLanguage(string inputString)
	{
		//Checks if the authorization is already been done
		if (string.IsNullOrEmpty(authorizationToken))
		{
			StartCoroutine(GetTranslationAuth());
			yield return new WaitForSeconds(3);
			Debug.Log("Auth Token aquired!");
		}
		else
		{
			Debug.Log("Auth Token already aquired!");
		}

		//Initialize languageCode variable
		string languageCode = null;

		//API Request
		string uri = TEXT_TRANSLATION_ENDPOINT + "/Detect?appid=Bearer " + authorizationToken + "&text=" + inputString;
		WWW web = new WWW(uri);
		while (!web.isDone)
		{
			yield return web;
		}
		string result = web.text;

		//Gets the language code
		languageCode = result.Substring(68, 2);
		Debug.Log("Language detected: " + languageCode);

		//Sets the global variable
		detectedLanguageCode = languageCode;

		GameObject.FindGameObjectWithTag("LangDetection").GetComponent<Text>().text = detectedLanguageCode;
	}

	//Translation from one language to another
	public IEnumerator TranslateFromTo(string inputString, string languageTo)
	{
		string translation,result;
		string[] temp;
		//Checks if the authorization is already been done
		if (string.IsNullOrEmpty(authorizationToken))
		{
			StartCoroutine(GetTranslationAuth());
			yield return new WaitForSeconds(3);
			Debug.Log("Auth Token aquired!");
		}
		else
		{
			Debug.Log("Auth Token already aquired!");
		}

		//API Request
		string uri = TEXT_TRANSLATION_ENDPOINT + "/Translate?appid=Bearer " + authorizationToken + "&text=" + inputString + "&to=" + languageTo;

		if (string.IsNullOrEmpty(inputString) || string.IsNullOrEmpty(languageTo))
		{
			Debug.Log("ERROR: input string or language to parameters are missing!");
		}
		else
		{
			WWW web = new WWW(uri);
			while (!web.isDone)
			{
				yield return web;
			}
			//Saves the result
			result = web.text;
			//Editing output string
			int remaining = result.Length - 68; //takes the last part of the output
			result = result.Substring(68, remaining);
			temp = result.Split(new string[] { "<" }, StringSplitOptions.None);
			translation = temp[0];
			Debug.Log("The translation is: " + translation);
			GameObject.FindGameObjectWithTag("Translation").GetComponent<Text>().text = translation;
		}		
	}


	//LUIS API
	public IEnumerator LUISModelCall(string inputText)
	{
		string result;
		//Checks if the authorization is already been done
		if (!string.IsNullOrEmpty(inputText))
		{
			//First detect language
			StartCoroutine(DetectTextLanguage(inputText));
			yield return new WaitForSeconds(6);
			Debug.Log("Language detected...");
			WWWForm form = new WWWForm();

			if (string.IsNullOrEmpty(detectedLanguageCode))
			{
				Debug.Log("Detected language is empty!");
			}
			else
			{
				switch (detectedLanguageCode)
				{
					case "en":
						//API Request
						string uri_en = LUIS_ENDPOINT + "apps/" + LUIS_EN_MODEL_APPID + "?subscription-key=" + LUIS_SUBSCRIPTION_KEY_EN + "&timezoneOffset=0&verbose=true&q=" + inputText;
						WWW web_en = new WWW(uri_en);
						while (!web_en.isDone)
						{
							yield return web_en;
						}
						//Saves the result
						result = web_en.text;
						Debug.Log("LUIS response: " + result);
						break;
					case "it":
						//API Request
						string uri_it = LUIS_ENDPOINT + "apps/" + LUIS_IT_MODEL_APPID + "?subscription-key=" + LUIS_SUBSCRIPTION_KEY_IT + "&timezoneOffset=0&verbose=true&q=" + inputText;
						WWW web_it = new WWW(uri_it);
						while (!web_it.isDone)
						{
							yield return web_it;
						}
						//Saves the result
						result = web_it.text;
						LUIS luis_response = JsonUtility.FromJson<LUIS>(result);
						Debug.Log("LUIS response: " + luis_response.topScoringIntent);
						break;
				}
			}
		}

	}

}
