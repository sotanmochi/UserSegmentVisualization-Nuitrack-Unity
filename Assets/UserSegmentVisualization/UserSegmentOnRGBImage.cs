using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSegmentOnRGBImage : MonoBehaviour
{
    [SerializeField]
    Material material;

    [SerializeField]
    Color32 maskColor = Color.magenta;

    int frameWidth = 0;
    int frameHeight = 0;

    Texture2D frameTexture;
    byte[] colorFrame;
    byte[] maskedFrame;

    void Start()
    {
        NuitrackManager.onColorUpdate += DrawColorFrame;
        NuitrackManager.onUserTrackerUpdate += DrawUserSegment;

        nuitrack.OutputMode colorOutputMode = NuitrackManager.ColorSensor.GetOutputMode();
        int colorFrameWidth = colorOutputMode.XRes;
        int colorFrameHeight = colorOutputMode.YRes;
        Debug.Log("Color frame: " + colorFrameWidth + "x" + colorFrameHeight);

        NuitrackManager.DepthSensor.SetMirror(false);
        nuitrack.OutputMode depthOutputMode = NuitrackManager.DepthSensor.GetOutputMode();
        int depthFrameWidth = depthOutputMode.XRes;
        int depthFrameHeight = depthOutputMode.YRes;
        Debug.Log("Depth frame: " + depthFrameWidth + "x" + depthFrameHeight);

        frameWidth = colorFrameWidth;
        frameHeight = colorFrameHeight;
        frameTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);

        maskedFrame = new byte[frameWidth * frameHeight * 4];

        material.mainTexture = frameTexture;
    }

    void OnDestroy()
    {
        NuitrackManager.onColorUpdate -= DrawColorFrame;
        NuitrackManager.onUserTrackerUpdate -= DrawUserSegment;
    }

    void DrawColorFrame(nuitrack.ColorFrame frame)
    {
        colorFrame = frame.Data; // RGB24

        for (int i = 0; i < colorFrame.Length; i += 3)
        {
            byte temp = colorFrame[i];
            colorFrame[i] = colorFrame[i + 2];
            colorFrame[i + 2] = temp;
        }

        frameTexture.LoadRawTextureData(colorFrame);
        frameTexture.Apply();
    }

    void DrawUserSegment(nuitrack.UserFrame frame)
    {
        if (frame.Users.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < (frameWidth * frameHeight); i++)
        {
            int ptr = i * 3; // RGB24
            if (frame[i] == 0)
            {
                maskedFrame[ptr] = colorFrame[ptr];
                maskedFrame[ptr + 1] = colorFrame[ptr + 1];
                maskedFrame[ptr + 2] = colorFrame[ptr + 2];
            }
            else
            {
                maskedFrame[ptr] = maskColor.r;
                maskedFrame[ptr + 1] = maskColor.g;
                maskedFrame[ptr + 2] = maskColor.b;
            }
        }

        frameTexture.LoadRawTextureData(maskedFrame);
        frameTexture.Apply();
    }
}
