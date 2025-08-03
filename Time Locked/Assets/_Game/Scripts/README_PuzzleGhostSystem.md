# Puzzle Ghost System - Kurulum ve Kullanım

Bu sistem, oyuncunun 90 saniye boyunca puzzle ile etkileşime geçmemesi durumunda hayalet spawn edip oyuncuyu bir sonraki puzzle noktasına götüren bir mekanizma sağlar.

## Sistem Bileşenleri

### 1. PuzzleTimerManager
- **Konum**: `Assets/Gurkan_Dev/_Game/Scripts/PuzzleTimerManager.cs`
- **Amaç**: Oyuncunun puzzle etkileşimlerini takip eder ve 90 saniye sonra hayalet spawn eder
- **Ayarlar**:
  - `inactivityThreshold`: Hayalet spawn süresi (varsayılan: 90 saniye)
  - `ghostPrefab`: Spawn edilecek hayalet prefab'ı
  - `puzzlePoints`: Puzzle noktalarının dizisi
  - `ghostSpawnDistance`: Oyuncudan ne kadar uzakta spawn olacağı

### 2. GhostGuide
- **Konum**: `Assets/Gurkan_Dev/_Game/Scripts/GhostGuide.cs`
- **Amaç**: Hayaletin oyuncuyu puzzle noktalarına yönlendirmesi
- **Özellikler**:
  - NavMesh kullanarak hareket
  - Oyuncuya yaklaşma ve rehberlik
  - Oyuncuyu teleport etme
  - Görsel ve ses efektleri

### 3. PuzzleInteraction
- **Konum**: `Assets/Gurkan_Dev/_Game/Scripts/PuzzleInteraction.cs`
- **Amaç**: Puzzle nesnelerinin etkileşim mantığı
- **Özellikler**:
  - Timer'ı sıfırlama
  - Tamamlanma efektleri
  - Materyal değişimi

## Kurulum Adımları

### 1. PuzzleTimerManager Kurulumu
1. Sahnede boş bir GameObject oluşturun
2. `PuzzleTimerManager` scriptini ekleyin
3. Inspector'da ayarları yapın:
   - Ghost Prefab: Hayalet prefab'ını atayın
   - Puzzle Points: Puzzle noktalarını sürükleyin
   - Spawn Distance: Spawn mesafesini ayarlayın

### 2. Puzzle Noktaları Oluşturma
1. Her puzzle noktası için boş GameObject oluşturun
2. İsimlendirin (örn: "PuzzlePoint1", "PuzzlePoint2")
3. Bu noktaları PuzzleTimerManager'a atayın

### 3. Puzzle Nesneleri Oluşturma
1. Puzzle olacak nesneye `PuzzleInteraction` scriptini ekleyin
2. Collider ekleyin (trigger olabilir)
3. Outline component'i ekleyin (opsiyonel)
4. Inspector'da ayarları yapın:
   - Puzzle Name: Puzzle adı
   - Interaction Text: Etkileşim metni
   - Completion Effect: Tamamlanma efekti
   - Completed Material: Tamamlanma materyali

### 4. Hayalet Prefab'ı Hazırlama
1. Mevcut hayalet prefab'ını kullanın veya yeni oluşturun
2. `GhostGuide` scriptini ekleyin
3. NavMeshAgent component'i ekleyin
4. Animator ekleyin (opsiyonel)
5. AudioSource ekleyin (opsiyonel)
6. Light component'i ekleyin (opsiyonel)

## Kullanım Senaryosu

1. **Oyuncu puzzle ile etkileşime geçer** → Timer sıfırlanır
2. **90 saniye geçer** → Hayalet spawn olur
3. **Hayalet oyuncuya yaklaşır** → Rehberlik başlar
4. **Oyuncu puzzle noktasına yaklaşır** → Teleport edilir
5. **Sonraki puzzle noktasına geçer** → Süreç tekrarlanır

## Debug Özellikleri

- **OnGUI**: Ekranda kalan süreyi gösterir
- **Console Logs**: Sistem durumunu loglar
- **Gizmos**: Scene view'da görsel debug bilgileri

## Özelleştirme

### Timer Süresini Değiştirme
```csharp
// PuzzleTimerManager'da
public float inactivityThreshold = 90f; // 90 saniye
```

### Hayalet Hareket Hızını Değiştirme
```csharp
// GhostGuide'da
public float moveSpeed = 3f; // Hareket hızı
```

### Teleport Mesafesini Değiştirme
```csharp
// GhostGuide'da
public float teleportDistance = 3f; // Teleport mesafesi
```

## Gereksinimler

- Unity 2022.3 veya üzeri
- NavMesh (Navigation System)
- Input System (opsiyonel)
- Audio System (opsiyonel)

## Sorun Giderme

### Hayalet Spawn Olmuyor
- PuzzleTimerManager'ın sahnede olduğundan emin olun
- Ghost prefab'ının atandığından emin olun
- Puzzle noktalarının atandığından emin olun

### Hayalet Hareket Etmiyor
- NavMesh'in oluşturulduğundan emin olun
- NavMeshAgent component'inin eklendiğinden emin olun
- NavMesh'in güncel olduğundan emin olun

### Timer Sıfırlanmıyor
- Puzzle nesnelerinde `PuzzleInteraction` scriptinin olduğundan emin olun
- `IInteractable` interface'inin implement edildiğinden emin olun
- RaycastInteraction'ın çalıştığından emin olun 