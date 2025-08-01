using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InspectManager : MonoBehaviour
{
    public Volume postProcessingVolume;
    private DepthOfField dof;

    void Start()
    {
        postProcessingVolume.profile.TryGet(out dof);
    }

    public void EnableBlur()
    {
        dof.active = true;
        dof.focusDistance.value = 0.5f;  // yakın odak, arka plan blur
    }

    public void DisableBlur()
    {
        dof.active = false;
    }
}
