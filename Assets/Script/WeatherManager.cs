using Cysharp.Threading.Tasks;
using SimpleJSON;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    static WeatherManager()
    {
        // SSL 인증서 무시 (테스트용)
        ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
    }

    public TMP_Text TPLocation;
    public TMP_Text weatherText;
    public TMP_Text TPTemp;
    public TMP_Text maxText;
    public TMP_Text minText;
    public Image weatherImage;

    [Header("Skybox Materials")]
    public Material sunriseSkybox;
    public Material sunsetSkybox;
    public Material snowSkybox;
    public Material dayClearSkybox;
    public Material dayCloudySkybox;
    public Material dayRainSkybox;
    public Material nightClearSkybox;
    public Material nightCloudySkybox;
    public Material nightRainSkybox;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private GameObject rainParticleLight;
    [SerializeField] private GameObject rainParticleHeavy;
    [SerializeField] private GameObject snowParticle;

    private readonly string baseUrl = "https://littledeer.shop/weather/nowweather";
    private readonly string defaultCityApi = "Tokyo";
    private readonly string defaultCityFront = "東京都";

    private Material currentSkybox;
    private bool isTransitioning = false;

    void Start()
    {
        WeatherRefreshLoop().Forget();
    }

    private async UniTaskVoid WeatherRefreshLoop()
    {
        while (true)
        {
            await GetLocationAndWeather();
            await UniTask.Delay(TimeSpan.FromMinutes(1));
        }
    }

    private async UniTask GetLocationAndWeather()
    {
        try
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.LogWarning("위치 서비스가 꺼져 있습니다. 도쿄로 고정합니다.");
                await GetWeather(defaultCityApi, defaultCityFront);
                return;
            }

            Input.location.Start();

            int waitTime = 0;
            while (Input.location.status == LocationServiceStatus.Initializing && waitTime < 10)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                waitTime++;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning("위치 정보를 가져올 수 없습니다. 도쿄로 고정합니다.");
                await GetWeather(defaultCityApi, defaultCityFront);
                return;
            }

            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            if (latitude == 0f || float.IsNaN(latitude)) latitude = 35.7f;
            if (longitude == 0f || float.IsNaN(longitude)) longitude = 139.7f;

            string geoUrl =
                $"https://api.bigdatacloud.net/data/reverse-geocode-client?latitude={latitude}&longitude={longitude}&localityLanguage=ja";

            string geoJson = await GetRequestAsync(geoUrl);

            var geoData = JSON.Parse(geoJson);
            string countryCode = geoData["countryCode"];
            string cityNameApi = geoData["city"];
            string prefecture = geoData["principalSubdivision"];
            string locality = geoData["locality"];

            if (countryCode != "JP")
            {
                cityNameApi = defaultCityApi;
                prefecture = defaultCityFront;
                locality = "";
            }

            string cityNameFront = $"{prefecture} {locality}".Trim();
            await GetWeather(cityNameApi, cityNameFront);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GetLocationAndWeather] 예외 발생: {e.Message}");
            await GetWeather(defaultCityApi, defaultCityFront);
        }
        finally
        {
            Input.location.Stop();
        }
    }

    public string CurrentMainWeather { get; private set; }
    public DateTime CurrentSunrise { get; private set; }
    public DateTime CurrentSunset { get; private set; }

    private async UniTask GetWeather(string cityNameApi, string cityNameFront)
    {
        string url = $"{baseUrl}?city={cityNameApi}";
        Debug.Log("요청 URL: " + url);

        try
        {
            string json = await GetRequestAsync(url);
            var data = JSON.Parse(json);

            string main = data["weather"]["main"];
            string description = data["weather"]["description"];
            int temp = (int)data["main"]["temp"];
            int tempMin = (int)data["main"]["temp_min"];
            int tempMax = (int)data["main"]["temp_max"];
            string rawIcon = data["weather"]["icon"];
            if (string.IsNullOrEmpty(rawIcon)) rawIcon = "01";
            string iconCode = new string(rawIcon.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(iconCode)) iconCode = "01";

            string sunriseStr = data["sys"]["sunrise"];
            string sunsetStr = data["sys"]["sunset"];

            CurrentMainWeather = main;

            string[] formats = {
                "yyyy/M/d H:mm:ss","yyyy/M/d HH:mm:ss",
                "yyyy/MM/dd H:mm:ss","yyyy/MM/dd HH:mm:ss",
                "yyyy-MM-dd H:mm:ss","yyyy-MM-dd HH:mm:ss"
            };

            if (!DateTime.TryParseExact(sunriseStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime sunrise))
                sunrise = DateTime.Today.AddHours(6);
            if (!DateTime.TryParseExact(sunsetStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime sunset))
                sunset = DateTime.Today.AddHours(18);

            CurrentSunrise = sunrise;
            CurrentSunset = sunset;

            TPLocation.text = cityNameFront;
            weatherText.text = description;
            TPTemp.text = $"{temp}°C";
            maxText.text = $"最高 {tempMax}°";
            minText.text = $"最低 {tempMin}°";

            dialogueManager?.ShowWeatherDialogue(main, temp, sunrise, sunset);

            LoadWeatherIconLocal(iconCode);
            await ApplySkyboxByTime(sunrise, sunset, main);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GetWeather] 예외 발생: {e.Message}");
        }
    }

    private void LoadWeatherIconLocal(string iconCode)
    {
        try
        {
            if (string.IsNullOrEmpty(iconCode)) iconCode = "01";
            iconCode = new string(iconCode.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(iconCode)) iconCode = "01";

            string[] possiblePaths = new string[]
            {
                $"Icons/{iconCode}",
                $"icons/{iconCode}",
                $"Resources/Icons/{iconCode}",
                $"Resources/icons/{iconCode}"
            };

            Sprite loadedSprite = null;

            foreach (var path in possiblePaths)
            {
                loadedSprite = Resources.Load<Sprite>(path);
                if (loadedSprite != null)
                {
                    Debug.Log($"[LoadWeatherIconLocal] 아이콘 로드 성공 ✅ : {path}");
                    break;
                }
            }

            if (loadedSprite == null)
            {
                loadedSprite = Resources.Load<Sprite>("Icons/01");
                if (loadedSprite != null)
                    Debug.LogWarning($"[LoadWeatherIconLocal] 기본 아이콘(01)으로 대체됨 ⚠️");
                else
                    Debug.LogError($"[LoadWeatherIconLocal] 기본 아이콘(01) 로드 실패 ❌");
            }

            if (weatherImage != null && loadedSprite != null)
            {
                weatherImage.sprite = loadedSprite;
                weatherImage.preserveAspect = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadWeatherIconLocal] 예외 발생: {e.Message}");
        }
    }

    private async UniTask<string> GetRequestAsync(string url)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception($"요청 실패: {req.error}");
            return req.downloadHandler.text;
        }
    }

    private async UniTask ApplySkyboxByTime(DateTime sunrise, DateTime sunset, string mainWeather)
    {
        // ✅ 안드로이드 호환: UTC + 9 (JST)
        DateTime now = DateTime.UtcNow.AddHours(9);

        if (sunrise.Year < 2000 || sunset.Year < 2000)
        {
            sunrise = DateTime.Today.AddHours(6);
            sunset = DateTime.Today.AddHours(18);
        }

        DateTime sunriseStart = sunrise.AddMinutes(-30);
        DateTime sunriseEnd = sunrise.AddMinutes(30);
        DateTime sunsetStart = sunset.AddMinutes(-30);
        DateTime sunsetEnd = sunset.AddMinutes(30);

        Material targetSkybox = null;

        if (rainParticleLight) rainParticleLight.SetActive(false);
        if (rainParticleHeavy) rainParticleHeavy.SetActive(false);
        if (snowParticle) snowParticle.SetActive(false);

        bool isSunrise = now >= sunriseStart && now < sunriseEnd;
        bool isSunset = now >= sunsetStart && now < sunsetEnd;
        bool isDay = now >= sunriseEnd && now < sunsetStart;
        bool isNight = !isSunrise && !isDay && !isSunset;

        if (isSunrise) targetSkybox = sunriseSkybox;
        else if (isSunset) targetSkybox = sunsetSkybox;
        else if (isDay)
        {
            switch (mainWeather)
            {
                case "Clear": targetSkybox = dayClearSkybox; break;
                case "Clouds": targetSkybox = dayCloudySkybox; break;
                case "Rain":
                case "Drizzle":
                case "Thunderstorm":
                    targetSkybox = dayRainSkybox;
                    if (rainParticleLight) rainParticleLight.SetActive(true);
                    if (rainParticleHeavy && mainWeather != "Drizzle") rainParticleHeavy.SetActive(true);
                    break;
                case "Snow":
                    targetSkybox = snowSkybox;
                    if (snowParticle) snowParticle.SetActive(true);
                    break;
                default:
                    targetSkybox = dayCloudySkybox;
                    break;
            }
        }
        else if (isNight)
        {
            switch (mainWeather)
            {
                case "Clear": targetSkybox = nightClearSkybox; break;
                case "Clouds": targetSkybox = nightCloudySkybox; break;
                case "Rain":
                case "Drizzle":
                case "Thunderstorm":
                    targetSkybox = nightRainSkybox;
                    if (rainParticleLight) rainParticleLight.SetActive(true);
                    if (rainParticleHeavy && mainWeather != "Drizzle") rainParticleHeavy.SetActive(true);
                    break;
                case "Snow":
                    targetSkybox = snowSkybox;
                    if (snowParticle) snowParticle.SetActive(true);
                    break;
                default:
                    targetSkybox = nightCloudySkybox;
                    break;
            }
        }

        Debug.Log($"[Skybox] {now:HH:mm:ss} | {mainWeather} | {(isSunrise ? "SUNRISE" : isSunset ? "SUNSET" : isDay ? "DAY" : "NIGHT")}");

        if (targetSkybox != null && RenderSettings.skybox != targetSkybox)
        {
            RenderSettings.skybox = targetSkybox;
            DynamicGI.UpdateEnvironment();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                Camera.main.backgroundColor = Color.black;
            }
            await SmoothSkyboxTransition(targetSkybox, 0f);
        }
    }

    private async UniTask SmoothSkyboxTransition(Material targetSkybox, float duration)
    {
        isTransitioning = true;

        if (currentSkybox == null)
            currentSkybox = new Material(RenderSettings.skybox);

        Material startMat = new Material(RenderSettings.skybox);
        Material tempMat = new Material(startMat);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            tempMat.Lerp(startMat, targetSkybox, t);
            RenderSettings.skybox = tempMat;
            DynamicGI.UpdateEnvironment();
            await UniTask.Yield();
        }

        RenderSettings.skybox = targetSkybox;
        currentSkybox = targetSkybox;
        DynamicGI.UpdateEnvironment();
        isTransitioning = false;
    }
}
