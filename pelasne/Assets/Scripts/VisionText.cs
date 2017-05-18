using Promises;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class VisionText : Vision
{

    // public
    public string VisionKey;
    public string TranslationKey;
    public string Region;
    public string TranslateTo;

    // local
    private string accessToken;
    private DateTime lastToken = DateTime.MinValue;

    private void AddCertificateChainValidation()
    {
        ServicePointManager.ServerCertificateValidationCallback += (System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
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
        };
    }

    [Serializable]
    [XmlRoot("xml")]
    public class TranslationResponse
    {
        [XmlElement("string", Namespace = "http://schemas.microsoft.com/2003/10/Serialization/")]
        public string text;
    }

    private Promise<string> QueryForTranslation(string data, string from, string to)
    {
        Deferred<string> deferred = new Deferred<string>();

        // register callback support (required for WebClient)
        if (ServicePointManager.ServerCertificateValidationCallback == null) AddCertificateChainValidation();

        // get an access token
        using (WebClient client = new WebClient())
        {
            //client.Headers.Add("Authorization", "Bearer " + accessToken);
            client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs e) =>
            {
                try
                {
                    if (e.Cancelled)
                    {
                        _status = StatusOptions.failed;
                        deferred.Reject("web call was cancelled.");
                    }
                    else if (e.Error != null)
                    {
                        _status = StatusOptions.failed;
                        deferred.Reject(e.Error.Message);
                    }
                    else
                    {
                        string response_string = System.Text.Encoding.Default.GetString(e.Result);
                        string encapsulated = "<xml>" + response_string + "</xml>";
                        XmlSerializer serializer = new XmlSerializer(typeof(TranslationResponse));
                        using (TextReader reader = new StringReader(encapsulated))
                        {
                            TranslationResponse result = (TranslationResponse) serializer.Deserialize(reader);
                            deferred.Resolve(result.text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _status = StatusOptions.failed;
                    deferred.Reject(ex.Message);
                }
                Reset();
            };
            string parameters = "appid=Bearer " + Uri.EscapeUriString(accessToken) + "&text=" + Uri.EscapeUriString(data) + "&from=" + Uri.EscapeUriString(from) + "&to=" + Uri.EscapeUriString(to);
            client.DownloadDataAsync(new System.Uri("https://api.microsofttranslator.com/v2/http.svc/Translate?" + parameters));
        }

        return deferred.Promise();
    }

    private Promise<string> QueryForAccessToken()
    {
        Deferred<string> deferred = new Deferred<string>();

        // register callback support (required for WebClient)
        if (ServicePointManager.ServerCertificateValidationCallback == null) AddCertificateChainValidation();

        // get an access token
        using (WebClient client = new WebClient())
        {
            client.Headers.Add("Ocp-Apim-Subscription-Key", TranslationKey);
            client.UploadDataCompleted += (object sender, UploadDataCompletedEventArgs e) =>
            {
                try
                {
                    if (e.Cancelled)
                    {
                        _status = StatusOptions.failed;
                        deferred.Reject("web call was cancelled.");
                    }
                    else if (e.Error != null)
                    {
                        _status = StatusOptions.failed;
                        deferred.Reject(e.Error.Message);
                    }
                    else
                    {
                        accessToken = System.Text.Encoding.Default.GetString(e.Result);
                        lastToken = DateTime.Now;
                        deferred.Resolve();
                    }
                }
                catch (Exception ex)
                {
                    _status = StatusOptions.failed;
                    deferred.Reject(ex.Message);
                }
                Reset();
            };
            byte[] buffer = { 0 };
            client.UploadDataAsync(new System.Uri("https://api.cognitive.microsoft.com/sts/v1.0/issueToken"), buffer);
        }

        return deferred.Promise();
    }

    [Serializable]
    private class TextResponseWord
    {
        public string boundingBox;
        public string text;
    }

    [Serializable]
    private class TextResponseLine
    {
        public string boundingBox;
        public List<TextResponseWord> words;
    }

    [Serializable]
    private class TextResponseRegion
    {
        public string boundingBox;
        public List<TextResponseLine> lines;
    }

    [Serializable]
    private class TextResponse
    {
        public string language;
        public string orientation;
        public List<TextResponseRegion> regions;
    }

    private void QueryForText(byte[] data, OnMessageHandler callback)
    {

        // register callback support (required for WebClient)
        if (ServicePointManager.ServerCertificateValidationCallback == null) AddCertificateChainValidation();

        // make the web call
        using (WebClient client = new WebClient())
        {
            client.Headers.Add("Ocp-Apim-Subscription-Key", VisionKey);
            client.Headers.Add("Content-Type", "application/octet-stream");
            client.UploadDataCompleted += (object sender, UploadDataCompletedEventArgs e) =>
            {
                try
                {
                    if (e.Cancelled)
                    {
                        _status = StatusOptions.failed;
                        RaiseError("web call was cancelled.", callback);
                    }
                    else if (e.Error != null)
                    {
                        _status = StatusOptions.failed;
                        RaiseError(e.Error.Message, callback);
                    }
                    else
                    {
                        string response_string = System.Text.Encoding.Default.GetString(e.Result);
                        TextResponse response_object = JsonUtility.FromJson<TextResponse>(response_string);
                        if (response_object.regions != null && response_object.regions.Count > 0)
                        {
                            List<string> lines = new List<string>();
                            response_object.regions.ForEach(region =>
                            {
                                region.lines.ForEach(line =>
                                {
                                    List<string> words = new List<string>();
                                    line.words.ForEach(word =>
                                    {
                                        words.Add(word.text);
                                    });
                                    lines.Add(string.Join(" ", words.ToArray()));
                                });
                            });

                            // translate if requested
                            if (!string.IsNullOrEmpty(TranslationKey) && !string.IsNullOrEmpty(TranslateTo) && TranslateTo != response_object.language)
                            {
                                Promise<string> accessToken_promise = QueryForAccessToken();
                                accessToken_promise.Done(() =>
                                {
                                    Promise<string> translation_promise = QueryForTranslation(string.Join(" ", lines.ToArray()), response_object.language, "en");
                                    translation_promise.Done((msg) =>
                                    {
                                        List<string> translated = new List<string>();
                                        translated.Add(msg);
                                        RaiseMessage(new Message() { language = "en", lines = translated }, callback);
                                    });
                                    translation_promise.Fail((msg) =>
                                    {
                                        RaiseError(msg, callback);
                                    });
                                });
                                accessToken_promise.Fail((msg) =>
                                {
                                    RaiseError(msg, callback);
                                });
                            }
                            else
                            {
                                RaiseMessage(new Message() { language = response_object.language, lines = lines }, callback);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    _status = StatusOptions.failed;
                    RaiseError(ex.Message, callback);
                }
                Reset();
            };
            client.UploadDataAsync(new System.Uri("https://" + Region + ".api.cognitive.microsoft.com/vision/v1.0/ocr?language=unk&detectOrientation=false"), data);
        }

    }

    public override void Look(Texture2D texture, OnMessageHandler callback)
    {
        QueryForText(texture.EncodeToPNG(), callback);
    }

}