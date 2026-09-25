namespace TicTacCOSTCO.Unity
{
    using System.Collections.Generic;
    using TicTacCOSTCO.DataStructures;
    using UnityEngine;

    public class PersistentGameConfiguration : MonoBehaviour
    {
        public static PersistentGameConfiguration Singleton { get; private set; } = null;

        public int Width { get; private set; }
        public int Height { get; private set; }

        [Range(3, 20)]
        [SerializeField]
        private int _StartWidth = 5;

        [Range(3, 20)]
        [SerializeField]
        private int _StartHeight = 5;

        [Range(GameState.MINIMUMPLAYERS, 6)]
        [SerializeField]
        private int _StartPlayerCount = 2;

        public IReadOnlyList<PlayerProfile> Players => _Players;
        private readonly List<PlayerProfile> _Players = new List<PlayerProfile>();

        [SerializeField]
        private List<Sprite> spritesForTurns = new List<Sprite>();
        [SerializeField]
        private List<Color> knockoutColorsForTurns = new List<Color>();

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

            for (int ii = 0; ii < this._StartPlayerCount; ii++)
            {
                AddPlayer();
            }
        }

        public void UpdateWidth(int newValue)
        {
            this.Width = newValue;
        }

        public void UpdateHeight(int newValue)
        {
            this.Height = newValue;
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

                    // HACK: Re-assign the sprite
                    // They should keep it, but we'll want to retool how sprites are assigned first
                    this._Players[ii].RepresenterSprite = this.spritesForTurns[ii];
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
        }

        /// <summary>
        /// Adds a new player. The returned PlayerProfile
        /// will already have the appropriate <see cref="PlayerProfile.Index"/>.
        /// </summary>
        /// <returns></returns>
        public PlayerProfile AddPlayer()
        {
            PlayerProfile newProfile = new PlayerProfile();

            newProfile.Index = this._Players.Count;
            newProfile.RepresenterSprite = spritesForTurns[newProfile.Index];
            newProfile.KnockoutColor = knockoutColorsForTurns[newProfile.Index];
            newProfile.IsAI = false;
            newProfile.PlayerName = "Human";

            this._Players.Add(newProfile);

            return newProfile;
        }
    }
}
