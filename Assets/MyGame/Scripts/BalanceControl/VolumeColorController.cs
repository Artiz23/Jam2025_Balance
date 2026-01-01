using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeColorController : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    private float minSaturation = -100f;
    private float maxSaturation = 100f;
    private float minExposure = -3f;
    private float maxExposure = 3f;

    private float balanceValue = 0f;

    void Start()
    {
        
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        
        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            
            UpdateColor(0f);
        }
        else
        {
            Debug.LogWarning("ColorAdjustments не найден в профиле Volume!");
        }
    }

    
    public void UpdateColor(float value)
    {
        if (colorAdjustments == null) return;

        
        balanceValue = Mathf.Clamp(value, -1f, 1f);

        
        float saturation = Mathf.Lerp(minSaturation, maxSaturation, (balanceValue + 1f) / 2f); // От -100 до 100
        float exposure = Mathf.Lerp(minExposure, maxExposure, (balanceValue + 1f) / 2f);      // От -3 до 3

        
        colorAdjustments.saturation.value = saturation;
        colorAdjustments.postExposure.value = exposure;
    }
}