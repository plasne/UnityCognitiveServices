using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class QRDetect : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // per: http://answers.unity3d.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }

    /*
    private IEnumerator Fetch()
    {
        Texture2D texture = (Texture2D)Resources.Load("funeral-qr-code", typeof(Texture2D));

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Ocp-Apim-Subscription-Key", key);
        headers.Add("Content-Type", "application/octet-stream");
        byte[] data = texture.GetRawTextureData();
        var client = new WWW("https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Description,Tags", data, headers);
        string s = client.text;
        yield return client;
    }
    */

    [Serializable]
    public class Prediction
    {
        public string TagId;
        public string Tag;
        public float Probability;
    }

    [Serializable]
    public class TrainedResponse
    {
        public string Id;
        public string Project;
        public string Iteration;
        public string Created;
        public List<Prediction> Predictions;
    }

    public IEnumerator Detect(Texture2D texture)
    {

        bool finished = false;
        bool isQR = false;

        //StartCoroutine( Fetch());

        //Texture2D texture = (Texture2D)Resources.Load("funeral-qr-code", typeof(Texture2D));

        using (WebClient client = new WebClient())
        {
            client.Headers.Add("Prediction-Key", "4c4370fcac684e9687e46121d7dacc30");
            client.Headers.Add("Content-Type", "application/octet-stream");
            client.UploadDataCompleted += (object sender, UploadDataCompletedEventArgs e) =>
            {
                string response_string = System.Text.Encoding.Default.GetString(e.Result);
                TrainedResponse response_object = JsonUtility.FromJson<TrainedResponse>(response_string);
                if (response_object.Predictions.Count > 0)
                {
                    Prediction most_likely = response_object.Predictions[0];
                    isQR = (most_likely.Tag == "QR" && most_likely.Probability >= 0.7f);
                }
                Debug.Log("isQR? " + isQR);
                finished = true;
            };
            client.UploadDataAsync(new System.Uri("https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/02747810-f650-4ed2-9344-625bd93dd126/image"), texture.EncodeToPNG());
            yield return new WaitUntil(() => finished);
        }


    }

}
