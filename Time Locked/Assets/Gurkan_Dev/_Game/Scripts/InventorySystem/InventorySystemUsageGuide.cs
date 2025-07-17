using UnityEngine;

/// <summary>
/// Geliştirilmiş Envanter Sistemi Kullanım Kılavuzu ve Test Helper'ı
/// 
/// KURULUM:
/// 1. DOTween'i projenize import edin (Window > DOTween Utility Panel > Setup DOTween)
/// 2. Bir GameObject'e PlayerInventory scripti ekleyin
/// 3. Aynı GameObject'e InventorySystem ve HeldItemManager scriptlerini ekleyin
/// 4. UI Canvas'ına InventoryUIController scripti ekleyin
/// 5. PlayerInventory'deki referansları bağlayın
/// 
/// UI KURULUMU:
/// - 4 adet slot Image'ı oluşturun
/// - Slot arkaplanları (opsiyonel) için ek Image'lar ekleyin
/// - HeldItemPanel gerekmez artık - scale animasyonları kullanılıyor
/// 
/// KULLANIM:
/// - 1, 2, 3, 4 tuşları: Envanterdeki eşyaları ele alma/geri koyma
/// - E tuşu: Elimizdeki eşyayı kullanma
/// - Sağ Tık: Taşımayı iptal etme (eşyayı geri koyma)
/// 
/// YENİ ÖZELLİKLER:
/// - DOTween ile smooth scale animasyonları
/// - Tutulan eşya büyür, diğerleri küçülür
/// - Sağ tık ile hızlı iptal
/// - Pulse efektleri
/// - Gelişmiş görsel geri bildirim
/// </summary>
public class InventorySystemUsageGuide : MonoBehaviour
{
    [Header("Test Items")]
    [SerializeField] private InventoryItemData[] testItems;
    
    [Header("Components (Auto-assigned)")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private HeldItemManager heldItemManager;
    [SerializeField] private InventoryUIController uiController;

    [Header("Animation Test Settings")]
    [SerializeField] private bool showAnimationDemo = true;
    [SerializeField] private float demoInterval = 3f;

    private void Start()
    {
        // Componentleri otomatik bul
        FindComponents();

        // Test eşyalarını ekle
        AddTestItems();
        
        // Kullanım talimatlarını göster
        ShowUsageInstructions();

        // DOTween kontrolü
        CheckDOTweenInstallation();

        // Demo animasyonu başlat
        if (showAnimationDemo)
        {
            StartCoroutine(AnimationDemo());
        }
    }

    private void FindComponents()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
        
        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();
            
        if (heldItemManager == null)
            heldItemManager = FindObjectOfType<HeldItemManager>();

        if (uiController == null)
            uiController = FindObjectOfType<InventoryUIController>();
    }

    private void CheckDOTweenInstallation()
    {
        #if !DOTWEEN_ENABLED
        Debug.LogError("❌ DOTween is not installed! Please install DOTween from the Asset Store for scale animations to work.");
        Debug.LogError("📥 Window > DOTween Utility Panel > Setup DOTween");
        #else
        Debug.Log("✅ DOTween is properly installed and ready!");
        #endif
    }

    private void AddTestItems()
    {
        if (testItems == null || testItems.Length == 0)
        {
            Debug.LogWarning("⚠️ Test items not assigned. Create some InventoryItemData ScriptableObjects and assign them.");
            return;
        }

        foreach (var item in testItems)
        {
            if (item != null)
            {
                playerInventory.TryAddItem(item);
            }
        }
    }

    private void ShowUsageInstructions()
    {
        Debug.Log("=== 🎮 ENHANCED INVENTORY SYSTEM v2.0 ===");
        Debug.Log("🔧 NEW FEATURES:");
        Debug.Log("• DOTween scale animations");
        Debug.Log("• Right-click cancel functionality");
        Debug.Log("• Pulse effects on interactions");
        Debug.Log("• Smooth visual transitions");
        Debug.Log("");
        Debug.Log("🎯 CONTROLS:");
        Debug.Log("• 1, 2, 3, 4: Pick up/Put back items (with scale animations)");
        Debug.Log("• E: Use held item at interaction zones only");
        Debug.Log("• Right Click: Cancel carrying (instant put back)");
        Debug.Log("");
        Debug.Log("🎨 VISUAL FEEDBACK:");
        Debug.Log("• Held item: Scales up (1.3x)");
        Debug.Log("• Other items: Scale down (0.8x) when something is held");
        Debug.Log("• Smooth DOTween animations (0.3s duration)");
        Debug.Log("• Pulse effects on pickup and use");
        Debug.Log("");
        Debug.Log("⚙️ UI SETUP NOTES:");
        Debug.Log("• No HeldItemPanel needed anymore");
        Debug.Log("• Slot scale animations provide feedback");
        Debug.Log("• Make sure DOTween is installed");
        Debug.Log("========================================");
    }

    // Animasyon demo coroutine
    private System.Collections.IEnumerator AnimationDemo()
    {
        yield return new WaitForSeconds(2f);

        if (inventorySystem.slots[0] != null)
        {
            Debug.Log("🎭 Starting animation demo...");
            
            // İlk eşyayı al
            playerInventory.TakeItemFromSlot(0);
            yield return new WaitForSeconds(demoInterval);

            // Geri koy
            if (heldItemManager.IsHoldingItem)
            {
                playerInventory.PutBackHeldItem();
                yield return new WaitForSeconds(demoInterval);
            }

            Debug.Log("🎭 Animation demo completed!");
        }
    }

    // Test metodları
    [ContextMenu("Add Random Test Item")]
    public void AddRandomTestItem()
    {
        if (testItems != null && testItems.Length > 0)
        {
            var randomItem = testItems[Random.Range(0, testItems.Length)];
            if (randomItem != null)
            {
                playerInventory.TryAddItem(randomItem);
                Debug.Log($"➕ Added {randomItem.itemName} to inventory");
            }
        }
    }

    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        // Elimdeki eşyayı bırak
        if (heldItemManager.IsHoldingItem)
        {
            heldItemManager.UseHeldItem();
        }

        // Tüm slotları temizle
        for (int i = 0; i < inventorySystem.slots.Length; i++)
        {
            if (inventorySystem.slots[i] != null)
            {
                inventorySystem.ConsumeItemFromSlot(i);
            }
        }

        // UI'yi güncelle ve animasyonları sıfırla
        uiController.RefreshUI(inventorySystem, heldItemManager);
        uiController.ResetAllSlots();
        
        Debug.Log("🧹 Inventory cleared and animations reset");
    }

    [ContextMenu("Test Scale Animations")]
    public void TestScaleAnimations()
    {
        StartCoroutine(ScaleAnimationTest());
    }

    private System.Collections.IEnumerator ScaleAnimationTest()
    {
        Debug.Log("🧪 Testing scale animations...");
        
        for (int i = 0; i < 4; i++)
        {
            if (inventorySystem.IsSlotAvailable(i))
            {
                Debug.Log($"Testing slot {i + 1}");
                uiController.SetHeldSlot(i);
                yield return new WaitForSeconds(1f);
            }
        }

        uiController.ResetAllSlots();
        Debug.Log("✅ Scale animation test completed!");
    }

    [ContextMenu("Show Inventory Status")]
    public void ShowInventoryStatus()
    {
        if (playerInventory != null)
        {
            playerInventory.PrintInventoryStatus();
        }
    }

    [ContextMenu("Toggle Right-Click Cancel")]
    public void ToggleRightClickCancel()
    {
        if (playerInventory != null)
        {
            // Bu özelliği açma/kapama
            bool currentState = playerInventory.GetComponent<PlayerInventory>().enableRightClickCancel;
            playerInventory.SetRightClickCancelEnabled(!currentState);
        }
    }

    // Klavye kısayolları ile test
    private void Update()
    {
        // Test kısayolları
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddRandomTestItem();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowInventoryStatus();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TestScaleAnimations();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRightClickCancel();
        }
    }

    private void OnGUI()
    {
        // Test bilgileri için gelişmiş GUI
        GUILayout.BeginArea(new Rect(10, 10, 350, 200));
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        
        GUILayout.Label("🎮 ENHANCED INVENTORY v2.0", headerStyle);
        
        GUILayout.Space(5);
        GUILayout.Label("TEST CONTROLS:");
        GUILayout.Label("T: Add random item");
        GUILayout.Label("C: Clear inventory + reset animations");
        GUILayout.Label("I: Show inventory status");
        GUILayout.Label("P: Test scale animations");
        GUILayout.Label("R: Toggle right-click cancel");
        
        GUILayout.Space(5);
        GUILayout.Label("GAME CONTROLS:");
        GUILayout.Label("1,2,3,4: Pick up/Put back (with animations)");
        GUILayout.Label("E: Use at interaction zones");
        GUILayout.Label("Right Click: Cancel carrying");

        // DOTween durumu
        GUILayout.Space(5);
        #if DOTWEEN_ENABLED
        GUI.color = Color.green;
        GUILayout.Label("✅ DOTween: READY");
        #else
        GUI.color = Color.red;
        GUILayout.Label("❌ DOTween: NOT INSTALLED");
        #endif
        GUI.color = Color.white;

        GUILayout.EndArea();
    }
} 