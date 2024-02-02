using UnityEngine;

[ExecuteInEditMode]
public class DayNightCycle : MonoBehaviour {
    [SerializeField] private Material _skyboxMaterial;

    private Light _mainLight;
    private WorldClock _clock;

    [Header("Sun/Moon appearances")]
    [SerializeField] private float _horizonOffsetDegree = 3f;
    [SerializeField] private float _mainLightRotY = 45f;
    [SerializeField] private Texture2D _sunTexture;
    [SerializeField] private Vector2 _sunTiling = new Vector2(1.5f, 1.5f);
    [SerializeField] private Texture2D _moonTexture;
    [SerializeField] private Vector2 _moonTiling = new Vector2(2.5f, 2.5f);

    [Header("Sky colors")]
    [SerializeField] private Color _dayColor = new Color(19f / 255f, 114f / 255f, 166f / 255f); // peak at sunriseEndTime
    [SerializeField] private Color _dawnColor = new Color(19f / 255f, 35f / 255f, 55f / 255f); // peak at sunriseStartTime  
    [SerializeField] private Color _duskColor = new Color(70f / 255f, 53f / 255f, 43f / 255f); // peak at sunsetStartTime
    [SerializeField] private Color _nightColor = new Color(1f / 255f, 1f / 255f, 2f / 255f) * -1f; // peak at sunsetEndTime

    [Header("Fog colors")]
    [SerializeField] private Color _dayFogColor = new Color(240f / 255f, 240f / 255f, 240f / 255f);
    [SerializeField] private Color _nightFogColor = new Color(5 / 255f, 5 / 255f, 5 / 255f);

    [Header("Main light colors")]
    [SerializeField] private Color _mainLightDayColor = new Color(255f / 255f, 240f / 255f, 225f / 255f);
    [SerializeField] private Color _mainLightNightColor = new Color(101f / 255f, 110f / 255f, 152f / 255f);
    [SerializeField] private Color _mainLightduskColor = new Color(255f / 255f, 206f / 255f, 158f / 255f);
    [SerializeField] private Color _mainLightDawnColor = new Color(255f / 255f, 206f / 255f, 158f / 255f);

    [Header("Clouds opacity")]
    [SerializeField] private float _dayCloudsOpcacity = 2.54f;
    [SerializeField] private float _nightCloudsOpacity = 0.12f;
    [SerializeField] private float _dayHorizonCloudsOpcacity = 1f;
    [SerializeField] private float _nightHorizonCloudsOpacity = 0.05f;

    [Header("Ambient light intensity")]
    [SerializeField] private float _ambientMinIntensity = 0.2f;
    [SerializeField] private float _ambientMaxIntensity = 0.75f;

    [Header("Main light intensity")]
    [SerializeField] private float _mainLightMinIntensity = 0.4f;
    [SerializeField] private float _mainLightMaxIntensity = 1.2f;

    // Update is called once per frame
    void Update() {
        if(_clock == null) {
            _clock = GetComponent<WorldClock>();
        }
        if(_mainLight == null) {
            _mainLight = GetComponent<Light>();
        }

        float mainLightLerpValue = _clock.Clock.dayRatio > 0 ? _clock.Clock.dayRatio : _clock.Clock.nightRatio;
        float sunRotation = Mathf.Lerp(0 - _horizonOffsetDegree, 180 + _horizonOffsetDegree, mainLightLerpValue);
        transform.eulerAngles = new Vector3(sunRotation, _mainLightRotY, 0);

        // Update main light rotation with sky material
        ShareMainLightRotation();

        // Lerping lights
        UpdateLightIntensity();

        // Lerping sky color
        UpdateSkyColor();

        // Lerping fog color
        UpdateFogColor();

        // Lerping cloud opacity
        UpdateCloudsOpacity();

        // Update main light texture
        UpdateMainLightTexture();

        UpdateLightColor();
    }

    private void UpdateSkyColor() {
        Color skyColor = _skyboxMaterial.GetColor("_GradientColor1");
        if(_clock.Clock.dawnRatio > 0 && _clock.Clock.dawnRatio < 1) {
            skyColor = Color.Lerp(_nightColor, _dawnColor, _clock.Clock.dawnRatio);
        }
        if(_clock.Clock.brightRatio > 0) {
            if(_clock.Clock.brightRatio < 0.2f) {
                skyColor = Color.Lerp(_dawnColor, _dayColor, _clock.Clock.brightRatio / 0.2f);
            } else if(_clock.Clock.brightRatio < 1f) {
                skyColor = _dayColor;
            }
        }
        if(_clock.Clock.darkRatio > 0) {
            if(_clock.Clock.darkRatio < 0.05f) {
                skyColor = Color.Lerp(_duskColor, _nightColor, _clock.Clock.darkRatio / 0.05f);
            } else if(_clock.Clock.darkRatio < 1f) {
                skyColor = _nightColor;
            }
        }
        if(_clock.Clock.duskRatio > 0 && _clock.Clock.duskRatio < 1) {
            skyColor = Color.Lerp(_dayColor, _duskColor, _clock.Clock.duskRatio);
        }

        _skyboxMaterial.SetColor("_GradientColor1", skyColor);
    }

    private void UpdateLightColor() {
        Color lightColor = _mainLight.color;
        if(_clock.Clock.dawnRatio > 0 && _clock.Clock.dawnRatio < 1) {
            lightColor = Color.Lerp(_mainLightNightColor, _mainLightDawnColor, _clock.Clock.dawnRatio);
        }
        if(_clock.Clock.brightRatio > 0) {
            if(_clock.Clock.brightRatio < 0.2f) {
                lightColor = Color.Lerp(_mainLightDawnColor, _mainLightDayColor, _clock.Clock.brightRatio / 0.2f);
            } else if(_clock.Clock.brightRatio < 1f) {
                lightColor = _mainLightDayColor;
            }
        }
        if(_clock.Clock.darkRatio > 0) {
            if(_clock.Clock.darkRatio < 0.05f) {
                lightColor = Color.Lerp(_mainLightduskColor, _mainLightNightColor, _clock.Clock.darkRatio / 0.05f);
            } else if(_clock.Clock.darkRatio < 1f) {
                lightColor = _mainLightNightColor;
            }
        }
        if(_clock.Clock.duskRatio > 0 && _clock.Clock.duskRatio < 1) {
            lightColor = Color.Lerp(_mainLightDayColor, _mainLightduskColor, _clock.Clock.duskRatio);
        }
        _mainLight.color = lightColor;
    }

    private void UpdateFogColor() {
        Color fogColor = RenderSettings.fogColor;
        if(_clock.Clock.dawnRatio > 0 && _clock.Clock.dawnRatio < 1) {
            fogColor = Color.Lerp(fogColor, _dayFogColor, _clock.Clock.dawnRatio);
        }
        if(_clock.Clock.brightRatio > 0 && _clock.Clock.brightRatio < 1) {
            fogColor = _dayFogColor;
        }
        if(_clock.Clock.duskRatio > 0 && _clock.Clock.duskRatio < 1) {
            fogColor = Color.Lerp(_dayFogColor, _nightFogColor, _clock.Clock.duskRatio);
        }
        if(_clock.Clock.darkRatio > 0 && _clock.Clock.darkRatio < 1) {
            fogColor = _nightFogColor;
        }
        RenderSettings.fogColor = fogColor;
    }

    private void UpdateLightIntensity() {
        // Ambient light intensity
        RenderSettings.ambientIntensity = AdjustIntensity(_ambientMinIntensity, _ambientMaxIntensity, _clock.Clock.dawnRatio, _clock.Clock.duskRatio); ;

        // Main light intensity
        _mainLight.intensity = AdjustIntensity(_mainLightMinIntensity, _mainLightMaxIntensity, _clock.Clock.dawnRatio, _clock.Clock.duskRatio);
    }


    private float AdjustIntensity(float minIntensity, float fullIntensity, float dawnRatio, float duskRatio) {
        if(duskRatio > 0) {
            return Mathf.Lerp(fullIntensity, minIntensity, duskRatio);
        } else {
            return Mathf.Lerp(minIntensity, fullIntensity, dawnRatio);
        }
    }

    private void UpdateCloudsOpacity() {
        if(_clock.Clock.dawnRatio > 0 && _clock.Clock.dawnRatio < 1) {
            _skyboxMaterial.SetFloat("_Clouds_Opacity", Mathf.Lerp(_nightCloudsOpacity, _dayCloudsOpcacity, _clock.Clock.dawnRatio));
            _skyboxMaterial.SetFloat("_Horizon_Clouds_Opacity", Mathf.Lerp(_nightHorizonCloudsOpacity, _dayHorizonCloudsOpcacity, _clock.Clock.dawnRatio));
        }
        if(_clock.Clock.brightRatio > 0 && _clock.Clock.brightRatio < 1) {
            _skyboxMaterial.SetFloat("_Clouds_Opacity", _dayCloudsOpcacity);
            _skyboxMaterial.SetFloat("_Horizon_Clouds_Opacity", _dayHorizonCloudsOpcacity);
        }
        if(_clock.Clock.duskRatio > 0 && _clock.Clock.duskRatio < 1) {
            _skyboxMaterial.SetFloat("_Clouds_Opacity", Mathf.Lerp(_dayCloudsOpcacity, _nightCloudsOpacity, _clock.Clock.duskRatio));
            _skyboxMaterial.SetFloat("_Horizon_Clouds_Opacity", Mathf.Lerp(_dayHorizonCloudsOpcacity, _nightHorizonCloudsOpacity, _clock.Clock.duskRatio));
        }
        if(_clock.Clock.darkRatio > 0 && _clock.Clock.darkRatio < 1) {
            _skyboxMaterial.SetFloat("_Clouds_Opacity", _nightCloudsOpacity);
            _skyboxMaterial.SetFloat("_Horizon_Clouds_Opacity", _nightHorizonCloudsOpacity);
        }
    }

    private void UpdateMainLightTexture() {
        // Update texture 
        if(_clock.Clock.dayRatio > 0) {
            _skyboxMaterial.SetTexture("_SunMoon", _sunTexture);
            _skyboxMaterial.SetTextureScale("_SunMoon", _sunTiling);
        } else {
            _skyboxMaterial.SetTexture("_SunMoon", _moonTexture);
            _skyboxMaterial.SetTextureScale("_SunMoon", _moonTiling);
        }
    }

    private void ShareMainLightRotation() {
        _skyboxMaterial.SetVector("_MainLightForward", transform.forward);
        _skyboxMaterial.SetVector("_MainLightUp", transform.up);
        _skyboxMaterial.SetVector("_MainLightRight", transform.right);
    }
}
