using UnityEngine;
using Unity.Netcode;

public class ClockController : NetworkBehaviour
{
    [Header("Clock Settings")]
    public Transform hourHand;
    public Transform minuteHand;
    public int targetHour = 3;
    public int targetMinute = 15;

    [Header("Interaction")]
    public float rotationSpeed = 30f;
    public KeyCode adjustKey = KeyCode.E;

    [Header("Clock Identity")]
    public string clockId = ""; // Her saat için benzersiz ID (Inspector'da manuel olarak ayarlayın)

    [Header("Starting Time")]
    public float startingMinutes = 0f; // Başlangıç zamanı (dakika cinsinden)

    // Mevcut zaman (network synchronized)
    private NetworkVariable<float> currentMinutes = new NetworkVariable<float>(0f);

    void Awake()
    {
        // Eğer clockId boşsa, otomatik bir ID oluştur
        if (string.IsNullOrEmpty(clockId))
        {
            clockId = "Clock_" + GetInstanceID(); // Daha basit ID
        }
    }

    public override void OnNetworkSpawn()
    {
        // Server'da başlangıç değerini ayarla
        if (IsServer)
        {
            currentMinutes.Value = startingMinutes;
        }

        // Değişiklikleri dinle
        currentMinutes.OnValueChanged += OnTimeChanged;
        UpdateClockVisuals();
        Debug.Log($"Clock {clockId} initialized: {currentMinutes.Value:F0} minutes = {GetCurrentTimeString()}");
    }

    void OnTimeChanged(float oldValue, float newValue)
    {
        UpdateClockVisuals();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Sadece bu saate yakın olan oyuncu input verebilsin
        // ClockInteraction script'i bunu halledecek, burada input almayalım
    }

    public void AdjustTime()
    {
        // Client'tan server'a istek gönder
        AdjustTimeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void AdjustTimeServerRpc()
    {
        // Her zaman tam 10 dakika ekle
        float oldMinutes = currentMinutes.Value;
        float newMinutes = (oldMinutes + 10f) % 720f;
        if (newMinutes < 0) newMinutes += 720f;

        currentMinutes.Value = newMinutes;
        Debug.Log($"Clock {clockId}: {oldMinutes:F0} -> {newMinutes:F0} minutes ({GetCurrentTimeString()})");
    }
    
    void UpdateClockVisuals()
    {
        // Toplam dakikayı saat ve dakikaya çevir
        float totalMinutes = currentMinutes.Value;

        // Dakika hesaplama: 0-59 dakika arası
        float currentMinute = totalMinutes % 60f;

        // YELKOVAN (Dakika çubuğu):
        // 60 dakika = 360 derece, her dakika = 6 derece
        // 10 dakika = 60 derece (6 kez E basınca tam tur)
        // 150° offset var (yelkovan 12'de 150° gösteriyor)
        float minuteAngle = (currentMinute * 6f) + 150f;

        // AKREP (Saat çubuğu):
        // Toplam dakikayı kullan, böylece tam tur attığında geri gitmez
        // 720 dakika = 360 derece, her dakika = 0.5 derece
        // 0° offset (akrep 12'de 0° gösteriyor)
        float hourAngle = (totalMinutes * 0.5f) % 360f;

        // Debug bilgisi
        Debug.Log($"Clock {clockId}: {GetCurrentTimeString()} | Total: {totalMinutes:F0}min | Hour: ({hourAngle:F1}°) | Minute: ({minuteAngle:F1}°)");

        // Rotation uygula
        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, hourAngle);

        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, minuteAngle);
    }
    
    public bool IsCorrectTime()
    {
        int currentHour = Mathf.FloorToInt(currentMinutes.Value / 60f) % 12;
        if (currentHour == 0) currentHour = 12; // 12 saatlik sistemde 0 = 12

        int currentMinute = Mathf.FloorToInt(currentMinutes.Value % 60f);

        return currentHour == targetHour && currentMinute == targetMinute;
    }

    public string GetCurrentTimeString()
    {
        int hour = Mathf.FloorToInt(currentMinutes.Value / 60f) % 12;
        if (hour == 0) hour = 12; // 12 saatlik sistemde 0 = 12

        int minute = Mathf.FloorToInt(currentMinutes.Value % 60f);

        return $"{hour:D2}:{minute:D2}";
    }

    // Debug için ek bilgi
    public string GetDetailedTimeInfo()
    {
        float totalMinutes = currentMinutes.Value;
        int hour = Mathf.FloorToInt(totalMinutes / 60f) % 12;
        if (hour == 0) hour = 12;
        int minute = Mathf.FloorToInt(totalMinutes % 60f);

        return $"{hour:D2}:{minute:D2} (Total: {totalMinutes:F0} min, Steps: {totalMinutes/10f:F0})";
    }
}
