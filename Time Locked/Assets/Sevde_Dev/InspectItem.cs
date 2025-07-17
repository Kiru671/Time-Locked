using UnityEngine;

public class InspectSystem : MonoBehaviour
{
    public Transform objectToInspect;   // Drag‑and‑drop ile Inspector’dan atıyorsun
    public float rotationSpeed = 100f;

    private Vector3 prevMousePos;

    void Update()
    {
        // Sol tuşa ilk bastığımız kare
        if (Input.GetMouseButtonDown(0))
            prevMousePos = Input.mousePosition;

        // Sol tuş basılıyken her kare
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - prevMousePos;

            // Eksenleri ayırarak döndürmek daha pürüzsüz sonuç verir
            float rotX =  delta.y * rotationSpeed * Time.deltaTime;
            float rotY = -delta.x * rotationSpeed * Time.deltaTime;

            // Dünya eksenine göre döndür
            objectToInspect.Rotate(Vector3.right,  rotX, Space.World);
            objectToInspect.Rotate(Vector3.up,     rotY, Space.World);

            prevMousePos = Input.mousePosition;
        }
    }
}
