# Ghost Trigger System - Kullanım Kılavuzu

## Genel Bakış

GhostTriggerSystem, belirli bir item'in 90 saniye boyunca tutulması ve puzzle'da ilerleme kaydedilmemesi durumunda ghost'u tetikleyen bir sistemdir.

## Sistem Akışı

1. **Tetikleyici (Trigger)**: Oyuncu, belirli bir sınıfa ait bir item'i 90 saniye boyunca tutar
2. **Aktifleştirme (Activation)**: Koşullar sağlandığında ghost aktifleşir
3. **Spawn**: Ghost, oyuncunun yakınında spawn olur
4. **Hedefe Yönelme**: Ghost, puzzle'ın bir sonraki waypoint'ine doğru hareket eder
5. **Bekleme**: Hedef waypoint'e ulaştığında 5-10 saniye bekler
6. **Yok Olma**: Bekleme süresi dolunca ghost yok olur
7. **Yeniden Döngü**: Koşullar hala sağlanıyorsa döngü tekrar başlar

## Kurulum

### 1. GhostTriggerSystem Bileşeni Ekleme

1. Sahnede boş bir GameObject oluşturun
2. GhostTriggerSystem script'ini ekleyin
3. Gerekli ayarları yapın:

```
Trigger Settings:
- Trigger Time: 90 (saniye)
- Ghost Wait Time: 7 (saniye)
- Spawn Distance From Player: 5 (metre)

Ghost Settings:
- Ghost Prefab: Ghost prefab'ınızı atayın
- Waypoints: Puzzle waypoint'lerini atayın
- Player Transform: Oyuncu transform'unu atayın

Item Classes:
- Trigger Item Classes: Tetikleyici item sınıflarını ekleyin (örn: "Key", "Book", "Tool")

Visual Effects:
- Ghost Spawn Effect: Spawn efekti prefab'ı
- Ghost Spawn Sound: Spawn sesi
```

### 2. Item Sınıfları Tanımlama

InventoryItemData'da item'larınız için sınıf tanımlayın:

```
Item Name: "Eski Anahtar"
Item Class: "Key"
Is Trigger Item: true

Item Name: "Gizli Kitap"
Item Class: "Book"
Is Trigger Item: true
```

### 3. Puzzle İlerleme Takibi

Puzzle'larınızda ilerleme kaydedildiğinde PuzzleTimerManager'ı güncelleyin:

```csharp
// Puzzle tamamlandığında
PuzzleTimerManager.Instance.OnPuzzleProgress();
```

## Özellikler

### Otomatik Tetikleme
- Item 90 saniye tutulduğunda otomatik tetiklenir
- Puzzle'da ilerleme varsa süre sıfırlanır

### Akıllı Spawn
- Ghost oyuncuya yakın ama güvenli bir mesafede spawn olur
- NavMesh üzerinde geçerli pozisyon bulur

### Döngüsel Sistem
- Koşullar sağlandığı sürece ghost tekrar spawn olur
- Item bırakıldığında veya puzzle'da ilerleme kaydedildiğinde durur

### Debug Gizmos
- Scene view'da spawn mesafesi ve waypoint'ler görünür
- Debug bilgileri konsola yazdırılır

## API Metodları

### Public Metodlar

```csharp
// Belirli bir item sınıfının tetikleyicisini sıfırla
public void ResetTrigger(string itemClass)

// Ghost'u zorla durdur
public void ForceStopGhost()
```

### Private Metodlar

```csharp
// Item tutma sürelerini kontrol et
private void CheckItemHoldTimes()

// Ghost'u tetikle
private void TriggerGhost(string itemClass)

// Ghost spawn et
private void SpawnGhost()

// Ghost döngüsü
private IEnumerator GhostCycle(string itemClass)
```

## Örnek Kullanım

### 1. Basit Tetikleyici

```csharp
// GhostTriggerSystem'de
triggerItemClasses = new string[] { "Key", "Book" };
```

### 2. Puzzle İlerleme Takibi

```csharp
// PuzzleInteraction.cs'de
public void CompletePuzzle()
{
    // Puzzle tamamlandığında
    if (PuzzleTimerManager.Instance != null)
    {
        PuzzleTimerManager.Instance.OnPuzzleProgress();
    }
}
```

### 3. Manuel Kontrol

```csharp
// Ghost'u manuel olarak durdur
ghostTriggerSystem.ForceStopGhost();

// Belirli bir item'in tetikleyicisini sıfırla
ghostTriggerSystem.ResetTrigger("Key");
```

## Sorun Giderme

### Ghost Spawn Olmuyor
- Ghost prefab'ının atandığından emin olun
- Waypoint'lerin atandığından emin olun
- Player Transform'un atandığından emin olun

### Item Tetiklemiyor
- Item'ın itemClass'ının doğru tanımlandığından emin olun
- PlayerInventory'nin sahnede olduğundan emin olun

### Ghost Sürekli Spawn Oluyor
- Puzzle'da ilerleme kaydedildiğinde OnPuzzleProgress() çağrıldığından emin olun
- Item bırakıldığında sistem otomatik olarak durur

## Performans Notları

- Sistem her frame'de item kontrolü yapar
- Ghost spawn edildiğinde performans etkisi minimaldir
- NavMesh hesaplamaları optimize edilmiştir 