using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class World : MonoBehaviour
{

    // references
    private RawImage Preview;
    private Text Response;
    private VisionText VisionText;
    private VisionQR VisionQR;

    // locals
    private WebCamTexture camTexture;

    void OnDestroy()
    {
        camTexture.Stop();
    }

    private void Awake()
    {

        // get all references
        Preview = GameObject.Find("Preview").GetComponent<RawImage>();
        Response = GameObject.Find("Response").GetComponent<Text>();
        VisionText = GameObject.Find("Vision").GetComponent<VisionText>();
        VisionQR = GameObject.Find("Vision").GetComponent<VisionQR>();

        // start polling for vision
        VisionText.PollTime = 3.0f;
        VisionText.OnMessage += (object source, Vision.Message message) =>
        {
            if (!message.error)
            {
                Response.text = "TEXT (" + message.language + "): " + string.Join(" ", message.lines.ToArray());
            }
            else
            {
                Response.text = "ERROR: " + message.lines.First();
            }
        };

        // start polling for QR
        VisionQR.PollTime = 0.2f;
        VisionQR.OnMessage += (object source, Vision.Message message) => {
            if (!message.error)
            {
                Response.text = "QR: " + string.Join(" ", message.lines.ToArray());
            }
            else
            {
                Response.text = "ERROR: " + message.lines.First();
            }
        };

    }

    void Start()
    {

        // start the webcam
        camTexture = new WebCamTexture();
        camTexture.requestedHeight = Screen.height;
        camTexture.requestedWidth = Screen.width;
        if (camTexture != null)
        {
            camTexture.Play();
            VisionText.WebCamTexture = camTexture;
            VisionQR.WebCamTexture = camTexture;
            Preview.texture = camTexture;
            Response.text = "Looking...";
        }

    }

}