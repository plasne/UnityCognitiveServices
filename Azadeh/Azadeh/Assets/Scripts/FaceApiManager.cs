using System;
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

            var index = 0;
            var xPosition = 55f;
            var yPosition = 0f;

            foreach (var item in resultObject.FaceApiResultItems)
            {

                xPosition = (index % 3 == 0) ? 55f : xPosition + 100f;
                yPosition = yPosition + ((index % 3 == 0) ? 100f : 0); 

                index = index + 1;

                if (item.faceAttributes.gender == "female")
                {

                    var f = Instantiate(Female) as GameObject;

                    f.name = string.Format("female-{0}", item.faceId);
                    f.transform.SetParent(canvas.transform);
                    f.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    f.transform.localScale = new Vector3(10.0f, 10.0f, 0.0f);
                    f.GetComponent<Character>().Age = item.faceAttributes.age;


                }
                else if (item.faceAttributes.gender == "male")
                {
                    var m = Instantiate(Male) as GameObject;
                    m.name = string.Format("male-{0}", item.faceId);
                    m.transform.SetParent(canvas.transform);
                    m.transform.localPosition = new Vector3(xPosition, yPosition, 0f);
                    m.transform.localScale = new Vector3(10.0f, 10.0f, 0.0f);

                    m.GetComponent<Character>().Age = item.faceAttributes.age;

                }

            }
        }
        _webCamTexture.Play();
    }




}
