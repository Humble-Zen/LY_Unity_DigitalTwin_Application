using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using WeatherApp.Models;

public class WeatherManager : MonoBehaviour
{
    // UI Text References
    public TMP_Text cityText;
    public TMP_Text weatherText;
    public TMP_Text tempText;
   // public TMP_Text feelsLikeText;
    public TMP_Text minTempText;
    public TMP_Text maxTempText;
    public TMP_Text humidityText;
    public TMP_Text pressureText;
    public TMP_Text windSpeedText;
   // public TMP_Text visibilityText;
   // public TMP_Text sunriseText;
   // public TMP_Text sunsetText;

    // New City Input Field
    public TMP_InputField cityInputField;

    // Font Assets
    public TMP_FontAsset defaultFont;        // For English
    public TMP_FontAsset devanagariFont;     // For Hindi, Marathi

    // Rain Effect Reference
    public ParticleSystem rainEffect;

    // API Configuration
    private string apiKey = "fa993d23535506cb36d0ae03f8870763";
    private string city = "Mumbai";
    private string units = "metric";
    private string lang = "en";

    // Dropdown for Language Selection
    public TMP_Dropdown languageDropdown;

    // Reference to all text components that need font switching
    private TMP_Text[] allTextComponents;

    private Dictionary<string, Dictionary<string, string>> localizedStrings = new Dictionary<string, Dictionary<string, string>>()
    {
        {
            "en", new Dictionary<string, string>()
            {
                {"city", "City"},
                {"weather", "Weather"},
                {"temperature", "Temperature"},
               // {"feelsLike", "Feels Like"},
                {"minTemp", "Min Temp"},
                {"maxTemp", "Max Temp"},
                {"humidity", "Humidity"},
                {"pressure", "Pressure"},
                {"windSpeed", "Wind Speed"},
               // {"visibility", "Visibility"},
               // {"sunrise", "Sunrise"},
               // {"sunset", "Sunset"}
            }
        },
        {
            "mr", new Dictionary<string, string>()
            {
                {"city", "शहर"},
                {"weather", "हवामान"},
                {"temperature", "तापमान"},
               // {"feelsLike", "जाणवते"},
                {"minTemp", "किमान तापमान"},
                {"maxTemp", "कमाल तापमान"},
                {"humidity", "आर्द्रता"},
                {"pressure", "दाब"},
                {"windSpeed", "वाऱ्याचा वेग"},
                //{"visibility", "दृश्यमानता"},
                //{"sunrise", "सूर्योदय"},
                //{"sunset", "सूर्यास्त"}
            }
        },
        {
            "hi", new Dictionary<string, string>()
            {
                {"city", "शहर"},
                {"weather", "मौसम"},
                {"temperature", "तापमान"},
                //{"feelsLike", "महसूस होता है"},
                {"minTemp", "न्यूनतम तापमान"},
                {"maxTemp", "अधिकतम तापमान"},
                {"humidity", "नमी"},
                {"pressure", "दबाव"},
                {"windSpeed", "हवा की गति"},
               // {"visibility", "दृश्यता"},
               // {"sunrise", "सूर्योदय"},
               // {"sunset", "सूर्यास्त"}
            }
        }
    };

    private Dictionary<string, Dictionary<string, string>> weatherTranslations = new Dictionary<string, Dictionary<string, string>>()
    {
        {
            "mr", new Dictionary<string, string>()
            {
                {"clear sky", "निरभ्र आकाश"},
                {"few clouds", "किरकोळ ढग"},
                {"scattered clouds", "विखुरलेले ढग"},
                {"broken clouds", "तुटक ढग"},
                {"shower rain", "सरी"},
                {"rain", "पाऊस"},
                {"thunderstorm", "मेघगर्जना"},
                {"snow", "बर्फ"},
                {"mist", "धुके"},
                {"overcast clouds", "ढगाळ"},
                {"light rain", "हलका पाऊस"},
                {"moderate rain", "मध्यम पाऊस"},
                {"heavy rain", "जोरदार पाऊस"},
                {"thunderstorm with rain", "पावसासह मेघगर्जना"},
                {"haze", "धुरके"},
                {"fog", "धुके"},
                {"drizzle", "रिमझिम पाऊस"},
                {"smoke", "धूर"},
                {"dust", "धूळ"},
                {"sand", "वाळू"},
                {"partly cloudy", "अंशतः ढगाळ"}
            }
        }
    };

    private Dictionary<string, Dictionary<string, string>> cityTranslations = new Dictionary<string, Dictionary<string, string>>()
    {
        {
            "mr", new Dictionary<string, string>()
            {
                            {"Mumbai", "मुंबई"},
            {"Pune", "पुणे"},
            {"Delhi", "दिल्ली"},
            {"Nagpur", "नागपूर"},
            {"Nashik", "नाशिक"},
            {"Aurangabad", "औरंगाबाद"},
            {"Solapur", "सोलापूर"},
            {"Kolhapur", "कोल्हापूर"},
            {"Thane", "ठाणे"},
            {"Navi Mumbai", "नवी मुंबई"},
            {"New York", "न्यू यॉर्क"},
            {"Los Angeles", "लॉस एंजेलिस"},
            {"Chicago", "शिकागो"},
            {"Houston", "ह्यूस्टन"},
            {"Phoenix", "फीनिक्स"},
            {"Philadelphia", "फिलाडेल्फिया"},
            {"San Antonio", "सॅन अँटोनिओ"},
            {"San Diego", "सॅन डिएगो"},
            {"Dallas", "डॅलस"},
            {"San Jose", "सॅन जोस"},
            {"London", "लंडन"},
            {"Berlin", "बर्लिन"},
            {"Paris", "पॅरिस"},
            {"Tokyo", "टोकियो"},
            {"Sydney", "सिडनी"},
            {"Moscow", "मॉस्को"},
            {"Toronto", "टोरांटो"},
            {"Singapore", "सिंगापूर"},
            {"Rome", "रोम"},
            {"Madrid", "मद्रिद"},
            {"São Paulo", "साओ पाऊलो"},
            {"Buenos Aires", "ब्यूनस आयर्स"},
            {"Rio de Janeiro", "रिओ दे जानेरो"},
            {"Cape Town", "केप टाऊन"},
            {"Lagos", "लागोस"},
            {"Cairo", "काहिरा"},
            {"Jakarta", "जकार्ता"},
            {"Manila", "मनीला"},
            {"Seoul", "सियोल"},
            {"Hong Kong", "हाँग काँग"},
            {"Istanbul", "इस्तंबूल"},
            {"Kuala Lumpur", "कुआलालंपुर"},
            {"Bangkok", "बँकॉक"},
            {"Bangalore", "बंगलोर"},
            {"Chennai", "चेन्नई"},
            {"Kolkata", "कोलकाता"},
            {"Hyderabad", "हैदराबाद"},
            {"Ahmedabad", "आधेराबाद"},
            {"Surat", "सूरत"},
            {"Jaipur", "जयपूर"},
            {"Lucknow", "लखनऊ"},
            {"Kanpur", "कानपूर"},
            {"Chandigarh", "चंदीगढ"},
            {"Coimbatore", "कोयंबटूर"},
            {"Indore", "इंदोर"},
            {"Vadodara", "वडोदरा"},
            {"Visakhapatnam", "विशाखापत्तनम"},
            {"Bhopal", "भोपाल"},
            {"Patna", "पटना"},
            {"Agra", "आग्रा"},
            {"Rajkot", "राजकोट"},
            {"Madurai", "मदुरै"},
            {"Caracas", "कॅराकस"},
            {"Lima", "लिमा"},
            {"Shanghai", "शांघाय"},
            {"Beijing", "बीजिंग"},
            {"Chengdu", "चेंगदू"},
            {"Hangzhou", "हांगझोउ"},
            {"Tianjin", "तियानजिन"},
            {"Guangzhou", "ग्वांगझू"},
            {"Shenzhen", "शेन्जेन"},
            {"Taipei", "तैपेई"},
            {"Busan", "बुसान"},
            {"Melbourne", "मेलबर्न"},
            {"Brisbane", "ब्रिसबेन"},
            {"Perth", "पर्थ"},
            {"Auckland", "ऑकलंड"},
            {"Wellington", "वेलिंग्टन"},
            {"Durban", "डर्बन"},
            {"Johannesburg", "जोहान्सबर्ग"},
            {"Nairobi", "नायरोबी"},
            {"Accra", "अक्रा"},
            {"Kigali", "किगाली"},
            {"Kinshasa", "किंशासा"},
            {"Port Moresby", "पोर्ट मोरेस्बी"},
            {"Dar es Salaam", "दार एझ सलाम"},
            {"Lusaka", "लुसाका"},
            {"Abuja", "अबूजा"},
            {"Freetown", "फ्रीटाउन"},
            {"Lome", "लोम"},
            {"Ouagadougou", "वागाडुगू"},
            {"Addis Ababa", "अदीस अबाबा"},
            {"Douala", "डुआला"},
            {"Libreville", "लिब्रेव्हिले"},
            {"Barcelona", "बार्सिलोना"},
            {"Vienna", "व्हिएना"},
            {"Stockholm", "स्टॉकहोम"},
            {"Oslo", "ऑस्लो"},
            {"Helsinki", "हेलसिंकी"},
            {"Zurich", "झुरीच"},
            {"Geneva", "जेनिव्हा"},
            {"Copenhagen", "कोपेनहेगन"},
            {"Amsterdam", "ऍम्स्टरडम"},
            {"Brussels", "ब्रसेल्स"},
            {"Hamburg", "हॅम्बर्ग"},
            {"Munich", "म्युनिक"},
            {"Frankfurt", "फ्रँकफर्ट"},
            {"Warsaw", "वारसॉ"},
            {"Athens", "एथिन्स"},
            {"Belgrade", "बेलग्रेड"},
            {"Vilnius", "विलनियस"},
            {"Tallinn", "टालिन"},
            {"Riga", "रीगा"},
            {"Bucharest", "बुखारेस्ट"},
            {"Prague", "प्राग"},
            {"Minsk", "मिन्स्क"},
            {"Sarajevo", "सरेजेवो"},
            {"Sofia", "सोफिया"},
            {"Skopje", "स्कोप्जे"},
            {"Tbilisi", "ट्बिलिसी"},
            {"Chisinau", "चिसिनाऊ"},
            {"Astana", "अस्ताना"},
            {"Almaty", "अल्माटी"},
            {"Baku", "बाकू"},
            {"Yerevan", "येरेवन"},
            {"Kiev", "कीव"},
            {"Bishkek", "बिश्केक"},
            {"Ashgabat", "अश्गाबाद"},
            {"Dushanbe", "दुशांबे"},
            {"Tashkent", "ताश्कंद"},
            {"Abu Dhabi", "अबू धाबी"},
            {"Dubai", "दुबई"}
            }
        }
    };

    void Awake()
    {
        allTextComponents = new TMP_Text[] {
            cityText, weatherText, tempText, /*feelsLikeText,*/
            minTempText, maxTempText, humidityText, pressureText,
            windSpeedText, /*visibilityText, sunriseText, sunsetText*/
        };
    }

    void Start()
    {
        SwitchFont(defaultFont);
        languageDropdown.onValueChanged.AddListener(delegate { OnLanguageChanged(); });

        // Set up the input field
        cityInputField.text = city;
        cityInputField.onEndEdit.AddListener(OnCityInputSubmitted);

        StartCoroutine(GetWeatherData());
    }

    void SwitchFont(TMP_FontAsset font)
    {
        foreach (var textComponent in allTextComponents)
        {
            if (textComponent != null)
            {
                textComponent.font = font;
            }
        }
    }

    void OnLanguageChanged()
    {
        switch (languageDropdown.value)
        {
            case 0: // English
                lang = "en";
                SwitchFont(defaultFont);
                break;
            case 1: // Hindi
                lang = "hi";
                SwitchFont(devanagariFont);
                break;
            case 2: // Marathi
                lang = "mr";
                SwitchFont(devanagariFont);
                break;
        }
        StartCoroutine(GetWeatherData());
    }

    void OnCityInputSubmitted(string newCity)
    {
        if (!string.IsNullOrWhiteSpace(newCity))
        {
            city = newCity.Trim();
            StartCoroutine(GetWeatherData());
        }
    }

    string GetLocalizedString(string key)
    {
        if (!localizedStrings.ContainsKey(lang))
            lang = "en";

        if (localizedStrings[lang].ContainsKey(key))
            return localizedStrings[lang][key];

        return localizedStrings["en"][key];
    }

    string TranslateWeatherDescription(string description)
    {
        if (lang == "mr" && weatherTranslations.ContainsKey("mr"))
        {
            string lowercaseDesc = description.ToLower();
            if (weatherTranslations["mr"].ContainsKey(lowercaseDesc))
            {
                return weatherTranslations["mr"][lowercaseDesc];
            }
        }
        return description;
    }

    string TranslateCityName(string cityName)
    {
        if (lang == "mr" && cityTranslations.ContainsKey("mr"))
        {
            if (cityTranslations["mr"].ContainsKey(cityName))
            {
                return cityTranslations["mr"][cityName];
            }
        }
        return cityName;
    }

    string ConvertNumberToLocalized(string numberString)
    {
        if (lang == "hi" || lang == "mr")
        {
            string[] englishDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] devanagariDigits = { "०", "१", "२", "३", "४", "५", "६", "७", "८", "९" };

            string result = "";
            foreach (char c in numberString)
            {
                int index = System.Array.IndexOf(englishDigits, c.ToString());
                if (index != -1)
                {
                    result += devanagariDigits[index];
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }
        return numberString;
    }

    IEnumerator GetWeatherData()
    {
        string apiLang = (lang == "mr") ? "en" : lang;
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units={units}&lang={apiLang}&appid={apiKey}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                weatherText.text = $"Failed to load weather data for {city}.";
                // Reset the input field to the last successful city
                cityInputField.text = city;  // Keep the original city name
            }
            else
            {
                try
                {
                    WeatherInfo weatherInfo = JsonUtility.FromJson<WeatherInfo>(webRequest.downloadHandler.text);

                    if (weatherInfo != null && weatherInfo.weather != null && weatherInfo.weather.Length > 0)
                    {
                        // Update the input field with the correct city name from the API
                        cityInputField.text = city; // Do not translate here, keep the original city name

                        string translatedCity = TranslateCityName(weatherInfo.name);
                        string translatedWeather = TranslateWeatherDescription(weatherInfo.weather[0].description);

                        cityText.text = $"{GetLocalizedString("city")}: {translatedCity}";  // Translate only the city name in UI
                        weatherText.text = $"{GetLocalizedString("weather")}: {translatedWeather}";
                        tempText.text = $"{GetLocalizedString("temperature")}: {ConvertNumberToLocalized(weatherInfo.main.temp.ToString("F1"))}°C";
                       // feelsLikeText.text = $"{GetLocalizedString("feelsLike")}: {ConvertNumberToLocalized(weatherInfo.main.feels_like.ToString("F1"))}°C";
                        minTempText.text = $"{GetLocalizedString("minTemp")}: {ConvertNumberToLocalized(weatherInfo.main.temp_min.ToString("F1"))}°C";
                        maxTempText.text = $"{GetLocalizedString("maxTemp")}: {ConvertNumberToLocalized(weatherInfo.main.temp_max.ToString("F1"))}°C";
                        humidityText.text = $"{GetLocalizedString("humidity")}: {ConvertNumberToLocalized(weatherInfo.main.humidity.ToString())}%";
                        pressureText.text = $"{GetLocalizedString("pressure")}: {ConvertNumberToLocalized(weatherInfo.main.pressure.ToString())} hPa";
                        windSpeedText.text = $"{GetLocalizedString("windSpeed")}: {ConvertNumberToLocalized(weatherInfo.wind.speed.ToString("F1"))} m/s";
                      //  visibilityText.text = $"{GetLocalizedString("visibility")}: {ConvertNumberToLocalized((weatherInfo.visibility / 1000.0).ToString("F1"))} km";

                        // Format time for India timezone (UTC+5:30)
                        System.DateTime sunriseTime = System.DateTimeOffset.FromUnixTimeSeconds(weatherInfo.sys.sunrise)
                            .ToOffset(System.TimeSpan.FromHours(5.5)).DateTime;
                        System.DateTime sunsetTime = System.DateTimeOffset.FromUnixTimeSeconds(weatherInfo.sys.sunset)
                            .ToOffset(System.TimeSpan.FromHours(5.5)).DateTime;

                       // sunriseText.text = $"{GetLocalizedString("sunrise")}: {ConvertNumberToLocalized(sunriseTime.ToString("HH:mm"))}";
                       // sunsetText.text = $"{GetLocalizedString("sunset")}: {ConvertNumberToLocalized(sunsetTime.ToString("HH:mm"))}";

                        if (weatherInfo.weather[0].main.ToLower().Contains("rain"))
                        {
                            rainEffect.Play();
                        }
                        else
                        {
                            rainEffect.Stop();
                        }
                    }
                    else
                    {
                        Debug.LogError("Weather data is null or empty");
                        weatherText.text = "Weather data unavailable.";
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                    weatherText.text = "Error parsing weather data.";
                }
            }
        }
    }

}