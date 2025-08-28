using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private List<Button> _spawnedModelButtons = new();
    private List<Button> _spawnedEnvironmentButtons = new();

    [Header("Spawnable button info")]
    [Tooltip("The prefab of the model button to spawn")]
    [SerializeField] private Button _modelUIBtn;
    [Tooltip("The prefab of the enviroment button to spawn")]
    [SerializeField] private Button _environmentUIBtn;
    [SerializeField] private TMP_Text _modelInfoText;

    [Header("Model button location info")]
    [Tooltip("The gameobject to spawn the model buttons under")]
    [SerializeField] private RectTransform _modelButtonHolder;
    [Tooltip("The gameobject to spawn the enviroment buttons under")]
    [SerializeField] private RectTransform _environmentButtonHolder;
    [SerializeField] private RectTransform _descriptionPanel;

    [Header("The buttons to change the render type")]
    public Button nextRenderType;
    public Button previousRenderType;
    public Button changeRenderType;
    private int _renderTypeIndex;

    [SerializeField] private Animator _descriptionPanelAnimator;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private TMP_Text _loadingText;

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

        foreach (var environment in EnvironmentManager.Instance.Environments)
        {
            SpawnEnvironmentButton(environment);
        }
    }

    private void OnEnable()
    {
        ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded += SpawnModelButton;
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
        Button newButton = Instantiate(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation, _modelButtonHolder);
        newButton.GetComponentInChildren<TMP_Text>().text = model.modelName;
        newButton.GetComponent<Image>().sprite = model.previewImage;
        newButton.onClick.AddListener(() => ModelManager.Instance.SelectModel(model));
        _spawnedModelButtons.Add(newButton);
    }

    /// <summary>
    /// Spawns a button for all the avalible enviroments
    /// </summary>
    /// <param name="model"> Model corresponding to the wanted model </param>
    private void SpawnEnvironmentButton(Environment environment)
    {
        if (environment == null) return;

        Button newButton = Instantiate(_environmentUIBtn, _environmentButtonHolder);
        newButton.GetComponentInChildren<TMP_Text>().text = environment.Name;
        newButton.onClick.AddListener(() => EnvironmentManager.Instance.ChangeEnvironment(environment));
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
            for (int i = _loadingActions.Count - 1; i >= 0; i--)
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
        await System.Threading.Tasks.Task.Delay(500);
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
        _modelInfoText.text =
        "Naam model: " + modelinfo.modelName + "\n"
            + "Omschrijving: " + modelinfo.description + "\n"
            + "Naam artist: " + modelinfo.creatorName + "\n"
            + "PollyCount = " + modelinfo.polyCount + "\n"
            + "TriCount = " + modelinfo.triCount + "\n"
            + "TextureCount = " + modelinfo.textureCount;
    }


    /// <summary>
    /// Handles the changing of the rendertexture from the UI side and gives te call to the ModelManager
    /// </summary>
    public void SetRenderIndex(int index)
    {
        _renderTypeIndex += index;
        if(_renderTypeIndex == -1)
        {
            _renderTypeIndex = 2;
        }

        if (_renderTypeIndex == 3)
        {
            _renderTypeIndex = 0;
        }
    }

    public void ChangeRenderType()
    {
        ModelManager.Instance.ChangeRenderType((RenderType)_renderTypeIndex);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
