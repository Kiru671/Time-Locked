using UnityEngine;
using UnityEditor;
using StarterAssets;

public class PuzzleGhostSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = false;
    public GameObject ghostPrefab;
    public Transform[] puzzlePoints;
    
    [Header("Debug")]
    public bool showSetupInfo = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPuzzleGhostSystem();
        }
    }
    
    [ContextMenu("Setup Puzzle Ghost System")]
    public void SetupPuzzleGhostSystem()
    {
        // PuzzleTimerManager oluştur
        GameObject timerManager = GameObject.Find("PuzzleTimerManager");
        if (timerManager == null)
        {
            timerManager = new GameObject("PuzzleTimerManager");
        }
        
        PuzzleTimerManager timer = timerManager.GetComponent<PuzzleTimerManager>();
        if (timer == null)
        {
            timer = timerManager.AddComponent<PuzzleTimerManager>();
        }
        
        // Ayarları yap
        if (ghostPrefab != null)
        {
            timer.ghostPrefab = ghostPrefab;
        }
        
        if (puzzlePoints != null && puzzlePoints.Length > 0)
        {
            timer.puzzlePoints = puzzlePoints;
        }
        
        // NavMesh kontrolü
        CheckNavMesh();
        
        if (showSetupInfo)
        {
            Debug.Log("Puzzle Ghost System kurulumu tamamlandı!");
            Debug.Log($"Timer Manager: {timerManager.name}");
            Debug.Log($"Ghost Prefab: {(ghostPrefab != null ? ghostPrefab.name : "Atanmamış")}");
            Debug.Log($"Puzzle Points: {puzzlePoints?.Length ?? 0}");
        }
    }
    
    private void CheckNavMesh()
    {
        // NavMesh'in var olup olmadığını kontrol et
        if (UnityEngine.AI.NavMesh.CalculateTriangulation().vertices.Length == 0)
        {
            Debug.LogWarning("NavMesh bulunamadı! Hayalet hareket edemeyecek. NavMesh oluşturun.");
        }
    }
    
    [ContextMenu("Create Sample Puzzle Points")]
    public void CreateSamplePuzzlePoints()
    {
        // Örnek puzzle noktaları oluştur
        for (int i = 0; i < 3; i++)
        {
            GameObject point = new GameObject($"PuzzlePoint_{i + 1}");
            point.transform.position = transform.position + Vector3.forward * (i + 1) * 5f;
            
            // Gizmo için sphere ekle
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(point.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.5f;
            
            // Renderer'ı gizle
            sphere.GetComponent<Renderer>().enabled = false;
            
            // Collider'ı kaldır
            DestroyImmediate(sphere.GetComponent<Collider>());
        }
        
        Debug.Log("Örnek puzzle noktaları oluşturuldu!");
    }
    
    [ContextMenu("Create Sample Puzzle Items")]
    public void CreateSamplePuzzleItems()
    {
        // Örnek puzzle nesneleri oluştur
        for (int i = 0; i < 3; i++)
        {
            GameObject puzzleItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            puzzleItem.name = $"PuzzleItem_{i + 1}";
            puzzleItem.transform.position = transform.position + Vector3.right * (i + 1) * 3f;
            
            // PuzzleInteraction scriptini ekle
            PuzzleInteraction interaction = puzzleItem.AddComponent<PuzzleInteraction>();
            interaction.puzzleName = $"Puzzle {i + 1}";
            
            // Outline ekle (QuickOutline asset'i eksik olduğu için devre dışı)
            // Outline outline = puzzleItem.AddComponent<Outline>();
            // outline.OutlineMode = Outline.Mode.OutlineAll;
            // outline.OutlineColor = Color.yellow;
            // outline.OutlineWidth = 2f;
            
            // Collider'ı trigger yap
            Collider collider = puzzleItem.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        Debug.Log("Örnek puzzle nesneleri oluşturuldu!");
    }
    
    private void OnDrawGizmos()
    {
        if (puzzlePoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < puzzlePoints.Length; i++)
            {
                if (puzzlePoints[i] != null)
                {
                    Gizmos.DrawWireSphere(puzzlePoints[i].position, 1f);
                    
                    if (i < puzzlePoints.Length - 1 && puzzlePoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(puzzlePoints[i].position, puzzlePoints[i + 1].position);
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PuzzleGhostSetup))]
public class PuzzleGhostSetupEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PuzzleGhostSetup setup = (PuzzleGhostSetup)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup Puzzle Ghost System"))
        {
            setup.SetupPuzzleGhostSystem();
        }
        
        if (GUILayout.Button("Create Sample Puzzle Points"))
        {
            setup.CreateSamplePuzzlePoints();
        }
        
        if (GUILayout.Button("Create Sample Puzzle Items"))
        {
            setup.CreateSamplePuzzleItems();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Bu script puzzle ghost sistemini otomatik olarak kurar. Önce NavMesh oluşturduğunuzdan emin olun.", MessageType.Info);
    }
}
#endif 