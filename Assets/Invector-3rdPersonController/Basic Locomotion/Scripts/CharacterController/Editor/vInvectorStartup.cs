using UnityEditor;

namespace Invector.vCharacterController
{
    [InitializeOnLoad]
    public class vInvectorStartup
    {
        static vInvectorStartup()
        {
            EditorApplication.update -= TriggerWelcomeScreen;
            EditorApplication.update += TriggerWelcomeScreen;
        }

        private static void TriggerWelcomeScreen()
        {
            var showAtStartup = vEditorStartupPrefs.DisplayWelcomeScreen && EditorApplication.timeSinceStartup < 30f;

            if (showAtStartup)
            {
                vInvectorWelcomeWindow.Open();
            }
            EditorApplication.update -= TriggerWelcomeScreen;
        }

        private static void PlayModeChanged()
        {
            EditorApplication.update -= TriggerWelcomeScreen;
        }
    }
}