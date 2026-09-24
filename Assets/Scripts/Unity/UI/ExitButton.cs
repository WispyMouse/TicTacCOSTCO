namespace TicTacCOSTCO.Unity.UI
{
    using UnityEngine;

    public class ExitButton : MonoBehaviour
    {
        public void OnPress()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
