using TMPro;
using UnityEngine;

public class PlayerSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inLobbyLabel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visual settings")] [SerializeField] private float occupiedAlpha = 1f; // 100 % opaque when taken

    [SerializeField] private float emptyAlpha = .25f; // 25 % opaque when free


    public void Refresh(bool occupied)
    {
        inLobbyLabel.SetActive(occupied); // Show “In Lobby” only when taken
        canvasGroup.alpha = occupied ? occupiedAlpha : emptyAlpha;
    }
}