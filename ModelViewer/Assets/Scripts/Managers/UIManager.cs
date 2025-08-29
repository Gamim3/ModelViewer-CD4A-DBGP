using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField, Range(0.1f, 5f)] private float _transitionSpeed = 1f;

    [Header("UX")]
    [SerializeField] private Animator _uiAnimator;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private TMP_Text _loadingText;

    [Header("Tabs")]
    [SerializeField] private List<Tab> _tabs;

    [SerializeField] private Color _activeButtonColor;
    [SerializeField] private Color _inactiveButtonColor;


    private List<AsyncOperation> _loadingActions = new();

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

        ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded += SpawnModelButton;

        _nextRenderType.onClick.AddListener(() => ChangeRenderType(1));
        _previousRenderType.onClick.AddListener(() => ChangeRenderType(-1));

        if (_tabs.Count > 0)
        {
            foreach (var tab in _tabs)
            {
                tab.TabButton.onClick.AddListener(() => OpenTab(tab));
            }
            OpenTab(_tabs[0]);
        }

        _quitButton.onClick.AddListener(() => _quitPanel.SetActive(!_quitPanel.activeSelf));
        _quitYesButton.onClick.AddListener(QuitApplication);
        _quitNoButton.onClick.AddListener(() => _quitPanel.SetActive(false));
        _quitPanel.SetActive(false);

        _resetViewButton.onClick.AddListener(() => _cameraController.ResetCamera());

        if (_modelInfoText != null)
            _modelInfoText.text = "No model selected";
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

        int loadedCount = 0;
        while (_loadingActions.Count > loadedCount)
        {
            _loadingScreen.SetActive(true);
            for (int i = 0; i < _loadingActions.Count; i++)
            {
                if (_loadingActions[i].isDone)
                {
                    loadedCount++;
                }
            }
            await System.Threading.Tasks.Task.Yield();

            if (_loadingText != null)
                _loadingText.text = $"Loading...\n{loadedCount} / {_loadingActions.Count}";
        }

        _loadingText.text = "Loading complete!";
        await System.Threading.Tasks.Task.Delay(2000);
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
        $" \n Omschrijving: {modelinfo.description}" +
        $" \n Naam artist: {modelinfo.creatorName}" +
        $" \n PollyCount: {modelinfo.polyCount}" +
        $" \n TriCount:  {modelinfo.triCount}" +
        $" \n TextureCount: {modelinfo.textureCount}";
    }

    /// <summary>
    /// Handles the changing of the rendertexture from the UI side and gives te call to the ModelManager
    /// </summary>
    public void ChangeRenderType(int index)
    {
        if (_renderTypeAnimator != null)
        {
            float targetValue = _renderTypeIndex + index;
            if (_renderTypeIndex == 0 && index == -1)
            {
                _renderTypeAnimator.SetFloat(_renderTypeBlendName, 3);
                targetValue = 2;
            }

            IEnumerator LerpRenderType()
            {
                while (Mathf.Abs(_renderTypeAnimator.GetFloat(_renderTypeBlendName) - targetValue) > 0.01f)
                {
                    float currentValue = _renderTypeAnimator.GetFloat(_renderTypeBlendName);
                    float newValue = Mathf.MoveTowards(currentValue, targetValue, _transitionSpeed * Time.deltaTime);
                    _renderTypeAnimator.SetFloat(_renderTypeBlendName, newValue);
                    yield return null;
                }
                _renderTypeAnimator.SetFloat(_renderTypeBlendName, targetValue);

                if (targetValue == 3)
                {
                    _renderTypeAnimator.SetFloat(_renderTypeBlendName, 0);
                }
            }
            StartCoroutine(LerpRenderType());
        }

        _renderTypeIndex = (_renderTypeIndex + index) % Enum.GetNames(typeof(RenderType)).Length;

        ModelManager.Instance.ChangeRenderType((RenderType)_renderTypeIndex);
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

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

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
