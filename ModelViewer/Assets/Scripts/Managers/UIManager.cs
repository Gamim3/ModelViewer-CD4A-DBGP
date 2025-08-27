using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private List<Button> _spawnedModelButtons = new();
    [Header("Spawnable button info")]
    [SerializeField] private Button _modelUIBtn;
    [SerializeField] private TMP_Text _modelInfoText;
    [Header("Model button location info")]
    [SerializeField] private RectTransform _modelButtonHolder;
    [SerializeField] private RectTransform _descriptionPanel;

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
    }

    private void OnEnable()
    {
        //TEMP uitgezet wegens geen model manager in test scene
        ModelManager.Instance.OnModelChanged += RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded += SpawnButton;
    }

    private void OnDisable()
    {
        ModelManager.Instance.OnModelChanged -= RefreshModelInfo;
        ModelManager.Instance.OnModelLoaded -= SpawnButton;
    }

    /// <summary>
    /// Spawns a button for the latest model that was preloaded
    /// </summary>
    /// <param name="model"> Model corresponding to the wanted model </param>
    public void SpawnButton(Model model)
    {
        Button newButton = Instantiate(_modelUIBtn, _modelButtonHolder.transform.position, _modelButtonHolder.transform.rotation, _modelButtonHolder);
        newButton.GetComponentInChildren<TMP_Text>().text = model.modelName;
        newButton.GetComponent<Image>().sprite = model.previewImage;
        newButton.onClick.AddListener(() => ModelManager.Instance.SelectModel(model));
        _spawnedModelButtons.Add(newButton);
    }

    public void OnEnviromentButtonClicked(int index)
    {
        //call naar enviromentcontroller om de enviroment te veranderen
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

    public void QuitApplication()
    {
        Application.Quit();
    }
}
