using UnityEngine;

public class Inspectable : MonoBehaviour
{
    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;

    public void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
}