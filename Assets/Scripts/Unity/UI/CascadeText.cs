using System.Text;
using TMPro;
using UnityEngine;

public class CascadeText : MonoBehaviour
{
    public TMP_Text Text;

    public string TextString = "CASCADE";
    public float HeightDiffAtMaximum = 20f;
    public float SecondsPerCycle = 1f;
    public float AdditionalSpacingPerIndex = .1f;

    float curTime { get; set; } = 0;

    void Update()
    {
        curTime += Time.deltaTime;

        StringBuilder cascadeText = new StringBuilder();
        float halfHeight = HeightDiffAtMaximum / 2f;

        for (int ii = 0; ii < TextString.Length; ii++)
        {
            if (TextString[ii] == '\\' && TextString[ii + 1] == 'n')
            {
                cascadeText.Append("\n");
                ii++;
                continue;
            }

            float sampledTime = Mathf.PingPong(this.curTime + AdditionalSpacingPerIndex * ii, this.SecondsPerCycle / 2f);
            float sampledHeight = Mathf.Lerp(-halfHeight, halfHeight, Mathf.InverseLerp(0, this.SecondsPerCycle, sampledTime));

            cascadeText.Append($"<voffset={sampledHeight}px>" + TextString[ii] + "</voffset>");
        }

        this.Text.text = cascadeText.ToString();
    }
}
