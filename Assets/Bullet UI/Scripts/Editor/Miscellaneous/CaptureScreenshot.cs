#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CaptureScreenshot
{
    [MenuItem("Tools/Capture Screenshot")]
    static void Capture()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Please hit PLAY before trying to capture a screenshot.");
            return;
        }

        string fileName = "Screenshot";
        string directory = Application.persistentDataPath + "/Screenshots";

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots");
        }

        DirectoryInfo dirInfo = new DirectoryInfo(directory);

        string fullFilePath = directory + "/" + fileName + "_" + dirInfo.GetFiles().Length + ".png";

        ScreenCapture.CaptureScreenshot(fullFilePath);
        Debug.LogWarning("Screenshot captured! File Path: " + fullFilePath);
    }
}
#endif
