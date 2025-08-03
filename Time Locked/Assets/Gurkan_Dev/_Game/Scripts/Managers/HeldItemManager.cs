// ===== FIXED HeldItemManager.cs =====

using System.Collections;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;

public class HeldItemManager : NetworkBehaviour
{
    [Header("Hand Settings")] [SerializeField]
    private Transform handTransform;

    [SerializeField] private Vector3 handOffset = Vector3.zero;
    [SerializeField] private Vector3 handRotation = Vector3.zero;
    [SerializeField] private float handScale = 1f;

    [Header("Animation Settings")] [SerializeField]
    private bool enableBobbing = true;

    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private bool enableSway = true;
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float swaySmooth = 4f;
    [SerializeField] private bool enableTilt = true;
    [SerializeField] private float tiltAmount = 5f;
    [SerializeField] private bool enableMovementSway = true;
    [SerializeField] private float movementSwayAmount = 0.04f;
    [SerializeField] private float movementTiltAmount = 8f;

    [Header("Controller Integration")] [SerializeField]
    private FirstPersonController fpsController;

    [SerializeField] private bool autoFindController = true;

    private InventoryItemData currentHeldItem;
    public NetworkObject currentHeldNetworkItem;
    private int heldItemSlotIndex = -1;
    private GameObject currentHeldWorldObject;
    private bool isInInspectMode = false;
    private Vector3 objectOriginalScale;
    [SerializeField] private Transform camFollow;

    // Animation variables
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalRotation;
    private float bobTimer = 0f;
    private Vector2 currentSway;
    private Vector2 currentMovementSway;
    private StarterAssetsInputs fpsInput;

    public InventoryItemData CurrentHeldItem => currentHeldItem;
    public int HeldItemSlotIndex => heldItemSlotIndex;
    public bool IsHoldingItem => currentHeldItem != null;
    public bool IsInInspectMode => isInInspectMode;

    private NoteController _noteController;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("=== REGISTERED NETWORK PREFABS ===");
            var prefabsList = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

            for (int i = 0; i < prefabsList.Count; i++)
            {
                var prefab = prefabsList[i];
                Debug.Log(
                    $"[{i}] Prefab: {prefab.Prefab.name} - Hash: {prefab.Prefab.GetComponent<NetworkObject>().PrefabIdHash}");
            }

            if (autoFindController && fpsController == null)
            {
                fpsController = FindObjectOfType<FirstPersonController>();
                if (fpsController == null)
                {
                    Debug.LogWarning("FirstPersonController not found! Hand animations may not work properly.");
                }
            }
        }

        if (fpsController != null)
        {
            fpsInput = fpsController.GetComponent<StarterAssetsInputs>();
            if (fpsInput == null)
            {
                Debug.LogWarning("StarterAssetsInputs not found on FirstPersonController!");
            }
        }
    }

    // Add this method to your HeldItemManager for debugging
    [ContextMenu("Debug Hand Transform")]
    private void DebugHandTransform()
    {
        Debug.Log($"🤏 Hand Transform Debug:");
        Debug.Log($"   - handTransform: {handTransform?.name ?? "NULL"}");
        Debug.Log($"   - handTransform position: {handTransform?.position ?? Vector3.zero}");
        Debug.Log($"   - handOffset: {handOffset}");
        Debug.Log($"   - handRotation: {handRotation}");
        Debug.Log($"   - handScale: {handScale}");
        Debug.Log($"   - currentHeldWorldObject: {currentHeldWorldObject?.name ?? "NULL"}");
    }

    private void Update()
    {
        if (currentHeldWorldObject != null && handTransform != null && !isInInspectMode)
        {
            // Check if the object has a FollowTransform component
            FollowTransform followTransform = currentHeldWorldObject.GetComponent<FollowTransform>();

            // Only run our animation system if there's no FollowTransform
            if (followTransform == null)
            {
                UpdateHeldItemAnimation();
            }
        }

        if (currentHeldWorldObject != null && currentHeldWorldObject.CompareTag("Readable"))
        {
            if (Input.GetKey(KeyCode.F))
            {
                _noteController = currentHeldWorldObject.GetComponent<NoteController>();
                if (_noteController != null)
                    _noteController.ShowNote(fpsController);
            }
        }
    }

    // FIXED: Take item method
    // FIXED: TakeItem method - get the correct client ID
    // In HeldItemManager.cs
    public bool TakeItem(InventoryItemData item, int slotIndex, GameObject worldObject = null)
    {
        if (IsHoldingItem) return false;

        currentHeldItem = item;
        heldItemSlotIndex = slotIndex;

        if (worldObject != null)
        {
            currentHeldNetworkItem = worldObject.GetComponent<NetworkObject>();
            if (currentHeldNetworkItem == null)
            {
                Debug.LogError("Current held item does not have a NetworkObject component!");
                return false;
            }

            // Ensure the object is active before proceeding
            worldObject.SetActive(true);

            // Request server to handle the ownership transfer
            RequestItemOwnershipServerRpc(currentHeldNetworkItem.NetworkObjectId,
                NetworkManager.Singleton.LocalClientId);

            ShowItemInHand(worldObject, NetworkManager.Singleton.LocalClientId);
        }

        return true;
    }

    [ServerRpc]
    private void RequestItemOwnershipServerRpc(ulong itemNetworkId, ulong requestingClientId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId,
                out NetworkObject itemNetObj))
        {
            Debug.LogError($"Could not find NetworkObject with ID {itemNetworkId}");
            return;
        }

        // Change ownership to the client who requested it
        itemNetObj.ChangeOwnership(requestingClientId);
    }

    // FIXED: ShowItemInHand with clientId parameter
    private void ShowItemInHand(GameObject worldObject, ulong requestingClientId)
    {
        if (handTransform == null)
        {
            Debug.LogWarning("Hand transform not assigned!");
            return;
        }

        currentHeldWorldObject = worldObject;
        currentHeldNetworkItem = worldObject.GetComponent<NetworkObject>();


        if (currentHeldNetworkItem == null)
        {
            Debug.LogError("Current held item does not have a NetworkObject component!");
            return;
        }

        objectOriginalScale = worldObject.transform.localScale;
        originalLocalRotation = handRotation;

        // FIXED: Now we have requestingClientId
        if (IsServer)
        {
            Debug.Log(
                $"🔍 SetupHeldItemServerRpc called: ItemId={currentHeldNetworkItem.NetworkObjectId}, OwnerClientId={requestingClientId}");
            SetupHeldItemServerRpc(currentHeldNetworkItem.NetworkObjectId, requestingClientId);
        }
        else
        {
            Debug.Log(
                $"🔍 SetupHeldItemServerRpc called (Client): ItemId={currentHeldNetworkItem.NetworkObjectId}, OwnerClientId={requestingClientId}");
            SetupHeldItemServerRpc(currentHeldNetworkItem.NetworkObjectId, requestingClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetupHeldItemServerRpc(ulong itemNetworkId, ulong newOwnerClientId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId,
                out NetworkObject itemNetObj))
        {
            Debug.LogError($"Could not find NetworkObject with ID {itemNetworkId}");
            return;
        }

        // Change ownership to the client who picked it up
        itemNetObj.ChangeOwnership(newOwnerClientId);

        // Setup the item for being held
        SetupHeldItemClientRpc(itemNetworkId, newOwnerClientId);
    }

    [ClientRpc]
    private void SetupHeldItemClientRpc(ulong itemNetworkId, ulong ownerClientId)
    {
        Debug.Log(
            $"🔍 SetupHeldItemClientRpc called: ItemId={itemNetworkId}, OwnerClientId={ownerClientId}, LocalClientId={NetworkManager.Singleton.LocalClientId}");

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId,
                out NetworkObject itemNetObj))
        {
            Debug.LogError($"❌ Could not find NetworkObject with ID {itemNetworkId}");
            return;
        }

        GameObject worldObject = itemNetObj.gameObject;
        Debug.Log($"🎯 Found NetworkObject: {worldObject.name}");

        // Activate the object FIRST
        worldObject.SetActive(true);
        Debug.Log($"✅ Activated object: {worldObject.name}");

        // Only the owner should set up the FollowTransform
        if (itemNetObj.IsOwner)
        {
            StartCoroutine(WaitForOwnership(itemNetObj,
                () => { SetupFollowTransform(itemNetObj.gameObject, ownerClientId); }));

            Debug.Log(
                $"🎮 This client ({NetworkManager.Singleton.LocalClientId}) is the owner, setting up FollowTransform...");

            // Remove existing FollowTransform if any
            FollowTransform existingFollow = worldObject.GetComponent<FollowTransform>();
            if (existingFollow != null)
            {
                Debug.Log($"🗑️ Destroying existing FollowTransform");
                Destroy(existingFollow);
            }

            // Check if handTransform is valid
            if (handTransform == null)
            {
                Debug.LogError($"❌ handTransform is null! Cannot create FollowTransform");
                return;
            }

            Debug.Log($"🤏 HandTransform found: {handTransform.name} at {handTransform.position}");

            // Add new FollowTransform with hand settings
            FollowTransform followTransform = worldObject.AddComponent<FollowTransform>();
            Debug.Log($"✅ Added FollowTransform component to {worldObject.name}");

            // Initialize with hand settings
            followTransform.Initialize(worldObject, handTransform, ownerClientId, handOffset, handRotation, handScale);
            Debug.Log(
                $"✅ Initialized FollowTransform with handOffset={handOffset}, handRotation={handRotation}, handScale={handScale}");

            // Set up physics
            Rigidbody rb = worldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                Debug.Log($"🎈 Set Rigidbody to kinematic");
            }

            Collider col = worldObject.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                Debug.Log($"🚫 Disabled Collider");
            }

            bobTimer = 0f;

            // Verify the component was added
            FollowTransform verifyFollow = worldObject.GetComponent<FollowTransform>();
            if (verifyFollow != null)
            {
                Debug.Log($"✅ FollowTransform successfully attached and verified!");
            }
            else
            {
                Debug.LogError($"❌ FollowTransform failed to attach!");
            }
        }
        else
        {
            Debug.Log(
                $"👁️ This client ({NetworkManager.Singleton.LocalClientId}) is not the owner ({ownerClientId}), skipping FollowTransform setup");
        }

        Debug.Log($"📋 Setup held item {worldObject.name} for owner {ownerClientId} - COMPLETE");
    }

    private IEnumerator WaitForOwnership(NetworkObject netObj, System.Action onOwnershipConfirmed)
    {
        float timeout = 2f; // Max wait time in seconds
        float elapsed = 0f;

        while (!netObj.IsOwner && elapsed < timeout)
        {
            yield return null; // Wait for next frame
            elapsed += Time.deltaTime;
        }

        if (netObj.IsOwner)
        {
            Debug.Log($"✅ Ownership confirmed for {netObj.name}!");
            onOwnershipConfirmed?.Invoke();
        }
        else
        {
            Debug.LogWarning($"⏰ Timed out waiting for ownership of {netObj.name}");
        }
    }

    private void SetupFollowTransform(GameObject worldObject, ulong ownerClientId)
    {
        if (handTransform == null)
        {
            Debug.LogError("❌ Hand transform is null!");
            return;
        }

        // Remove existing FollowTransform
        FollowTransform existing = worldObject.GetComponent<FollowTransform>();
        if (existing != null) Destroy(existing);

        FollowTransform follow = worldObject.AddComponent<FollowTransform>();
        follow.Initialize(worldObject, handTransform, ownerClientId, handOffset, handRotation, handScale);
        Debug.Log("✅ FollowTransform setup complete");

        Rigidbody rb = worldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = worldObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }


    // Put back item method
    public InventoryItemData PutBackItem()
    {
        if (!IsHoldingItem)
            return null;

        InventoryItemData itemToReturn = currentHeldItem;

        HideItemFromHand();

        currentHeldItem = null;
        heldItemSlotIndex = -1;

        return itemToReturn;
    }

    // Use held item method
    public InventoryItemData UseHeldItem()
    {
        if (!IsHoldingItem)
            return null;

        InventoryItemData usedItem = currentHeldItem;

        HideItemFromHand();

        currentHeldItem = null;
        heldItemSlotIndex = -1;

        return usedItem;
    }

    // FIXED: HideItemFromHand method
    public void HideItemFromHand()
    {
        if (currentHeldWorldObject == null || currentHeldNetworkItem == null)
            return;

        Destroy(currentHeldWorldObject.GetComponent<FollowTransform>());

        if (IsServer)
        {
            ReleaseHeldItemServerRpc(currentHeldNetworkItem.NetworkObjectId);
        }
        else
        {
            ReleaseHeldItemServerRpc(currentHeldNetworkItem.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReleaseHeldItemServerRpc(ulong itemNetworkId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId,
                out NetworkObject itemNetObj))
            return;

        ReleaseHeldItemClientRpc(itemNetworkId);
    }

    [ClientRpc]
    private void ReleaseHeldItemClientRpc(ulong itemNetworkId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId,
                out NetworkObject itemNetObj))
            return;

        GameObject worldObject = itemNetObj.gameObject;

        // Restore original scale
        worldObject.transform.localScale = objectOriginalScale;

        // Clean up FollowTransform
        FollowTransform followTransform = worldObject.GetComponent<FollowTransform>();
        if (followTransform != null)
        {
            Destroy(followTransform);
        }

        // Restore physics
        Rigidbody rb = worldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = worldObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Hide the object
        worldObject.SetActive(false);

        Debug.Log($"📋 Released held item {worldObject.name}");

        // Clear references if this is the current held object
        if (currentHeldWorldObject == worldObject)
        {
            currentHeldWorldObject = null;
            currentHeldNetworkItem = null;
        }

        _noteController.DisableNote();
        _noteController = null;
    }

    // REMAINING ANIMATION CODE (unchanged)
    private void UpdateHeldItemAnimation()
    {
        if (fpsInput == null || fpsController == null) return;

        Vector2 moveInput = fpsInput.move;
        Vector2 lookInput = fpsInput.look;
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isGrounded = fpsController.Grounded;
        bool isSprinting = fpsInput.sprint;

        Vector3 targetPosition = originalLocalPosition;
        Vector3 targetRotation = originalLocalRotation;

        if (enableBobbing && isGrounded)
        {
            float speedMultiplier = 1f;
            if (isMoving)
            {
                speedMultiplier = isSprinting ? 2f : 1.5f;
            }

            bobTimer += Time.deltaTime * bobbingSpeed * speedMultiplier;

            float bobOffset = Mathf.Sin(bobTimer) * bobbingAmount;
            targetPosition.y += bobOffset;
            if (isMoving)
            {
                targetPosition.z += Mathf.Cos(bobTimer * 0.5f) * bobbingAmount * 0.5f;
            }
        }

        if (enableSway && lookInput.magnitude > 0.01f)
        {
            Vector2 targetSway = new Vector2(-lookInput.x, -lookInput.y) * swayAmount;
            currentSway = Vector2.Lerp(currentSway, targetSway, Time.deltaTime * swaySmooth);

            targetPosition.x += currentSway.x;
            targetPosition.y += currentSway.y;

            targetRotation.z += currentSway.x * 10f;
            targetRotation.x += currentSway.y * 10f;
        }
        else
        {
            currentSway = Vector2.Lerp(currentSway, Vector2.zero, Time.deltaTime * swaySmooth);
        }

        if (enableMovementSway && isGrounded)
        {
            Vector2 targetMovementSway = new Vector2(-moveInput.x, -moveInput.y) * movementSwayAmount;
            currentMovementSway = Vector2.Lerp(currentMovementSway, targetMovementSway, Time.deltaTime * swaySmooth);

            targetPosition.x += currentMovementSway.x;
            targetPosition.z += currentMovementSway.y * 0.5f;

            float tiltMultiplier = isSprinting ? 1.5f : 1f;
            targetRotation.z += moveInput.x * movementTiltAmount * tiltMultiplier;
            targetRotation.x += moveInput.y * movementTiltAmount * 0.5f * tiltMultiplier;

            if (isMoving)
            {
                float movementBob = Mathf.Sin(bobTimer * 1.5f) * bobbingAmount * 0.3f;
                targetPosition.y += movementBob;
            }
        }

        if (enableTilt && isMoving && isGrounded)
        {
            float tiltOffset = Mathf.Sin(bobTimer * 2f) * tiltAmount;
            targetRotation.z += tiltOffset;
        }

        if (isGrounded && !isMoving)
        {
            float landingBob = Mathf.Sin(bobTimer * 0.5f) * bobbingAmount * 0.3f;
            targetPosition.y += landingBob;
        }

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

    // UTILITY METHODS (unchanged)
    public GameObject GetHeldWorldObject() => currentHeldWorldObject;
    public Vector3 GetOriginalScale() => objectOriginalScale;
    public void SetHandTransform(Transform newHandTransform) => handTransform = newHandTransform;

    public void SetAnimationSettings(bool bobbing, bool sway, bool tilt, bool movementSway = true)
    {
        enableBobbing = bobbing;
        enableSway = sway;
        enableTilt = tilt;
        enableMovementSway = movementSway;
    }

    public void SetFPSController(FirstPersonController controller)
    {
        fpsController = controller;
        if (controller != null)
        {
            fpsInput = controller.GetComponent<StarterAssetsInputs>();
        }
    }

    public void SetInspectMode(bool inspectMode)
    {
        isInInspectMode = inspectMode;
        Debug.Log($"🎯 Held item inspect mode: {(inspectMode ? "ON" : "OFF")}");
    }

    public void ReleaseForInspect()
    {
        if (currentHeldWorldObject == null) return;

        currentHeldWorldObject.transform.SetParent(null);
        isInInspectMode = true;

        Debug.Log($"🔍 Released {currentHeldWorldObject.name} for inspection");
    }

    public void RecoverFromInspect()
    {
        if (currentHeldWorldObject == null || !isInInspectMode) return;

        ShowItemInHand(currentHeldWorldObject, NetworkManager.Singleton.LocalClientId);
        isInInspectMode = false;

        Debug.Log($"🔍 Recovered {currentHeldWorldObject.name} from inspection");
    }
}