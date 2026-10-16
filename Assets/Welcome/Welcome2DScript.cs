using UnityEngine;

namespace Unity.U2D.Welcome
{
    [HelpURL("https://unity.com/features/2d")]
    class Welcome2DScript : ScriptableObject
    {
        public static void Open2DLandingPage()
        {
            Application.OpenURL("https://unity.com/features/2d");
        }
    }
}
