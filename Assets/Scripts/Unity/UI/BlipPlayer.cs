namespace TicTacCOSTCO.Unity.UI
{
    using UnityEngine;

    public class BlipPlayer : MonoBehaviour
    {
        public void PlayBlip()
        {
            RandomAudioPlayer.Singleton.PlayRandomSound();
        }
    }
}
