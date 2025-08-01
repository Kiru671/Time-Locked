using UnityEngine;

public class Inspectable : MonoBehaviour
{
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
    [HideInInspector] public Vector3 originalScale;

    public void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale; // Orijinal scale'i de kaydet
    }
}