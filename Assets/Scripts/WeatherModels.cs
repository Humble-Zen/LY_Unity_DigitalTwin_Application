namespace WeatherApp.Models
{
    [System.Serializable]
    public class WeatherInfo
    {
        public Weather[] weather;
        public Main main;
        public Wind wind;
        public long visibility;
        public Sys sys;
        public string name;
    }

    [System.Serializable]
    public class Weather
    {
        public int id;
        public string main;
        public string description;
        public string icon;
    }

    [System.Serializable]
    public class Main
    {
        public float temp;
        public float feels_like;
        public float temp_min;
        public float temp_max;
        public int pressure;
        public int humidity;
    }

    [System.Serializable]
    public class Wind
    {
        public float speed;
        public float deg;
    }

    [System.Serializable]
    public class Sys
    {
        public long sunrise;
        public long sunset;
    }
}