using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUIController : MonoBehaviour
{
    [Header("Slot Images (4 adet)")]
    [SerializeField] private Image[] slotImages;

    [Header("Slot Labels (isteğe bağlı)")]
    [SerializeField] private TextMeshProUGUI[] slotLabels;

    [Header("Slot Backgrounds (geçici olarak boş slotları göstermek için)")]
    [SerializeField] private Image[] slotBackgrounds;

    [Header("Animation Settings")]
    [SerializeField] private float heldItemScale = 1.3f; // Tutulan eşyanın scale değeri
    [SerializeField] private float normalScale = 1.0f; // Normal scale değeri
    [SerializeField] private float otherItemsScale = 0.8f; // Diğer eşyaların scale değeri (tutulan varken)
    [SerializeField] private float animationDuration = 0.3f; // Animasyon süresi
    [SerializeField] private Ease animationEase = Ease.OutBack; // Animasyon tipi

    [Header("Visual Settings")]
    [SerializeField] private Color normalSlotColor = Color.white;
    [SerializeField] private Color temporarilyEmptySlotColor = new Color(1f, 1f, 1f, 0.3f); // Şeffaf beyaz
    [SerializeField] private Color heldItemSlotColor = new Color(1f, 1f, 0f, 0.5f); // Sarımsı

    private Vector3[] originalScales; // Orijinal scale değerlerini saklamak için
    private int currentHeldSlot = -1; // Şu anda tutulan slot

    private void Awake()
    {
        // Orijinal scale değerlerini kaydet
        SaveOriginalScales();
    }

    private void SaveOriginalScales()
    {
        if (slotImages != null)
        {
            originalScales = new Vector3[slotImages.Length];
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] != null)
                {
                    originalScales[i] = slotImages[i].transform.localScale;
                }
            }
        }
    }

    public void RefreshUI(InventorySystem inventorySystem, HeldItemManager heldItemManager)
    {
        RefreshSlots(inventorySystem, heldItemManager);
        UpdateSlotAnimations(heldItemManager);
    }

    private void RefreshSlots(InventorySystem inventorySystem, HeldItemManager heldItemManager)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i >= inventorySystem.slots.Length) continue;

            // Slot durumunu kontrol et
            bool isTemporarilyEmpty = inventorySystem.IsSlotTemporarilyEmpty(i);
            bool isHeldSlot = heldItemManager.IsHoldingItem && heldItemManager.HeldItemSlotIndex == i;
            InventoryItemData slotItem = inventorySystem.GetSlotItem(i);

            // Slot görselini güncelle
            if (slotItem != null || isTemporarilyEmpty)
            {
                if (isTemporarilyEmpty)
                {
                    // Geçici olarak boş slot - orijinal eşyayı şeffaf göster
                    slotImages[i].sprite = inventorySystem.slots[i]?.icon;
                    slotImages[i].enabled = true;
                    slotImages[i].color = temporarilyEmptySlotColor;
                }
                else
                {
                    // Normal dolu slot
                    slotImages[i].sprite = slotItem.icon;
                    slotImages[i].enabled = true;
                    slotImages[i].color = normalSlotColor;
                }
            }
            else
            {
                // Boş slot
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
                slotImages[i].color = normalSlotColor;
            }

            // Slot arkaplanını güncelle
            if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
            {
                if (isHeldSlot)
                {
                    slotBackgrounds[i].color = heldItemSlotColor;
                }
                else
                {
                    slotBackgrounds[i].color = normalSlotColor;
                }
            }

            // Slot etiketini güncelle
            if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
            {
                if (slotItem != null)
                {
                    slotLabels[i].text = slotItem.itemName;
                    if (isTemporarilyEmpty)
                    {
                        slotLabels[i].text += "\n(Right-click to cancel)";
                        slotLabels[i].color = Color.yellow;
                    }
                    else
                    {
                        slotLabels[i].color = Color.white;
                    }
                }
                else
                {
                    slotLabels[i].text = "";
                    slotLabels[i].color = Color.white;
                }
            }
        }
    }

    private void UpdateSlotAnimations(HeldItemManager heldItemManager)
    {
        int newHeldSlot = heldItemManager.IsHoldingItem ? heldItemManager.HeldItemSlotIndex : -1;

        // Eğer held slot değişmişse animasyon yap
        if (newHeldSlot != currentHeldSlot)
        {
            AnimateSlots(newHeldSlot);
            currentHeldSlot = newHeldSlot;
        }
    }

    private void AnimateSlots(int heldSlotIndex)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            Transform slotTransform = slotImages[i].transform;
            Vector3 targetScale;

            if (heldSlotIndex == -1)
            {
                // Hiçbir eşya tutulmuyor - tüm slotları normal boyuta getir
                targetScale = originalScales[i] * normalScale;
            }
            else if (i == heldSlotIndex)
            {
                // Tutulan slot - büyüt
                targetScale = originalScales[i] * heldItemScale;
            }
            else
            {
                // Diğer slotlar - küçült
                targetScale = originalScales[i] * otherItemsScale;
            }

            // DOTween animasyonu
            slotTransform.DOScale(targetScale, animationDuration)
                .SetEase(animationEase)
                .SetUpdate(true); // TimeScale'den etkilenmesin
        }
    }

    // Held slot'a pulse efekti ekle
    public void PulseHeldSlot()
    {
        if (currentHeldSlot >= 0 && currentHeldSlot < slotImages.Length && slotImages[currentHeldSlot] != null)
        {
            Transform slotTransform = slotImages[currentHeldSlot].transform;
            Vector3 currentScale = slotTransform.localScale;
            
            slotTransform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f)
                .SetUpdate(true);
        }
    }

    // Tüm animasyonları durdur ve normal boyutlara getir
    public void ResetAllSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].transform.DOKill();
                slotImages[i].transform.localScale = originalScales[i];
            }
        }
        currentHeldSlot = -1;
    }

    // Manuel slot animasyonu tetikleme
    public void SetHeldSlot(int slotIndex)
    {
        AnimateSlots(slotIndex);
        currentHeldSlot = slotIndex;
    }

    // Eski RefreshUI metodu (geriye uyumluluk için)
    public void RefreshUI(InventoryItemData[] slots)
    {
        Debug.LogWarning("Old RefreshUI method called. Please use RefreshUI(InventorySystem, HeldItemManager) instead.");
        
        // Eğer eski sistem kullanılıyorsa tüm slotları normal boyutta göster
        ResetAllSlots();
        
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i >= slots.Length) continue;

            if (slots[i] != null)
            {
                slotImages[i].sprite = slots[i].icon;
                slotImages[i].enabled = true;
                slotImages[i].color = normalSlotColor;

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

    private void OnDestroy()
    {
        // Tween'leri temizle
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].transform.DOKill();
            }
        }
    }
}