using TMPro;
using UnityEngine;

namespace NeuroQuest.MiniGames.Highway
{
    public class HighwayVisualItem : MonoBehaviour
    {
        [Header("Color Layers | Index 0=Gray, 1=Yellow, 2=Red, 3=Blue, 4=Green")]
        [SerializeField] private GameObject[] colorLayers = new GameObject[5];

        [Header("Text")]
        [SerializeField] private TMP_Text tagText;

        public int ColorCode { get; private set; }
        public int TagCode { get; private set; }

        public void Configure(int colorCode, int tagCode, bool showTag)
        {
            ColorCode = Mathf.Clamp(colorCode, 0, 4);
            TagCode = Mathf.Clamp(tagCode, 0, 4);

            for (int i = 0; i < colorLayers.Length; i++)
            {
                if (colorLayers[i] != null)
                {
                    colorLayers[i].SetActive(i == ColorCode);
                }
            }

            if (tagText != null)
            {
                tagText.gameObject.SetActive(showTag && TagCode > 0);
                tagText.text = showTag && TagCode > 0 ? GetPersianColorName(TagCode) : string.Empty;
            }
        }

        public static string GetPersianColorName(int code)
        {
            switch (code)
            {
                case 1:
                    return "زرد";
                case 2:
                    return "قرمز";
                case 3:
                    return "آبی";
                case 4:
                    return "سبز";
                default:
                    return "";
            }
        }
    }
}