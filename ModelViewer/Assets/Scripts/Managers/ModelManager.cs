using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ModelManager : MonoBehaviour
{
    #region Variables

    public static ModelManager Instance;

    [SerializeField] private RenderType _currentRenderType;
    [Header("Models")]
    [Tooltip("All models that can be viewed")]
    public Model[] allModels;
    [Space]
    [SerializeField] private List<Model> _spawnedModels = new();

    public Model SelectedModel { get => _selectedModelIndex >= 0 ? _spawnedModels[_selectedModelIndex] : null; }
    [Space]
    [SerializeField] private int _selectedModelIndex = -1;
    [Space]
    [SerializeField] private Transform _modelHolder;

    public Action<Model> OnModelLoaded;
    public Action<Model> OnModelChanged;

    #endregion

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

    private async void Start()
    {
        if (_modelHolder == null)
        {
            Debug.LogError("SpawnPos not set up in the inspector!");
            return;
        }
        float startTime = Time.time;
        await PreloadModels();
        Debug.Log($"Loaded {allModels.Length} models in {Time.time - startTime} seconds");
    }

    /// <summary>
    /// Asynchronously spawns all models in advance to reduce runtime load times
    /// </summary>
    /// <returns> The awaitable Task </returns>
    private async Task PreloadModels()
    {
        for (int i = 0; i < allModels.Length; i++)
        {
            var modelToSpawn = allModels[i];
            var spawnOperation = InstantiateAsync(modelToSpawn, _modelHolder);
            UIManager.Instance.AddLoadingAction(spawnOperation);
            var spawnedModel = await spawnOperation;
            _spawnedModels.Add(spawnedModel[0]);

            spawnedModel[0].SetActive(false);
            OnModelLoaded?.Invoke(spawnedModel[0]);
        }
    }

    /// <summary>
    /// Selects a model based on reference
    /// </summary>
    /// <param name="model"> Model to view </param>
    public void SelectModel(Model model)
    {
        if (SelectedModel != null)
        {
            SelectedModel.SetActive(false);
        }

        _selectedModelIndex = _spawnedModels.FindIndex(x => x == model);

        if (_selectedModelIndex != -1)
        {
            SelectedModel.ChangeRenderMode(_currentRenderType);
            SelectedModel.SetActive(true);
        }

        OnModelChanged?.Invoke(SelectedModel);
    }

    /// <summary>
    /// Selects a model based on it's index in the list
    /// </summary>
    /// <param name="index"> Index of model to view </param>
    public void SelectModel(int index)
    {
        if (index < 0 || index >= _spawnedModels.Count)
        {
            SelectModel(null);
            return;
        }

        SelectModel(_spawnedModels[index]);
    }

    /// <summary>
    /// Changes the render type of the currently selected model
    /// </summary>
    /// <param name="renderType"> RenderType to switch to </param>
    public void ChangeRenderType(RenderType renderType)
    {
        if (_currentRenderType == renderType) return;

        _currentRenderType = renderType;

        if (SelectedModel == null) return;

        SelectedModel.ChangeRenderMode(_currentRenderType);

        if (renderType == RenderType.Clay)
        {
            EnvironmentManager.Instance.ToggleSolidBackground(true);
        }
        else
        {
            EnvironmentManager.Instance.ToggleSolidBackground(false);
        }
    }

}

public enum RenderType
{
    Textured, Clay, Unlit
}