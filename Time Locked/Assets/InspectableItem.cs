using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isInspecting = false;

    public Transform inspectPoint;  // Kameranın önündeki nokta

    void OnMouseDown()
    {
        if (!isInspecting)
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            transform.SetParent(inspectPoint);   // Kameranın önüne yapıştır
            transform.localPosition = Vector3.zero;   // InspectPoint’in merkezine
            transform.localRotation = Quaternion.identity;

            isInspecting = true;
        }
    }

    void Update()
    {
        if (isInspecting)
        {
            float rotateX = Input.GetAxis("Mouse X") * 5f;
            float rotateY = Input.GetAxis("Mouse Y") * 5f;
            transform.Rotate(Vector3.up, -rotateX, Space.World);
            transform.Rotate(Vector3.right, rotateY, Space.World);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                transform.SetParent(null);
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                isInspecting = false;
            }
        }
    }
}
