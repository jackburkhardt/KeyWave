using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Kawase Blur")]
public class KawaseBlurVolumeSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard deviation (spread) of the blur. Grid size is approx. 3x larger.")]
    public ClampedIntParameter strength = new ClampedIntParameter(2, 2, 15);
    public ClampedIntParameter downsample = new ClampedIntParameter(1, 1, 4);

    public bool IsActive()
    {
        return (strength.value > 0.0f) && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}