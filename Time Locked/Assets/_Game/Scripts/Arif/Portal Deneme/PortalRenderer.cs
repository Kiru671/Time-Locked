using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class PortalRenderer : MonoBehaviour
{
    Camera mainCam;
    Portal[] portals;

    void Start()
    {
        mainCam = GetComponent<Camera>();
        // Cache all portals at start
        portals = FindObjectsOfType<Portal>();
    }

    void LateUpdate()
    {
        foreach (var p in portals)
        {
            if (p.linkedPortal == null) continue;
            if (!IsVisible(p)) continue;

            RenderPortal(p, p.linkedPortal);
        }
    }

    bool IsVisible(Portal p)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        return GeometryUtility.TestPlanesAABB(planes, p.GetComponent<Renderer>().bounds);
    }

    void RenderPortal(Portal src, Portal dst)
    {
        Camera mainCam = Camera.main; // or cache this

        // Position: map mainCam
        Vector3 camPosInSrc = src.transform.InverseTransformPoint(mainCam.transform.position);
        src.portalCam.transform.position = dst.transform.TransformPoint(camPosInSrc);

        // Rotation: map mainCam
        Quaternion camRotInSrc = Quaternion.Inverse(src.transform.rotation) * mainCam.transform.rotation;
        src.portalCam.transform.rotation = dst.transform.rotation * camRotInSrc;



        // Copy projection & do oblique clipping
        src.portalCam.fieldOfView   = mainCam.fieldOfView;
        src.portalCam.nearClipPlane = 0.01f;
        src.portalCam.farClipPlane  = mainCam.farClipPlane;

        // Calculate clip plane at src portal’s surface
        Vector4 clipPlane = CameraSpacePlane(
            src.portalCam,
            src.transform.position,
            src.transform.forward,
            1f
        );
        src.portalCam.projectionMatrix = mainCam.CalculateObliqueMatrix(clipPlane);
        
        src.portalCam.cullingMask &= ~(1 << LayerMask.NameToLayer("PortalSurface"));
        
        src.portalCam.Render();
    }

    // Generates the oblique clip plane in camera space
    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * 0.1f;
        Matrix4x4 mat = cam.worldToCameraMatrix;
        Vector3 cPos = mat.MultiplyPoint(offsetPos);
        Vector3 cNorm = mat.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cNorm.x, cNorm.y, cNorm.z, -Vector3.Dot(cPos, cNorm));
    }
}
