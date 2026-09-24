namespace TicTacCOSTCO.Unity.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using TicTacCOSTCO.Unity;
    using TMPro;

    public class SelectBoardScreen : MonoBehaviour
    {
        public Slider WidthSlider;
        public TMP_Text WidthValueLabel;

        public Slider HeightSlider;
        public TMP_Text HeightValueLabel;

        void Start()
        {
            this.WidthSlider.SetValueWithoutNotify(PersistentGameConfiguration.Singleton.Width);
            this.WidthValueLabel.text = PersistentGameConfiguration.Singleton.Width.ToString();
            this.HeightSlider.SetValueWithoutNotify(PersistentGameConfiguration.Singleton.Height);
            this.HeightValueLabel.text = PersistentGameConfiguration.Singleton.Height.ToString();
        }

        public void UpdateFromSliders(int _)
        {
            PersistentGameConfiguration.Singleton.UpdateWidth(Mathf.RoundToInt(this.WidthSlider.value));
            this.WidthValueLabel.text = PersistentGameConfiguration.Singleton.Width.ToString();
            PersistentGameConfiguration.Singleton.UpdateHeight(Mathf.RoundToInt(this.HeightSlider.value));
            this.HeightValueLabel.text = PersistentGameConfiguration.Singleton.Height.ToString();
        }
    }
}
