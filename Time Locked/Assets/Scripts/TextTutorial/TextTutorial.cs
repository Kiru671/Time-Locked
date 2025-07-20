using System;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextTutorial
{
    public class TextTutorial : MonoBehaviour
    {
        [SerializeField] private RowProperties[] rowProperties;
        [SerializeField] private GameObject rowPrefab;

        [SerializeField] private RawImage bg;
        [SerializeField] private GameObject[] rows;
        
        private VerticalLayoutGroup verticalLayoutGroup;
        private HorizontalLayoutGroup horizontalLayoutGroup;
        
        private float bgHeight;
        private float remainingHeight;
        private float rowHeight;

        private bool isReversed;
        
        void Start()
        {
            
            bgHeight = bg.GetComponent<RectTransform>().sizeDelta.y;
            verticalLayoutGroup = bg.GetComponent<VerticalLayoutGroup>();
            
            rows = new GameObject[rowProperties.Length];
            for (int i = 0; i < rowProperties.Length; i++)
            {
                rows[i] = Instantiate(rowPrefab, bg.transform);
                horizontalLayoutGroup = rows[i].GetComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.reverseArrangement = isReversed;
                isReversed = !isReversed;
            }

            remainingHeight = bgHeight - verticalLayoutGroup.padding.top - verticalLayoutGroup.padding.bottom - (verticalLayoutGroup.spacing * (rows.Length - 1));
            Debug.Log(remainingHeight);
            rowHeight = remainingHeight / rows.Length;
            Debug.Log(rowHeight);
            
            
            for (int i = 0; i < rows.Length; i++)
            {
                TextMeshProUGUI tmp = rows[i].GetComponentInChildren<TextMeshProUGUI>();
                tmp.text = rowProperties[i].description;

                Image img = rows[i].GetComponentInChildren<Image>();
                RectTransform rectTransform = img.GetComponent<RectTransform>();

                float calculatedHeight = rowHeight;
                float calculatedWidth = rowProperties[i].aspectRatio > 0 ? calculatedHeight * rowProperties[i].aspectRatio : calculatedHeight;
                
                if (calculatedHeight > rowHeight)
                {
                    calculatedHeight = rowHeight;
                    calculatedWidth = rowProperties[i].aspectRatio > 0 ? calculatedHeight * rowProperties[i].aspectRatio : calculatedHeight;
                }

                rectTransform.sizeDelta = new Vector2(calculatedWidth, calculatedHeight);
                img.sprite = rowProperties[i].image;
            }
        }
    }
    [Serializable]
    public class RowProperties
    {
        public Sprite image;
        public string description;

        public float aspectRatio;
    }
}
