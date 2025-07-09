using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LobbyCodeSegmentedInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField hiddenInput;
    [SerializeField] private TMP_Text[] boxLabels;    // size 6
    [SerializeField] private RawImage[]  boxImages;      
    [SerializeField] private Button   confirmButton;  // enable when full

    [Header("Visuals")]
    [SerializeField] private Color emptyColor   = new(1,1,1,0.25f);
    [SerializeField] private Color filledColor  = Color.white;
    [SerializeField] private Color activeColor  = new(1,1,1,0.75f);

    private const int CodeLength = 6;

    void Awake ()
    {
        hiddenInput.characterLimit          = CodeLength;
        hiddenInput.contentType             = TMP_InputField.ContentType.Alphanumeric;
        hiddenInput.onValueChanged.AddListener(Refresh);
        
        Refresh(string.Empty);
    }
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != hiddenInput.gameObject)
        {
            hiddenInput.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(hiddenInput.gameObject);
        }
    }

    void OnEnable() => hiddenInput.ActivateInputField();
    
    private void Refresh(string value)
    {
        int len = value.Length;

        for (int i = 0; i < boxLabels.Length; i++)
        {
            bool hasChar = i < len;
            boxLabels[i].text  = hasChar ? value[i].ToString().ToUpper() : "";

            if (boxImages.Length == boxLabels.Length)
            {
                // Colour-code: filled, current cursor box, or empty
                if      (hasChar)              boxImages[i].color = filledColor;
                else if (i == len)             boxImages[i].color = activeColor;
                else                           boxImages[i].color = emptyColor;
            }
        }

        // Only let them press “✓” once all 6 chars are in
        confirmButton.interactable = len == CodeLength;
    }
    
    public void Clear()
    {
        hiddenInput.text = "";
        Refresh("");
    }
}
