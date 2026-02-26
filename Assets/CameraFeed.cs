using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CameraFeed : MonoBehaviour
{
    public Button showButton;  // The button that shows the camera feed
    public Button captureButton;  // The button to capture and save the photo
    public RawImage rawImage;  // The RawImage UI element to display the feed
    private WebCamTexture webCamTexture;  // Holds the camera feed
    private string savePath;  // Path to save captured photos

    void Start()
    {
        // Initially hide the RawImage
        rawImage.enabled = false;

        // Add a listener to the show button
        showButton.onClick.AddListener(ToggleCameraFeed);

        // Add a listener to the capture button
        captureButton.onClick.AddListener(CaptureAndSavePhoto);

        // Set up the save path
        savePath = Application.persistentDataPath + "/Photos";

        // Ensure the directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log("Photo save directory created: " + savePath);
        }
    }

    void ToggleCameraFeed()
    {
        // If the camera feed is already playing, stop it
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            rawImage.enabled = false;  // Hide the RawImage
        }
        else
        {
            // Initialize the camera feed
            if (webCamTexture == null)
            {
                webCamTexture = new WebCamTexture();
                rawImage.texture = webCamTexture;
            }

            // Start the camera feed
            webCamTexture.Play();
            rawImage.enabled = true;  // Show the RawImage with the feed
        }
    }

    void CaptureAndSavePhoto()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            // Create a new Texture2D with the same size as the camera feed
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);

            // Get the camera feed pixels and apply them to the Texture2D
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Generate a file name based on the current timestamp
            string fileName = "Photo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string filePath = Path.Combine(savePath, fileName);

            // Encode the texture to PNG and save it
            byte[] photoBytes = photo.EncodeToPNG();
            File.WriteAllBytes(filePath, photoBytes);
            Debug.Log("Photo saved at: " + filePath);

            // Free memory used by the Texture2D
            Destroy(photo);
        }
        else
        {
            Debug.LogWarning("Camera feed is not active. Unable to capture photo.");
        }
    }
}
