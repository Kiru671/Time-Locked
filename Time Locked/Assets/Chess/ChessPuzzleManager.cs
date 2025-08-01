using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class PuzzleMove
{
    public string from;
    public string to;
    public string description;
}

[System.Serializable]
public class PuzzleLine
{
    public List<PuzzleMove> moves = new List<PuzzleMove>();
    public string description;
}

public class ChessPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public bool isWhiteTurn = true;
    public bool puzzleCompleted = false;

    [Header("Puzzle Lines")]
    public List<PuzzleLine> puzzleLines = new List<PuzzleLine>();

    private ChessBoardManager boardManager;
    private int currentMoveIndex = 0;
    private PuzzleLine currentLine = null;
    private bool waitingForPlayerMove = true;
    private string lastBlackMove = "";

    void Start()
    {
        boardManager = FindObjectOfType<ChessBoardManager>();
        SetupPuzzle();
    }

    void SetupPuzzle()
    {
        puzzleLines.Clear();

        PuzzleLine mainLine = new PuzzleLine();
        mainLine.description = "Main Line: 1. Se4";
        puzzleLines.Add(mainLine);

        Debug.Log("Puzzle setup complete. White to move!");
        Debug.Log("Find mate in 3 moves starting with Se4!");
    }

    public bool ValidateMove(string fromPosition, string toPosition, PieceColor pieceColor)
    {
        Debug.Log($"Hamle kabul edildi: {fromPosition} -> {toPosition} ({pieceColor})");
        return true;
    }

    bool ValidateWhiteMove(string fromPosition, string toPosition)
    {
        if (currentMoveIndex == 0)
        {
            if (fromPosition == "f5" && toPosition == "e4")
            {
                Debug.Log("✓ Doğru! 1. Se4 oynandı.");
                return true;
            }
            else
            {
                Debug.Log("✗ Yanlış hamle! İlk hamle 1. Se4 olmalı.");
                Debug.Log("İpucu: Şahı f5'ten e4'e taşı!");
                return false;
            }
        }

        else if (currentMoveIndex == 2)
        {
            switch (lastBlackMove)
            {
                case "Bd6xf4":
                    if (fromPosition == "c4" && toPosition == "d6")
                    {
                        Debug.Log("✓ Doğru! 2. Ad6 oynandı.");
                        return true;
                    }
                    break;

                case "Ka4-b5":
                    if (fromPosition == "g4" && toPosition == "d7")
                    {
                        Debug.Log("✓ Doğru! 2. Vxd7+ oynandı.");
                        return true;
                    }
                    break;

                case "Ka4-b3":
                    if (fromPosition == "g4" && toPosition == "e2")
                    {
                        Debug.Log("✓ Doğru! 2. Ve2 oynandı.");
                        return true;
                    }
                    break;

                default:
                    Debug.Log($"⚠ Siyahın hamlesi bilinmiyor: {lastBlackMove}");
                    return true;
            }

            Debug.Log("✗ Yanlış ikinci hamle!");
            return false;
        }

        else if (currentMoveIndex == 4)
        {
            switch (lastBlackMove)
            {
                case "Bd6xf4":
                    if (fromPosition == "g4" && (toPosition == "d1" || toPosition == "d7"))
                    {
                        Debug.Log("✓ Mat hamlesi! 3. Vd1# veya Vxd7#");
                        return true;
                    }
                    break;

                case "Ka4-b5":
                    if (fromPosition == "d7" && (toPosition == "c6" || toPosition == "b7"))
                    {
                        Debug.Log("✓ Mat hamlesi! 3. Vc6# veya Vb7#");
                        return true;
                    }
                    break;

                case "Ka4-b3":
                    if ((fromPosition == "c4" && (toPosition == "b2" || toPosition == "b6")))
                    {
                        Debug.Log("✓ Mat hamlesi! 3. Ab2# veya Ab6#");
                        return true;
                    }
                    else if (fromPosition == "g4" && toPosition == "e2")
                    {
                        Debug.Log("İkinci hamle Ve2 oynanmıştı zaten.");
                        return false;
                    }
                    break;

                default:
                    Debug.Log("⚠ Siyahın hamlesine uygun mat hamlesi tanımlanmadı.");
                    return true;
            }

            Debug.Log("✗ Yanlış mat hamlesi!");
            return false;
        }

        return true;
    }

    public void OnMoveExecuted(string fromPosition, string toPosition, PieceColor pieceColor)
    {
        if (puzzleCompleted) return;

        currentMoveIndex++;

        Debug.Log($"Hamle kaydedildi: {fromPosition} -> {toPosition} ({pieceColor})");
        Debug.Log($"Toplam hamle sayısı: {currentMoveIndex}");

        // Otomatik siyah hamle şimdilik kapalı
        /*
        if (isWhiteTurn)
        {
            isWhiteTurn = false;
            StartCoroutine(PlayBlackMoveAfterDelay(1.0f));
        }
        else
        {
            isWhiteTurn = true;
            waitingForPlayerMove = true;
        }
        */

        CheckForMate();
    }

    IEnumerator PlayBlackMoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayBlackMove();
    }

    void PlayBlackMove()
    {
        Debug.Log(">>> PlayBlackMove çağrıldı <<<");

        string[] possibleBlackMoves = GetPossibleBlackResponses();

        Debug.Log($"Olası siyah hamleler: {possibleBlackMoves.Length} adet");

        if (possibleBlackMoves.Length > 0)
        {
            string selectedMove = possibleBlackMoves[Random.Range(0, possibleBlackMoves.Length)];
            Debug.Log($"Seçilen siyah hamle: {selectedMove}");

            string[] moveParts = selectedMove.Split('-');

            if (moveParts.Length == 2)
            {
                Debug.Log($"Hamle ayrıştırıldı: {moveParts[0]} -> {moveParts[1]}");
                ExecuteAutoMove(moveParts[0], moveParts[1], PieceColor.Black);
            }
            else
            {
                Debug.LogError($"Hamle formatı hatalı: {selectedMove}");
            }
        }
        else
        {
            Debug.LogWarning("Siyah için hamle bulunamadı!");
        }
    }

    string[] GetPossibleBlackResponses()
    {
        Debug.Log($"GetPossibleBlackResponses çağrıldı. currentMoveIndex: {currentMoveIndex}");

        switch (currentMoveIndex)
        {
            case 1:
                Debug.Log("Siyah için 1. hamle sonrası cevaplar hazırlanıyor...");
                return new string[] {
                    "d6-f4",
                    "a4-b5",
                    "a4-b3"
                };

            case 3:
                Debug.Log("Siyah için 2. hamle sonrası cevaplar hazırlanıyor...");
                return new string[] {
                    "b5-c5",
                    "b5-a6",
                    "b3-a2",
                    "f4-e3"
                };

            default:
                Debug.Log("Bu hamle için siyah cevabı tanımlanmamış");
                return new string[0];
        }
    }

    void ExecuteMove(string fromPosition, string toPosition)
    {
        // Hamleyi board manager'a bildir
    }

    void ExecuteAutoMove(string fromPosition, string toPosition, PieceColor color)
    {
        Debug.Log($">>> ExecuteAutoMove: {fromPosition} -> {toPosition} ({color}) <<<");

        if (boardManager != null)
        {
            ChessPieceController piece = boardManager.GetPieceAt(fromPosition);
            if (piece != null && piece.pieceColor == color)
            {
                Debug.Log($"✓ Taş bulundu: {piece.pieceType} ({piece.pieceColor}) at {fromPosition}");
                piece.AutoMoveTo(toPosition);
                lastBlackMove = $"{fromPosition}-{toPosition}";
            }
            else
            {
                if (piece == null)
                    Debug.LogError($"✗ {fromPosition} pozisyonunda taş bulunamadı!");
                else
                    Debug.LogError($"✗ Taş rengi uyuşmuyor! Beklenen: {color}, Bulunan: {piece.pieceColor}");
            }
        }
        else
        {
            Debug.LogError("✗ BoardManager bulunamadı!");
        }
    }

    void CheckForMate()
    {
        if (currentMoveIndex >= 6)
        {
            Debug.Log("Puzzle tamamlandı! Mat!");
            puzzleCompleted = true;
        }
    }

    public void OnWrongMove(ChessPieceController piece)
    {
        Debug.Log("Yanlış hamle! Taş geri döndürülüyor.");
        piece.ReturnToOriginalPosition();
    }

    [ContextMenu("Reset Puzzle")]
    public void ResetPuzzle()
    {
        currentMoveIndex = 0;
        isWhiteTurn = true;
        puzzleCompleted = false;
        waitingForPlayerMove = true;
        Debug.Log("Puzzle sıfırlandı!");
    }

    [ContextMenu("Show Hint")]
    public void ShowHint()
    {
        if (currentMoveIndex == 0)
        {
            Debug.Log("İpucu: Se4 oyna!");
        }
        else
        {
            Debug.Log("İpucu: Puzzle hatlarını takip et!");
        }
    }
}
