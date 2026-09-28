using UnityEngine;

namespace TicTacCOSTCO.Unity.UI
{
    public class BlipPlayer : MonoBehaviour
    {
        public void PlayBlip()
        {
            AudioPlayer.Singleton.PlayBlip();
        }
    }
}
