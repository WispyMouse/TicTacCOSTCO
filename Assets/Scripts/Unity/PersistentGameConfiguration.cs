namespace TicTacCOSTCO.Unity
{
    using UnityEngine;

    public class PersistentGameConfiguration : MonoBehaviour
    {
        public static PersistentGameConfiguration Singleton { get; private set; } = null;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PlayerCount { get; private set; }

        [Range(3, 20)]
        [SerializeField]
        private int _StartWidth = 5;

        [Range(3, 20)]
        [SerializeField]
        private int _StartHeight = 5;

        [Range(2, 6)]
        [SerializeField]
        private int _StartPlayerCount = 2;

        private void Awake()
        {
            // If we already have an instance, don't make another
            if (Singleton != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Singleton = this;
            DontDestroyOnLoad(this.gameObject);

            this.Width = _StartWidth;
            this.Height = _StartHeight;
            this.PlayerCount = _StartPlayerCount;
        }

        public void UpdateWidth(int newValue)
        {
            this.Width = newValue;
        }

        public void UpdateHeight(int newValue)
        {
            this.Height = newValue;
        }
    }
}
