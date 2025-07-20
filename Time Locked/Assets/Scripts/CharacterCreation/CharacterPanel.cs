using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    public event Action<int> OnCharacterSelected;

    public int id;
    public GameObject selectedFrame;
    public Image iconImage;

    public void OnPressed()
    {
        if (id == -1)
        {
            Debug.LogWarning("ID for character button: " + gameObject.name + " is missing");
            return;
        }
        
        OnCharacterSelected?.Invoke(id);
    }

    public void SetButton(int id, Sprite icon)
    {
        this.id = id;
        iconImage.sprite = icon;
    }

    public void SelectPanel()
    {
        selectedFrame.SetActive(true);
    }

    public void DeselectPanel()
    {
        selectedFrame.SetActive(false);
    }
}
