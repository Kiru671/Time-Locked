using UnityEngine;
using StarterAssets;
using Unity.Netcode;
using Unity.VisualScripting;

public class HeldItemManager : NetworkBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] private Transform handTransform; // Elimdeki objenin görüneceği yer
    [SerializeField] private Vector3 handOffset = Vector3.zero; // El pozisyon offseti
    [SerializeField] private Vector3 handRotation = Vector3.zero; // El rotasyon offseti
    [SerializeField] private float handScale = 1f; // Eldeki obje scale'i

    [Header("Animation Settings")]
    [SerializeField] private bool enableBobbing = true; // Sallanma efekti
    [SerializeField] private float bobbingSpeed = 2f; // Sallanma hızı
    [SerializeField] private float bobbingAmount = 0.05f; // Sallanma miktarı
    [SerializeField] private bool enableSway = true; // Mouse ile sway efekti
    [SerializeField] private float swayAmount = 0.02f; // Sway miktarı
    [SerializeField] private float swaySmooth = 4f; // Sway smoothness
    [SerializeField] private bool enableTilt = true; // Yürürken eğilme
    [SerializeField] private float tiltAmount = 5f; // Eğilme derecesi
    [SerializeField] private bool enableMovementSway = true; // WASD ile sway efekti
    [SerializeField] private float movementSwayAmount = 0.04f; // Hareket sway miktarı
    [SerializeField] private float movementTiltAmount = 8f; // Hareket eğilme miktarı

    [Header("Controller Integration")]
    [SerializeField] private FirstPersonController fpsController; // FPS Controller referansı
    [SerializeField] private bool autoFindController = true; // Otomatik controller bulma

    private InventoryItemData currentHeldItem;
    public NetworkObject currentHeldNetworkItem;
    private int heldItemSlotIndex = -1;
    private GameObject currentHeldWorldObject; // Elimizdeki fiziksel obje
    private bool isInInspectMode = false; // Inspect modunda mı
    private Vector3 objectOriginalScale; // Objenin gerçek orijinal scale'i (handScale uygulanmadan önce)
    [SerializeField] private Transform camFollow;

    // Animation variables
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalRotation;
    private float bobTimer = 0f;
    private Vector2 currentSway;
    private Vector2 currentMovementSway;
    private StarterAssetsInputs fpsInput; // FPS Controller input'u

    public InventoryItemData CurrentHeldItem => currentHeldItem;
    public int HeldItemSlotIndex => heldItemSlotIndex;
    public bool IsHoldingItem => currentHeldItem != null;
    public bool IsInInspectMode => isInInspectMode;

    private void Start()
    {
         if (NetworkManager.Singleton != null)
    {
        Debug.Log("=== REGISTERED NETWORK PREFABS ===");
        var prefabsList = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        
        for (int i = 0; i < prefabsList.Count; i++)
        {
            var prefab = prefabsList[i];
            Debug.Log($"[{i}] Prefab: {prefab.Prefab.name} - Hash: {prefab.Prefab.GetComponent<NetworkObject>().PrefabIdHash}");
        }
    }
        
        // FPS Controller'ı otomatik bul
        if (autoFindController && fpsController == null)
        {
            fpsController = FindObjectOfType<FirstPersonController>();
            if (fpsController == null)
            {
                Debug.LogWarning("FirstPersonController not found! Hand animations may not work properly.");
            }
        }
        RequestSpawnCameraServerRpc();
        RequestSpawnServerRpc(camFollow.GetComponent<NetworkObject>().NetworkObjectId);
        RequestSpawnServerRpc(handTransform.GetComponent<NetworkObject>().NetworkObjectId);
        NetworkObject camTR_ = camFollow.GetComponent<NetworkObject>();
        NetworkObject handTransformNetworkObject = handTransform.GetComponent<NetworkObject>();
        RequestReparentServerRpc(handTransformNetworkObject.NetworkObjectId, camTR_.NetworkObjectId);
        RequestReparentServerRpc(camTR_.NetworkObjectId, GetComponent<NetworkObject>().NetworkObjectId);
        
        // Input componenti al
        if (fpsController != null)
        {
            fpsInput = fpsController.GetComponent<StarterAssetsInputs>();
            if (fpsInput == null)
            {
                Debug.LogWarning("StarterAssetsInputs not found on FirstPersonController!");
            }
        }
    }

    private void Update()
    {
        if (currentHeldWorldObject != null && handTransform != null && !isInInspectMode)
        {
            UpdateHeldItemAnimation();
        }
    }

    // Eşyayı ele alma (dünya objesini de belirt)
    public bool TakeItem(InventoryItemData item, int slotIndex, GameObject worldObject = null)
    {
        if (IsHoldingItem)
        {
            return false; // Zaten bir eşya tutuyor
        }

        currentHeldItem = item;
        if (worldObject != null)
        {
            currentHeldNetworkItem = worldObject.GetComponent<NetworkObject>();
            if (currentHeldNetworkItem == null)
            {
                Debug.LogError("Current held item does not have a NetworkObject component!");
                return false;
            }

            heldItemSlotIndex = slotIndex;

            // Eğer dünya objesi verilmişse elimde göster
            if (worldObject != null)
            {
                ShowItemInHand(worldObject);
            }
        }
        return true;
    }

    // Eşyayı geri bırakma
    public InventoryItemData PutBackItem()
    {
        if (!IsHoldingItem)
            return null;

        InventoryItemData itemToReturn = currentHeldItem;
        
        // Eldeki objeyi gizle
        HideItemFromHand();
        
        currentHeldItem = null;
        heldItemSlotIndex = -1;
        
        return itemToReturn;
    }

    // Eşyayı kullanma (consume etme)
    public InventoryItemData UseHeldItem()
    {
        if (!IsHoldingItem)
            return null;

        InventoryItemData usedItem = currentHeldItem;
        
        // Eldeki objeyi gizle
        HideItemFromHand();
        
        currentHeldItem = null;
        heldItemSlotIndex = -1;
        
        return usedItem;
    }

    // Objeyi elimde göster
    private void ShowItemInHand(GameObject worldObject)
    {
        if (handTransform == null)
        {
            Debug.LogWarning("Hand transform not assigned!");
            return;
        }

        currentHeldWorldObject = worldObject;
        currentHeldNetworkItem = worldObject.GetComponent<NetworkObject>();
        
        // Objenin orijinal scale'ini kaydet (handScale uygulanmadan önce)
        objectOriginalScale = worldObject.transform.localScale;
        
        // Objeyi elin pozisyonuna getir
        RequestReparentServerRpc(currentHeldNetworkItem.NetworkObjectId, handTransform.GetComponent<NetworkObject>().NetworkObjectId);
        worldObject.transform.localPosition = handOffset;
        worldObject.transform.localRotation = Quaternion.Euler(handRotation);
        worldObject.transform.localScale = objectOriginalScale * handScale; // Orijinal scale * handScale
        
        // Orijinal pozisyon ve rotasyonu kaydet (animasyon için)
        originalLocalPosition = handOffset;
        originalLocalRotation = handRotation;
        
        // Objeyi aktif et (pickup sırasında deaktif edilmişti)
        worldObject.SetActive(true);
        
        // Rigidbody varsa kinematic yap (elimden düşmesin)
        Rigidbody rb = worldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Collider'ı deaktif et (elimdeyken collision olmasın)
        Collider col = worldObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Animation timer'ı sıfırla
        bobTimer = 0f;
        
        Debug.Log($"📋 Showing {worldObject.name} in hand with FPS-integrated animations");
    }

    // Objeyi elimden gizle
    private void HideItemFromHand()
    {
        if (currentHeldWorldObject == null)
            return;
            
        // Parent'ı kaldır
        currentHeldWorldObject.transform.SetParent(null);
        
        // *** ÖNEMLİ: Scale'i orijinaline geri döndür ***
        currentHeldWorldObject.transform.localScale = objectOriginalScale;
        Debug.Log($"📋 Restored original scale: {objectOriginalScale} for {currentHeldWorldObject.name}");
        
        // Rigidbody'yi normale döndür
        Rigidbody rb = currentHeldWorldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        // Collider'ı aktif et
        Collider col = currentHeldWorldObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Objeyi gizle (envantere geri dönüyor)
        currentHeldWorldObject.SetActive(false);
        
        Debug.Log($"📋 Hidden {currentHeldWorldObject.name} from hand");
        
        currentHeldWorldObject = null;
    }

    // Eldeki obje animasyonunu güncelle (FPS Controller ile entegre)
    private void UpdateHeldItemAnimation()
    {
        if (fpsInput == null || fpsController == null) return;

        // FPS Controller'dan hareket verilerini al
        Vector2 moveInput = fpsInput.move; // WASD
        Vector2 lookInput = fpsInput.look; // Mouse
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isGrounded = fpsController.Grounded;
        bool isSprinting = fpsInput.sprint;

        // Animasyonları hesapla
        Vector3 targetPosition = originalLocalPosition;
        Vector3 targetRotation = originalLocalRotation;

        // Bobbing efekti (yürürken yukarı aşağı sallanma)
        if (enableBobbing && isGrounded)
        {
            float speedMultiplier = 1f;
            if (isMoving)
            {
                speedMultiplier = isSprinting ? 2f : 1.5f; // Sprint'te daha hızlı
            }
            
            bobTimer += Time.deltaTime * bobbingSpeed * speedMultiplier;
            
            float bobOffset = Mathf.Sin(bobTimer) * bobbingAmount;
            targetPosition.y += bobOffset;
            
            // Yürürken hafif ileri geri hareket
            if (isMoving)
            {
                targetPosition.z += Mathf.Cos(bobTimer * 0.5f) * bobbingAmount * 0.5f;
            }
        }

        // Sway efekti (mouse hareketiyle yan sallanma)
        if (enableSway && lookInput.magnitude > 0.01f)
        {
            Vector2 targetSway = new Vector2(-lookInput.x, -lookInput.y) * swayAmount;
            currentSway = Vector2.Lerp(currentSway, targetSway, Time.deltaTime * swaySmooth);
            
            targetPosition.x += currentSway.x;
            targetPosition.y += currentSway.y;
            
            // Rotasyon sway'i
            targetRotation.z += currentSway.x * 10f;
            targetRotation.x += currentSway.y * 10f;
        }
        else
        {
            // Sway'i yavaşça sıfırla
            currentSway = Vector2.Lerp(currentSway, Vector2.zero, Time.deltaTime * swaySmooth);
        }

        // Movement Sway efekti (WASD ile momentum tabanlı sallanma)
        if (enableMovementSway && isGrounded)
        {
            // Momentum efekti: Sağa gidince obje sola eğilir (atalet)
            Vector2 targetMovementSway = new Vector2(-moveInput.x, -moveInput.y) * movementSwayAmount;
            currentMovementSway = Vector2.Lerp(currentMovementSway, targetMovementSway, Time.deltaTime * swaySmooth);
            
            // Pozisyon sway'i
            targetPosition.x += currentMovementSway.x;
            targetPosition.z += currentMovementSway.y * 0.5f; // Z ekseni için daha az

            // Rotasyon sway'i (momentum tabanlı eğilme)
            float tiltMultiplier = isSprinting ? 1.5f : 1f; // Sprint'te daha fazla eğilme
            targetRotation.z += moveInput.x * movementTiltAmount * tiltMultiplier; // Sağa gidince sağa eğil
            targetRotation.x += moveInput.y * movementTiltAmount * 0.5f * tiltMultiplier; // İleri gidince hafif öne eğil
            
            // Ek bobbing efekti hareket ederken
            if (isMoving)
            {
                float movementBob = Mathf.Sin(bobTimer * 1.5f) * bobbingAmount * 0.3f;
                targetPosition.y += movementBob;
            }
        }

        // Tilt efekti (yürürken eğilme)
        if (enableTilt && isMoving && isGrounded)
        {
            float tiltOffset = Mathf.Sin(bobTimer * 2f) * tiltAmount;
            targetRotation.z += tiltOffset;
        }

        // Landing efekti (yere indiğinde)
        if (isGrounded && !isMoving)
        {
            float landingBob = Mathf.Sin(bobTimer * 0.5f) * bobbingAmount * 0.3f;
            targetPosition.y += landingBob;
        }

        // Smooth geçiş
        currentHeldWorldObject.transform.localPosition = Vector3.Lerp(
            currentHeldWorldObject.transform.localPosition, 
            targetPosition, 
            Time.deltaTime * 8f
        );
        
        currentHeldWorldObject.transform.localRotation = Quaternion.Lerp(
            currentHeldWorldObject.transform.localRotation,
            Quaternion.Euler(targetRotation),
            Time.deltaTime * 6f
        );
    }

    // Elimdeki objeyi al (placement için)
    public GameObject GetHeldWorldObject()
    {
        return currentHeldWorldObject;
    }

    // Objenin orijinal scale'ini al
    public Vector3 GetOriginalScale()
    {
        return objectOriginalScale;
    }

    // El transform'ını değiştirme
    public void SetHandTransform(Transform newHandTransform)
    {
        handTransform = newHandTransform;
    }

    // Animation ayarlarını runtime'da değiştirme
    public void SetAnimationSettings(bool bobbing, bool sway, bool tilt, bool movementSway = true)
    {
        enableBobbing = bobbing;
        enableSway = sway;
        enableTilt = tilt;
        enableMovementSway = movementSway;
    }

    // FPS Controller referansını manuel ayarlama
    public void SetFPSController(FirstPersonController controller)
    {
        fpsController = controller;
        if (controller != null)
        {
            fpsInput = controller.GetComponent<StarterAssetsInputs>();
        }
    }

    // Inspect modu kontrolü
    public void SetInspectMode(bool inspectMode)
    {
        isInInspectMode = inspectMode;
        Debug.Log($"🎯 Held item inspect mode: {(inspectMode ? "ON" : "OFF")}");
    }

    // Inspect modu için objeyi geçici olarak serbest bırak
    public void ReleaseForInspect()
    {
        if (currentHeldWorldObject == null) return;
        
        // Sadece parent'ı kaldır - pozisyon ayarlamasını ItemInspector yapsın
        currentHeldWorldObject.transform.SetParent(null);
        isInInspectMode = true;
        
        Debug.Log($"🔍 Released {currentHeldWorldObject.name} for inspection");
    }

    // Inspect'ten sonra objeyi geri al
    public void RecoverFromInspect()
    {
        if (currentHeldWorldObject == null || !isInInspectMode) return;
        
        // Objeyi tekrar elin pozisyonuna getir
        ShowItemInHand(currentHeldWorldObject);
        isInInspectMode = false;
        
        Debug.Log($"🔍 Recovered {currentHeldWorldObject.name} from inspection");
    }
    
    [ServerRpc]
    void RequestReparentServerRpc(ulong networkObjectId, ulong newParentId)
    {
        NetworkObject obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        Transform newParent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[newParentId].transform;
        obj.transform.SetParent(newParent);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(ulong objId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objId, out var obj))
        {
            obj.Spawn();
        }
    }
    [ServerRpc]
    private void RequestSpawnCameraServerRpc()
    {
        camFollow.GetComponent<NetworkObject>().Spawn();
        handTransform.gameObject.GetComponent<NetworkObject>().Spawn();
    }
} 