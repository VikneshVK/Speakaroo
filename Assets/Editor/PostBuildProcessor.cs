using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class PostBuildProcessor
{
    [PostProcessBuild] 
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        // Check if the build target is iOS
        if (buildTarget == BuildTarget.iOS)
        {
            
            string plistPath = Path.Combine(path, "Info.plist");

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            plist.root.SetString(
                "NSMicrophoneUsageDescription",
                "This app requires microphone access to record audio for Speech Therapy"
            );
            
            plist.WriteToFile(plistPath);
        }
    }
}
