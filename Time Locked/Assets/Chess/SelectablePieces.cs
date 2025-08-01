// using UnityEngine;

// public class SelectablePiece : MonoBehaviour
// {
//     private Vector3 originalPosition;
//     private bool isSelected = false;
//     private ChessTile[] allTiles;
//     private Camera cam;

//     void Start()
//     {
//         originalPosition = transform.position;
//         allTiles = FindObjectsOfType<ChessTile>();
//         cam = Camera.main;
//     }

//     void OnMouseDown()
//     {
//         if (isSelected)
//         {
//             DeselectPiece();
//         }
//         else
//         {
//             SelectPiece();
//         }
//     }

//     void Update()
//     {
//         if (isSelected && Input.GetMouseButtonDown(0))
//         {
//             Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 if (hit.collider != null && hit.collider.CompareTag("Tile"))
//                 {
//                     Vector3 targetPos = hit.collider.transform.position;
//                     Debug.Log("Tıklanan kare: " + hit.collider.gameObject.name);

//                     // Sadece X ve Z alınır, Y sabit kalır
//                     transform.position = new Vector3(
//                         targetPos.x,
//                         originalPosition.y,
//                         targetPos.z
//                     );

//                     DeselectPiece();
//                 }
//                 else
//                 {
//                     // Tahta dışı → taşı eski yerine döndür
//                     transform.position = originalPosition;
//                     DeselectPiece();
//                 }
//             }
//             else
//             {
//                 transform.position = originalPosition;
//                 DeselectPiece();
//             }
//         }
//     }

//     void SelectPiece()
//     {
//         isSelected = true;

//         // Taşı yukarı kaldır
//         transform.position = originalPosition + new Vector3(0, 0.3f, 0);

//         // Tüm kareleri yak
//         foreach (var tile in allTiles)
//         {
//             tile.Highlight(true);
//         }
//     }

//     void DeselectPiece()
//     {
//         isSelected = false;

//         // Yüksekliği sabitle
//         transform.position = new Vector3(
//             transform.position.x,
//             originalPosition.y,
//             transform.position.z
//         );

//         foreach (var tile in allTiles)
//         {
//             tile.Highlight(false);
//         }
//     }
// }
