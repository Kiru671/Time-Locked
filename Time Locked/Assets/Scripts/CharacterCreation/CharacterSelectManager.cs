using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    public Transform panelParent;
    public GridLayoutGroup panelLayout;

    public GameObject panelPrefab;
    public Sprite[] characterSprites;
    public int[] characterIds;
    public Dictionary<int, CharacterPanel> characterPanels;

    private int currentId = -1;

    IEnumerator Start()
    {
        characterPanels = new Dictionary<int, CharacterPanel>();
        
        for (int i = 0; i < characterSprites.Length; i++) // Loop across all sprites and instantiate a panel for each one
        {
            GameObject panel = Instantiate(panelPrefab, panelParent);
            panel.name = "Panel : " + i;
            CharacterPanel panelComp = panel.GetComponent<CharacterPanel>();
            characterPanels.Add(characterIds[i], panelComp);
            panelComp.SetButton(characterIds[i], characterSprites[i]);
            panelComp.OnCharacterSelected += SelectCharacter;

            if (i == characterIds.Length - 1) // Set viewport length after all panels are placed
            {
                yield return null; // Waiting one frame to allow grid layout to move spawned objects
                RectTransform finalPanel = panel.GetComponent<RectTransform>();
                // This requires the panel objects to be anchored at the top and pivoted at the bottom.
                float length = finalPanel.anchoredPosition.y;
                RectTransform parentRect = panelParent.GetComponent<RectTransform>();
                parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, -length + panelLayout.padding.bottom);
            }
        }
    }

    public void SelectCharacter(int characterId)
    {
        if (currentId != -1)
            characterPanels[currentId].DeselectPanel();

        currentId = characterId;
        characterPanels[characterId].SelectPanel();
    }
}
