// using UnityEngine;

// public class ChessPiece : MonoBehaviour
// {
//     private Vector3 originalPosition;
//     private bool isSelected = false;
//     private Camera currentCamera;

//     void Start()
//     {
//         originalPosition = transform.position;

//         // Aktif kamera hangisiyse onu al
//         currentCamera = Camera.main;
//     }

//     void OnMouseDown()
// {
//     isSelected = true;
//     Debug.Log("Taş seçildi: " + gameObject.name);
// }

//         void Update()
//     {
//         if (isSelected && Input.GetMouseButtonDown(0))
//         {
//             Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 Debug.Log("Tıklanan yer: " + hit.collider.name);
//                 if (hit.collider.CompareTag("Tile"))
//                 {
//                     transform.position = hit.collider.transform.position;
//                     originalPosition = transform.position;
//                 }
//                 else
//                 {
//                     transform.position = originalPosition;
//                 }

//                     isSelected = false;
//             }
//         }
//     }
// }
