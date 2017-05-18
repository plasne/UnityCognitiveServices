using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Helper;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.WebCam;


public class FaceApiManager : MonoBehaviour
{
    //cognetive services api settings
    public string Location;
    public string SubscriptionKey;



    //settings
    public InputField Status;
    public GameObject Male;
    public GameObject Female;
    public RawImage Preview;


    private WebCamTexture _webCamTexture;
    private FaceApiHelper _faceApiHelper;

    // Use this for initialization
    void Start()
    {
        _faceApiHelper = new FaceApiHelper(this, Location, SubscriptionKey);


        //option 1
        //var imageFilePath = Application.dataPath + @"\Images\Cookie.jpg";
        //_faceApiHelper.Detect(imageFilePath, Callback);


        //webcam
        _webCamTexture = new WebCamTexture();
        Preview.material.mainTexture = _webCamTexture;
        _webCamTexture.Play();

    }


    void Update()
    {
        //This is to take the picture, save it and stop capturing the camera image.
        if (_webCamTexture.isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            _webCamTexture.Pause();
            Texture2D snap = new Texture2D(_webCamTexture.width, _webCamTexture.height);
            snap.SetPixels(_webCamTexture.GetPixels());
            snap.Apply();
            var bytes = snap.EncodeToPNG();
            _faceApiHelper.Detect(bytes, Callback);


        }

    }

    public void Callback()
    {

        //log results
        var result = _faceApiHelper.Results;
        if (!string.IsNullOrEmpty(result))
        {
            Debug.Log(result);
            if (Status != null)
            {
                if (Status.text.Length > 9000)
                {
                    Status.text = "";
                }
                Status.text = Status.text + (!string.IsNullOrEmpty(Status.text) ? Environment.NewLine : "") + result;
            }
            //it's a hack there should be a better way
            var temp = "{\"FaceApiResultItems\":" + result + "}";

            //add objects
            var resultObject = JsonUtility.FromJson<FaceApiResultItemCollection>(temp);
            var canvas = GameObject.Find("EmojiCanvas");

            //remove prefabs from the canvas
            if (canvas.transform.childCount > 0)
            {
                var count = canvas.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    var tmpobj = canvas.transform.GetChild(0).gameObject;
                    tmpobj.transform.SetParent(null);
                    DestroyObject(tmpobj);
                }
            }

            var index = 1;
            var xPosition = -50f;
            var yPosition = 0f;

            foreach (var item in resultObject.FaceApiResultItems)
            {

                xPosition = (index % 3 == 0) ? 55f : xPosition + 100f;
                yPosition = yPosition - ((index % 3 == 0) ? 50f : 0);

                index = index + 1;
                var characterPrefab = Resources.Load("character") as GameObject;
                var character = GameObject.Instantiate(characterPrefab);
                if (character == null)
                {

                    return;
                }
                character.name = string.Format("{0}", item.faceId);
                character.transform.SetParent(canvas.transform);
                character.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                character.transform.localScale = new Vector3(10f, 10f, 0.0f);


                //if (item.faceAttributes.gender == "female")
                //{

                //    character = Instantiate(Female) as GameObject;

                //    character.name = string.Format("female-{0}", item.faceId);



                //}
                //else
                //{
                //    character = Instantiate(Male) as GameObject;
                //    character.name = string.Format("male-{0}", item.faceId);
                //}


                character.GetComponent<Character>().Gender = item.faceAttributes.gender;
                character.GetComponent<Character>().Age = item.faceAttributes.age;
                character.GetComponent<Character>().HasGlasses = (item.faceAttributes.glasses != "NoGlasses");

                var bestEmotion = item.faceAttributes.emotion.GetType().GetFields()
                    .OrderByDescending(e => e.GetValue(item.faceAttributes.emotion)).FirstOrDefault();
                if (bestEmotion != null)
                {
                    character.GetComponent<Character>().Emotion = bestEmotion.Name;
                    character.GetComponent<Character>().EmotionValue = (float)bestEmotion.GetValue(item.faceAttributes.emotion);

                }

            }
        }
        _webCamTexture.Play();
    }




}
