using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
        hintPanel.SetActive(true);
    }

    public void HideHint()
    {
        hintPanel.SetActive(false);
    }
}
