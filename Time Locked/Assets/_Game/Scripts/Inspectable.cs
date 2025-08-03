using UnityEngine;

public class Inspectable : MonoBehaviour
{
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Transform originalParent;

    public void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    public void RestoreOriginalTransform()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
} 