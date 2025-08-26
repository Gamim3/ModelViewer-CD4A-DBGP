using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Environments")]
    [SerializeField] private Environment[] _environments;
    [SerializeField] private Environment _currentEnvironment;

    [SerializeField] private EnvironmentType _currentEnvironmentType;
    public EnvironmentType CurrentEnvironmentType
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

    private int _currentEnvironmentIndex = 0;
    [Header("References")]
    [SerializeField] private Renderer _skyboxRenderer;

    private void Start()
    {
        if (_environments.Length > 0)
        {
            ChangeEnvironment(_environments[0]);
        }
    }

    /// <summary>
    /// Changes the current environment to the specified one.
    /// </summary>
    /// <param name="newEnvironment"> Environment to change to </param>
    public void ChangeEnvironment(Environment newEnvironment)
    {
        if (newEnvironment == null || newEnvironment == _currentEnvironment) return;

        // Update skybox
        if (_skyboxRenderer != null && newEnvironment.skybox != null)
        {
            _skyboxRenderer.material.SetTexture(_currentEnvironmentIndex == 0 ? 1 : 0, newEnvironment.skybox.material.mainTexture);
        }

        // Update lighting
        RenderSettings.sun.transform.rotation = Quaternion.Euler(newEnvironment.lightAngle.x, newEnvironment.lightAngle.y, newEnvironment.lightAngle.z);

        // Deactivate previous environment light objects
        if (_currentEnvironment != null)
        {
            foreach (var lightObj in _currentEnvironment.lightObjects)
            {
                if (lightObj != null)
                {
                    lightObj.SetActive(false);
                }
            }
        }

        // Activate new environment light objects
        foreach (var lightObj in newEnvironment.lightObjects)
        {
            if (lightObj != null)
            {
                lightObj.SetActive(true);
            }
        }

        _currentEnvironment = newEnvironment;
    }

    /// <summary>
    /// Changes the lighting based on the specified environment type.
    /// </summary>
    /// <param name="environmentType"> EnvironmentType to switch lighting to </param>
    public void ChangeLighting(EnvironmentType environmentType)
    {
        var newEnvironment = System.Array.Find(_environments, env => env.environmentType == environmentType);
        if (newEnvironment != null)
        {
            ChangeEnvironment(newEnvironment);
        }
        else
        {
            Debug.LogWarning($"Environment of type {environmentType} not found.");
        }
    }


    public enum EnvironmentType
    {
        Light,
        Dark,
        Sunset,
    }

    [CreateAssetMenu(fileName = "New Environment", menuName = "Environment")]
    public class Environment : ScriptableObject
    {
        public string Name;
        public EnvironmentType environmentType;

        public Skybox skybox;
        public Vector3 lightAngle;
        public GameObject[] lightObjects;
    }
}
