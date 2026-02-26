using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CropMoistureManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown cropDropdown;
    [SerializeField] private TextMeshProUGUI minMoistureText;
    [SerializeField] private TextMeshProUGUI maxMoistureText;

    // Hardcoded crop data
    private Dictionary<string, (int minMoisture, int maxMoisture)> cropData = new Dictionary<string, (int, int)>
    {
        { "Coffee", (45, 85) },
        { "Cotton", (30, 65) },
        { "Groundnut", (25, 55) },
        { "Jute", (50, 80) },
        { "Maize", (40, 70) },
        { "Millets", (35, 55) },
        { "Mustard", (30, 60) },
        { "Rice", (30, 60) },
        { "Soybean", (40, 65) },
        { "Spices", (35, 65) },
        { "Sugarcane", (45, 75) },
        { "Tea", (40, 80) },
        { "Tobacco", (40, 70) },
        { "Wheat", (25, 50) }
    };

    private void Start()
    {
        // Populate dropdown with crop names
        cropDropdown.ClearOptions();
        cropDropdown.AddOptions(new List<string>(cropData.Keys));

        // Add listener for dropdown value changes
        cropDropdown.onValueChanged.AddListener(OnCropSelected);

        // Initialize the display with the first crop's data
        UpdateMoistureValues(cropDropdown.options[cropDropdown.value].text);
    }

    private void OnCropSelected(int index)
    {
        string selectedCrop = cropDropdown.options[index].text;
        UpdateMoistureValues(selectedCrop);
    }

    private void UpdateMoistureValues(string crop)
    {
        if (cropData.TryGetValue(crop, out var moistureValues))
        {
            minMoistureText.text = $"Min: {moistureValues.minMoisture}%";
            maxMoistureText.text = $"Max: {moistureValues.maxMoisture}%";
        }
        else
        {
            Debug.LogError($"Crop '{crop}' not found in the data!");
        }
    }
}
