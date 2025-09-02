using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private CameraController _cameraController;

    private List<GalleryButton> _spawnedModelButtons = new();
    private List<GalleryButton> _spawnedEnvironmentButtons = new();

    [Header("References")]
    [Tooltip("The prefab of the model button to spawn")]
    [SerializeField] private GalleryButton _modelUIBtn;
    [Tooltip("The prefab of the enviroment button to spawn")]
    [SerializeField] private GalleryButton _environmentUIBtn;
    [SerializeField] private TMP_Text _modelInfoText;
    [Space]
    [Tooltip("The gameobject to spawn the model buttons under")]
    [SerializeField] private RectTransform _modelButtonHolder;
    [Tooltip("The gameobject to spawn the enviroment buttons under")]
    [SerializeField] private RectTransform _environmentButtonHolder;
    [SerializeField] private RectTransform _descriptionPanel;

    [Header("Quitting")]
    [SerializeField] private Button _quitButton;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private Button _quitYesButton;
    [SerializeField] private Button _quitNoButton;

    [Header("Reset View")]
    [SerializeField] private Button _resetViewButton;

    [Header("RenderType")]
    [SerializeField] private Button _nextRenderType;
    [SerializeField] private Button _previousRenderType;
    private int _renderTypeIndex;
    [SerializeField] private Animator _renderTypeAnimator;
    [SerializeField] private string _renderTypeBlendName = "Blend";
    [SerializeField, Range(0.1f, 5f)] private float _renderTransitionSpeed = 1f;
    private Coroutine _renderTypeCoroutine;
    private int _targetValue;

    [Header("Gallery")]
    [SerializeField] private Button[] _galleryButtons;
    [SerializeField] private Animator _galleryAnimator;
    [SerializeField] private string _galleryCanOpenName = "CanOpen";

    [Header("Loading Screen")]
    [SerializeField] private Animator _startLoadingAnimator;
    [SerializeField] private Animator _modelLoadingAnimator;
    [SerializeField] private string _loadingBlendName = "Blend";
    [SerializeField, Range(0.1f, 5f)] private float _loadingTransitionSpeed = 2f;
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _backgroundLoadScreen;
    [SerializeField] private TMP_Text _loadingText;
    private List<AsyncOperation> _loadingActions = new();
    private List<AsyncOperation> _completedLoadingActions = new();

    [Header("Tabs")]
    [SerializeField] private List<Tab> _tabs;

    [SerializeField] private Color _activeButtonColor;
    [SerializeField] private Color _inactiveButtonColor;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (var environment in EnvironmentManager.Instance.Environments)
        {
            SpawnEnvironmentButton(environment);
        }

        // Subscribe to model events
        ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded += SpawnModelButton;

        // Subascribe to ChangeRendertype events
        _nextRenderType.onClick.AddListener(() => StartCoroutine(ChangeRenderType(-1)));
        _previousRenderType.onClick.AddListener(() => StartCoroutine(ChangeRenderType(1)));

        if (_tabs.Count > 0)
        {
            foreach (var tab in _tabs)
            {
                tab.TabButton.onClick.AddListener(() => OpenTab(tab));
            }
        }

        // Setup quitting
        _quitButton.onClick.AddListener(() => _quitPanel.SetActive(!_quitPanel.activeSelf));
        _quitYesButton.onClick.AddListener(QuitApplication);
        _quitNoButton.onClick.AddListener(() => _quitPanel.SetActive(false));
        _quitPanel.SetActive(false);

        _resetViewButton.onClick.AddListener(() => _cameraController.ResetCamera());

        if (_modelInfoText != null)
            _modelInfoText.text = "No model selected";

        foreach (var button in _galleryButtons)
        {
            button.onClick.AddListener(() => ToggleGallery(true));
        }
    }

    private void OnDisable()
    {
        ModelManager.Instance.OnModelChanged -= RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded -= SpawnModelButton;
    }

    /// <summary>
    /// Spawns a button for the latest model that was preloaded
    /// </summary>
    /// <param name="model"> Model corresponding to the wanted model </param>
    private void SpawnModelButton(Model model)
    {
        GalleryButton newButton = Instantiate(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation, _modelButtonHolder);
        newButton.image.sprite = model.previewImage;
        newButton.button.onClick.AddListener(() => ModelManager.Instance.SelectModel(model));
        _spawnedModelButtons.Add(newButton);
    }

    /// <summary>
    /// Spawns a button for all the avalible enviroments
    /// </summary>
    /// <param name="environment"> Model corresponding to the wanted model </param>
    private void SpawnEnvironmentButton(Environment environment)
    {
        if (environment == null) return;

        GalleryButton newButton = Instantiate(_environmentUIBtn, _environmentButtonHolder);
        newButton.button.onClick.AddListener(() => EnvironmentManager.Instance.ChangeEnvironment(environment));
        newButton.image.sprite = environment.previewImage;
        _spawnedEnvironmentButtons.Add(newButton);
    }

    /// <summary>
    /// Handles the loading screen while there are still loading actions in the list
    /// </summary>
    private async void LoadScreen()
    {
        if (_loadingScreen.activeSelf) return;

        _loadingScreen.SetActive(true);

        _startLoadingAnimator.SetFloat(_loadingBlendName, 1);

        int loadedCount = 0;
        while (_loadingActions.Count > loadedCount)
        {
            for (int i = 0; i < _loadingActions.Count; i++)
            {
                if (_completedLoadingActions.Contains(_loadingActions[i]))
                    continue;

                if (_loadingActions[i].isDone)
                {
                    _completedLoadingActions.Add(_loadingActions[i]);
                    loadedCount++;
                }
            }

            _loadingText.text = $"Loading...\n{loadedCount} / {_loadingActions.Count}";

            await Task.Yield();
        }

        await Task.Delay(1000);
        _loadingText.text = "Loading complete!";
        await Task.Delay(1000);

        while (_startLoadingAnimator.GetFloat(_loadingBlendName) > 0)
        {
            float currentValue = _startLoadingAnimator.GetFloat(_loadingBlendName);
            float newValue = Mathf.MoveTowards(currentValue, 0, Time.deltaTime * _loadingTransitionSpeed);
            _startLoadingAnimator.SetFloat(_loadingBlendName, newValue);
            await Task.Yield();
        }

        _startLoadingAnimator.SetFloat(_loadingBlendName, 0);

        _loadingScreen.SetActive(false);
        _loadingActions.Clear();
    }

    public void AddLoadingAction(AsyncOperation operation)
    {
        _loadingActions.Add(operation);
        LoadScreen();
    }

    /// <summary>
    /// Sets all the inforation of a model in the info UI based on last clicked model.
    /// </summary>
    /// <param name="modelinfo"></param>
    private void RefreshModelInfo(Model modelinfo)
    {
        if (_modelInfoText == null) return;
        _modelInfoText.text =
        $"Naam model: {modelinfo.modelName}" +
        $"\nOmschrijving: {modelinfo.description}" +
        $"\nNaam artist: {modelinfo.creatorName}" +
        $"\nPollyCount: {modelinfo.polyCount}" +
        $"\nTriCount:  {modelinfo.triCount}" +
        $"\nTextureCount: {modelinfo.textureCount}";
    }

    /// <summary>
    /// Turns on a loadingscreen behind the UI for <paramref name="milliseconds"/> milliseconds
    /// </summary>
    /// <param name="milliseconds"> Milliseconds  </param>
    public async void LoadScreen(float milliseconds)
    {
        Debug.Log($"Loading screen for {milliseconds} milliseconds");
        if (_loadingText != null)
            _loadingText.text = "Loading model...";

        float currentLoadTime = 0;

        _backgroundLoadScreen.SetActive(true);

        while (_modelLoadingAnimator.GetFloat(_loadingBlendName) < 1)
        {
            float currentValue = _modelLoadingAnimator.GetFloat(_loadingBlendName);
            float newValue = Mathf.MoveTowards(currentValue, 1, Time.deltaTime * _loadingTransitionSpeed);
            _modelLoadingAnimator.SetFloat(_loadingBlendName, newValue);

            await Task.Yield();

            currentLoadTime += Time.deltaTime * 1000;
            if (currentLoadTime >= milliseconds / 2)
                break;
        }

        _modelLoadingAnimator.SetFloat(_loadingBlendName, 1);

        await Task.Delay((int)milliseconds - (int)currentLoadTime);

        while (_modelLoadingAnimator.GetFloat(_loadingBlendName) > 0)
        {
            float currentValue = _modelLoadingAnimator.GetFloat(_loadingBlendName);
            float newValue = Mathf.MoveTowards(currentValue, 0, Time.deltaTime * _loadingTransitionSpeed);
            _modelLoadingAnimator.SetFloat(_loadingBlendName, newValue);

            await Task.Yield();

            currentLoadTime += Time.deltaTime * 1000;
            if (currentLoadTime >= milliseconds)
                break;
        }

        _modelLoadingAnimator.SetFloat(_loadingBlendName, 0);

        _backgroundLoadScreen.SetActive(false);
    }

    /// <summary>
    /// Handles the changing of the rendertexture from the UI side and gives te call to the ModelManager
    /// </summary>
    public IEnumerator ChangeRenderType(int value)
    {
        while (_renderTypeCoroutine != null)
        {
            yield return null;
        }

        // Calculate the target value based on current index and input value
        _targetValue = _renderTypeIndex + value;
        if (_renderTypeAnimator != null)
        {
            if (_renderTypeIndex == 0 && value == -1)
            {
                _renderTypeAnimator.SetFloat(_renderTypeBlendName, 3);
                _targetValue = 2;
            }

            _renderTypeCoroutine = StartCoroutine(LerpRenderType());
        }

        _renderTypeIndex = _targetValue;

        if (_targetValue >= 3)
        {
            _renderTypeIndex = 0;
        }

        ModelManager.Instance.ChangeRenderType((RenderType)_renderTypeIndex);
        yield return null;
    }

    /// <summary>
    /// Handles the lerping of the render type animation
    /// </summary>
    private IEnumerator LerpRenderType()
    {
        while (Mathf.Abs(_renderTypeAnimator.GetFloat(_renderTypeBlendName) - _targetValue) > 0.01f)
        {
            float currentValue = _renderTypeAnimator.GetFloat(_renderTypeBlendName);
            float newValue = Mathf.MoveTowards(currentValue, _targetValue, _renderTransitionSpeed * Time.deltaTime);
            _renderTypeAnimator.SetFloat(_renderTypeBlendName, newValue);
            yield return null;
        }

        _renderTypeAnimator.SetFloat(_renderTypeBlendName, _targetValue);

        if (_targetValue == 3)
        {
            _renderTypeAnimator.SetFloat(_renderTypeBlendName, 0);
        }

        _renderTypeCoroutine = null;
    }

    /// <summary>
    /// Opens <paramref name="tab"/> and closes all others
    /// </summary>
    /// <param name="tab"> Tab to open </param>
    private void OpenTab(Tab tab)
    {
        if (tab.TabAnimator != null)
            tab.TabAnimator.SetTrigger("Toggle");
        else
        {
            foreach (var t in _tabs)
            {
                if (t.TabContent == null) continue;

                bool isActive = t == tab;
                t.TabContent.gameObject.SetActive(isActive);

                t.TabImage.color = isActive ? _activeButtonColor : _inactiveButtonColor;
            }
        }
    }

    public void ToggleGallery(bool value)
    {
        _galleryAnimator.SetBool(_galleryCanOpenName, value);
        if (!value)
        {
            foreach (var tab in _tabs)
            {
                tab.TabImage.color = _inactiveButtonColor;
            }
        }
    }


    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// Class to hold all the references for a tab
    /// </summary>
    [Serializable]
    private class Tab
    {
        public Button TabButton;
        public RectTransform TabContent;
        public Image TabImage;
        public Animator TabAnimator;

        public Tab(Button button, RectTransform content)
        {
            TabButton = button;
            TabContent = content;
        }
    }
}
