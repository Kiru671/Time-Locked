using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FollowTransform: NetworkBehaviour
{
    private GameObject thisObject;
    private Transform targetTransform;
    private bool initialized;
    private ulong expectedOwnerClientId;
    private float ownershipCheckTimer = 0f;
    private bool ownershipConfirmed = false;
    private bool isCorrectClient;
    private const float OWNERSHIP_CHECK_DURATION = 3f;

    // Hand positioning
    private Vector3 handOffset = Vector3.zero;
    private Vector3 handRotation = Vector3.zero;
    private float handScale = 1f;
    private Vector3 originalScale = Vector3.one;

    public void Initialize(GameObject worldObject, Transform target, ulong ownerClientId, 
                          Vector3 offset = default(Vector3), Vector3 rotation = default(Vector3), float scale = 1f)
    {
        Debug.Log($"🚀 FollowTransform.Initialize called:");
        Debug.Log($"   - worldObject: {worldObject?.name ?? "NULL"}");
        Debug.Log($"   - target: {target?.name ?? "NULL"} at {target?.position ?? Vector3.zero}");
        Debug.Log($"   - ownerClientId: {ownerClientId}");
        Debug.Log($"   - offset: {offset}");
        Debug.Log($"   - rotation: {rotation}");
        Debug.Log($"   - scale: {scale}");
        
        if (worldObject == null)
        {
            Debug.LogError("❌ worldObject is null in Initialize!");
            return;
        }
        
        if (target == null)
        {
            Debug.LogError("❌ target is null in Initialize!");
            return;
        }
        
        thisObject = worldObject;
        targetTransform = target;
        expectedOwnerClientId = ownerClientId;
        handOffset = offset;
        handRotation = rotation;
        handScale = scale;
        originalScale = worldObject.transform.localScale;
        
        initialized = true;
        ownershipCheckTimer = 0f;
        ownershipConfirmed = false;
        
        Debug.Log($"✅ FollowTransform initialized successfully for {thisObject.name}");
    }
    
    // In FollowTransform.cs
    private void Start()
    {
        Debug.Log($"FollowTransform started on {gameObject.name}. IsOwner: {IsOwner}, LocalClientId: {NetworkManager.Singleton.LocalClientId}, OwnerClientId: {OwnerClientId}");
    
        // Add a small delay to ensure ownership is properly set
        StartCoroutine(DelayedOwnershipCheck());
    }

    private IEnumerator DelayedOwnershipCheck()
    {
        yield return new WaitForSeconds(0.1f);
    
        isCorrectClient = NetworkManager.Singleton.LocalClientId == expectedOwnerClientId;
        bool hasOwnership = IsOwner;
    
        Debug.Log($"Delayed ownership check: LocalClient={NetworkManager.Singleton.LocalClientId}, Expected={expectedOwnerClientId}, IsOwner={hasOwnership}");
    
        if (isCorrectClient && hasOwnership)
        {
            ownershipConfirmed = true;
            Debug.Log($"Ownership confirmed for {gameObject.name}");
        }
    }

    private void Update()
    {
        if (!initialized || targetTransform == null)
            return;

        // Handle ownership verification in Update instead of coroutine
        if (!ownershipConfirmed && ownershipCheckTimer < OWNERSHIP_CHECK_DURATION)
        {
            ownershipCheckTimer += Time.deltaTime;
            
            isCorrectClient = NetworkManager.Singleton.LocalClientId == expectedOwnerClientId;
            bool hasOwnership = IsOwner;
            
            // More lenient ownership check - allow if either condition is true OR if expectedOwnerClientId is 0 (server)
            bool allowMovement = (isCorrectClient && hasOwnership) || 
                                (expectedOwnerClientId == 0 && hasOwnership) || 
                                (isCorrectClient && expectedOwnerClientId == 0);
            
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"🔍 Ownership check: LocalClient={NetworkManager.Singleton.LocalClientId}, Expected={expectedOwnerClientId}, IsOwner={hasOwnership}, Allow={allowMovement}");
            }
            
            if (allowMovement)
            {
                ownershipConfirmed = true;
                Debug.Log($"✅ Ownership confirmed for {gameObject.name}. LocalClientId: {NetworkManager.Singleton.LocalClientId}, IsOwner: {IsOwner}");
            }
            else if (ownershipCheckTimer >= OWNERSHIP_CHECK_DURATION)
            {
                Debug.LogWarning($"⚠️ Ownership timeout for {gameObject.name}. Allowing movement anyway...");
                // Still allow movement even without confirmed ownership for debugging
                ownershipConfirmed = true;
            }
        }

        // Only move if ownership is confirmed (or timed out)
        if (!ownershipConfirmed)
            return;

        // More lenient final check
        isCorrectClient = NetworkManager.Singleton.LocalClientId == expectedOwnerClientId;
        bool allowFinalMovement = (isCorrectClient && IsOwner) || 
                                 (expectedOwnerClientId == 0 && IsOwner) || 
                                 (isCorrectClient && expectedOwnerClientId == 0);
        
        if (!allowFinalMovement)
        {
            if (Time.frameCount % 60 == 0)
                Debug.LogWarning($"❌ Final ownership check failed: LocalClient={NetworkManager.Singleton.LocalClientId}, Expected={expectedOwnerClientId}, IsOwner={IsOwner}");
            return;
        }

        // Apply hand positioning with offset
        Vector3 targetPosition = targetTransform.position + targetTransform.TransformDirection(handOffset);
        Quaternion targetRotation = targetTransform.rotation * Quaternion.Euler(handRotation);
        
        // Set the object's world position and rotation
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = originalScale * handScale;
        
        // Debug the positioning every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🔄 FollowTransform: {gameObject.name} | Hand: {targetTransform.position} | Object: {transform.position} | Offset: {handOffset}");
        }
    }
}