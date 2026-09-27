namespace TicTacCOSTCO.Unity
{
    using UnityEngine;

    public class PlayerProfile
    {
        public int Index;
        public string PlayerName;
        public bool IsAI;
        public int RepresenterSpriteIndex;
        public Color KnockoutColor;

        public int AICoreIndex;

        public Sprite SpriteRepresentation
        {
            get
            {
                return PersistentGameConfiguration.Singleton.PlayerIconOptions[this.RepresenterSpriteIndex];
            }
        }

        public AICore AICore
        {
            get
            {
                return PersistentGameConfiguration.Singleton.AICores[this.AICoreIndex];
            }
        }
    }
}
