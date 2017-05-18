using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

//https://www.packtpub.com/books/content/using-rest-api-unity-part-1-what-rest-and-basic-queries
namespace Assets.Scripts.Helper
{
    [Serializable]
    public class FaceApiResultItemCollection
    {
        public List<FaceApiResultItem> FaceApiResultItems;
    }

    [Serializable]
    public class FaceApiResultItem
    {
        public string faceId;
        public FaceRectangle faceRectangle;
        public FaceAttributes faceAttributes;

    }
    [Serializable]
    public class FaceRectangle
    {
        public float top;
        public float left;
        public float width;
        public float height;
    }

    [Serializable]
    public class FaceAttributes
    {
        public string gender;
        public string glasses;
        public float age;
        public FacialHair facialHair;
        public Emotion emotion;
        

    }
    [Serializable]
    public class Emotion
    {

        public float anger;
        public float contempt;
        public float disgust;
        public float fear;
        public float happiness;
        public float neutral;
        public float sadness;
        public float surprise;
    }

    [Serializable]
    public class FacialHair
    {
        public float moustache;
        public float beard;
        public float sideburns;
    }


   
    

    public class FaceApiHelper
    {

        private readonly MonoBehaviour _caller;
        private readonly string _subscriptionKey;
        private readonly string _queryString = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,facialhair,glasses,emotion";//,

        private string uri = "https://{location}.api.cognitive.microsoft.com/face/v1.0/";
        private string _results;

        private Dictionary<string, string> _headers;
        public String Results
        {
            get
            {
                return _results;
            }
        }



        public FaceApiHelper(MonoBehaviour caller, string location, string subscriptionKey)
        {
            uri = uri.Replace("{location}", location);
            _caller = caller;
            _subscriptionKey = subscriptionKey;
            _headers =
                new Dictionary<string, string>
                {
                    {"Ocp-Apim-Subscription-Key", _subscriptionKey},
                    {"Content-Type", "application/octet-stream"}
                };

        }


        internal WWW Detect(byte[] imageByteData, Action onCompleteCallback)
        {

            var requesturi = uri + "detect?" + _queryString;
            var www = new WWW(requesturi, imageByteData, _headers);
            _caller.StartCoroutine(WaitForRequest(www, onCompleteCallback));
            return www;
        }

        public WWW Detect(string imageFilePath, System.Action onCompleteCallback)
        {
            var requesturi = uri + "detect?" + _queryString;
            var byteData = GetImageAsByteArray(imageFilePath);
            var www = new WWW(requesturi, byteData, _headers);
            _caller.StartCoroutine(WaitForRequest(www, onCompleteCallback));
            return www;

        }
        private byte[] GetImageAsByteArray(string imageFilePath)
        {
            var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
        private IEnumerator WaitForRequest(WWW www, System.Action onComplete)
        {
            yield return www;
            // check for errors
            if (string.IsNullOrEmpty(www.error))
            {
                _results = www.text;
                onComplete();
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}
