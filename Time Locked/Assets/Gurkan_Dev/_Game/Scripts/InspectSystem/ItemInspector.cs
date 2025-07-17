using UnityEngine;
using StarterAssets;

public class ItemInspector : MonoBehaviour
{
    private FirstPersonController fpsController;
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public Transform inspectHolder; // Kamera önünde obje tutmak için boş bir GameObject
    public float rotateSpeed = 100f;

    private Camera cam;
    private Inspectable currentItem;
    private bool isInspecting = false;
    private bool isInspectingHeldItem = false; // Eldeki eşyayı mı inspect ediyoruz

    public GameObject promptUI;
    private bool canInspect = false;
    
    private HeldItemManager heldItemManager; // HeldItemManager referansı

    void Start()
    {
        cam = Camera.main;
        fpsController = Object.FindFirstObjectByType<FirstPersonController>();
        heldItemManager = Object.FindFirstObjectByType<HeldItemManager>();
        
        if (heldItemManager == null)
        {
            Debug.LogWarning("HeldItemManager not found! Tab inspect for held items won't work properly.");
        }
    }

    void Update()
    {
        if (!isInspecting)
        {
            ShowPromptIfLookingAtInspectable();

            // F tuşu ile dünyada incele (envantere almadan)
            if (Input.GetKeyDown(KeyCode.F) && canInspect)
            {
                TryInspectItem();
            }
        }
        else if (isInspecting)
        {
            // Eldeki eşya inspect ediyorsak ve artık elimizde eşya yoksa inspect'i sonlandır
            if (isInspectingHeldItem && heldItemManager != null && !heldItemManager.IsHoldingItem)
            {
                Debug.Log("🚫 Held item was dropped during inspect! Ending inspect mode.");
                EndInspectDueToItemLoss();
                return;
            }

            RotateItem();

            // Çıkış tuşları (context'e göre)
            bool shouldExit = Input.GetKeyDown(KeyCode.Escape);
            
            if (isInspectingHeldItem)
            {
                // Eldeki eşya için Tab veya Esc
                shouldExit = shouldExit || Input.GetKeyDown(KeyCode.Tab);
            }
            else
            {
                // Dünya eşyası için F veya Esc
                shouldExit = shouldExit || Input.GetKeyDown(KeyCode.F);
            }

            if (shouldExit)
            {
                ReturnItem();
            }
        }
    }

    void ShowPromptIfLookingAtInspectable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.GetComponent<Inspectable>())
            {
                promptUI.SetActive(true);
                canInspect = true;
                return;
            }
        }

        promptUI.SetActive(false);
        canInspect = false;
    }

    void TryInspectItem()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            Inspectable inspectable = hit.collider.GetComponent<Inspectable>();
            if (inspectable != null)
            {
                currentItem = inspectable;
                currentItem.SaveOriginalTransform();

                currentItem.transform.SetParent(inspectHolder);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;

                isInspecting = true;

                fpsController.canLook = false; // kamera dönmesini durdur
                promptUI.SetActive(false);
                
                Debug.Log($"🔍 Inspecting {currentItem.name} (Press F or Esc to stop)");
            }
        }
    }

    void RotateItem()
    {
        float rotX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        float rotY = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

        currentItem.transform.Rotate(Vector3.up, rotX, Space.World);
        currentItem.transform.Rotate(Vector3.right, rotY, Space.World);
    }

    void ReturnItem()
    {
        if (isInspectingHeldItem && heldItemManager != null)
        {
            // Eldeki eşyayı geri al
            heldItemManager.RecoverFromInspect();
            Debug.Log($"🔍 Stopped inspecting held item: {currentItem.name}");
        }
        else
        {
            // Dünya eşyasını orijinal pozisyonuna geri koy
            currentItem.transform.SetParent(null);
            currentItem.transform.position = currentItem.originalPosition;
            currentItem.transform.rotation = currentItem.originalRotation;
            Debug.Log($"🔍 Stopped inspecting world item: {currentItem.name}");
        }
        
        currentItem = null;
        isInspecting = false;
        isInspectingHeldItem = false;

        fpsController.canLook = true; // kamerayı tekrar aktif et
    }

    // Dışarıdan çağrılabilir (envanter sisteminden)
    public void InspectItem(GameObject item)
    {
        if (isInspecting) return; // Zaten bir şey inspect ediyorsa

        Inspectable inspectable = item.GetComponent<Inspectable>();
        if (inspectable == null)
        {
            Debug.LogWarning($"Object {item.name} doesn't have Inspectable component!");
            return;
        }

        // Eşya elimizde mi kontrol et
        bool isFromHand = heldItemManager != null && 
                         heldItemManager.IsHoldingItem && 
                         heldItemManager.GetHeldWorldObject() == item;

        currentItem = inspectable;

        if (isFromHand)
        {
            // Eldeki eşyayı inspect ediyoruz
            isInspectingHeldItem = true;
            heldItemManager.ReleaseForInspect(); // HeldItemManager'dan geçici olarak çıkar
            Debug.Log($"🎯 Inspecting held item: {currentItem.name} (Press Esc or Tab to stop)");
        }
        else
        {
            // Dünyadan inspect ediyoruz
            isInspectingHeldItem = false;
            currentItem.SaveOriginalTransform(); // Sadece dünya eşyaları için kaydet
            Debug.Log($"🎯 Inspecting world item: {currentItem.name} (Press Esc or F to stop)");
        }

        // Her durumda inspectHolder'a temiz bir şekilde yerleştir
        currentItem.transform.SetParent(inspectHolder);
        
        // ZORLA local transform'u sıfırla - bu kesinlikle 0,0,0 olmalı
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        
        // Scale'i doğru ayarla
        Vector3 correctScale;
        if (isInspectingHeldItem && heldItemManager != null)
        {
            // Eldeki eşya için HeldItemManager'dan orijinal scale'i al
            correctScale = heldItemManager.GetOriginalScale();
            Debug.Log($"🔍 Using held item original scale: {correctScale}");
        }
        else
        {
            // Dünya eşyası için kaydettiğimiz orijinal scale'i kullan
            correctScale = currentItem.originalScale;
            Debug.Log($"🔍 Using world item original scale: {correctScale}");
        }
        
        currentItem.transform.localScale = correctScale;
        
        // Güvenlik için bir kez daha kontrol et
        if (currentItem.transform.localPosition != Vector3.zero)
        {
            Debug.LogWarning("Local position is not zero, forcing it!");
            currentItem.transform.localPosition = Vector3.zero;
        }

        isInspecting = true;
        fpsController.canLook = false;
    }

    // Obje kaybedildiği için inspect modunu sonlandır
    private void EndInspectDueToItemLoss()
    {
        // Sadece inspect modunu temizle, objeyi dokunma (PlayerInventory hallediyor)
        currentItem = null;
        isInspecting = false;
        isInspectingHeldItem = false;

        fpsController.canLook = true; // Kamerayı tekrar aktif et
        
        Debug.Log("🔍 Inspect mode ended due to item loss - camera unlocked");
    }

    // Inspect durumunu kontrol et
    public bool IsInspecting => isInspecting;
}