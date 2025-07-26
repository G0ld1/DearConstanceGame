using UnityEngine;
using System;

public class ScreenshotUtility : MonoBehaviour
{
    
    public KeyCode screenshotKey = KeyCode.F12;
    public int superSize = 2; // 1 = normal, 2 = 2x resolution, etc.



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    public void TakeScreenshot()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"Screenshot_{timestamp}.png";
        ScreenCapture.CaptureScreenshot(filename, superSize);
        Debug.Log($"Screenshot saved: {filename}");
    }
}
