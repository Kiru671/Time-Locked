using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private TextMeshProUGUI interactionText;

    private Camera _playerCamera;
    private static bool _interactionTextWarningShown = false;

    void Update()
    {
        if (!IsOwner) return;

        // If camera is not yet found, try to find it..
        if (_playerCamera == null)
        {
            _playerCamera = Camera.main;
            // If we still can't find it, return and try again next frame. This prevents the null reference.
            if (_playerCamera == null) return;
        }

        // Hide text by default.
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactionText != null)
                {
                    interactionText.text = interactable.GetInteractionText();
                    interactionText.gameObject.SetActive(true);
                }
                else if (!_interactionTextWarningShown)
                {
                     Debug.LogWarning("PlayerInteraction: 'Interaction Text' is not assigned in the Inspector on your Player Prefab.", this);
                     _interactionTextWarningShown = true;
                }
            }
        }
    }
} 