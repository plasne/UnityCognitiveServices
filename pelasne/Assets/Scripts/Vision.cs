using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Vision : MonoBehaviour
{

    // editor properties
    public Texture2D texture;

    public enum StatusOptions
    {
        wait,
        look,
        looking,
        failed
    }

    protected StatusOptions _status = StatusOptions.wait;

    public StatusOptions Status {
        get
        {
            return _status;
        }
    }

    private float _pollTime = 3.0f; // 3 second default

    public float PollTime {
        get
        {
            return _pollTime;
        }
        set
        {
            _pollTime = value;
        }
    }

    protected float _accrued = 0.0f;

    protected void Reset()
    {
        _accrued = 0.0f;
        _status = StatusOptions.wait;
    }

    public WebCamTexture WebCamTexture { get; set; }

    public class Message
    {
        public bool error = false;
        public string language;
        public List<string> lines = new List<string>();
    }

    protected Queue<Message> _messages = new Queue<Message>();

    public abstract void Look(Texture2D texture, OnMessageHandler callback = null);

    public void Look(WebCamTexture texture, OnMessageHandler callback = null)
    {
        Texture2D snap = new Texture2D(texture.width, texture.height);
        snap.SetPixels(texture.GetPixels());
        snap.Apply();
        Look(snap, callback);
    }

    public delegate void OnMessageHandler(object source, Message message);

    public event OnMessageHandler OnMessage;

    protected void RaiseMessage(Message message, OnMessageHandler callback)
    {
        if (callback != null)
        {
            callback.Invoke(this, message);
        }
        else
        {
            _messages.Enqueue(message);
        }
    }

    protected void RaiseMessage(string language, string text, OnMessageHandler callback)
    {
        List<string> lines = new List<string>();
        lines.Add(text);
        RaiseMessage(new Message() { language = language, lines = lines }, callback);
    }

    protected void RaiseMessage(string text, OnMessageHandler callback)
    {
        List<string> lines = new List<string>();
        lines.Add(text);
        RaiseMessage(new Message() { lines = lines }, callback);
    }

    protected void RaiseError(string text, OnMessageHandler callback)
    {
        List<string> lines = new List<string>();
        lines.Add(text);
        RaiseMessage(new Message() { error = true, lines = lines }, callback);
    }

    private void Update()
    {

        // look at the web cam every so often
        if (WebCamTexture != null && WebCamTexture.isPlaying) {
            switch (Status)
            {
                case StatusOptions.wait:
                case StatusOptions.failed:
                    _accrued += Time.deltaTime;
                    if (_accrued > PollTime) _status = StatusOptions.look;
                    break;
                case StatusOptions.looking:
                    // nothing to do
                    break;
                case StatusOptions.look:
                    _status = StatusOptions.looking;
                    Look(WebCamTexture);
                    break;
            }
        }

        // dispatch messages
        while (_messages.Count > 0 && OnMessage != null)
        {
            OnMessage(this, _messages.Dequeue());
        }

    }

}