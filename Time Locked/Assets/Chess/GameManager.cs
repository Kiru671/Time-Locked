using UnityEngine;
using System.Collections.Generic;

public class ChessManager : MonoBehaviour
{
    public Camera chessCamera;
    private GameObject selectedPiece;
    private List<GameObject> highlightedTiles = new List<GameObject>();

    [Header("Materials")]
    public Material normalMaterial;
    public Material highlightMaterial;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = chessCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                if (clickedObject.CompareTag("Piece"))
                {
                    SelectPiece(clickedObject);
                }
                else if (clickedObject.CompareTag("Tile") && selectedPiece != null)
                {
                    MovePieceToTile(clickedObject);
                }
            }
        }
    }

    void SelectPiece(GameObject piece)
    {
        selectedPiece = piece;
        Debug.Log("Taş seçildi: " + piece.name);
        ShowValidMoves();
    }

    void MovePieceToTile(GameObject tile)
    {
        selectedPiece.transform.position = new Vector3(tile.transform.position.x, selectedPiece.transform.position.y, tile.transform.position.z);
        Debug.Log("Taş " + tile.name + " karesine taşındı.");
        selectedPiece = null;
        ClearHighlights();
    }

    void ShowValidMoves()
    {
        ClearHighlights();

        GameObject[] allTiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in allTiles)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileRenderer.material = highlightMaterial;
                highlightedTiles.Add(tile);
            }
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileRenderer.material = normalMaterial;
            }
        }

        highlightedTiles.Clear();
    }
}
