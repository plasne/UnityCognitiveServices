using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using ZXing;

public class VisionQR : Vision
{

    private BarcodeReader barcodeReader = new BarcodeReader { AutoRotate = false };

    private void Query(Color32[] data, int width, int height, OnMessageHandler callback)
    {

        Thread thread = new Thread(new ThreadStart(() => {
            try
            {
                Result result = barcodeReader.Decode(data, width, height);
                if (result != null)
                {
                    RaiseMessage(result.Text, callback);
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex.Message, callback);
            }
            Reset();
        }));
        thread.Start();

    }

    public override void Look(Texture2D texture, OnMessageHandler callback)
    {
        Query(texture.GetPixels32(), texture.width, texture.height, callback);
    }

}