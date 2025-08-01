using UnityEngine;
using Unity.Netcode;

public class ClockPuzzleManager : NetworkBehaviour
{
    [Header("Puzzle Settings")]
    public ClockController[] clocks;
    public GameObject puzzleSolvedEffect;

    
    private NetworkVariable<bool> isPuzzleSolved = new NetworkVariable<bool>(false);
    
    public override void OnNetworkSpawn()
    {
        isPuzzleSolved.OnValueChanged += OnPuzzleStateChanged;
    }
    
    void Update()
    {
        if (!IsServer) return;
        
        CheckPuzzleCompletion();
        UpdateDebugUI();
    }
    
    void CheckPuzzleCompletion()
    {
        if (isPuzzleSolved.Value) return;

        // Her saatin kendi hedef zamanında olup olmadığını kontrol et
        bool allClocksCorrect = true;
        foreach (var clock in clocks)
        {
            if (!clock.IsCorrectTime())
            {
                allClocksCorrect = false;
                break;
            }
        }

        if (allClocksCorrect)
        {
            isPuzzleSolved.Value = true;
            OnPuzzleSolvedServerRpc();
        }
    }
    
    [ServerRpc]
    void OnPuzzleSolvedServerRpc()
    {
        string clockInfo = "";
        foreach (var clock in clocks)
        {
            clockInfo += $"{clock.clockId}: {clock.GetCurrentTimeString()} (Target: {clock.targetHour:D2}:{clock.targetMinute:D2}) ";
        }

        Debug.Log($"🎉 PUZZLE SOLVED! All clocks show their target times! {clockInfo}");

        // Tüm oyunculara puzzle çözüldü mesajı gönder
        ShowPuzzleSolvedMessageClientRpc();
    }

    [ClientRpc]
    void ShowPuzzleSolvedMessageClientRpc()
    {
        Debug.Log("🎉 PUZZLE SOLVED! Both clocks are synchronized! 🎉");
        // Burada UI mesajı veya ses efekti ekleyebilirsiniz
    }
    
    void OnPuzzleStateChanged(bool oldValue, bool newValue)
    {
        if (newValue && puzzleSolvedEffect != null)
        {
            puzzleSolvedEffect.SetActive(true);
        }
    }
    
    void UpdateDebugUI()
    {
        string debugInfo = "Clock Target Status:\n";
        bool allClocksCorrect = true;

        for (int i = 0; i < clocks.Length; i++)
        {
            var clock = clocks[i];
            bool isCorrect = clock.IsCorrectTime();
            string status = isCorrect ? "✓" : "✗";

            debugInfo += $"Clock {i + 1}: {clock.GetCurrentTimeString()} (Target: {clock.targetHour:D2}:{clock.targetMinute:D2}) {status}\n";

            if (!isCorrect) allClocksCorrect = false;
        }

        debugInfo += $"\nTarget Status: {(allClocksCorrect ? "ALL TARGETS REACHED ✓" : "TARGETS NOT REACHED")}";
        debugInfo += $"\nPuzzle Status: {(isPuzzleSolved.Value ? "SOLVED ✓" : "UNSOLVED")}";

        // Debug log'u çok sık yazdırmamak için sadece değişiklik olduğunda yazdır
        if (Time.frameCount % 60 == 0) // Her saniye bir kez
        {
            Debug.Log(debugInfo);
        }
    }
}