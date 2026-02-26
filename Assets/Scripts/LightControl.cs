using UnityEngine;
using UnityEngine.UI; // To work with the Slider component

public class LightControl : MonoBehaviour
{
    // Reference to the Directional Light
    public Light directionalLight;

    // Reference to the Slider for controlling the light color
    public Slider lightColorSlider;

    void Start()
    {
        // Set the initial value of the slider (optional)
        lightColorSlider.value = 0.5f; // Example initial value (middle)

        // Add listener to the slider to call UpdateLightColor when the slider value changes
        lightColorSlider.onValueChanged.AddListener(UpdateLightColor);
    }

    // Function to update the light color based on the slider value
    public void UpdateLightColor(float value)
    {
        // Interpolate between white and black based on the slider value
        // White color for value = 0, Black color for value = 1
        Color lightColor = Color.Lerp(Color.white, Color.black, value);

        // Set the directional light's color
        if (directionalLight != null)
        {
            directionalLight.color = lightColor;
        }
    }
}
