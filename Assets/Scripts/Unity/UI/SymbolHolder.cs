namespace TicTacCOSTCO.Unity.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SymbolHolder : MonoBehaviour
    {
        public TurnOrderHolder TurnOrderHolder;

        public List<Sprite> SpritesToChooseFrom = new List<Sprite>();

        public Transform HolderParent;
        public Button SpriteButtonPF;

        private int curSide { get; set; } = 0;

        void Start()
        {
            foreach (Sprite sprite in SpritesToChooseFrom)
            {
                Button newButton = Instantiate(SpriteButtonPF, this.HolderParent);
                newButton.GetComponent<Image>().sprite = sprite;
                newButton.onClick.AddListener(() => { this.SetSprite(sprite); });
            }
        }

        public void OpenForSide(int side)
        {
            this.curSide = side;
        }

        void SetSprite(Sprite sprite)
        {
            this.TurnOrderHolder.SpritesForTurns[this.curSide] = sprite;
        }
    }
}