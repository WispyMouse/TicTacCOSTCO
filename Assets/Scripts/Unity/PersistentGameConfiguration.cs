namespace TicTacCOSTCO.Unity
{
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class PersistentGameConfiguration : MonoBehaviour
    {
        public delegate void GameplaySettingsUpdated();
        public GameplaySettingsUpdated OnGameplaySettingsUpdated;

        public const string HUMAN = "Human";
        public static PersistentGameConfiguration Singleton { get; private set; } = null;

        public int Width { get; private set; }
        public int Height { get; private set; }

        [Range(3, 12)]
        [SerializeField]
        private int _StartWidth = 5;

        [Range(3, 12)]
        [SerializeField]
        private int _StartHeight = 5;

        [Range(BoardStateHolder.MINIMUMPLAYERS, 6)]
        [SerializeField]
        private int _StartPlayerCount = 2;

        public IReadOnlyList<PlayerProfile> Players => _Players;
        private readonly List<PlayerProfile> _Players = new List<PlayerProfile>();

        [SerializeField]
        private List<int> _coresByIndex = new List<int>();

        [SerializeField]
        private List<Color> knockoutColorsForTurns = new List<Color>();

        public List<Sprite> PlayerIconOptions = new List<Sprite>();

        public List<AICore> AICores = new List<AICore>();

        private void Awake()
        {
            // If we already have an instance, don't make another
            if (Singleton != null)
            {
                Destroy(this.gameObject);
                return;
            }

            // This is now the singleton; unparent it so that we can mark as DontDestroyOnLoad
            Singleton = this;
            this.transform.SetParent(null);
            DontDestroyOnLoad(this.gameObject);

            this.Width = _StartWidth;
            this.Height = _StartHeight;

            for (int ii = 0; ii < this._StartPlayerCount; ii++)
            {
                PlayerProfile newProfile = AddPlayer();

#if UNITY_EDITOR
                // Quick AI core test
                if (this._coresByIndex.Count > ii && this._coresByIndex[ii] >= 0)
                {
                    newProfile.AICoreIndex = this._coresByIndex[ii];
                    newProfile.IsAI = true;
                    newProfile.PlayerName = this.AICores[this._coresByIndex[ii]].name;
                }
#endif
            }

            this.OnGameplaySettingsUpdated?.Invoke();
        }

        public void UpdateWidth(int newValue)
        {
            this.Width = newValue;
            this.OnGameplaySettingsUpdated?.Invoke();
        }

        public void UpdateHeight(int newValue)
        {
            this.Height = newValue;
            this.OnGameplaySettingsUpdated?.Invoke();
        }

        /// <summary>
        /// Goes through the <see cref="Players"/> list, and removes the specified player.
        /// Every PlayerProfile past that will have its <see cref="PlayerProfile.Index"/> reduced by one.
        /// </summary>
        public void RemovePlayer(PlayerProfile toRemove)
        {
            bool startRemoving = false;

            for (int ii = 0; ii < this.Players.Count; ii++)
            {
                // Reduce the index so that we have a stable 0-N
                if (startRemoving)
                {
                    this._Players[ii].Index--;
                    continue;
                }

                // If this is what we're trying to remove, snip it
                if (this.Players[ii] == toRemove)
                {
                    startRemoving = true;
                    this._Players.RemoveAt(ii);

                    // Move the iterator back one, since this has removed something
                    ii--;

                    continue;
                }
            }

            this.OnGameplaySettingsUpdated?.Invoke();
        }

        /// <summary>
        /// Adds a new player. The returned PlayerProfile
        /// will already have the appropriate <see cref="PlayerProfile.Index"/>.
        /// </summary>
        /// <returns></returns>
        public PlayerProfile AddPlayer()
        {
            PlayerProfile newProfile = new PlayerProfile();

            // Find the first index not used by anyone for the icon
            // It'll see if anyone is using the first, and if not, use that;
            // otherwise check the second, etc.
            int iconIndex = 0;
            for (int ii = 0, count = this.PlayerIconOptions.Count; ii < count; ii++)
            {
                // Check each player to see if they're using this index
                bool unused = true;
                for (int jj = 0, jjcount = this.Players.Count; jj < jjcount; jj++)
                {
                    if (this.Players[jj].RepresenterSpriteIndex == ii)
                    {
                        unused = false;
                        break;
                    }
                }
                if (unused)
                {
                    iconIndex = ii;
                    break;
                }
            }

            newProfile.Index = this._Players.Count;
            newProfile.RepresenterSpriteIndex = Random.Range(0, this.PlayerIconOptions.Count);
            newProfile.KnockoutColor = knockoutColorsForTurns[newProfile.Index];
            newProfile.IsAI = false;
            newProfile.PlayerName = HUMAN;

            this._Players.Add(newProfile);

            this.OnGameplaySettingsUpdated?.Invoke();

            return newProfile;
        }
    }
}
