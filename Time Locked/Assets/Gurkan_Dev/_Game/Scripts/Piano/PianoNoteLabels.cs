using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PianoNoteLabels : MonoBehaviour
{
    [Header("Label Settings")]
    public GameObject CanvasPrefab; // Canvas prefab you created
    public bool ShowLabelsOnStart = true;
    
    [Header("References")]
    public PianoKeyController PianoKeyController;
    
    private List<GameObject> createdLabels = new List<GameObject>();
    
    void Start()
    {
        if (PianoKeyController == null)
            PianoKeyController = FindObjectOfType<PianoKeyController>();
            
        if (ShowLabelsOnStart)
            CreateNoteLabels();
    }
    
    [ContextMenu("Create Note Labels")]
    public void CreateNoteLabels()
    {
        // Clear existing labels first
        ClearLabels();
        
        foreach (var kvp in PianoKeyController.PianoNotes)
        {
            string noteName = kvp.Key;
            PianoKey pianoKey = kvp.Value;
            
            CreateLabelForKey(pianoKey, noteName);
        }
    }
    
    [ContextMenu("Clear Labels")]
    public void ClearLabels()
    {
        foreach (GameObject label in createdLabels)
        {
            if (label != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(label);
                #else
                Destroy(label);
                #endif
            }
        }
        createdLabels.Clear();
    }
    
    void CreateLabelForKey(PianoKey pianoKey, string noteName)
    {
        if (CanvasPrefab == null)
        {
            Debug.LogError("Canvas Prefab is not assigned!");
            return;
        }
        
        // Instantiate the canvas prefab
        GameObject canvasGO = Instantiate(CanvasPrefab, pianoKey.transform);
        canvasGO.name = $"NoteLabel_{noteName}";
        
        // Set position based on layer
        Vector3 position;
        if (pianoKey.gameObject.layer == LayerMask.NameToLayer("Piano_Key_White"))
        {
            position = new Vector3(0, 0.000025f, -0.00176f);
        }
        else if (pianoKey.gameObject.layer == LayerMask.NameToLayer("Piano_Key_Black"))
        {
            position = new Vector3(0, 0.00026f, -0.00055f);
        }
        else
        {
            // Default position for white keys if layer doesn't match
            position = new Vector3(0, 0.000025f, -0.00176f);
        }
        
        canvasGO.transform.localPosition = position;
        canvasGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Find TextMeshPro component in the prefab (search in children too)
        TextMeshProUGUI tmpText = canvasGO.GetComponentInChildren<TextMeshProUGUI>();
        
        if (tmpText != null)
        {
            tmpText.text = FormatNoteName(noteName);
        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in canvas prefab for key {noteName}");
        }
        
        createdLabels.Add(canvasGO);
    }
    

    
    string FormatNoteName(string noteName)
    {
        // Format the note name for better display
        // Convert sharp symbol to proper musical symbol if needed
        return noteName.Replace("#", "♯");
    }
    
    public void ToggleLabels()
    {
        foreach (GameObject label in createdLabels)
        {
            if (label != null)
                label.SetActive(!label.activeSelf);
        }
    }
    
    public void SetLabelsVisible(bool visible)
    {
        foreach (GameObject label in createdLabels)
        {
            if (label != null)
                label.SetActive(visible);
        }
    }
} 