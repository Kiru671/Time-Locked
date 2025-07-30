using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIController : MonoBehaviour
{
    [Header("Slot Images (4 adet)")]
    [SerializeField] private Image[] slotImages;

    [Header("Slot Labels (isteğe bağlı)")]
    [SerializeField] private TextMeshProUGUI[] slotLabels;

    // Singleton
    public static InventoryUIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RefreshUI(InventoryItemData[] slots)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i >= slots.Length) continue;

            if (slots[i] != null)
            {
                slotImages[i].sprite = slots[i].icon;
                slotImages[i].enabled = true;

                if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                {
                    slotLabels[i].text = slots[i].itemName;
                }
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;

                if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
                {
                    slotLabels[i].text = "";
                }
            }
        }
    }
}
