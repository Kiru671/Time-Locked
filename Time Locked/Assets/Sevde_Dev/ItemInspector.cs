using UnityEngine;
using StarterAssets;

public class ItemInspector : MonoBehaviour
{
    private FirstPersonController fpsController;
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public Transform inspectHolder; // Kamera önünde obje tutmak için boş bir GameObject
    public float rotateSpeed = 100f;

    private Camera cam;
    private Inspectable currentItem;
    private bool isInspecting = false;

    public GameObject promptUI;
    private bool canInspect = false;

    void Start()
    {
        cam = Camera.main;
        fpsController = Object.FindFirstObjectByType<FirstPersonController>();
    }

    void Update()
    {
        if (!isInspecting)
        {
            ShowPromptIfLookingAtInspectable();

            if (Input.GetKeyDown(KeyCode.E) && canInspect)
            {
                TryInspectItem();
            }

        }

        if (isInspecting)
        {
            RotateItem();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnItem();
            }
        }
    }


    void ShowPromptIfLookingAtInspectable()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            if (hit.collider.GetComponent<Inspectable>())
            {
                promptUI.SetActive(true);
                canInspect = true;
                return;
            }
        }

        promptUI.SetActive(false);
        canInspect = false;
    }

    void TryInspectItem()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            Inspectable inspectable = hit.collider.GetComponent<Inspectable>();
            if (inspectable != null)
            {
                currentItem = inspectable;
                currentItem.SaveOriginalTransform();

                currentItem.transform.SetParent(inspectHolder);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;

                isInspecting = true;

                fpsController.canLook = false; // kamera dönmesini durdur
                promptUI.SetActive(false);
            }
        }
    }

    void RotateItem()
    {
        float rotX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        float rotY = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

        currentItem.transform.Rotate(Vector3.up, rotX, Space.World);
        currentItem.transform.Rotate(Vector3.right, rotY, Space.World);
    }

    void ReturnItem()
    {
        currentItem.transform.SetParent(null);
        currentItem.transform.position = currentItem.originalPosition;
        currentItem.transform.rotation = currentItem.originalRotation;

        currentItem = null;
        isInspecting = false;

        fpsController.canLook = true; // kamerayı tekrar aktif et
    }
}
