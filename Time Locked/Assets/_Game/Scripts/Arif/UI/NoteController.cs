using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class NoteController : MonoBehaviour
{
    [Header("Input")] [SerializeField] private KeyCode closeKey;

    // Runtime reference to the player that opened this note. This is set when ShowNote is called
    private FirstPersonController currentPlayer;


    [Header("UI Text")] [SerializeField] private GameObject noteCanvas;

    [SerializeField] private TMP_Text noteTextAreaUI;

    [Space(10)] [SerializeField] [TextArea]
    private string noteText;
<<<<<<< Updated upstream
=======
    
    
    [Space(10)] [SerializeField]
    private Color color;
    [Space(10)] [SerializeField]
    private bool wrap;
    [Space(10)] [SerializeField]
    private bool autoSize;
    
>>>>>>> Stashed changes

    [Space(10)] [SerializeField] private UnityEvent openEvent; // For sound effects
    private bool isOpen;
    
<<<<<<< Updated upstream
=======
   
>>>>>>> Stashed changes
    
    void Update()
    {
        if(isOpen && Input.GetKeyDown(closeKey))
            DisableNote();
    }

    // Opens the note for the specified local player
    public void ShowNote(FirstPersonController player)
    {
        noteTextAreaUI.text = noteText;
<<<<<<< Updated upstream
=======
        noteTextAreaUI.textWrappingMode = (wrap) ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        noteTextAreaUI.color = color;
        noteTextAreaUI.enableAutoSizing = autoSize;
>>>>>>> Stashed changes
        noteCanvas.SetActive(true);
        openEvent.Invoke();
        currentPlayer = player;
        DisablePlayer(true);
        isOpen = true;
    }

    void DisableNote()
    {
        noteCanvas.SetActive(false);
        DisablePlayer(false);
        isOpen = false;
    }

    void DisablePlayer(bool disable)
    {
        if (currentPlayer == null) return;

        currentPlayer.enabled = !disable;

        // Disable input components so movement is completely halted
        var playerInput = currentPlayer.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = !disable;

        var starterInputs = currentPlayer.GetComponent<StarterAssets.StarterAssetsInputs>();
        if (starterInputs != null) starterInputs.enabled = !disable;
    }
}