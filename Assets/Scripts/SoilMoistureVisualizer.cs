using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class TwilioSMSManager : MonoBehaviour
{
    private const string ACCOUNT_SID = "ACb5e96f853866ddb20155b3533a4ba567";
    private const string AUTH_TOKEN = "41e4278db2a02d0ea466bb1ed227d81b";
    private const string FROM_NUMBER = "+12293039475";
    private const string TO_NUMBER = "+919819225333";
    private const string TWILIO_API_URL = "https://api.twilio.com/2010-04-01/Accounts/{0}/Messages.json";

    private bool lastMotorStatus = false;

    [System.Serializable]
    public class SensorData
    {
        public string flame_detected;
        public string motor_status;
        public int soil_sensor_1;
        public int soil_sensor_2;
        public float temperature;
        public float humidity;
        public string timestamp;
    }

    public void SendMotorStatusSMS(bool isMotorOn)
    {
        if (isMotorOn != lastMotorStatus)
        {
            lastMotorStatus = isMotorOn;
            string message = $"Irrigation System Update: Motor is now {(isMotorOn ? "ON" : "OFF")} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            StartCoroutine(SendSMS(message));
        }
    }

    private IEnumerator SendSMS(string message)
    {
        string url = string.Format(TWILIO_API_URL, ACCOUNT_SID);

        WWWForm form = new WWWForm();
        form.AddField("To", TO_NUMBER);
        form.AddField("From", FROM_NUMBER);
        form.AddField("Body", message);

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ACCOUNT_SID}:{AUTH_TOKEN}"));
        request.SetRequestHeader("Authorization", $"Basic {auth}");

        Debug.Log($"Sending SMS: {message}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("SMS sent successfully!");
        }
        else
        {
            Debug.LogError($"Failed to send SMS: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }

        request.Dispose();
    }
}

public class SoilMoistureVisualizer : MonoBehaviour
{
    [Header("Firebase Configuration")]
    [SerializeField] private string databaseUrl = "https://finalparul-default-rtdb.asia-southeast1.firebasedatabase.app/";

    private DatabaseReference sensorDataRef;
    private DatabaseReference motorControlRef;
    private FirebaseApp app;
    private string lastKey = "";
    private bool isMotorOn = false;
    private bool isAutoMode = false;
    private TwilioSMSManager smsManager;

    [Header("UI References - Sensor Data")]
    [SerializeField] private TextMeshProUGUI soilSensor1Text;
    [SerializeField] private TextMeshProUGUI soilSensor2Text;
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI humidityText;
    [SerializeField] private TextMeshProUGUI flameDetectedText;
    [SerializeField] private TextMeshProUGUI motorStatusText;
    [SerializeField] private TextMeshProUGUI lastUpdateText;

    [Header("UI References - Control Buttons")]
    [SerializeField] private Button motorToggleButton;
    [SerializeField] private TextMeshProUGUI motorButtonText;
    [SerializeField] private Button automateButton;
    [SerializeField] private TextMeshProUGUI automateButtonText;
    [SerializeField] private TextMeshProUGUI controlModeText;

    [Header("Visualization References")]
    [SerializeField] private GameObject soilCube1;
    [SerializeField] private GameObject soilCube2;
    [SerializeField] private ParticleSystem[] Sprinklers;

    [Header("Motor Control Settings")]
    [SerializeField] private float moistureLowerThreshold = 500f;
    [SerializeField] private float moistureUpperThreshold = 800f;

    private void Start()
    {
        Debug.Log("SoilMoistureVisualizer Starting...");
        smsManager = gameObject.AddComponent<TwilioSMSManager>();
        InitializeFirebase();
        InitializeUI();
        InitializeSprinklers();
    }

    private void InitializeUI()
    {
        Debug.Log("Initializing UI components...");

        if (motorToggleButton != null)
        {
            motorToggleButton.onClick.AddListener(ToggleMotor);
            Debug.Log("Motor toggle button listener added");
        }
        else
        {
            Debug.LogError("Motor toggle button reference is missing!");
        }

        if (automateButton != null)
        {
            automateButton.onClick.AddListener(ToggleControlMode);
            Debug.Log("Automate button listener added");
        }
        else
        {
            Debug.LogError("Automate button reference is missing!");
        }

        UpdateButtonText();
        UpdateControlModeText();
    }

    private void InitializeSprinklers()
    {
        Debug.Log($"Initializing {(Sprinklers != null ? Sprinklers.Length : 0)} sprinklers...");

        if (Sprinklers == null || Sprinklers.Length == 0)
        {
            Debug.LogError("No sprinklers assigned in the inspector!");
            return;
        }

        foreach (var sprinkler in Sprinklers)
        {
            if (sprinkler == null)
            {
                Debug.LogError("Null sprinkler found in array!");
                continue;
            }

            sprinkler.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            sprinkler.Clear();
            Debug.Log($"Initialized sprinkler: {sprinkler.gameObject.name}");
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("Initializing Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            app = FirebaseApp.Create(new AppOptions
            {
                DatabaseUrl = new System.Uri(databaseUrl)
            });

            Debug.Log("Firebase initialized successfully");

            sensorDataRef = FirebaseDatabase.DefaultInstance.GetReference("sensor_data");
            motorControlRef = FirebaseDatabase.DefaultInstance.GetReference("motor_control");

            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(true);

            ListenForLatestData();
            ListenForMotorStatus();
        });
    }

    private void ListenForLatestData()
    {
        Debug.Log("Setting up sensor data listener...");
        sensorDataRef.LimitToLast(1).ValueChanged += HandleSensorDataChange;
    }

    private void ListenForMotorStatus()
    {
        Debug.Log("Setting up motor status listener...");
        motorControlRef.ValueChanged += HandleMotorStatusChange;
    }

    private void HandleSensorDataChange(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Database error: {args.DatabaseError.Message}");
            return;
        }

        if (!args.Snapshot.Exists)
        {
            Debug.Log("No sensor data available");
            return;
        }

        foreach (var child in args.Snapshot.Children)
        {
            var jsonData = JsonConvert.SerializeObject(child.Value);
            Debug.Log($"Raw JSON Data: {jsonData}");

            try
            {
                var sensorData = JsonConvert.DeserializeObject<TwilioSMSManager.SensorData>(jsonData);
                UpdateUI(sensorData);
                UpdateCubeVisualizations(sensorData);

                if (isAutoMode)
                {
                    CheckAndUpdateMotorStatus(sensorData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deserializing sensor data: {ex.Message}\nJSON: {jsonData}");
            }
        }
    }

    private void HandleMotorStatusChange(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Motor status error: {args.DatabaseError.Message}");
            return;
        }

        Debug.Log("Received motor status update");

        if (args.Snapshot.Exists && args.Snapshot.Child("status").Exists)
        {
            string status = args.Snapshot.Child("status").Value.ToString();
            isMotorOn = status == "On";
            Debug.Log($"Motor status changed to: {status}");

            if (smsManager != null)
            {
                smsManager.SendMotorStatusSMS(isMotorOn);
            }

            UpdateButtonText();
            UpdateSprinklerEffect();
        }
    }

    private void UpdateSprinklerEffect()
    {
        Debug.Log($"UpdateSprinklerEffect called. Motor status: {isMotorOn}");

        if (Sprinklers == null)
        {
            Debug.LogError("Sprinklers array is null!");
            return;
        }

        foreach (var sprinkler in Sprinklers)
        {
            if (sprinkler == null)
            {
                Debug.LogError("Found null sprinkler in array!");
                continue;
            }

            if (isMotorOn && !sprinkler.isPlaying)
            {
                sprinkler.Clear();
                sprinkler.Play();
            }
            else if (!isMotorOn && sprinkler.isPlaying)
            {
                sprinkler.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private void ToggleControlMode()
    {
        isAutoMode = !isAutoMode;
        Debug.Log($"Control mode changed to: {(isAutoMode ? "Automatic" : "Manual")}");

        UpdateControlModeText();
        UpdateAutomateButtonText();

        if (isAutoMode)
        {
            CheckLastSensorData();
        }
    }

    private void CheckLastSensorData()
    {
        Debug.Log("Checking last sensor data...");

        sensorDataRef.LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to retrieve last sensor data");
                return;
            }

            var snapshot = task.Result;
            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    var jsonData = JsonConvert.SerializeObject(child.Value);
                    var sensorData = JsonConvert.DeserializeObject<TwilioSMSManager.SensorData>(jsonData);
                    if (isAutoMode)
                    {
                        CheckAndUpdateMotorStatus(sensorData);
                    }
                    break;
                }
            }
        });
    }

    private void UpdateControlModeText()
    {
        if (controlModeText != null)
        {
            controlModeText.text = $"Control Mode: {(isAutoMode ? "Automatic" : "Manual")}";
            controlModeText.color = isAutoMode ? Color.green : Color.white;
        }
    }

    private void UpdateAutomateButtonText()
    {
        if (automateButtonText != null)
        {
            automateButtonText.text = isAutoMode ? "Switch to Manual" : "Switch to Automatic";
        }
    }

    private void UpdateUI(TwilioSMSManager.SensorData data)
    {
        if (temperatureText != null)
            temperatureText.text = $"Temperature: {data.temperature}°C";

        if (humidityText != null)
            humidityText.text = $"Humidity: {data.humidity}%";

        if (soilSensor1Text != null)
            soilSensor1Text.text = $"Soil Sensor 1: {data.soil_sensor_1}";

        if (soilSensor2Text != null)
            soilSensor2Text.text = $"Soil Sensor 2: {data.soil_sensor_2}";

        if (flameDetectedText != null)
        {
            flameDetectedText.text = $"Flame Detected: {data.flame_detected}";
            flameDetectedText.color = data.flame_detected == "Yes" ? Color.red : Color.green;
        }

        if (motorStatusText != null)
        {
            motorStatusText.text = $"Motor Status: {data.motor_status}";
            motorStatusText.color = data.motor_status == "On" ? Color.green : Color.white;
        }

        if (lastUpdateText != null)
            lastUpdateText.text = $"Last Updated: {data.timestamp}";
    }

    private void UpdateCubeVisualizations(TwilioSMSManager.SensorData data)
    {
        if (soilCube1 != null)
        {
            soilCube1.GetComponent<Renderer>().material.color = GetColorBasedOnMoisture(data.soil_sensor_1);
        }

        if (soilCube2 != null)
        {
            soilCube2.GetComponent<Renderer>().material.color = GetColorBasedOnMoisture(data.soil_sensor_2);
        }
    }

    private Color GetColorBasedOnMoisture(int moisture)
    {
        if (moisture >= 800 && moisture <= 1024)
        {
            return new Color(1, 0, 0, 0.5f); // Red for very dry soil
        }
        else if (moisture >= 0 && moisture <= 500)
        {
            return new Color(0, 1, 0, 0.5f); // Green for optimal moisture level
        }
        else if (moisture > 500 && moisture < 800)
        {
            return new Color(1f, 0.647f, 0f, 0.5f); // Orange for moderate moisture
        }
        else
        {
            return Color.yellow; // Default for any other range
        }
    }

    private void CheckAndUpdateMotorStatus(TwilioSMSManager.SensorData data)
    {
        int moisture1 = data.soil_sensor_1;
        int moisture2 = data.soil_sensor_2;

        float averageMoisture = (moisture1 + moisture2) / 2f;
        Debug.Log($"Average moisture: {averageMoisture}");

        bool shouldMotorBeOn = false;

        if (averageMoisture < moistureLowerThreshold && isMotorOn)
        {
            shouldMotorBeOn = false;
            Debug.Log($"Moisture below threshold ({averageMoisture}). Stopping motor.");
        }
        else if (averageMoisture > moistureUpperThreshold && !isMotorOn)
        {
            shouldMotorBeOn = true;
            Debug.Log($"Moisture above threshold ({averageMoisture}). Starting motor.");
        }
        else
        {
            return;
        }

        if (shouldMotorBeOn != isMotorOn)
        {
            UpdateMotorStatus(shouldMotorBeOn);
        }
    }

    private void ToggleMotor()
    {
        if (!isAutoMode)
        {
            isMotorOn = !isMotorOn;
            string newStatus = isMotorOn ? "On" : "Off";
            Debug.Log($"Manually toggling motor to: {newStatus}");

            Dictionary<string, object> motorData = new Dictionary<string, object>
            {
                { "status", newStatus },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            motorControlRef.UpdateChildrenAsync(motorData).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Motor control update failed: {task.Exception}");
                    isMotorOn = !isMotorOn;
                    UpdateButtonText();
                }
                else
                {
                    Debug.Log($"Motor status manually updated to: {newStatus}");
                    UpdateSprinklerEffect();
                }
            });

            UpdateButtonText();
        }
        else
        {
            Debug.Log("Cannot manually toggle motor in automatic mode");
        }
    }

    private void UpdateMotorStatus(bool turnOn)
    {
        Debug.Log($"Updating motor status to: {(turnOn ? "On" : "Off")}");

        Dictionary<string, object> motorData = new Dictionary<string, object>
        {
            { "status", turnOn ? "On" : "Off" },
            { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
       
        };

        motorControlRef.UpdateChildrenAsync(motorData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Motor control update failed: {task.Exception}");
            }
            else
            {
                isMotorOn = turnOn;

                // Send SMS notification
                if (smsManager != null)
                {
                    smsManager.SendMotorStatusSMS(isMotorOn);
                }

                UpdateButtonText();
                UpdateSprinklerEffect();
                Debug.Log($"Motor status automatically updated to: {(turnOn ? "On" : "Off")}");
            }
        });
    }

    private void UpdateButtonText()
    {
        if (motorButtonText != null)
        {
            motorButtonText.text = isMotorOn ? "Turn Motor Off" : "Turn Motor On";
            motorButtonText.color = isAutoMode ? new Color(1, 1, 1, 0.5f) : Color.white;
            Debug.Log($"Updated button text to: {motorButtonText.text}");
        }
        else
        {
            Debug.LogError("Motor button text reference is missing!");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("SoilMoistureVisualizer being destroyed...");

        if (sensorDataRef != null)
        {
            sensorDataRef.ValueChanged -= HandleSensorDataChange;
            Debug.Log("Unsubscribed from sensor data events");
        }

        if (motorControlRef != null)
        {
            motorControlRef.ValueChanged -= HandleMotorStatusChange;
            Debug.Log("Unsubscribed from motor control events");
        }

        if (motorToggleButton != null)
        {
            motorToggleButton.onClick.RemoveListener(ToggleMotor);
            Debug.Log("Removed motor toggle button listener");
        }

        if (automateButton != null)
        {
            automateButton.onClick.RemoveListener(ToggleControlMode);
            Debug.Log("Removed automate button listener");
        }

        if (Sprinklers != null)
        {
            foreach (var sprinkler in Sprinklers)
            {
                if (sprinkler != null)
                {
                    sprinkler.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    sprinkler.Clear();
                    Debug.Log($"Stopped and cleared sprinkler: {sprinkler.gameObject.name}");
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate thresholds
        if (moistureLowerThreshold >= moistureUpperThreshold)
        {
            Debug.LogWarning("Lower threshold must be less than upper threshold!");
            moistureLowerThreshold = moistureUpperThreshold - 100f;
        }

        // Validate sprinklers array
        if (Sprinklers != null)
        {
            for (int i = 0; i < Sprinklers.Length; i++)
            {
                if (Sprinklers[i] == null)
                {
                    Debug.LogWarning($"Sprinkler at index {i} is null!");
                }
                else if (Sprinklers[i].GetComponent<ParticleSystem>() == null)
                {
                    Debug.LogWarning($"GameObject at index {i} does not have a ParticleSystem component!");
                }
            }
        }
    }
#endif
}
