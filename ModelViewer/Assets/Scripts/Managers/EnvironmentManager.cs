using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [Header("Environments")]
    [Tooltip("All available environments")]
    [SerializeField] private Environment[] _environments;
    [SerializeField, ReadOnly] private Environment _currentEnvironment;
    [Tooltip("Environment to load on start(will pick first environment if empty)")]
    [SerializeField] private Environment _defaultEnvironment;
    [Tooltip("Speed at which to transition between environments")]
    [SerializeField] private float _transitionSpeed = 1f;

    public Environment[] Environments => _environments;
    private Coroutine _environmentLerpCoroutine;
    private Coroutine _solidBackgroundLerpCoroutine;

    [Header("Lighting")]
    [SerializeField] private GameObject[] _lightObjects;
    [SerializeField] private GameObject[] _darkObjects;
    [SerializeField] private GameObject[] _sunsetObjects;

    [SerializeField] private LightingType _currentEnvironmentType;
    public LightingType CurrentEnvironmentType
    {
        get { return _currentEnvironmentType; }
        set
        {
            if (_currentEnvironmentType != value)
            {
                _currentEnvironmentType = value;
                ChangeLighting(_currentEnvironmentType);
            }
        }
    }
    [Space]
    private int _currentTextureIndex = 0;
    private int _currentSolidBackgroundIndex = 0;

    [Header("References")]
    private Material Skybox
    {
        get => RenderSettings.skybox;
        set => RenderSettings.skybox = value;
    }

    [SerializeField] private string _skyBoxTextureOneName = "_Cubemap";
    [SerializeField] private string _skyBoxTextureTwoName = "_Cubemap2";
    [SerializeField] private string _skyBoxLerpName = "_Lerp";
    [SerializeField] private string _solidBackgroundLerpName = "_LerpSolidBackground";

    public Action<Environment> OnEnvironmentChanged;
    public Action<LightingType> OnLightingChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        Skybox.SetFloat(_skyBoxLerpName, 0f);
        Skybox.SetFloat(_solidBackgroundLerpName, 0f);

        _currentTextureIndex = 0;
        _currentSolidBackgroundIndex = 0;

        if (_defaultEnvironment != null)
        {
            ChangeEnvironment(_defaultEnvironment);
        }
        else if (_environments.Length > 0)
        {
            ChangeEnvironment(_environments[0]);
        }

    }

    // DEBUG REMOVE LATER
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeEnvironment(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeEnvironment(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeEnvironment(2);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ChangeLighting(LightingType.Light);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeLighting(LightingType.Dark);
        }
    }

    /// <summary>
    /// Changes the current environment to the <paramref name="newEnvironment"/>.
    /// </summary>
    /// <param name="newEnvironment"> Environment to change to </param>
    public void ChangeEnvironment(Environment newEnvironment)
    {
        if (newEnvironment == null || newEnvironment == _currentEnvironment) return;

        _currentTextureIndex = _currentTextureIndex == 1 ? 0 : 1;

        if (_environmentLerpCoroutine != null)
        {
            StopCoroutine(_environmentLerpCoroutine);
            _environmentLerpCoroutine = null;
        }

        // Update skybox
        if (newEnvironment.skyboxTexture != null)
        {
            var targetTexture = newEnvironment.skyboxTexture;
            _environmentLerpCoroutine = StartCoroutine(LerpSkyBox(targetTexture));
        }

        // Update sun
        if (RenderSettings.sun != null)
            RenderSettings.sun.transform.rotation = Quaternion.Euler(newEnvironment.lightAngle.x, newEnvironment.lightAngle.y, newEnvironment.lightAngle.z);

        ChangeLighting(newEnvironment.lightingType);

        _currentEnvironment = newEnvironment;
        OnEnvironmentChanged?.Invoke(newEnvironment);
    }

    /// <summary>
    /// Changes the current environment based on the <paramref name="index"/> in the _environments array.
    /// </summary>
    /// <param name="index"> Index of environment to change to </param>
    public void ChangeEnvironment(int index)
    {
        if (index < 0 || index >= _environments.Length) return;

        ChangeEnvironment(_environments[index]);
    }

    /// <summary>
    /// Changes the lighting based on the specified <paramref name="environmentType"/>.
    /// </summary>
    /// <param name="environmentType"> EnvironmentType to switch lighting to </param>
    public void ChangeLighting(LightingType environmentType)
    {
        foreach (var light in _lightObjects)
        {
            if (light != null)
            {
                light.SetActive(environmentType == LightingType.Light);
            }
        }
        foreach (var dark in _darkObjects)
        {
            dark.SetActive(environmentType == LightingType.Dark);
        }
        foreach (var sunset in _sunsetObjects)
        {
            sunset.SetActive(environmentType == LightingType.Sunset);
        }

        _currentEnvironmentType = environmentType;
        OnLightingChanged?.Invoke(environmentType);
    }

    /// <summary>
    /// Toggles the solid background on and off with a smooth transition.
    /// </summary>
    public void ToggleSolidBackground(bool value)
    {
        if (value && _currentSolidBackgroundIndex == 1) return;

        _currentSolidBackgroundIndex = value ? 1 : 0;

        if (_solidBackgroundLerpCoroutine != null)
        {
            StopCoroutine(_solidBackgroundLerpCoroutine);
            _solidBackgroundLerpCoroutine = null;
        }

        _solidBackgroundLerpCoroutine = StartCoroutine(LerpSolidBackground());
    }

    /// <summary>
    /// Smoothly lerps the skybox texture transition using it's shader. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator LerpSkyBox(Cubemap targetTexture)
    {
        if (Skybox == null || Skybox.HasFloat(_skyBoxLerpName) == false)
        {
            RenderSettings.skybox.mainTexture = targetTexture;
            Debug.Log($"Skybox material does not contain {_skyBoxLerpName}, setting texture directly.");
            yield break;
        }

        if (_currentTextureIndex == 0)
        {
            Skybox.SetTexture(_skyBoxTextureOneName, targetTexture);
        }
        else
        {
            Skybox.SetTexture(_skyBoxTextureTwoName, targetTexture);
        }

        while (Skybox.GetFloat(_skyBoxLerpName) != _currentTextureIndex)
        {
            var currentLerpValue = Skybox.GetFloat(_skyBoxLerpName);
            var t = Time.deltaTime;
            var lerpGoal = _currentTextureIndex == 1 ? currentLerpValue + t * _transitionSpeed : currentLerpValue - t * _transitionSpeed;

            lerpGoal = Mathf.Clamp01(lerpGoal);

            Skybox.SetFloat(_skyBoxLerpName, lerpGoal);

            yield return null;
        }
    }

    private IEnumerator LerpSolidBackground()
    {
        if (Skybox == null || Skybox.HasFloat(_solidBackgroundLerpName) == false)
        {
            Debug.LogError("Shader did not contain solid background lerp property!");
            yield break;
        }

        while (Skybox.GetFloat(_solidBackgroundLerpName) != _currentSolidBackgroundIndex)
        {
            var currentLerpValue = Skybox.GetFloat(_solidBackgroundLerpName);
            var t = Time.deltaTime;
            var lerpGoal = _currentSolidBackgroundIndex == 1 ? currentLerpValue + t * _transitionSpeed : currentLerpValue - t * _transitionSpeed;

            lerpGoal = Mathf.Clamp01(lerpGoal);
            Skybox.SetFloat(_solidBackgroundLerpName, lerpGoal);

            yield return null;
        }
    }
}

public enum LightingType
{
    Light,
    Dark,
    Sunset,
}